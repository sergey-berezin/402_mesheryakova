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
using System.Windows.Shapes;
using System.Collections;
using System.Windows.Navigation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Threading;
using System.IO;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using WpfApp;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfApp
{
    public partial class DatabaseWindow : Window
    {
        //список информации про изображения
        public ObservableCollection<ImageInfo> ImagesCollection { get; } = new();
        public DatabaseWindow()
        {
            ImagesCollection.CollectionChanged += delegate (object? sender, NotifyCollectionChangedEventArgs e) { RaisePropertyChanged(nameof(ImagesCollection)); };
            using (var db = new ImagesContext())
            {
                foreach (var imageInfo in db.ImagesInfo)
                    ImagesCollection.Add(imageInfo);
            }

            InitializeComponent();
            this.DataContext = this;

        }

        private void DeleteImageClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var imageInfo = ImagesCollection[ImagesCollectionListBox.SelectedIndex];
                using (var db = new ImagesContext())
                {
                    var imageToDelete = db.ImagesInfo.Where(x => x.Id == imageInfo.Id).Include(x => x.Details).First();
                    if (imageToDelete == null) return;

                    db.ImagesDetails.Remove(imageToDelete.Details);
                    db.ImagesInfo.Remove(imageToDelete);
                    db.SaveChanges();
                    ImagesCollection.Remove(imageInfo);
                }
            }
            catch (Exception e1)
            {
                if (ImagesCollectionListBox.SelectedIndex == -1)
                    MessageBox.Show("Select image to delete");
                else MessageBox.Show(e1.Message);
            }
        }

        private void ListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ImagesCollectionListBox.SelectedIndex < 0) DeleteFromDbButton.IsEnabled = false;
            else DeleteFromDbButton.IsEnabled = true;
        }

        //событие изменения данных
        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void ImagesCollectionListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
