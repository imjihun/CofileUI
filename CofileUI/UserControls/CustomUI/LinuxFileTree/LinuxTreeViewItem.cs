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
	
	public class LinuxTreeViewItem : TreeViewItem
	{
		private LinuxTreeView root_view;

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
				if(value == null)
					this.ContextMenu = null;
				else if(value.Length == 0)
				{
					this.OriginBackground = Brushes.LightCyan;
				}
				else
				{
					this.OriginBackground = Brushes.LightGreen;
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
		public LinuxTreeViewItem(LinuxTreeView _view, string _path, SftpFile _file, string header, bool _isDirectory)
		{
			root_view = _view;
			if(header == null && _path != null)
			{
				string[] splited = _path.Split('/');
				header = splited[splited.Length - 1];
			}
			
			InitHeader(header, _isDirectory);
			this.Cursor = Cursors.Hand;
			this.Path = _path;

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
					root_view.Selected_list.Add(this);
					OriginBackground = this.Background;
					this.Background = LinuxTreeView._Color.Background_selected;
					if(!this.IsDirectory)
					{
						this.Foreground = LinuxTreeView._Color.File_foreground_selected;
					}
				}
				else
				{
					root_view.Selected_list.Remove(this);
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
			while(root_view.Selected_list.Count > 0)
			{
				root_view.Selected_list[0].MySelected = false;
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
			else if(WindowMain.bShift && root_view.Selected_list.Count > 0)
			{
				ItemCollection Items = (this.Parent as ItemsControl)?.Items;
				if(Items != null)
				{
					int idx_start = Items.IndexOf(root_view.Selected_list[0]);
					int idx_end = Items.IndexOf(this);

					if(idx_start >= 0 && idx_end >= 0)
					{
						// 선택 초기화
						while(root_view.Selected_list.Count > 0)
							root_view.Selected_list[0].MySelected = false;

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
			if(root_view.Selected_list.IndexOf(this) < 0)
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
		// remind_path = '/' 부터 시작
		private void _ReLoadChild()
		{
			SftpFile[] files;
			files = WindowMain.current?.EnableConnect?.SshManager?.PullListInDirectory(this.path);
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
					ltvi = new LinuxTreeViewItem(root_view, file.FullName, file, file.Name, true);
					this.Items.Insert(count_have_directory++, ltvi);	
				}
				else
				{
					ltvi = new LinuxTreeViewItem(root_view, file.FullName, file, file.Name, false);
					this.Items.Add(ltvi);
				}
			}
			root_view.Last_refresh = this;

			root_view.SetConfigInfo();

		}
		public void ReLoadChild()
		{
			IsExpanded = false;
			IsExpanded = true;
		}
		#endregion
	}
}
