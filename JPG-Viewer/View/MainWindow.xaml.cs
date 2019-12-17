using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ObservableCollection<string> FoundImages { get; set; }
        public string CurrentDirectory { get; set; }
        DirectoryViewModel directoryViewModel { get; set; }
        public MainWindow()
        {
            FolderBrowserDialog selectedFolderDialog = new FolderBrowserDialog();
            selectedFolderDialog.ShowDialog();
            directoryViewModel = new DirectoryViewModel(selectedFolderDialog.SelectedPath);
            walker = new JPEGWalker(selectedFolderDialog.SelectedPath);
            viewer = new EXIFViewer();
            InitializeComponent();
            FoundImagesListView.DataContext = directoryViewModel;
            CurrentDirectoryTextBlock.Text = directoryViewModel.CurrentDirectory;
        }

        private void NewWindowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }

        private void FoundImagesListView_Changed(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (FoundImagesListView.SelectedItem != null)
            {
                if (FoundImagesListView.SelectedItem.ToString().EndsWith(".jpg") ||
                    FoundImagesListView.SelectedItem.ToString().EndsWith(".jpeg"))
                {
                    ExifMetadataListView.ItemsSource = viewer.ReadExifInFile(FoundImagesListView.SelectedItem.ToString());
                    FileLength_Label.Content = $"Размер: { walker.GetFileLength(FoundImagesListView.SelectedItem.ToString())} КБ";
                }
                else
                {
                    directoryViewModel.UpdateDirectioryCommand.Execute(FoundImagesListView.SelectedItem.ToString());
                    CurrentDirectoryTextBlock.Text = directoryViewModel.CurrentDirectory;
                }
            }
        }

        private void ChangeDirectoryToFavorites_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (FoundImagesListView.DataContext.GetType() == typeof(DirectoryViewModel))
                FoundImagesListView.DataContext = new FavoritesViewModel();
            else
                FoundImagesListView.DataContext = directoryViewModel;
        }
    }
}
