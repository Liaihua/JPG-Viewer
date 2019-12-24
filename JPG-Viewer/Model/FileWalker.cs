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
                if ((file.Extension.Contains("jpg")  || file.Extension.Contains("JPG") ||
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

        public List<ImageModel> SearchJPEGInsideDirectory(string dir)
        {
            if (string.IsNullOrWhiteSpace(dir))
                return null;
            List<ImageModel> searchedJPEGs = new List<ImageModel>();
            DirectoryInfo directory = new DirectoryInfo(dir);
            foreach (FileInfo file in directory.GetFiles("*"))
            {
                if ((file.Extension.Contains("jpg") || file.Extension.Contains("JPG") ||
                     file.Extension.Contains("jpeg") || file.Extension.Contains("JPEG")))
                    searchedJPEGs.Add(new ImageModel(file.FullName));
            }
            foreach (DirectoryInfo childDir in directory.GetDirectories())
            {
                try
                {
                    searchedJPEGs.AddRange(SearchJPEGInsideDirectory(childDir.FullName));
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

        // Здесь будет псевдокод

        // 1. Получаем изображения из выбранной директории
        // 2. Если изображений нет, тогда ничего не делаем
        // 3. Для каждого изображения загружаются размер, дата и название устройства (автоматически
        // выполняется в конструкторе ImageModel)
        // 4. Проходя по списку полученных изображений, мы формируем словарь следующего содержания:

        //      <Значение параметра сортировки> -> <List<ImageModel>> (если речь идет о дате снимка
        // и названии устройства)

        //    В случае с размером изображения все не так просто. Я рассматриваю вариант создания
        // диапазонов, выраженных в виде степеней двойки. Если в какой-то диапазон не входит ни одна
        // фотография (в 1 КБ, к примеру), то мы его сразу отсекаем
        //
        // 
        // 5. Пользователю предлагается выбрать
        // нужную папку, где он будет сохранять результаты. В другом случае мы просто сбрасываем все результаты
        // поиска.
        // Также пользователь может выбрать, копировать ли изображения, или перемещать их.
        // 6. В выбранной папке сохраняются все результаты поиска
        //

        public void GenerateSortedImages(bool duplicate)
        {

        }

        public Dictionary<string, List<ImageModel>> SortImagesByCriterion(string path, string criterion)
        {
            List<ImageModel> SearchedImages = SearchJPEGInsideDirectory(path);
            if (SearchedImages.Count == 0)
                return null;
            Dictionary<string, List<ImageModel>> searchedImagesByCriterion =
            SortImagesByStringValue(criterion, ref SearchedImages);
            
            return searchedImagesByCriterion;
        }

        private Dictionary<string, List<ImageModel>> SortImagesByStringValue(string criterion, ref List<ImageModel> images)
        {
            Dictionary<string, List<ImageModel>> searchedImagesByCriterion = new Dictionary<string, List<ImageModel>>();
            switch (criterion)
            {
                case "DeviceName":
                    foreach (var image in images)
                    {
                        if (!searchedImagesByCriterion.ContainsKey(image.DeviceName))
                            searchedImagesByCriterion.Add(image.DeviceName, new List<ImageModel>());

                        searchedImagesByCriterion[image.DeviceName].Add(image);
                    }
                    break;
                case "WhenShot":
                    foreach (var image in images)
                    {
                        string key = (image.WhenShot.Length != 0) ? image.WhenShot.Substring(0, 10).Replace(':','_') : "";
                        if (!searchedImagesByCriterion.ContainsKey(key))
                            searchedImagesByCriterion.Add(key, new List<ImageModel>());

                        searchedImagesByCriterion[key].Add(image);
                    }
                    break;
                case "Size":
                    foreach(var image in images)
                    {
                        if (!searchedImagesByCriterion.ContainsKey(image.ImageResolution))
                            searchedImagesByCriterion.Add(image.ImageResolution, new List<ImageModel>());

                        searchedImagesByCriterion[image.ImageResolution].Add(image);
                    }
                    break;
            }
            return searchedImagesByCriterion;
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
