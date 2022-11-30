using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using NuGetArcFace;
using Ookii.Dialogs.Wpf;
using System.IO;
using SixLabors.ImageSharp.PixelFormats;
using System.Threading;
using ImageControl = System.Windows.Controls.Image;
using Image = SixLabors.ImageSharp.Image;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;

namespace Prac2_1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ArcFaceUse arcface = new();

        private CancellationTokenSource cts;

        private VistaFolderBrowserDialog folderDialog = new();

        private TaskDialog cancellationDialog = new()
        {
            Buttons =
            {
                new TaskDialogButton("OK")
            },
            WindowTitle = "Cancellation",
            Content = "Calculations were stopped by user.",
            MainIcon = TaskDialogIcon.Warning
        };

        private string firstFolderPath = "";

        private string secondFolderPath = "";


        public MainWindow()
        {
            InitializeComponent();
        }


        public void OpenFolderDialog(object sender, RoutedEventArgs e)
        {
            Button source = (Button)e.Source;
            if (source.Name == "Open1Button")
            {
                folderDialog.ShowDialog();
                firstFolderPath = folderDialog.SelectedPath;
                Path1.Text = firstFolderPath.Split('\\').Last();
                FillImageListFromFolder(firstFolderPath, ImagesList1);
            }
            else if (source.Name == "Open2Button")
            {
                folderDialog.ShowDialog();
                secondFolderPath = folderDialog.SelectedPath;
                Path2.Text = secondFolderPath.Split('\\').Last();
                FillImageListFromFolder(secondFolderPath, ImagesList2);
            }
            else
            {
                throw new Exception("Unexpected call for folder dialog");
            }

        }

        public async void Calculate(object sender, RoutedEventArgs e)
        {
            CalculateButton.IsEnabled = false;
            CancellationButton.IsEnabled = true;
            ProgressBar.Value = 0;

            var firstSelectedItem = (StackPanel)ImagesList1.SelectedItem;
            var secondSelectedItem = (StackPanel)ImagesList2.SelectedItem;

            using var face1 = Image.Load<Rgb24>(firstFolderPath + "\\"
                + firstSelectedItem.Children.OfType<TextBlock>().Last().Text);
            using var face2 = Image.Load<Rgb24>(secondFolderPath + "\\"
                + secondSelectedItem.Children.OfType<TextBlock>().Last().Text);

            face1.Mutate(x => x.Resize(112, 112));
            face2.Mutate(x => x.Resize(112, 112));

            var images = new Image<Rgb24>[] { face1, face2 };

            cts = new CancellationTokenSource();
            Progress<int> progress = new();
            progress.ProgressChanged += ReportProgress;

            Thread.Sleep(100);
            var results = await arcface.DistAndSimAsMatrix(images, cts.Token, progress);
            Thread.Sleep(100);

            if (cts.Token.IsCancellationRequested)
            {
                ProgressBar.Value = 0;
                Similarity.Text = "Not stated";
                Distance.Text = "Not stated";
            }
            else
            {
                Similarity.Text = $"{results.Item2[0, 1]}";
                Distance.Text = results.Item1[0, 1].ToString();
            }

            CalculateButton.IsEnabled = true;
            CancellationButton.IsEnabled = false;
        }

        public void Cancellation(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
            cancellationDialog.Show();
            Similarity.Text = "Not stated";
            Distance.Text = "Not stated";
            ProgressBar.Value = 0;
        }


        private void ListSelectionChange(object? sender, SelectionChangedEventArgs e)
        {
            if (ImagesList1.SelectedItems.Count > 0 && ImagesList2.SelectedItems.Count > 0)
            {
                CalculateButton.IsEnabled = true;
            }
        }

        private void ReportProgress(object? sender, int e)
        {
            ProgressBar.Value = e;
        }


        private void FillImageListFromFolder(string path, ListBox list)
        {
            var imagePaths = Directory.GetFiles(path)
                .Where(path => path.EndsWith(".jpg") || path.EndsWith(".png") || path.EndsWith(".jpeg"));
            list.Items.Clear();
            foreach (var imgpath in imagePaths)
            {
                ImageControl image = new()
                {
                    Source = new BitmapImage(new Uri(imgpath)),
                    Width = 60
                };

                TextBlock textBlock = new TextBlock();
                textBlock.Text = imgpath.Split('\\').Last();
                textBlock.Padding = new Thickness(10, 25, 0, 0);
                StackPanel panel = new();
                panel.Orientation = Orientation.Horizontal;
                panel.Children.Add(image);
                panel.Children.Add(textBlock);
                list.Items.Add(panel);
            }
        }
    }
}
