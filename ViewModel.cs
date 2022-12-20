
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;



namespace WpfApp
{
	public class FileItem
	{
		private string name;
		private string ext;
		private string path;
		public string Name { get { return name; } }
		public string Ext { get { return ext; } }
		public string Path { get { return path; } }

		public FileItem(string file_path)
		{
			name = System.IO.Path.GetFileName(file_path);
			ext = System.IO.Path.GetExtension(file_path);
			path = file_path;
		}
	}


	public class ViewModel : INotifyPropertyChanged
	{
		public ViewModel()
		{
			FolderPath1 = "folder 1";
			FolderPath2 = "folder 2";
			ImagePath1 = "";
			ImagePath2 = "";

			FilesList1 = new ObservableCollection<FileItem>();
			FilesList2 = new ObservableCollection<FileItem>();

			//подписка на изменение
			FilesList1.CollectionChanged += delegate (object? sender, NotifyCollectionChangedEventArgs e) { RaisePropertyChanged(nameof(FilesList1)); };
			FilesList2.CollectionChanged += delegate (object? sender, NotifyCollectionChangedEventArgs e) { RaisePropertyChanged(nameof(FilesList2)); };

			CalculationEnable = true;
			Cancellable = false;
		}

		//событие изменения данных
		public event PropertyChangedEventHandler? PropertyChanged;
		public void RaisePropertyChanged([CallerMemberName] string propertyName = "") =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


		//данные для отображения

		//список файлов
		public ObservableCollection<FileItem> FilesList1 { get; set; }
		public ObservableCollection<FileItem> FilesList2 { get; set; }

		//путь до первого изображения
		private string img1;
		public string ImagePath1
		{
			get { return img1; }
			set { img1 = value; RaisePropertyChanged(nameof(ImagePath1)); }
		}

		//путь до второго изображения
		private string img2;
		public string ImagePath2
		{
			get { return img2; }
			set { img2 = value; RaisePropertyChanged(nameof(ImagePath2)); }
		}

		//1ая папка со списком изображений
		private string folder_path_1;
		public string FolderPath1
		{
			get { return folder_path_1; }
			set { folder_path_1 = value.Split('\\').Last(); RaisePropertyChanged(nameof(FolderPath1)); }
		}

		//2ая папка со списком изображений
		private string folder_path_2;
		public string FolderPath2
		{
			get { return folder_path_2; }
			set { folder_path_2 = value.Split('\\').Last(); RaisePropertyChanged(nameof(FolderPath2)); }
		}

		//Результаты 
		private float distance;
		public float Distance
		{
			get { return distance; }
			set { distance = value; RaisePropertyChanged(nameof(Distance)); }
		}

		private float similarity;
		public float Similarity
		{
			get { return similarity; }
			set { similarity = value; RaisePropertyChanged(nameof(Similarity)); }
		}

		//разрешение на запуск вычислений
		private bool changed;
		public bool CalculationEnable
		{
			get { return changed; }
			set { changed = value; RaisePropertyChanged(nameof(CalculationEnable)); }
		}

		//разрешение на отмену вычислений
		private bool cancellable;
		public bool Cancellable
		{
			get { return cancellable; }
			set { cancellable = value; RaisePropertyChanged(nameof(Cancellable)); }
		}
	}
}