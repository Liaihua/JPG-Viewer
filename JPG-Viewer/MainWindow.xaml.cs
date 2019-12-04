using System;
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
            FolderBrowserDialog selectedFolderDialog = new FolderBrowserDialog();
            selectedFolderDialog.ShowDialog();
            FoundImages = walker.FindJPEGInDirectory(selectedFolderDialog.SelectedPath);
            FoundPaths = walker.FindAllPathsInDirectory(selectedFolderDialog.SelectedPath);
            InitializeComponent();
        }

        private void FoundImagesListView_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (FoundImagesListView.SelectedItem != null)
                DumpedJPEGTextBlock.Text = walker.ReadExifInFile(FoundImagesListView.SelectedItem.ToString()); // "C:\\Users\\vovchenko\\Downloads\\JPEG_example_down.jpg"
        }
    }
}
