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

    // Есть также вариант по созданию функции, которая могла бы искать инфу по определенному тегу
    // (так то у меня есть ф-ии ReadExifTagsInStream и ReadExifInFile, но тут-то все выглядит довольно громоздко: 
    // они отвечают за выдачу **всех** тегов, а не отдельного)
    class JPEGWalker 
    {
        bool? LittleEndian;
        Dictionary<int, string> LoadedExifTags;
        List<ExifTag> ExifTags { get; set; }
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
                foreach (var d in directory.GetDirectories("*", SearchOption.AllDirectories))
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

        private ushort ChangeEndiannessOfUShort(ushort param)
        {
            return (ushort)((param << 8) | (param >> 8) & 0xFF);
        }

        private bool? CheckEndiannessInStream(BinaryReader reader) // true - для прямого порядка (LE), false - для обратного (BE)
        {
            reader.BaseStream.Seek(8, SeekOrigin.Current);
            ushort endianTag = reader.ReadUInt16();
            ushort Tag42 = reader.ReadUInt16();
            if (endianTag == 0x4949 && Tag42 == 0x002A)     // "II"
                return true;
            else if (endianTag == 0x4D4D && Tag42 == 0x2A00) // "MM"
                return false;
            else
                return null;
        }

        public byte[] ReadExifTag(BinaryReader reader, long tiffHeader)
        {
            reader.BaseStream.Seek(tiffHeader, SeekOrigin.Begin);
            ushort tag = EndiannessIO.ReadUInt16(reader.ReadUInt16(), LittleEndian.Value);
            ushort type = EndiannessIO.ReadUInt16(reader.ReadUInt16(), LittleEndian.Value);
            uint count = EndiannessIO.ReadUInt32(reader.ReadUInt32(), LittleEndian.Value);
            byte[] valueOrOffset = BitConverter.GetBytes(EndiannessIO.ReadUInt32(reader.ReadUInt32(), LittleEndian.Value));
            switch(type)
            {
                case (int)IFDTypeEnum.ASCII:
                    if (count > 4)
                    {
                        reader.BaseStream.Seek(EndiannessIO.ReadUInt32(BitConverter.ToUInt32(valueOrOffset, 0), false), SeekOrigin.Begin);
                        return reader.ReadBytes((int)count);
                    }
                    break;
            }
            return valueOrOffset;
        }

        private string ReadExifTagsInFileStream(BinaryReader reader) // Поиск тегов в файле
        {
            string foundTags = "";
            LittleEndian = CheckEndiannessInStream(reader);
            uint firstIFDOffset = reader.ReadUInt32();
            ushort entries = reader.ReadUInt16();
            if (LittleEndian.HasValue)
            {
                if (!LittleEndian.Value)
                {
                    firstIFDOffset = EndiannessIO.ReadUInt32(firstIFDOffset, LittleEndian.Value);
                    entries = ChangeEndiannessOfUShort(entries);
                }
            }
            else
                return "Неверные данные TIFF (Отсутствует порядок байтов)";

            if (firstIFDOffset < 0x00000008)
                return "Неверные данные TIFF (Неправильное смещение IFD)";

            for (int i = 0; i < entries; i++)
            {
                foundTags += ASCIIEncoding.ASCII.GetString(ReadExifTag(reader, reader.BaseStream.Position)) + '\n'; // набросок
            }
            
            ushort exifTag = 0;
            
            {
                exifTag = (ushort)(reader.ReadByte() << 0 | exifTag << 8);
                //foreach(var tagKeyValue in LoadedExifTags)
                //{
                    //if (tagKeyValue.Key == exifTag)
                    //    foundTags += tagKeyValue.Value + "\n";
                //}
                
                foundTags += Encoding.UTF8.GetString(new byte[] { (byte)(exifTag >> 0)}); // надо узнать информацию о маркерах метаинфы в спецификации
            }
            return foundTags;
        }

        public string ReadExifInFile(string path) // Поиск APP1 в файле
        {
            // Каждый сектор заголовка JPEG состоит из
            //      1. маркера (0xFFE1 - Exif)
            //      2. длины сектора (два байта) + 2б длина маркера
            //      3. самих данных
            // Надо найти спецификацию JPEG, наименования маркеров и сделать чтение по этим маркерам
            // Есть вариант с чтением до маркера SOS, используя switch(mark)
            string kindaExif = "";
            LoadedExifTags = GetExifTagsFromDictionary();
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    ushort mark = 0; // зта переменная используется в качестве двухбайтового контейнера
                    
                    while (fs.Position != fs.Length - 1 && mark != 0xFFDA) // Поиск тега APP1 вплоть до DQT
                    {
                        mark = (ushort)(reader.ReadByte() << 0 | mark << 8);
                        if(mark == 0xFFE1)
                        {
                            kindaExif += ReadExifTagsInFileStream(reader);
                            break;
                        }
                    }
                    if (kindaExif == "")
                        kindaExif = "Нет данных";
                    if (fs.Position == fs.Length - 1) // Если тег отсутствует
                        return "Информация недоступна";
                    kindaExif += GetFileLength(path);
                }
            }
            return kindaExif;
        }
        public Dictionary<int, string> GetExifTagsFromDictionary()
        {
            Dictionary<int, string> exifTags = new Dictionary<int, string>();
            using (FileStream fs = new FileStream("exif_tags.json", FileMode.Open, FileAccess.Read))
            {
                System.Runtime.Serialization.Json.DataContractJsonSerializer jsonSerializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(Dictionary<string, string>));
                foreach (var tag in (Dictionary<string, string>)jsonSerializer.ReadObject(fs))
                    exifTags.Add(int.Parse(tag.Key, System.Globalization.NumberStyles.HexNumber), tag.Value);
            }
            if(exifTags.Count < 1)
                return null;
            return exifTags;
        }
    }
}
