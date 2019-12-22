using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JPG_Viewer.Model;
namespace JPG_Viewer
{
    class FileWalker
    {
        public List<Model.AbstractFSObject> JPEGImagePaths;
        DirectoryInfo directoryInfo;

        public FileWalker(string path)
        {
            JPEGImagePaths = ListJPEGAndDirsInDirectory(path);
        }

        public List<AbstractFSObject> ListJPEGAndDirsInDirectory(string dir)
        {
            directoryInfo = new DirectoryInfo(GetDirectoryPath(dir));
            List<AbstractFSObject> JPEGsAndFiles = new List<AbstractFSObject>();
            JPEGsAndFiles.AddRange(FindAllPathsInDirectory(dir));
            JPEGsAndFiles.AddRange(FindJPEGInDirectory(dir));
            return JPEGsAndFiles;
        }

        public List<ImageModel> FindJPEGInDirectory(string dir)
        {
            if (string.IsNullOrWhiteSpace(dir))
                return null;
            List<ImageModel> JPEGPaths = new List<ImageModel>();

            try
            {
                foreach (FileInfo file in directoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly))
                {
                    if (file.Extension.Contains("jpg") || file.Extension.Contains("JPG") ||
                        file.Extension.Contains("jpeg") || file.Extension.Contains("JPEG"))
                        JPEGPaths.Add(new ImageModel(file.FullName));
                }
            }
            catch (Exception ex) { System.Windows.MessageBox.Show(ex.Message); }
            return JPEGPaths;
        }

        public List<ImageModel> SearchJPEGInsideDirectory(string dir, string pattern)
        {
            if (string.IsNullOrWhiteSpace(dir))
                return null;
            List<ImageModel> searchedJPEGs = new List<ImageModel>();
            DirectoryInfo directory = new DirectoryInfo(dir);
            foreach (FileInfo file in directory.GetFiles("*"))
            {
                if ((file.Extension.Contains("jpg") || file.Extension.Contains("JPG") ||
                     file.Extension.Contains("jpeg") || file.Extension.Contains("JPEG")) &&
                     file.Name.Contains(pattern))
                    searchedJPEGs.Add(new ImageModel(file.FullName));
            }
            foreach (DirectoryInfo childDir in directory.GetDirectories())
            {
                try
                {
                    searchedJPEGs.AddRange(SearchJPEGInsideDirectory(childDir.FullName, pattern));
                }
                catch { }
            }
            return searchedJPEGs;
        }

        public List<DirectoryModel> FindAllPathsInDirectory(string dir)
        {
            if (string.IsNullOrWhiteSpace(dir))
                return null;
            if (dir == "..")
                try
                {
                    return FindAllPathsInDirectory(directoryInfo.Parent.FullName);
                }
                catch (NullReferenceException)
                { System.Windows.MessageBox.Show("Вы в корневой директории"); }
            List<DirectoryModel> Dirs = new List<DirectoryModel>();
            Dirs.Add(new DirectoryModel { Name = ".." });

            try
            {
                foreach (var d in directoryInfo.GetDirectories("*", SearchOption.TopDirectoryOnly))
                {
                    Dirs.Add(new DirectoryModel { Name = d.Name });
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
