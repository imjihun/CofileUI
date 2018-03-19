using CofileUI.Classes;
using CofileUI.Windows;
using Newtonsoft.Json.Linq;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CofileUI.UserControls
{

	public class LinuxTreeView : TreeView
	{
		public static class _Color
		{
			#region Folder Color (directory)
			public static SolidColorBrush Folder_foreground = Brushes.DarkBlue;
			#endregion

			#region File Color
			public static SolidColorBrush File_foreground_selected = Brushes.White;
			public static SolidColorBrush File_foreground_unselected = Brushes.Black;
			#endregion

			#region Common Color
			public static SolidColorBrush Background_selected { get { return (SolidColorBrush)App.Current.Resources["AccentColorBrush"]; } }
			public static SolidColorBrush Background_unselected = null;
			#endregion
		}
		public static string[] IGNORE_FILENAME = new string[] {".", ".."};

		private List<LinuxTreeViewItem> selected_list = new List<LinuxTreeViewItem>();
		public List<LinuxTreeViewItem> Selected_list { get { return selected_list; } }

		public LinuxTreeViewItem last_refresh = null;
		public LinuxTreeViewItem Last_refresh { get { return last_refresh; } set { last_refresh = value; } }

		bool bool_show_hidden = true;
		public bool Bool_show_hidden
		{
			get { return bool_show_hidden; }
			set
			{
				bool_show_hidden = value;
				//LinuxTreeViewItem.Filter(tv_linux, Filter_string, bool_show_hidden);
			}
		}
		string filter_string = "";
		public string Filter_string
		{
			get { return filter_string; }
			set
			{
				filter_string = value;

				//LinuxTreeViewItem.Filter(tv_linux, filter_string, Bool_show_hidden);
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			//Log.Print(linked_jtoken);
			base.OnMouseMove(e);
			if(e.LeftButton == MouseButtonState.Pressed
				&& Selected_list.Count > 0)
			{
				DataObject data = new DataObject();
				data.SetData("Object", Selected_list);
				DragDrop.DoDragDrop(this, data, DragDropEffects.Copy);
			}

			//e.Handled = true;
		}

		public void ReLoadChild()
		{
			SftpFile[] files;
			files = WindowMain.current?.enableConnect?.sshManager?.PullListInDirectory("/");
			if(files == null)
			{
				return;
			}

			this.Items.Clear();

			int count_have_directory = 0;
			foreach(var file in files)
			{
				int i;
				for(i = 0; i < LinuxTreeView.IGNORE_FILENAME.Length; i++)
				{
					if(file.Name == LinuxTreeView.IGNORE_FILENAME[i])
						break;
				}
				if(i != LinuxTreeView.IGNORE_FILENAME.Length)
					continue;

				LinuxTreeViewItem ltvi;
				if(file.IsDirectory)
				{
					ltvi = new LinuxTreeViewItem(this, file.FullName, file, file.Name, true);
					this.Items.Insert(count_have_directory++, ltvi);
				}
				else
				{
					ltvi = new LinuxTreeViewItem(this, file.FullName, file, file.Name, false);
					this.Items.Add(ltvi);
				}
			}
		}
		public int Refresh(string path)
		{
			if(path == null
				|| path.Length <= 0
				|| path[0] != '/')
				return -1;

			string[] path_names = path.Split('/');

			this.ReLoadChild();

			if(path != null)
			{
				ItemsControl ic_cur = this;
				for(int i = 0; i < path_names.Length; i++)
				{
					if(ic_cur != null && path_names[i].Length > 0)
					{
						ItemCollection items = (ic_cur as ItemsControl)?.Items;
						ic_cur = null;
						if(items != null)
						{
							int j;
							for(j = 0; j < items.Count; j++)
							{
								if((items[j] as LinuxTreeViewItem)?.FileName == path_names[i])
								{
									ic_cur = items[j] as ItemsControl;
									(ic_cur as LinuxTreeViewItem)?.ReLoadChild();
									break;
								}
							}
							if(j == items.Count)
								return -2;
						}
					}
				}
			}
			return 0;
		}

		public int SetConfigInfo()
		{
			if(WindowMain.current?.enableConnect?.sshManager?.GetDaemonInfo(MainSettings.Path.PathDirConfigFile) == 0)
			{
				string daemon_info_json = FileContoller.Read(MainSettings.Path.PathDirConfigFile + "/daemon_info.json");
				if(daemon_info_json == null || daemon_info_json.Length == 0)
					daemon_info_json = Properties.Resources.daemon_info_default;

				JObject root;
				try
				{
					root = JObject.Parse(daemon_info_json);
				}
				catch(Exception e)
				{
					Console.WriteLine("JHLIM_DEBUG : JObject parse " + e.Message);
					return -2;
				}

				JArray jarr = root?["daemon_info"] as JArray;
				if(jarr == null)
					return -3;

				try
				{
					foreach(JObject jobj in jarr)
					{
						string[] arr_path_names = jobj["path"]?.ToString().Split('/');
						ItemsControl ic_cur = this;
						
						for(int i = 0; i < arr_path_names.Length; i++)
						{
							if(arr_path_names[i] == null || arr_path_names[i] == "")
								continue;

							int j;
							int count = ic_cur.Items.Count;
							for(j = 0; j < count; j++)
							{
								LinuxTreeViewItem ltvi_child = ic_cur.Items[j] as LinuxTreeViewItem;
								if(ltvi_child == null)
									continue;

								if(ltvi_child.FileName == arr_path_names[i])
								{
									if(ltvi_child.Path == jobj["path"]?.ToString())
									{
										ltvi_child.ConfigPath = jobj["config"]?.ToString();
										ic_cur = null;
									}
									else
									{
										ltvi_child.ConfigPath = "";
										ic_cur = ltvi_child;
									}
									break;
								}
							}
							if(j == count || ic_cur == null)
								break;
						}
					}
				}
				catch(Exception e)
				{
					Console.WriteLine("JHLIM_DEBUG : SetConfigInfo " + e.Message);
				}
			}
			return 0;
		}
	}
}
