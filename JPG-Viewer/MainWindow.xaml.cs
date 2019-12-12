using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace JPG_Viewer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        JPEGWalker walker;
        EXIFViewer viewer;
        public List<string> FoundImages { get; set; }
        public List<string> FoundPaths { get; set; }
        public MainWindow()
        {
            walker = new JPEGWalker();
            viewer = new EXIFViewer();
            FolderBrowserDialog selectedFolderDialog = new FolderBrowserDialog();
            selectedFolderDialog.ShowDialog();
            FoundImages = walker.FindJPEGInDirectory(selectedFolderDialog.SelectedPath);
            FoundPaths = walker.FindAllPathsInDirectory(selectedFolderDialog.SelectedPath);

            InitializeComponent();
        }

        private void FoundImagesListView_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (FoundImagesListView.SelectedItem != null)
            {
                ExifMetadataListView.ItemsSource = viewer.ReadExifInFile(FoundImagesListView.SelectedItem.ToString());
                FileLength_Label.Content = $"Размер: { walker.GetFileLength(FoundImagesListView.SelectedItem.ToString())} КБ";
            }
        }
        private void NewWindowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }
    }
}
