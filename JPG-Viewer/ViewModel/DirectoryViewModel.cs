using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPG_Viewer
{
    public class DirectoryViewModel : INotifyPropertyChanged
    {
        FileWalker walker;
        EXIFViewer viewer;
        private string currentDir;

        public ViewModel.CustomCommand<Model.DirectoryModel> UpdateDirectioryCommand;
        public ViewModel.CustomCommand<string> SearchImagesCommand;
        public ObservableCollection<Model.AbstractFSObject> FoundImagesAndDirs { get; set; }
        public ObservableCollection<Model.ImageModel> SearchedImages { get; set; }
        public string CurrentDirectory { get { return currentDir; } set { currentDir = value; OnPropertyChanged(nameof(CurrentDirectory)); } }

        public DirectoryViewModel(string path)
        {
            walker = new FileWalker(path);
            viewer = new EXIFViewer();
            FoundImagesAndDirs = new ObservableCollection<Model.AbstractFSObject>(walker.JPEGImagePaths);
            CurrentDirectory = walker.GetCurrentDirectory();
            SearchedImages = new ObservableCollection<Model.ImageModel>();
            UpdateDirectioryCommand = new ViewModel.CustomCommand<Model.DirectoryModel>((dir) =>
            {
                FoundImagesAndDirs.Clear();
                foreach (var d in walker.ListJPEGAndDirsInDirectory(CurrentDirectory + "\\" + dir.Name))
                    FoundImagesAndDirs.Add(d);
                CurrentDirectory = walker.GetCurrentDirectory();
            });
            SearchImagesCommand = new ViewModel.CustomCommand<string>((param) =>
            {
                SearchedImages.Clear();
                SearchedImages = new ObservableCollection<Model.ImageModel>(walker.SearchJPEGInsideDirectory(CurrentDirectory, param));
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
