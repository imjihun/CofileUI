using CofileUI.Classes;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CofileUI.UserControls
{
	class LinuxFileViewModel : ViewModelBase
	{
		ObservableCollection<LinuxFileModel> linuxFileTree = new ObservableCollection<LinuxFileModel>();
		public ObservableCollection<LinuxFileModel> LinuxFileTree
		{
			get { return linuxFileTree; }
			set { linuxFileTree = value; RaisePropertyChanged("LinuxFileTree"); }
		}

		List<LinuxFileModel> linuxFileTreeSelections = new List<LinuxFileModel>();
		public List<LinuxFileModel> LinuxFileTreeSelections
		{
			get { return linuxFileTreeSelections; }
			set { linuxFileTreeSelections = value; }
		}

		private RelayCommand mouseMoveCommand = null;
		public RelayCommand MouseMoveCommand {
			get
			{
				if(mouseMoveCommand == null)
					mouseMoveCommand = new RelayCommand(OnMouseMove);
				return mouseMoveCommand;
			}
		}
		void OnMouseMove(object obj)
		{
			MouseEventArgs e = obj as MouseEventArgs;
			if(e == null)
				return;

			if(e.LeftButton == MouseButtonState.Pressed
							&& LinuxFileTreeSelections.Count > 0)
			{
				Console.WriteLine("JHLIM_DEBUG : OnMouseMove");
				DataObject data = new DataObject();
				data.SetData("Object", LinuxFileTreeSelections);
				DragDrop.DoDragDrop(view, data, DragDropEffects.Copy);
			}
		}
		private UserControl view;

		public LinuxFileViewModel(UserControl _view)
		{
			view = _view;
			LinuxFileModel root = new LinuxFileModel(this){ Path = "/", IsDirectory = true, Name = "/" };
			LinuxFileTree.Add(root);
		}

	}
}
