using CofileUI.Classes;
using Newtonsoft.Json.Linq;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
			LinuxFileModel root = new LinuxFileModel(this){ Path = "/", IsDirectory = true, FileName = "/" };
			LinuxFileTree.Add(root);
		}
		public void Clear()
		{
			linuxFileTree[0].Children.Clear();
			LinuxFileTreeSelections.Clear();
		}
		public int RefreshLinuxFileTree(string path)
		{
			if(LinuxFileTree.Count == 0)
				return 0;

			LinuxFileModel lfm_cur = LinuxFileTree[0];
			if(lfm_cur != null)
				lfm_cur.IsExpanded = true;

			string[] names = path.Split('/');
			if(names != null)
			{
				for(int i = 0; i < names.Length; i++)
				{
					if(names[i] == null || names[i].Length == 0)
						continue;

					for(int j = 0; j < lfm_cur.Children.Count; j++)
					{
						if(names[i] == lfm_cur.Children[j]?.FileName)
						{
							lfm_cur = lfm_cur.Children[j];
							lfm_cur.IsExpanded = true;
							break;
						}
					}
				}
			}
			RefreshConfigInfo();
			return 0;
		}

		private int SetConfigInfo(string path, string config_info)
		{
			if(path == null
				|| path.Length <= 0
				|| path[0] != '/')
				return -1;

			string[] path_names = path.Split('/');

			if(path != null)
			{
				LinuxFileModel lt_cur = LinuxFileTree[0];
				for(int i = 0; i < path_names.Length; i++)
				{
					if(lt_cur != null && path_names[i].Length > 0)
					{
						ObservableCollection<LinuxFileModel> children = lt_cur.Children;
						lt_cur = null;
						if(children != null)
						{
							int j;
							for(j = 0; j < children.Count; j++)
							{
								if(children[j].FileName == path_names[i])
								{
									lt_cur = children[j];
									if(i == path_names.Length - 1)
									{
										children[j].BackGroundColor = Brushes.LightGreen;
										children[j].ConfigIndex = config_info;
									}
									else
										children[j].BackGroundColor = Brushes.LightCyan;
									break;
								}
							}
							if(j == children.Count)
								return -2;
						}
					}
				}
			}
			return 0;
		}
		private void RefreshConfigInfo()
		{
			JObject root = Cofile.current.configMenu.btnFileConfig.Root;
			JObject jobj_work_group_root = root?["work_group"] as JObject;
			if(jobj_work_group_root == null)
				return;

			foreach(var work in jobj_work_group_root.Properties())
			{
				JObject jobj_server_menu = work.Value as JObject;
				if(jobj_server_menu == null)
					continue;

				JArray jarr_processes = jobj_server_menu?["processes"] as JArray;
				if(jarr_processes == null)
					continue;

				int i = 0;
				foreach(var jprop_server_info in jarr_processes)
				{
					JObject jobj_process_info = jprop_server_info as JObject;
					if(jobj_process_info == null)
						continue;

					string dir = null;
					string daemon_yn = null;
					if(root["type"].ToString() == "file")
						dir = (jobj_process_info["enc_option"] as JObject)?["input_dir"]?.ToString();
					else
						dir = (jobj_process_info["comm_option"] as JObject)?["input_dir"]?.ToString();

					string daemon_keyword = "dir_monitoring_yn";
					if(root["type"].ToString() == "tail")
						daemon_keyword = "daemon_yn";

					JToken jcur = jobj_process_info;
					while(jcur != null
						&& daemon_yn == null)
					{
						daemon_yn = (jcur["comm_option"] as JObject)?[daemon_keyword]?.ToString();
						jcur = jcur.Parent;
						while(jcur != null
							&& jcur as JObject == null)
							jcur = jcur.Parent;
					}

					if(daemon_yn == "True")
					{
						SetConfigInfo(dir, root["type"] + "-" + work.Name + "-" + i);
					}
					i++;
				}
			}
		}
	}
}
