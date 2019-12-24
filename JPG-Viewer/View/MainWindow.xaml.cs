using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        FileWalker walker;
        EXIFViewer viewer;
        DirectoryViewModel DirectoryViewModel { get; set; }
        GridViewColumnHeader sortedColumnHeader = null;
        ListSortDirection direction = ListSortDirection.Ascending;
        public MainWindow()
        {
            FolderBrowserDialog selectedFolderDialog = new FolderBrowserDialog();
            if (selectedFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DirectoryViewModel = new DirectoryViewModel(selectedFolderDialog.SelectedPath);
                walker = new FileWalker(selectedFolderDialog.SelectedPath);
                viewer = new EXIFViewer();
                InitializeComponent();
                FoundImagesListView.DataContext = DirectoryViewModel;
            }
            else
                Close();
            selectedFolderDialog.Dispose();
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
                    DirectoryViewModel.UpdateDirectioryCommand.Execute(FoundImagesListView.SelectedItem);

                }
            }
        }

        //private void ChangeDirectoryToFavorites_MenuItem_Click(object sender, RoutedEventArgs e)
        //{
        //    if (FoundImagesListView.DataContext.GetType() == typeof(DirectoryViewModel))
        //        FoundImagesListView.DataContext = new FavoritesViewModel();
        //    else
        //        FoundImagesListView.DataContext = directoryViewModel;
        //}

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FoundImagesListView.DataContext.GetType() == typeof(DirectoryViewModel))
            {
                if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
                {
                    FoundImagesListView.ItemsSource = DirectoryViewModel.FoundImagesAndDirs;
                    return;
                }
                DirectoryViewModel.SearchImagesCommand.Execute(SearchTextBox.Text);
                if (DirectoryViewModel.SearchedImages.Count > 0)
                    FoundImagesListView.ItemsSource = DirectoryViewModel.SearchedImages;
                else
                    FoundImagesListView.ItemsSource = DirectoryViewModel.FoundImagesAndDirs;
            }
        }

        private void FoundImagesGridViewColumn_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = sender as GridViewColumnHeader;
            if (sortedColumnHeader != null)
            {
                FoundImagesListView.Items.SortDescriptions.Clear();
            }
            ListSortDirection currentDirection = (sortedColumnHeader == header && direction == ListSortDirection.Ascending) ? ListSortDirection.Descending : ListSortDirection.Ascending;
            sortedColumnHeader = header;
            direction = currentDirection;
            FoundImagesListView.Items.SortDescriptions.Add(new SortDescription(header.Name, currentDirection));
        }

        private void SortImagesByCriterion_Click(object sender, RoutedEventArgs e)
        {
            if (SortCriterionsComboBox.SelectedItem == null)
                return;
            DirectoryViewModel.DuplicateSortedImages = bool.Parse((sender as System.Windows.Controls.Button).Tag.ToString());
            DirectoryViewModel.SortImagesCommand.Execute((SortCriterionsComboBox.SelectedItem as TextBlock).Tag.ToString());
        }
    }
}
