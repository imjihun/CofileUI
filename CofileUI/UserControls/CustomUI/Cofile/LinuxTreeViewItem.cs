using MahApps.Metro.IconPacks;
using CofileUI.Classes;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CofileUI.Windows;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;

namespace CofileUI.UserControls
{
	/// <summary>
	/// LinuxTreeViewItem	-> Header -> (file or directory)Name TextBox
	///						-> Items -> LinuxTreeViewItem
	/// </summary>

	public interface LinuxTree
	{
		void ReLoadChild();
	}

	public class LinuxTreeView : TreeView, LinuxTree
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
					ltvi = new LinuxTreeViewItem(this, file.FullName, file, file.Name, true, this);
					this.Items.Insert(count_have_directory++, ltvi);
				}
				else
				{
					ltvi = new LinuxTreeViewItem(this, file.FullName, file, file.Name, false, this);
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
				LinuxTree lt_cur = this;
				for(int i = 0; i < path_names.Length; i++)
				{
					if(lt_cur != null && path_names[i].Length > 0)
					{
						ItemCollection items = (lt_cur as ItemsControl)?.Items;
						lt_cur = null;
						if(items != null)
						{
							int j;
							for(j = 0; j < items.Count; j++)
							{
								if((items[j] as LinuxTreeViewItem)?.FileName == path_names[i])
								{
									lt_cur = items[j] as LinuxTree;
									lt_cur.ReLoadChild();
									break;
								}
							}
							if(j == items.Count)
								return -2;
						}
					}
				}
			}
			RefreshConfigInfo();
			return 0;
		}
		private int SetConfigInfo(string path, string config_path)
		{
			if(path == null
				|| path.Length <= 0
				|| path[0] != '/')
				return -1;

			string[] path_names = path.Split('/');

			if(path != null)
			{
				LinuxTree lt_cur = this;
				for(int i = 0; i < path_names.Length; i++)
				{
					if(lt_cur != null && path_names[i].Length > 0)
					{
						ItemCollection items = (lt_cur as ItemsControl)?.Items;
						lt_cur = null;
						if(items != null)
						{
							int j;
							for(j = 0; j < items.Count; j++)
							{
								if((items[j] as LinuxTreeViewItem)?.FileName == path_names[i])
								{
									lt_cur = items[j] as LinuxTree;
									if(i == path_names.Length - 1)
									{
										(items[j] as LinuxTreeViewItem).OriginBackground = Brushes.LightGreen;
										(items[j] as LinuxTreeViewItem).ConfigPath = config_path;
									}
									else
										(items[j] as LinuxTreeViewItem).OriginBackground = Brushes.LightCyan;
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
		private void RefreshConfigInfo()
		{
			JObject root = Decrypt.current.configMenu.btnFileConfig.Root;
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
	public class LinuxTreeViewItem : TreeViewItem, LinuxTree
	{
		private LinuxTreeView view;

		private string path;
		public string Path
		{
			get { return path; }
			set
			{
				path = value;
			}
		}
		private bool isDirectory = false;
		public bool IsDirectory
		{
			get { return isDirectory; }
			set
			{
				isDirectory = value;
				if(value)
				{
					Label dummy = new Label();
					this.Items.Add(dummy);
					if(tbName == null)
						return;

					tbName.FontWeight = FontWeights.Bold;
					tbName.Foreground = LinuxTreeView._Color.Folder_foreground;
				}
				else
				{
				}
			}
		}

		public string FileName { get { if(FileInfo == null) return Path; else return FileInfo.Name; } }

		private SftpFile fileInfo;
		public SftpFile FileInfo { get { return fileInfo; } set { fileInfo = value; } }
		#region header
		// Casting ( Object To Grid_Header )
		public new DockPanel Header { get { return base.Header as DockPanel; } }
		private ContextMenu enableContextMenu = null;
		private TextBlock tbName;
		private TextBlock tbConfigPath;
		private Image img;
		public string ConfigPath
		{
			get { return tbConfigPath.Text; }
			set
			{
				tbConfigPath.Text = value;
				if(value == null || value.Length == 0)
					this.ContextMenu = null;
				else
				{
					this.ContextMenu = enableContextMenu;
				}
			}
		}
		public Brush HaederBackGround { set { tbName.Background = value; } }
		void InitHeader(string header, bool _isDirectory)
		{
			base.Header = new DockPanel();
			img = new Image();
			if(_isDirectory)
				img.Source = new BitmapImage(new System.Uri("/CofileUI;component/Resources/is_directory.png", System.UriKind.Relative));
			//img.Source = BitmapToImageSource(Properties.Resources.directory);
			else
				img.Source = new BitmapImage(new System.Uri("/CofileUI;component/Resources/file.png", System.UriKind.Relative));
			//img.Source = BitmapToImageSource(Properties.Resources.file);
			img.Height = img.Width = 20;
			DockPanel.SetDock(img, Dock.Left);
			this.Header.Children.Add(img);

			tbName = new TextBlock();
			tbName.Text = header;
			tbName.VerticalAlignment = VerticalAlignment.Center;
			DockPanel.SetDock(tbName, Dock.Left);
			this.Header.Children.Add(tbName);

			tbConfigPath = new TextBlock();
			tbConfigPath.VerticalAlignment = VerticalAlignment.Center;
			tbConfigPath.HorizontalAlignment = HorizontalAlignment.Right;
			tbConfigPath.Text = "";
			tbConfigPath.Margin = new Thickness(0, 0, 5, 0);
			DockPanel.SetDock(tbConfigPath, Dock.Right);
			this.Header.Children.Add(tbConfigPath);
		}
		#endregion
		public LinuxTreeViewItem(LinuxTreeView _view, string _path, SftpFile _file, string header, bool _isDirectory, LinuxTree _parent)
		{
			view = _view;
			if(header == null && _path != null)
			{
				string[] splited = _path.Split('/');
				header = splited[splited.Length - 1];
			}
			
			InitHeader(header, _isDirectory);
			this.Cursor = Cursors.Hand;
			this.Path = _path;
			this.parent = _parent;

			FileInfo = _file;

			//InitContextMenu();

			this.IsDirectory = _isDirectory;

			if(this.FileName != null && this.FileName.Length > 0 && this.FileName[0] == '.')
			{
				this.Header.Opacity = .5;
			}

			enableContextMenu = new ContextMenu();
			MenuItem item = new MenuItem();
			item.Header = "Kill";
			//item.Icon = new PackIconFontAwesome()
			//{
			//	Kind = PackIconFontAwesomeKind.Lock,
			//	VerticalAlignment = VerticalAlignment.Center,
			//	HorizontalAlignment = HorizontalAlignment.Center
			//};
			item.Click += (sender, e) =>
			{
				Console.WriteLine("JHLIM_DEBUG : Kill??");
			};
			enableContextMenu.Items.Add(item);
		}

		#region Multi Select From Mouse Handle
		private Brush originBackground = LinuxTreeView._Color.Background_unselected;
		public Brush OriginBackground { get { return originBackground; } set { originBackground = value; if(!this.MySelected) this.Background = value; } }
		private bool isMouseDownHandled = false;
		private bool mySelected = false;
		public bool MySelected
		{
			get { return mySelected; }
			set
			{
				mySelected = value;

				// 색 변경
				if(value)
				{
					view.Selected_list.Add(this);
					OriginBackground = this.Background;
					this.Background = LinuxTreeView._Color.Background_selected;
					if(!this.IsDirectory)
					{
						this.Foreground = LinuxTreeView._Color.File_foreground_selected;
					}
				}
				else
				{
					view.Selected_list.Remove(this);
					this.Background = OriginBackground;
					if(!this.IsDirectory)
					{
						this.Foreground = LinuxTreeView._Color.File_foreground_unselected;
					}
				}
			}
		}
		public new void Focus()
		{
			// 다른 선택 해제
			while(view.Selected_list.Count > 0)
			{
				view.Selected_list[0].MySelected = false;
			}
			MySelected = true;

			this.BringIntoView();
			//this.BringIntoView(new Rect(0,0,50,50));
			//// scroll bar sycronized
			//var tvItem = (LinuxTreeViewItem)this;
			//var itemCount = VisualTreeHelper.GetChildrenCount(tvItem);

			//for(var i = itemCount - 1; i >= 0; i--)
			//{
			//	var child = VisualTreeHelper.GetChild(tvItem, i);
			//	((FrameworkElement)child).BringIntoView();
			//}
		}
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			isMouseDownHandled = true;

			if(WindowMain.bCtrl)
				MySelected = !MySelected;
			else if(WindowMain.bShift && view.Selected_list.Count > 0)
			{
				ItemCollection Items = (this.Parent as ItemsControl)?.Items;
				if(Items != null)
				{
					int idx_start = Items.IndexOf(view.Selected_list[0]);
					int idx_end = Items.IndexOf(this);

					if(idx_start >= 0 && idx_end >= 0)
					{
						// 선택 초기화
						while(view.Selected_list.Count > 0)
							view.Selected_list[0].MySelected = false;

						// 선택
						int add_i = idx_start < idx_end ? 1 : -1;
						for(int i = idx_start; i != idx_end + add_i; i += add_i)
						{
							LinuxTreeViewItem child = Items[i] as LinuxTreeViewItem;
							if(child == null)
								continue;

							child.MySelected = true;
						}
					}
					else
						Focus();
				}
				else
					Focus();
			}
			else if(!MySelected)
				Focus();
			else
				isMouseDownHandled = false;

			if(e.ClickCount > 1)
				this.IsExpanded = !this.IsExpanded;
			e.Handled = true;
		}
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			if(!isMouseDownHandled)
				Focus();

			e.Handled = true;
		}
		protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
		{
			if(view.Selected_list.IndexOf(this) < 0)
				this.Focus();
			base.OnMouseRightButtonDown(e);
			e.Handled = true;
		}
		#endregion

		#region Load Directory And Refresh View
		protected override void OnExpanded(RoutedEventArgs e)
		{
			base.OnExpanded(e);
			this.Focus();
			
			this._ReLoadChild();

			this.BringIntoView();
		}
		//private bool flag_expanded_via_screen = true; 
		public new bool IsExpanded { get { return base.IsExpanded; }
			set
			{
				//flag_expanded_via_screen = false;
				base.IsExpanded = value;
				//flag_expanded_via_screen = true;
			}
		}
		private LinuxTree parent = null;
		public new LinuxTree Parent {
			get
			{
				return parent;
			}
		}
		// remind_path = '/' 부터 시작
		private void _ReLoadChild()
		{
			SftpFile[] files;
			files = WindowMain.current?.enableConnect?.sshManager?.PullListInDirectory(this.path);
			if(files == null)
			{
				this.IsExpanded = false;
				img.Source = new BitmapImage(new System.Uri("/CofileUI;component/Resources/directory_deny.png", System.UriKind.Relative));
				return;
			}
			img.Source = new BitmapImage(new System.Uri("/CofileUI;component/Resources/directory.png", System.UriKind.Relative));

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
					ltvi = new LinuxTreeViewItem(view, file.FullName, file, file.Name, true, this);
					this.Items.Insert(count_have_directory++, ltvi);	
				}
				else
				{
					ltvi = new LinuxTreeViewItem(view, file.FullName, file, file.Name, false, this);
					this.Items.Add(ltvi);
				}
			}
			view.Last_refresh = this;
			//LinuxTreeViewItem.Filter(this, filter_string, b_show_hidden);
		}
		public void ReLoadChild()
		{
			IsExpanded = true;
		}
		#endregion

		#region Visible Filter Via Regular Expresion
		
		public static void Filter(LinuxTreeViewItem parent, string filter_string, bool bShow_hidden)
		{
			if(parent == null)
				return;
			try
			{
				Regex r = new Regex(filter_string);
				filter_recursive(parent, r, bShow_hidden);
			}
			catch(Exception e)
			{
				Log.PrintError(e.Message, "UserControls.LinuxTreeViewItem.Filter");
			}
		}
		static void filter_recursive(LinuxTreeViewItem cur, Regex filter_string, bool bShow_hidden)
		{
			for(int i = 0; i < cur.Items.Count; i++)
			{
				LinuxTreeViewItem child = cur.Items[i] as LinuxTreeViewItem;
				if(child == null)
					continue;

				string name = child.FileName;
				if(!filter_string.IsMatch(name)
					|| (!bShow_hidden && name[0] == '.'))
					child.Visibility = Visibility.Collapsed;
				else
				{
					child.Visibility = Visibility.Visible;

					LinuxTreeViewItem parent = child.Parent as LinuxTreeViewItem;
					while(parent != null)
					{
						parent.Visibility = Visibility.Visible;
						parent = parent.Parent as LinuxTreeViewItem;
					}
				}

				if(child.IsDirectory)
					filter_recursive(child, filter_string, bShow_hidden);
			}
		}

		#endregion
	}
}
