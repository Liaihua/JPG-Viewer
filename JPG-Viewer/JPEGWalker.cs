using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JPG_Viewer
{
    class JPEGWalker
    {
        public List<string> JPEGImagePaths;
        DirectoryInfo directoryInfo;

        public JPEGWalker(string path)
        {
            JPEGImagePaths = ListJPEGAndDirsInDirectory(path);
        }

        public List<string> ListJPEGAndDirsInDirectory(string dir)
        {
            directoryInfo = new DirectoryInfo(GetDirectoryPath(dir));
            List<string> JPEGsAndFiles = FindAllPathsInDirectory(dir);
            JPEGsAndFiles.AddRange(FindJPEGInDirectory(dir));
            return JPEGsAndFiles;
        }

        public List<string> FindJPEGInDirectory(string dir)
        {
            if (string.IsNullOrWhiteSpace(dir))
                return null;
            List<string> JPEGPaths = new List<string>();
            
            try
            {
                foreach (FileInfo file in directoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly))
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
            if (dir == "..")
                try
                {
                    return FindJPEGInDirectory(directoryInfo.Parent.FullName);
                }
                catch (NullReferenceException)
                { System.Windows.MessageBox.Show("Вы в корневой директории"); }
            List<string> Dirs = new List<string>();
            Dirs.Add("..");

            try
            {
                foreach (var d in directoryInfo.GetDirectories("*", SearchOption.TopDirectoryOnly))
                {
                    //if (FindJPEGInDirectory(d.FullName).Count > 0)
                    Dirs.Add(d.Name);
                }
            }
            catch (Exception ex) { System.Windows.MessageBox.Show(ex.Message); }
            return Dirs;
        }

        public string GetCurrentDirectory() 
        {
            return directoryInfo.FullName;
        }

        public float GetFileLength(string path)
        {
            return new FileInfo(path).Length / 1024;
        }

        private string GetDirectoryPath(string dir)
        {
            if (Regex.IsMatch(dir, "\\w:\\S"))
                return dir;
            else
                return $"{directoryInfo.FullName}\\{dir}";
        }
    }
}
