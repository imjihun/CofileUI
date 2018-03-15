using CofileUI.Classes;
using Renci.SshNet.Sftp;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CofileUI.UserControls
{
	class LinuxFileModel : INotifyPropertyChanged
	{
		private SftpFile fileInfo = null;
		public SftpFile FileInfo { get { return fileInfo; } set { fileInfo = value; RaisePropertyChanged("FileInfo"); } }
		private bool isDirectory = false;
		public bool IsDirectory
		{
			get { return isDirectory; }
			set {
				isDirectory = value;
				RaisePropertyChanged("IsDirectory");

				if(isDirectory)
				{
					Icon = "/CofileUI;component/Resources/is_directory.png";
					FontWeight = FontWeights.Bold;
					FontColor = Brushes.DarkBlue;
				}
			}
		}
		private string path = null;
		public string Path { get { return path; } set { path = value; RaisePropertyChanged("Path"); } }

		private string icon = "/CofileUI;component/Resources/file.png";
		public string Icon { get { return icon; } set { icon = value; RaisePropertyChanged("Icon"); } }

		private string name = null;
		public string Name
		{
			get { return name; }
			set {
				name = value;
				RaisePropertyChanged("Name");

				if(name != null && name.Length > 0 && name[0] == '.')
				{
					Opacity = 0.5;
				}
			}
		}

		private FontWeight fontWeight = FontWeights.Normal;
		public FontWeight FontWeight { get { return fontWeight; } set { fontWeight = value; RaisePropertyChanged("FontWeight"); } }

		private Brush fontColor = Brushes.Black;
		public Brush FontColor { get { return fontColor; } set { fontColor = value; RaisePropertyChanged("FontColor"); } }

		private string configIndex = null;
		public string ConfigIndex { get { return configIndex; } set { configIndex = value; RaisePropertyChanged("ConfigIndex"); } }

		private Brush nameBackGroundColor = null;
		public Brush NameBackGroundColor { get { return nameBackGroundColor; } set { nameBackGroundColor = value; RaisePropertyChanged("NameBackGroundColor"); } }

		private double opacity = 1;
		public double Opacity { get { return opacity; } set { opacity = value; RaisePropertyChanged("Opacity"); } }
		
		private bool isExpanded = false;
		public bool IsExpanded {
			get { return isExpanded; }
			set {
				isExpanded = value;
				RaisePropertyChanged("IsExpanded");

				if(Children.Count == 0)
					ReLoadDirectory();
			}
		}

		private Brush backGroundColor = null;
		public Brush BackGroundColor { get { return backGroundColor; } set { backGroundColor = value; RaisePropertyChanged("BackGroundColor"); } }

		private bool realSelected = false;
		private bool isSelected = false;
		public bool IsSelected {
			get { return isSelected; }
			set {
				isSelected = false;
				RaisePropertyChanged("IsSelected");

				if(value)
				{
					realSelected = !realSelected;
					Console.WriteLine("JHLIM_DEBUG : this = " + Name + " real = " + realSelected);

					if(realSelected)
					{
						BackGroundColor = (SolidColorBrush)App.Current.Resources["AccentColorBrush"];
						viewModel.LinuxFileTreeSelections.Add(this);
					}
					else
					{
						BackGroundColor = null;
						viewModel.LinuxFileTreeSelections.Remove(this);
					}
				}
			}
		}

		private ObservableCollection<LinuxFileModel> children = new ObservableCollection<LinuxFileModel>();
		public ObservableCollection<LinuxFileModel> Children { get { return children; } set { children = value; RaisePropertyChanged("Children"); } }

		RelayCommand mouseLeftButtonUp = null;
		public RelayCommand MouseLeftButtonUp
		{
			get
			{
				if(mouseLeftButtonUp == null)
					mouseLeftButtonUp = new RelayCommand((obj) => { Console.WriteLine("JHLIM_DEBUG : MouseUp"); });
				return mouseLeftButtonUp;
			}
		}

		RelayCommand expandedCommand = null;
		public RelayCommand ExpandedCommand
		{
			get
			{
				if(expandedCommand == null)
					expandedCommand = new RelayCommand((obj) => { if(Children.Count == 0) ReLoadDirectory(); });
				return expandedCommand;
			}
		}
		private void ReLoadDirectory()
		{
			if(isDirectory)
			{
				string[] IGNORE_FILENAME = new string[] {".", ".."};

				SftpFile[] files;
				files = SSHController.PullListInDirectory(Path);
				if(files == null)
				{
					Icon = "/CofileUI;component/Resources/directory_deny.png";
					return;
				}
				Icon = "/CofileUI;component/Resources/directory.png";

				if(Children.Count > 0)
					return;
				Children.Clear();

				int count_have_directory = 0;
				foreach(var file in files)
				{
					int i;
					for(i = 0; i < IGNORE_FILENAME.Length; i++)
					{
						if(file.Name == IGNORE_FILENAME[i])
							break;
					}
					if(i != IGNORE_FILENAME.Length)
						continue;

					LinuxFileModel lfm_child;
					if(file.IsDirectory)
					{
						lfm_child = new LinuxFileModel(viewModel) { Path = file.FullName, FileInfo = file, IsDirectory = true, Name = file.Name };
						Children.Insert(count_have_directory++, lfm_child);
					}
					else
					{
						lfm_child = new LinuxFileModel(viewModel) { Path = file.FullName, FileInfo = file, IsDirectory = false, Name = file.Name };
						Children.Add(lfm_child);
					}
					
				}
			}
		}

		LinuxFileViewModel viewModel = null;
		public LinuxFileModel(LinuxFileViewModel _viewModel)
		{
			viewModel = _viewModel;
		}

		void RaisePropertyChanged(string prop)
		{
			if(PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }
		}
		public event PropertyChangedEventHandler PropertyChanged;
	}
}
