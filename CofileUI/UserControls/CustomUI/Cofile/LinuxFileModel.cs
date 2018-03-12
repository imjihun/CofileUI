using Renci.SshNet.Sftp;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace CofileUI.UserControls.CustomUI.Cofile
{
	public class LinuxFileModel : INotifyPropertyChanged
	{
		private SftpFile fileInfo = null;
		public SftpFile FileInfo
		{
			get { return fileInfo; }
			set { fileInfo = value; RaisePropertyChanged("FileInfo"); }
		}
		private bool isDirectory = false;
		public bool IsDirectory
		{
			get { return isDirectory; }
			set { isDirectory = value; RaisePropertyChanged("IsDirectory"); }
		}
		private string path = "";
		public string Path
		{
			get { return path; }
			set { path = value; RaisePropertyChanged("Path"); }
		}

		private ObservableCollection<LinuxFileModel> children = new ObservableCollection<LinuxFileModel>();
		public ObservableCollection<LinuxFileModel> Children
		{
			get { return children; }
			set { children = value; RaisePropertyChanged("Children"); }
		}

		void RaisePropertyChanged(string prop)
		{
			if(PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }
		}
		public event PropertyChangedEventHandler PropertyChanged;
	}
}
