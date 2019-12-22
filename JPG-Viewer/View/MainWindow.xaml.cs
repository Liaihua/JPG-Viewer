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
                if (FoundImagesListView.SelectedItem is Model.ImageModel)
                {
                    Model.ImageModel selectedImage = FoundImagesListView.SelectedItem as Model.ImageModel;
                    ExifMetadataListView.ItemsSource = viewer.ReadExifInFile(selectedImage.Name);
                    FileLength_Label.Content = $"Размер: { walker.GetFileLength(selectedImage.Name)} КБ";
                }
                else
                {
                    directoryViewModel.UpdateDirectioryCommand.Execute(FoundImagesListView.SelectedItem);
                    
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

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FoundImagesListView.DataContext.GetType() == typeof(DirectoryViewModel))
            {
                if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
                {
                    FoundImagesListView.ItemsSource = directoryViewModel.FoundImagesAndDirs;
                    return;
                }
                directoryViewModel.SearchImagesCommand.Execute(SearchTextBox.Text);
                if (directoryViewModel.SearchedImages.Count > 0)
                    FoundImagesListView.ItemsSource = directoryViewModel.SearchedImages;
                else
                    FoundImagesListView.ItemsSource = directoryViewModel.FoundImagesAndDirs;
            }
        }
    }
}
