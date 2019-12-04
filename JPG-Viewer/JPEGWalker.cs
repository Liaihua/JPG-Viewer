using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JPG_Viewer
{
    // У меня есть 2 основных варианта по созданию приложения:
    //  1. Продолжать разрабатывать функционал для чтения JPEG, затем п.2
    //  2. Временно перейти к попытке связать между собой БД и приложение (тут тоже есть свои проблемы), затем п.1
    class JPEGWalker 
    {
        bool isLittleEndian;
        public List<string> FindJPEGInDirectory(string dir)
        {
            if (string.IsNullOrWhiteSpace(dir))
                return null;
            List<string> JPEGPaths = new List<string>();
            DirectoryInfo directory = new DirectoryInfo(dir);
            try
            {
                foreach (FileInfo file in directory.GetFiles("*", SearchOption.AllDirectories))
                {
                    if (file.Extension.Contains("jpg")  ||
                        file.Extension.Contains("jpeg"))
                        JPEGPaths.Add(file.FullName);
                }
            }
            catch (Exception ex) { System.Windows.MessageBox.Show(ex.Message); }
            return JPEGPaths;
        }

        public List<string> FindAllPathsInDirectory(string dir) // Данный метод не предусматривает вложенность каталогов
        {
            if (string.IsNullOrWhiteSpace(dir))
                return null;
            List<string> Dirs = new List<string>();
            DirectoryInfo directory = new DirectoryInfo(dir);
            try
            {
                foreach (var d in directory.GetDirectories("*", SearchOption.AllDirectories).ToList())
                {
                    Dirs.Add(d.Parent + "/" + d.Name);
                }
            }
            catch (Exception ex) { System.Windows.MessageBox.Show(ex.Message); }
            return Dirs;
        }

        public float GetFileLength(string path)
        {
            return new FileInfo(path).Length / 1024;
        }

        private bool CheckEndiannessInStream(BinaryReader reader) // true - для прямого порядка (LE), false - для обратного (BE)
        {
            reader.BaseStream.Seek(8, SeekOrigin.Current);
            ushort endianTag = reader.ReadUInt16();
            ushort Tag42 = reader.ReadUInt16();
            reader.BaseStream.Seek(-12, SeekOrigin.Current);
            if (endianTag == 0x4949 && Tag42 == 0x002A) // "II"
                return true;
            else                                        // "MM"
                return false;
        }

        private string ReadExifTagsInFileStream(BinaryReader reader)
        {
            string foundTags = "";
            isLittleEndian = CheckEndiannessInStream(reader);
            ushort length = reader.ReadUInt16();
            if(!isLittleEndian)
                length = (ushort)(((length << 8) | (length >> 8) & 0xFF) - 2); // изменение порядка байтов с little на big-endian. Любезно предоставлено переполнением стека
            reader.BaseStream.Seek(8, SeekOrigin.Current); // пропускаем '<длина>Exif\0\0'
            
            ushort mark = 0;
            while (length-- > 0)
            {
                mark = (ushort)(reader.ReadByte() << 0 | mark << 8);
                switch (mark)
                {
                    case (ushort)EXIFMetadataEnum.Contrast:
                        MessageBox.Show("contrast");
                        break;
                    case (ushort)TIFFMetadataEnum.Make:
                        //MessageBox.Show("make");
                        break;
                }
                foundTags += Encoding.UTF8.GetString(new byte[] { (byte)(mark >> 0)}); // надо узнать информацию о маркерах метаинфы в спецификации
            }
            return foundTags;
        }

        private string ReadTiffTagsInFileStream(BinaryReader reader)
        {
            string foundTags = "";
            ushort length = reader.ReadUInt16();
            length = (ushort)(((length << 8) | (length >> 8) & 0xFF) - 2);
            ushort mark = 0;
            while (length-- != 0)
            {
                if (isLittleEndian)
                    mark = (ushort)(reader.ReadByte() << 0 | mark << 8);
                else
                    mark = (ushort)(reader.ReadByte() << 8 | mark << 0);
                
                /*
                switch(mark)
                {
                    case (ushort)TIFFMetadataEnum.DateTime:
                        MessageBox.Show($"{mark}");
                        break;
                }
                */
                foundTags += Encoding.UTF8.GetString(new byte[] { (byte)(mark >> 0)});
            }
            return foundTags;
        }

        public string ReadExifInFile(string path) // Функция начинает делать слишком много. Нужно переделать
        {
            // Каждый сектор заголовка JPEG состоит из
            //      1. маркера (0xFFE1 - Exif)
            //      2. длины сектора (два байта) + 2б длина маркера
            //      3. самих данных
            // Надо найти спецификацию JPEG, наименования маркеров и сделать чтение по этим маркерам
            // Есть вариант с чтением до маркера SOS, используя switch(mark)
            string kindaExif = "";

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    ushort mark = 0; // зта переменная используется в качестве двухбайтового контейнера
                                        
                    // 0xE0 - старый JFIF, 0xE1 - более новая версия стандарта Exif. Есть проблемы с чтением APP0 и APP1 одновременно
                    // У нас есть несколько вариантов развития событий:
                    //      1. Нет ни EXIF, ни TIFF. Просто доходим до конца файла. Вообще, разве есть кто-то, кто намеренно убирает APP0 или APP1?
                    //      2. Нет EXIF, но есть TIFF.
                    //      3. Нет TIFF, но есть EXIF.
                    //      4. Есть и EXIF, и TIFF.
                    
                    while (fs.Position != fs.Length - 1 && mark != 0xFFDA) // Поиск тегов APP0 и APP1, вплоть до DQT
                    {
                        mark = (ushort)(reader.ReadByte() << 0 | mark << 8);
                        switch (mark)
                        {
                            case (0xFFE0):
                                //kindaExif += ReadTiffTagsInFileStream(reader);
                                break;
                            case (0xFFE1):
                                kindaExif += ReadExifTagsInFileStream(reader);
                                break;
                        }
                    }
                    if (kindaExif == "")
                        kindaExif = "Нет данных";
                    if (fs.Position == fs.Length - 1) // Если оба тега отсутствуют
                        return "Информация недоступна";
                    kindaExif += GetFileLength(path);
                }
            }
            return kindaExif;
        }
    }
}
