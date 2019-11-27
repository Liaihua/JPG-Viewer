﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Forms;

namespace JPG_Viewer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        JPEGWalker walker;
        public List<string> FoundImages { get; set; }
        public List<string> FoundPaths { get; set; }
        public MainWindow()
        {
            walker = new JPEGWalker();
            //FolderBrowserDialog selectedFolderDialog = new FolderBrowserDialog();
            //selectedFolderDialog.ShowDialog();
            //FoundImages = walker.FindJPEGInDirectory(selectedFolderDialog.SelectedPath);
            //FoundPaths = walker.FindAllPathsInDirectory(selectedFolderDialog.SelectedPath);
            InitializeComponent();
        }

        private void ReadExifButton_Click(object sender, RoutedEventArgs e)
        {
            DumpedJPEGTextBlock.Text = walker.ReadExifInFile("C:\\Windows.old\\Users\\Татьяна - копия\\.vscode\\extensions\\formulahendry.code-runner-0.9.10\\images\\Coding.jpg");
        }
    }
}