﻿using System;
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
        JPEGWalker walker;
        EXIFViewer viewer;
        private string currentDir;
        
        public ObservableCollection<string> FoundImages { get; set ; }
        public string CurrentDirectory { get { return currentDir; } set { currentDir = value; OnPropertyChanged(nameof(CurrentDirectory)); } }

        public DirectoryViewModel(string path)
        {
            walker = new JPEGWalker(path);
            viewer = new EXIFViewer();
            FoundImages = new ObservableCollection<string>(walker.JPEGImagePaths);
            CurrentDirectory = walker.GetCurrentDirectory();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
