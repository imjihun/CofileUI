using CofileUI.Classes;
using Renci.SshNet.Sftp;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace CofileUI.UserControls.CustomUI.Cofile
{
	class LinuxFileViewModel : ViewModelBase
	{
		static string[] IGNORE_FILENAME = new string[] {".", ".."};
		public RelayCommand ExpandedCommand { get; set; }

		ObservableCollection<LinuxFileModel> root = new ObservableCollection<LinuxFileModel>();
		public ObservableCollection<LinuxFileModel> Root
		{
			get { return root; }
			set { root = value; RaisePropertyChanged("Root"); }
		}

		public LinuxFileViewModel()
		{
			ExpandedCommand = new RelayCommand(Expanded, null);

			Root.Add(new LinuxFileModel(){ Path = "/", IsDirectory = true });
		}

		private void Expanded(object obj)
		{
			TreeViewItem tvi = obj as TreeViewItem;
			
			Console.WriteLine("JHLIM_DEBUG : obj = " + obj);
		}
		//void ReLoadDirectory(object parameter)
		//{
		//	string remainned_path = parameter as string;

		//	SftpFile[] files;
		//	files = SSHController.PullListInDirectory(this.path);
		//	if(files == null)
		//	{
		//		this.IsExpanded = false;
		//		return;
		//	}

		//	this.Items.Clear();

		//	int count_have_directory = 0;
		//	foreach(var file in files)
		//	{
		//		int i;
		//		for(i = 0; i < IGNORE_FILENAME.Length; i++)
		//		{
		//			if(file.Name == IGNORE_FILENAME[i])
		//				break;
		//		}
		//		if(i != IGNORE_FILENAME.Length)
		//			continue;

		//		LinuxTreeViewItem ltvi;
		//		if(file.IsDirectory)
		//		{
		//			//this.Items.Insert(0, new LinuxTreeViewItem(file.FullName, file.Name, true));
		//			//this.Items.Add(new LinuxTreeViewItem(file.FullName, file.Name, true));
		//			ltvi = new LinuxTreeViewItem(file.FullName, file, file.Name, true, this);
		//			this.Items.Insert(count_have_directory++, ltvi);

		//			// remainned_path = '/' 부터 시작
		//			if(remainned_path != null)
		//			{
		//				string[] split = remainned_path.Split('/');
		//				if(split.Length > 1 && split[1] == file.Name)
		//				{
		//					ltvi.RefreshChild(remainned_path.Substring(split[1].Length + 1));
		//				}
		//			}
		//		}
		//		else
		//		{
		//			ltvi = new LinuxTreeViewItem(file.FullName, file, file.Name, false, this);
		//			this.Items.Add(ltvi);
		//		}
		//	}
		//}
		//int i = 0;
		//void AddUser(object parameter)
		//{
		//	if(sampleChild[0] as TreeData != null)
		//	{
		//		(sampleChild[0] as TreeData).Name = (i).ToString();
		//		Console.WriteLine((i).ToString());
		//		i++;
		//	}
		//}
	}
}
