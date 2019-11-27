using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    if (file.Extension.Contains("jpg") ||
                       file.Extension.Contains("jpeg"))
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

        public string ReadExifInFile(string path)
        {
            // Каждый сектор заголовка JPEG состоит из
            //      1. маркера (0xFFD8 - тип файла - JPEG)
            //      2. ДЛИНЫ СЕКТОРА (два байта)
            //      3. самих данных
            // Надо найти спецификацию JPEG, наименования маркеров и сделать чтение по этим маркерам
            string kindaExif = "";
            int exifMarkerIndex = 4;
            byte[] searchedExifLength = new byte[2];
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    byte swap;
                    while(!((searchedExifLength[0] = reader.ReadByte()) == 0xFF) && !((searchedExifLength[1] = reader.ReadByte()) == 0xE1))
                    {
                        swap = searchedExifLength[0];
                        searchedExifLength[0] = searchedExifLength[1];
                        searchedExifLength[1] = swap;
                    }
                }
            }
            return kindaExif;
        }
    }
}
