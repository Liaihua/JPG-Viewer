using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JPG_Viewer
{
    class JPEGWalker
    {
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
                        file.Extension.Contains("jpeg") || 
                        file.Extension.Contains("jfif"))
                        JPEGPaths.Add(file.FullName);
                }
            }
            catch (Exception ex) { System.Windows.MessageBox.Show(ex.Message); }
            return JPEGPaths;
        }

        public List<string> FindAllPathsInDirectory(string dir)
        {
            if (string.IsNullOrWhiteSpace(dir))
                return null;
            List<string> Dirs = new List<string>();
            DirectoryInfo directory = new DirectoryInfo(dir);
            try
            {
                foreach (var d in directory.GetDirectories("*", SearchOption.AllDirectories).ToList())
                {
                    Dirs.Add(d.Name);
                }
            }
            catch (Exception ex) { System.Windows.MessageBox.Show(ex.Message); }
            return Dirs;
        }

        private void Swap(ref byte a, ref byte b)
        {
            byte temp = a;
            a = b;
            b = temp;
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
            byte[] searchExifBuffer = { 0x00, 0x00 };
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    fs.Seek(0, SeekOrigin.Begin);                              
                    // 0xE0 - старый JFIF, 0xE1 - более новая версия стандарта Exif. Есть проблемы с чтением APP0 и APP1 одновременно
                    while (!((searchExifBuffer[0] == 0xFF) && ((searchExifBuffer[1] == 0xE0) || searchExifBuffer[1] == 0xE1))
                        && fs.Position != fs.Length - 1)
                    {
                        Swap(ref searchExifBuffer[0], ref searchExifBuffer[1]);
                        searchExifBuffer[1] = reader.ReadByte();
                    }
                    
                    if (fs.Position == fs.Length - 1)
                        return "Несовместимый формат";
                    
                    ushort length = reader.ReadUInt16();
                    length = (ushort)((length << 8) | (length >> 8) & 0xFF); // изменение порядка байтов с little на big-endian. Любезно предоставлено переполнением стека
                    MessageBox.Show(length.ToString());
                    
                    length -= 2;
                    while(length-- != 0)
                        kindaExif += Encoding.UTF8.GetString(reader.ReadBytes(1)); // надо узнать информацию о маркерах метаинфы в спецификации
                    
                }
            }
            return kindaExif;
        }
    }
}
