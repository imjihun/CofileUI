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

namespace CofileUI.UserControls
{
	/// <summary>
	/// LinuxTreeViewItem	-> Header -> (file or directory)Name TextBox
	///						-> Items -> LinuxTreeViewItem
	/// </summary>

	public class LinuxTree
	{
		private SftpFile fileinfo;
		public SftpFile Fileinfo { get { return fileinfo; } set { fileinfo = value; } }
		private List<LinuxTree> childs = new List<LinuxTree>();
		public List<LinuxTree> Childs { get { return childs; } set { childs = value; } }

		private bool IsLoaded = false;

		public LinuxTree(SftpFile _fileinfo)
		{
			Fileinfo = _fileinfo;
		}
		public bool LoadChild()
		{
			if(IsLoaded)
				return true;

			SftpFile[] files;

			files = SSHController.PullListInDirectory(Fileinfo.FullName);
			if(files == null)
				return false;

			this.Childs.Clear();
			foreach(var file in files)
				this.Childs.Add(new LinuxTree(file));

			IsLoaded = true;
			return true;
		}
		public LinuxTree[] GetChild()
		{
			LoadChild();
			return Childs.ToArray();
		}
		public bool ViewUpdate()
		{
			LoadChild();

			return true;
		}
	}

	
	public class LinuxTreeViewItem : TreeViewItem
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

		static string[] IGNORE_FILENAME = new string[] {".", ".."};

		private string path;
		public string Path
		{
			get { return path; }
			set
			{
				path = value;
				if(value == "/")
					this.Header.Text = value;
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
					TextBlock tb = this.Header.Tb_name as TextBlock;
					if(tb == null)
						return;

					tb.FontWeight = FontWeights.Bold;
					tb.Foreground = _Color.Folder_foreground;
				}
				else
				{
				}
			}
		}

		private SftpFile fileInfo;
		public SftpFile FileInfo { get { return fileInfo; } set { fileInfo = value; } }
		#region header
		public class Grid_Header : DockPanel
		{
			private TextBlock tb_name;
			public TextBlock Tb_name { get { return tb_name; } }
			//const int HEIGHT = 30;
			public string Text
			{
				get
				{
					if(tb_name == null)
						return null;
					return tb_name.Text;
					//tb.Text = newText;
				}
				set
				{
					if(tb_name == null)
						return;
					tb_name.Text = value;
				}
			}

			public TextBlock tb_config_path;

			public Grid_Header(string header, bool isDirectory)
			{
				//this.Height = HEIGHT;
				//this.Orientation = Orientation.Horizontal;
				Image img = new Image();
				if(isDirectory)
					img.Source = new BitmapImage(new System.Uri("/CofileUI;component/Resources/directory.png", System.UriKind.Relative));
					//img.Source = BitmapToImageSource(Properties.Resources.directory);
				else
					img.Source = new BitmapImage(new System.Uri("/CofileUI;component/Resources/file.png", System.UriKind.Relative));
					//img.Source = BitmapToImageSource(Properties.Resources.file);
				img.Height = img.Width = 20;
				DockPanel.SetDock(img, Dock.Left);
				this.Children.Add(img);

				tb_name = new TextBlock();
				tb_name.Text = header;
				tb_name.VerticalAlignment = VerticalAlignment.Center;
				DockPanel.SetDock(tb_name, Dock.Left);
				this.Children.Add(tb_name);

				tb_config_path = new TextBlock();
				tb_config_path.VerticalAlignment = VerticalAlignment.Center;
				tb_config_path.HorizontalAlignment = HorizontalAlignment.Right;
				tb_config_path.Text = "";
				tb_config_path.Margin = new Thickness(0, 0, 5, 0);
				DockPanel.SetDock(tb_config_path, Dock.Right);
				this.Children.Add(tb_config_path);
			}
			BitmapImage BitmapToImageSource(System.Drawing.Bitmap bitmap)
			{
				using(MemoryStream memory = new MemoryStream())
				{
					bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
					memory.Position = 0;
					BitmapImage bitmapimage = new BitmapImage();
					bitmapimage.BeginInit();
					bitmapimage.StreamSource = memory;
					bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
					bitmapimage.EndInit();

					return bitmapimage;
				}
			}

		}
		// Casting ( Object To Grid_Header )
		public new Grid_Header Header { get { return base.Header as Grid_Header; } set { base.Header = value; } }
		#endregion

		public LinuxTreeViewItem(string _path, SftpFile _file, string header, bool _isDirectory, LinuxTreeViewItem _parent)
		{
			if(header == null && _path != null)
			{
				string[] splited = _path.Split('/');
				header = splited[splited.Length - 1];
			}
			
			this.Header = new Grid_Header(header, _isDirectory);
			this.Cursor = Cursors.Hand;
			this.Path = _path;
			this.parent = _parent;

			FileInfo = _file;

			//InitContextMenu();

			this.IsDirectory = _isDirectory;

			if(this.IsDirectory)
			{
				// 임시
				Label dummy = new Label();
				this.Items.Add(dummy);
				TextBlock tb = this.Header.Tb_name as TextBlock;
				if(tb == null)
					return;

				tb.FontWeight = FontWeights.Bold;
				tb.Foreground = _Color.Folder_foreground;
			}

			if(this.Header.Text != null && this.Header.Text.Length > 0 && this.Header.Text[0] == '.')
			{
				this.Header.Opacity = .5;
			}
		}

		#region Context Menu
		private void InitContextMenu()
		{
			this.ContextMenu = new ContextMenu();
			MenuItem item = new MenuItem();
			item.Header = Cofile.current.Resources["String.Context.Encrypt"];
			item.Icon = new PackIconFontAwesome()
			{
				Kind = PackIconFontAwesomeKind.Lock,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			item.Click += OnClickEncrypt;
			this.ContextMenu.Items.Add(item);
			item = new MenuItem();
			item.Icon = new PackIconFontAwesome()
			{
				Kind = PackIconFontAwesomeKind.Unlock,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			item.Header = Cofile.current.Resources["String.Context.Decrypt"];
			item.Click += OnClickDecrypt;
			this.ContextMenu.Items.Add(item);
		}
		private void OnClickEncrypt(object sender, RoutedEventArgs e)
		{
			Cofile.current.ConfirmEncDec(selected_list, true);
		}
		private void OnClickDecrypt(object sender, RoutedEventArgs e)
		{
			Cofile.current.ConfirmEncDec(selected_list, false);
		}
		#endregion

		#region Multi Select From Mouse Handle
		private static List<LinuxTreeViewItem> selected_list = new List<LinuxTreeViewItem>();
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
					selected_list.Add(this);
					this.Background = _Color.Background_selected;
					if(!this.IsDirectory)
					{
						this.Foreground = _Color.File_foreground_selected;
					}
				}
				else
				{
					selected_list.Remove(this);
					this.Background = _Color.Background_unselected;
					if(!this.IsDirectory)
					{
						this.Foreground = _Color.File_foreground_unselected;
					}
				}
			}
		}
		public new void Focus()
		{
			// 다른 선택 해제
			while(selected_list.Count > 0)
			{
				selected_list[0].MySelected = false;
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
			if(WindowMain.bCtrl)
				MySelected = !MySelected;
			else if(WindowMain.bShift && selected_list.Count > 0)
			{
				LinuxTreeViewItem parent = this.Parent as LinuxTreeViewItem;
				if(parent != null)
				{
					int idx_start = parent.Items.IndexOf(selected_list[0]);
					int idx_end = parent.Items.IndexOf(this);

					if(idx_start >= 0 && idx_end >= 0)
					{
						// 선택 초기화
						while(selected_list.Count > 0)
							selected_list[0].MySelected = false;

						// 선택
						int add_i = idx_start < idx_end ? 1 : -1;
						for(int i = idx_start; i != idx_end + add_i; i += add_i)
						{
							LinuxTreeViewItem child = parent.Items[i] as LinuxTreeViewItem;
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
			else
				Focus();

			if(e.ClickCount > 1)
			{
				//this.RefreshChild();
				this.IsExpanded = !this.IsExpanded;
			}

			e.Handled = true;
		}
		protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
		{
			if(selected_list.IndexOf(this) < 0)
				this.Focus();
			base.OnMouseRightButtonDown(e);
			e.Handled = true;
		}
		#endregion
		
		protected override void OnMouseMove(MouseEventArgs e)
		{
			//Log.Print(linked_jtoken);
			base.OnMouseMove(e);
			if(e.LeftButton == MouseButtonState.Pressed
				&& LinuxTreeViewItem.selected_list.Count > 0)
			{
				DataObject data = new DataObject();
				data.SetData("Object", LinuxTreeViewItem.selected_list);
				DragDrop.DoDragDrop(this, data, DragDropEffects.Copy);
			}

			//e.Handled = true;
		}

		#region Load Directory And Refresh View
		protected override void OnExpanded(RoutedEventArgs e)
		{
			base.OnExpanded(e);
			this.Focus();

			//if(flag_expanded_via_screen)
			//{
			//	this.RefreshChild();
			//}
			this.RefreshChild();

			this.BringIntoView();
			//this.BringIntoView(new Rect(0, 0, 50, 50));
			//// scroll bar syncronized
			//var tvItem = (LinuxTreeViewItem)e.OriginalSource;
			//var itemCount = VisualTreeHelper.GetChildrenCount(tvItem);

			//for(var i = itemCount - 1; i >= 0; i--)
			//{
			//	var child = VisualTreeHelper.GetChild(tvItem, i);
			//	((FrameworkElement)child).BringIntoView();
			//}
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
		public static void Clear(LinuxTreeViewItem root)
		{
			if(root != null)
			{
				root.Items.Clear();
				root = null;
			}
		}
		public static LinuxTreeViewItem Last_Refresh = null;
		public void RefreshChild(string remained_path = null, bool bRefreshListView = true)
		{
			if(IsDirectory)
			{
				this.IsExpanded = true;

				RefreshDirectory(remained_path);

				// LinuxTreeViewItem 을 참조하여 ListView 를 재구성하기 때문에 LinuxTreeViewItem 이 Refresh 될 때 Refresh 해야함.
				if(bRefreshListView)
					Cofile.current.RefreshListView(this);
			}
		}
		private void RefreshDirectory(string remained_path = null)
		{
			Last_Refresh = this;

			ReLoadDirectory(remained_path);
			// refresh filter
			Cofile.Filter_string = Cofile.Filter_string;
		}
		private LinuxTreeViewItem parent = null;
		public new LinuxTreeViewItem Parent {
			get
			{
				LinuxTreeViewItem ltvi = base.Parent as LinuxTreeViewItem;
				if(ltvi == null)
					return parent;

				return ltvi;
			}
		}
		public void RefreshChildFromParent()
		{
			LinuxTreeViewItem par = this.Parent as LinuxTreeViewItem;
			if(par == null)
				return;

			par.RefreshChild();
		}
		// remind_path = '/' 부터 시작
		void ReLoadDirectory(string remainned_path = null)
		{
			SftpFile[] files;
			files = SSHController.PullListInDirectory(this.path);
			if(files == null)
			{
				this.IsExpanded = false;
				return;
			}

			this.Items.Clear();

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

				LinuxTreeViewItem ltvi;
				if(file.IsDirectory)
				{
					//this.Items.Insert(0, new LinuxTreeViewItem(file.FullName, file.Name, true));
					//this.Items.Add(new LinuxTreeViewItem(file.FullName, file.Name, true));
					ltvi = new LinuxTreeViewItem(file.FullName, file, file.Name, true, this);
					this.Items.Insert(count_have_directory++, ltvi);

					// remainned_path = '/' 부터 시작
					if(remainned_path != null)
					{
						string[] split = remainned_path.Split('/');
						if(split.Length > 1 && split[1] == file.Name)
						{
							ltvi.RefreshChild(remainned_path.Substring(split[1].Length + 1));
						}
					}
				}
				else
				{
					ltvi = new LinuxTreeViewItem(file.FullName, file, file.Name, false, this);
					this.Items.Add(ltvi);
				}
			}
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

				string name = child.Header.Text;
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

		public static int ChangeColor(LinuxTreeViewItem root, string _path, string config_path)
		{
			if(_path == null
				|| _path.Length <= 0
				|| _path[0] != '/'
				|| root == null)
				return -1;

			string[] arr_path = _path.Trim('/').Split('/');

			LinuxTreeViewItem cur = root;
			for(int i = 0; i < arr_path.Length; i++)
			{
				Console.WriteLine("JHLIM_DEBUG : " + cur.FileInfo?.Name + " / " + arr_path[i]);
				int j;
				int count = cur.Items.Count;
				for(j = 0; j < count; j++)
				{
					if((cur.Items[j] as LinuxTreeViewItem)?.FileInfo.Name == arr_path[i])
					{
						if(i == arr_path.Length - 1)
						{
							(cur.Items[j] as LinuxTreeViewItem).Header.Tb_name.Background = Brushes.LightGreen;
							(cur.Items[j] as LinuxTreeViewItem).Header.tb_config_path.Text = config_path;
						}
						else
							(cur.Items[j] as LinuxTreeViewItem).Header.Tb_name.Background = Brushes.LightCyan;
						cur = (cur.Items[j] as LinuxTreeViewItem);
						break;
					}
				}
				if(j == count)
					break;
			}

			return 0;
		}
		#endregion
	}
}
