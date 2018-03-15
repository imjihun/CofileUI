using CofileUI.Classes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MS.Internal.AppModel;
using MS.Win32;
using MahApps.Metro.IconPacks;
using Renci.SshNet.Sftp;
using System.Text.RegularExpressions;
using CofileUI.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Globalization;
using System.Windows.Controls.Primitives;
using Newtonsoft.Json.Linq;
using MahApps.Metro.Controls.Dialogs;
using CofileUI.UserControls.ConfigOptions;

namespace CofileUI.UserControls
{
	/// <summary>
	/// Cofile.xaml에 대한 상호 작용 논리
	/// </summary>
	public enum CofileType
	{
		undefined = 0
		, file
		, sam
		, tail
	}
	public partial class Cofile : UserControl
	{
		public static Cofile current;
		public bool bUpdated = false;
		LinuxTreeViewItem root;

		public Cofile()
		{
			InitializeComponent();

			//root = new LinuxTreeViewItem("/", null, "/", true, null);
			//treeView_linux_directory.Items.Add(root);

			LinuxFileViewModel vfvm = new LinuxFileViewModel(this);
			DataContext = vfvm;
			current = this;
			InitLinuxDirectory();
			this.Loaded += (sender, e) => {
				if(!bUpdated)
					Cofile.current.Refresh();
			};
			grid_config.Children.Add(new ConfigMenu() { DataContext = this });
		}
		#region Common
		
		string root_path = MainSettings.Path.PathDirPreviewFile;
		public int Refresh()
		{
			if(WindowMain.current == null)
				return -1;

			if(!SSHController.IsConnected)
				return -2;
			// 추가
			// root 의 path 는 null 로 초기화
			string working_dir = SSHController.WorkingDirectory;
			if(working_dir == null)
				return -1;

			LinuxFileModel lfm_cur = treeView_linux_directory1.Items?[0] as LinuxFileModel;
			if(lfm_cur != null)
				lfm_cur.IsExpanded = true;

			string[] names = working_dir.Split('/');
			Console.WriteLine("JHLIM_DEBUG : " + names);
			if(names != null)
			{
				for(int i = 0; i < names.Length; i++)
				{
					if(names[i] == null || names[i].Length == 0)
						continue;

					for(int j = 0; j < lfm_cur.Children.Count; j++)
					{
						if(names[i] == lfm_cur.Children[j]?.Name)
						{
							lfm_cur = lfm_cur.Children[j];
							lfm_cur.IsExpanded = true;
							break;
						}
					}
				}
			}

			//if(WindowMain.current == null)
			//	return -1;

			////if(ssh != null && ssh.IsConnected)
			////	ssh.Disconnect();
			////if(sftp != null && sftp.IsConnected)
			////	sftp.Disconnect();

			//// 삭제
			////treeView_linux_directory.Items.Clear();
			////listView_linux_files.Items.Clear();
			//root?.Items.Clear();
			//root = null;
			//treeView_linux_directory.Items.Clear();
			//listView_linux_files.Items.Clear();

			//// 추가
			////string home_dir = sftp.WorkingDirectory;
			//// root 의 path 는 null 로 초기화
			//string working_dir = SSHController.WorkingDirectory;
			//if(working_dir == null)
			//	return -1;

			//if(root == null)
			//{
			//	root = new LinuxTreeViewItem("/", null, "/", true, null);
			//	treeView_linux_directory.Items.Add(root);
			//}
			//root.RefreshChild(working_dir, false);
			//Cofile.current.RefreshListView(LinuxTreeViewItem.Last_Refresh);
			//Log.PrintLog("[refresh]", "UserControls.Cofile.Refresh");

			//bUpdated = true;

			return 0;
			//LinuxTreeViewItem.ReconnectServer();
		}
		public void Clear()
		{
			//root?.Items.Clear();
			//root = null;
			//treeView_linux_directory.Items.Clear();
			//listView_linux_files.Items.Clear();
			//SelectedConfigLocalPath = "Not Selected";
		}
		public void PrintCheckConfigFile()
		{
			Status.current.Clear();

			Log.ErrorIntoUI("Check the Config file [path = " + selectedConfigLocalPath + "]"
				, "Config Error", Status.current.richTextBox_status);

			string message = "Check the Config file" + Environment.NewLine + "path = " + selectedConfigLocalPath;
			WindowMain.current.ShowMessageDialog("Config Error", message);
		}
		private string selectedConfigLocalPath = null;
		public string SelectedConfigLocalPath
		{
			get
			{
				if(selectedConfigLocalPath == null)
				{
					PrintCheckConfigFile();
				}
				return selectedConfigLocalPath;
			}
			set
			{
				selectedConfigLocalPath = value;
				string[] splited = selectedConfigLocalPath.Split('\\');
				textBlock_selected_config_file_name.Text = splited[splited.Length - 1];
			}
		}
		public CofileType GetSelectedType()
		{
			CofileType type = CofileType.undefined;

			JToken jtok = GetSelectedConfigFile();
			if(jtok != null)
			{
				try
				{
					type = (CofileType)Enum.Parse(typeof(CofileType), jtok["type"].ToString(), true);
				}
				catch(Exception e)
				{
					string message = "Check the Config type" + Environment.NewLine + e.Message;
					Log.PrintError(message, "UserControls.Cofile.SelectedType");
					WindowMain.current.ShowMessageDialog("Config Error", message);
				}
			}
			return type;
		}

		public JToken GetSelectedConfigFile()
		{
			string path = SelectedConfigLocalPath;
			if(path == null)
				return null;

			JToken jtok = JsonController.ParseJson(FileContoller.Read(path));
			if(jtok == null)
			{
				PrintCheckConfigFile();
			}
			return jtok;
		}
		private void OnClickSelectConfigFile(object sender, EventArgs e)
		{
			////SftpFileTree root = SSHController.GetListConfigFile();
			////if(root == null)
			////	return;

			////Window_SelectJsonFile ws = new Window_SelectJsonFile(root);
			////if(ws.ShowDialog() == true)
			////	Selected_config_file_path = ws.FilePathRemote;

			//if(ConfigOption.current.InitOpenFile() != 0)
			//	return;

			//OpenFileDialog ofd = new OpenFileDialog();
			//// 초기경로 지정
			//ofd.InitialDirectory = ConfigOption.CurRootPathLocal;

			//if(ConfigOptionManager.Path != null)
			//{
			//	string dir_path = ConfigOptionManager.Path.Substring(0, ConfigOptionManager.Path.LastIndexOf('\\') + 1);
			//	DirectoryInfo d = new DirectoryInfo(dir_path);
			//	if(d.Exists)
			//		ofd.InitialDirectory = dir_path;
			//}

			//// 파일 열기
			//ofd.Filter = "JSon Files (.json)|*.json|All Files (*.*)|*.*";
			//if(ofd.ShowDialog() == true)
			//	SelectedConfigLocalPath = ofd.FileName;
		}

#endregion

#region Linux Directory View

		static bool bool_show_hidden = true;
		public static bool Bool_show_hidden
		{
			get { return bool_show_hidden; }
			set
			{
				bool_show_hidden = value;
			}
		}
		static string filter_string = "";
		public static string Filter_string
		{
			get { return filter_string; }
			set
			{
				filter_string = value;
			}
		}

		private void InitLinuxDirectory()
		{
			textBox_linux_directory_filter.TextChanged += delegate {
				Filter_string = textBox_linux_directory_filter.Text;
				RefreshListView(cur_LinuxTreeViewItem);
				LinuxTreeViewItem.Filter(root, Filter_string, Bool_show_hidden);
			};
			checkBox_hidden.Checked += delegate {
				Bool_show_hidden = true;
				RefreshListView(cur_LinuxTreeViewItem);
				LinuxTreeViewItem.Filter(root, Filter_string, Bool_show_hidden);
			};
			checkBox_hidden.Unchecked += delegate {
				Bool_show_hidden = false;
				RefreshListView(cur_LinuxTreeViewItem);
				LinuxTreeViewItem.Filter(root, Filter_string, Bool_show_hidden);
			};
			checkBox_hidden.IsChecked = false;

			checkBox_linux_detail.IsChecked = true;
		}
		public void RefreshListView(LinuxTreeViewItem cur)
		{
			if(cur == null)
				return;

			cur_LinuxTreeViewItem = cur;
			comboBox_listView_linuxpath.Text = cur.Path;

			list_LinuxListViewItem.Clear();
			LinuxListViewItem llvi = new LinuxListViewItem() { BindingName = "..", IsDirectory = true, LinuxTVI = cur.Parent as LinuxTreeViewItem };
			list_LinuxListViewItem.Add(llvi);
			for(int i = 0; i < cur.Items.Count; i++)
			{
				LinuxTreeViewItem ltvi = cur.Items[i] as LinuxTreeViewItem;
				if(ltvi == null)
					continue;
				llvi = new LinuxListViewItem() { BindingName = ltvi.FileName, IsDirectory = ltvi.IsDirectory, LinuxTVI = ltvi };
				list_LinuxListViewItem.Add(llvi);
			}

			listView_linux_files.Items.Clear();
			for(int i = 0; i < list_LinuxListViewItem.Count; i++)
			{
				if(list_LinuxListViewItem[i].LinuxTVI != null && list_LinuxListViewItem[i].LinuxTVI.Visibility == Visibility.Visible)
				{
					listView_linux_files.Items.Add(list_LinuxListViewItem[i]);
				}
			}
		}

		private void OnMouseMoveLinuxFile(object sender, MouseEventArgs e)
		{
			if(e.LeftButton == MouseButtonState.Pressed
				&& listView_linux_files.SelectedItems.Count > 0)
			{
				DataObject data = new DataObject();
				data.SetData("Object", listView_linux_files.SelectedItems);
				DragDrop.DoDragDrop(this, data, DragDropEffects.Copy);
			}
		}

		private void OnMouseDoubleClickLinuxFile(object sender, MouseButtonEventArgs e)
		{
			if(e.LeftButton == MouseButtonState.Pressed)
			{
				ListView lv = sender as ListView;
				if(lv == null)
					return;

				LinuxListViewItem selected = lv.SelectedItem as LinuxListViewItem;
				if(selected == null)
					return;

				if(selected.IsDirectory)
				{
					OpenDirectory(selected);
				}
				else
				{
					OpenEncryptFile();
				}
			}
		}
		private int OpenDirectory(LinuxListViewItem llvi)
		{
			if(llvi.IsDirectory)
			{
				if(llvi.LinuxTVI == null)
					return -1;

				llvi.LinuxTVI.ReLoadChild();
				//RefreshListView(selected.LinuxTVI);

				//llvi.LinuxTVI.IsExpanded = false;
				//llvi.LinuxTVI.IsExpanded = true;
				llvi.LinuxTVI.Focus();
			}
			return 0;
		}
		public static string PreviewExtension = "preview_extension_ll";
		static ulong Idx_Encrypt_File_Open = 0;

		public delegate int Tick();
		public static void StartCoRoutine(double time_ms, Tick tick, string timeoutTitle, string timeoutMessage)
		{
			System.Windows.Threading.DispatcherTimer dt = new System.Windows.Threading.DispatcherTimer()
			{
				Interval = new TimeSpan(0,0,0,1)
			};
			DateTime timeout = DateTime.Now.AddMilliseconds(time_ms);
			dt.Tick += delegate 
			{
				int? retval = tick?.Invoke();
				DateTime now = DateTime.Now;
				if(retval == 0)
					dt.Stop();
				else if(timeout < now)
				{
					if(timeoutTitle != null && timeoutMessage != null)
					{
						//Log.ErrorIntoUI(timeoutMessage, timeoutTitle, Status.current.richTextBox_status);
						Log.PrintError(timeoutMessage + "(time out)", "UserControls.Cofile.StartCoRoutine");
					}
					dt.Stop();
				}
			};
			dt.Start();
		}
		private int OpenEncryptFile()
		{
			if(listView_linux_files.SelectedItems.Count < 1)
				return -1;

			LinuxListViewItem llvi = listView_linux_files.SelectedItems[0] as LinuxListViewItem;
			if(llvi == null)
				return -2;

			string remote_path = llvi.LinuxTVI.Path;

			string local_filename = "temp" + Idx_Encrypt_File_Open++;
			string remote_path_dec_file  = FindDecryptFile(remote_path);
			if(remote_path_dec_file == null)
				return -3;

			if(SSHController.SendNRecvCofileCommandPreview(listView_linux_files.SelectedItems.Cast<Object>(), false))
			{
				StartCoRoutine(10000,
					delegate
					{
						if(SSHController.MoveFileToLocal(root_path, remote_path_dec_file, local_filename, 0))
						{
							llvi.LinuxTVI?.Parent?.ReLoadChild();
							Cofile.current.RefreshListView(Cofile.cur_LinuxTreeViewItem);

							string url_localfile = root_path + local_filename;
							string[] split = remote_path.Split('.');
							string expansion = split[split.Length - 2];
							if(expansion == "png"
								|| expansion == "dib"
								|| expansion == "bmp"
								|| expansion == "jpg"
								|| expansion == "jpeg"
								|| expansion == "jpe"
								|| expansion == "jfif"
								|| expansion == "gif"
								|| expansion == "tif"
								|| expansion == "tiff"
								)
							{
								Window_ViewImage wvi = new Window_ViewImage(LoadImage(url_localfile), llvi.LinuxTVI.FileName);
								FileContoller.DeleteFile(url_localfile);

								Point pt = this.PointToScreen(new Point(0, 0));
								wvi.Left = pt.X;
								wvi.Top = pt.Y;
								wvi.ShowDialog();
							}
							else
							{

								string str = FileContoller.Read(url_localfile);
								FileContoller.DeleteFile(url_localfile);

								Window_ViewFile wvf = new Window_ViewFile(str, llvi.LinuxTVI.FileName);

								Point pt = this.PointToScreen(new Point(0, 0));
								wvf.Left = pt.X;
								wvf.Top = pt.Y;
								wvf.ShowDialog();
							}
							return 0;
						}
						else
						{
							//Log.ErrorIntoUI("Check the Cofile Config File or Check File To Decrypt", "Decrypt Failed", Status.current.richTextBox_status);
							//Log.PrintError("Cant Download Decrypt File", "UserControls.Cofile.OpenEncryptFile");
							return -1;
						}
					}, "Check the Cofile Config File or Check File To Decrypt", "Decrypt Failed");
			}
			return -4;
		}
		string FindDecryptFile(string remote_path_enc_file)
		{
			JToken jtok = GetSelectedConfigFile();
			if(jtok == null)
				return null;

			JValue jval_type = jtok["type"] as JValue;
			if(jval_type != null && jval_type.Value.ToString() != "file")
			{
				Log.ViewMessage("'Preview' only supports 'type=file'", "Not Support", Status.current.richTextBox_status);
				return null;
			}
			if(jtok["dec_option"] == null)
			{
				Log.ErrorIntoUI("Check the dec_option.input_extension in Cofile Config file", "Decrypt Failed", Status.current.richTextBox_status);
				return null;
			}


			string output_extension = PreviewExtension;
			if(jtok["dec_option"]["input_extension"] != null)
			{
				JValue jval_input_extansion = jtok["dec_option"]["input_extension"] as JValue;

				string remote_path = remote_path_enc_file;
				if(remote_path.Length > jval_input_extansion.Value.ToString().Length + 1)
					//&& remote_path.Substring(remote_path.Length - jval_input_extansion.Value.ToString().Length) == jval_input_extansion.Value.ToString())
					remote_path = remote_path.Substring(0, remote_path.Length - jval_input_extansion.Value.ToString().Length - 1);

				string remote_path_dec_file = remote_path + "." + output_extension;

				return remote_path_dec_file;
			}
			else
			{
				return remote_path_enc_file + "." + output_extension;
			}
		}
		private BitmapImage LoadImage(string uri_ImageFile)
		{
			BitmapImage myRetVal = null;
			if(uri_ImageFile != null)
			{
				BitmapImage image = new BitmapImage();
				using(FileStream stream = File.OpenRead(uri_ImageFile))
				{
					image.BeginInit();
					image.CacheOption = BitmapCacheOption.OnLoad;
					image.StreamSource = stream;
					image.EndInit();
				}
				myRetVal = image;
			}
			return myRetVal;
		}

		private void OnKeyDownLinuxPath(object sender, KeyEventArgs e)
		{
			//if(e.Key == Key.Enter)
			//{
			//	root.RefreshChild(comboBox_listView_linuxpath.Text, false);
			//	RefreshListView(LinuxTreeViewItem.Last_Refresh);
			//}
		}

		private void OnClickRefresh(object sender, RoutedEventArgs e)
		{
			//if(root == null)
			//	Refresh();

			//else if(root != null)
			//{
			//	string path = null;
			//	if(LinuxTreeViewItem.Last_Refresh == null)
			//		path = SSHController.WorkingDirectory;
			//	else
			//		path = LinuxTreeViewItem.Last_Refresh.Path;

			//	if(path != null)
			//	{
			//		root.RefreshChild(path, false);
			//		RefreshListView(LinuxTreeViewItem.Last_Refresh);
			//	}
			//}
		}

		DateTime LastKeyDonwTime = DateTime.Now;
		double SecKeyDownInterval = .2;
		StringBuilder sb_keydown = new StringBuilder();
		private void OnKeyDownLinuxFiles(object sender, KeyEventArgs e)
		{
			LinuxListViewItem llvi;
			switch(e.Key)
			{
				case Key.Enter:
					llvi = listView_linux_files.SelectedItem as LinuxListViewItem;
					if(llvi == null)
						break;
					OpenDirectory(llvi);
					break;
				case Key.Back:
					if(listView_linux_files.Items.Count > 0)
					{
						llvi = listView_linux_files.Items[0] as LinuxListViewItem;
						if(llvi == null)
							break;

						OpenDirectory(llvi);
					}
					break;
				default:
					if((e.Key >= Key.A && e.Key <= Key.Z)
						|| e.Key == Key.OemPeriod)
					{
						if(LastKeyDonwTime.AddSeconds(SecKeyDownInterval) < DateTime.Now)
						{
							sb_keydown.Clear();
						}

						sb_keydown.Append(KeyCodeToChar(e.Key));
						FocusListView(listView_linux_files, sb_keydown.ToString());

						LastKeyDonwTime = DateTime.Now;

						// 'c' 입력시 넥스트 아이템으로 가게되어서 처리됨을 true로 줌
						e.Handled = true;
					}
					break;
			}
			e.Handled = true;
		}
		public char KeyCodeToChar(Key keyCode)
		{
			char keyChar = ' ';
			switch(keyCode)
			{
				case Key.OemPeriod:
					keyChar = '.';
					break;
				default:
					keyChar = keyCode.ToString().ToLower()[0];
					break;
			}
			return keyChar;
		}
		int FocusListView(ListView lv, string find)
		{
			int idx_start = lv.SelectedIndex;

			// find.Length == 1 일때는 lv.SelectedIndex == -1 일때도 포함.
			// 처음 키보드찾기를 할 때, find.Length 는 1부터 시작할 수 밖에 없음.
			if(find.Length == 1 || idx_start == -1)
			{
				idx_start++;
				if(idx_start >= lv.Items.Count)
					idx_start = 0;
			}

			int idx = idx_start;
			for(int cnt = 0; cnt < lv.Items.Count; cnt++)
			{
				LinuxListViewItem llvi = lv.Items[idx] as LinuxListViewItem;
				if(llvi == null)
					continue;
				string name = llvi.BindingName.ToLower();
				int j;
				for(j = 0; j < name.Length && j < find.Length; j++)
				{
					if(name[j] != find[j])
						break;
				}

				if(j == find.Length)
				{
					((UIElement)lv.ItemContainerGenerator.ContainerFromItem(lv.Items[idx])).Focus();
					break;
				}

				idx++;
				if(idx >= lv.Items.Count)
					idx = 0;
			}
			return 0;
		}

		private void OnPreviewMouseWheelListViewLinuxFiles(object sender, MouseWheelEventArgs e)
		{
			ScrollViewer scv = (ScrollViewer)sender;
			scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
			e.Handled = true;
		}

#endregion
		

		public class LinuxListViewItem
		{
			private string bindingName = "";
			public string BindingName { get { return bindingName; } set { bindingName = value; } }
			private bool isDirectory = false;
			public bool IsDirectory { get { return isDirectory; } set { isDirectory = value; } }
			private static string[] STR_UNITS = new string[] {"Bytes", "KB", "MB", "GB", "TB", "max" };
			public string Size
			{
				get
				{
					if(bindingName == "..")
						return null;

					float size = 0;
					string str_unit = STR_UNITS[0];

					// FileInfo == null 이면 '/' 인 최상루트 디렉토리
					if(linuxTVI.FileInfo != null)
					{
						size = linuxTVI.FileInfo.Length;
						float _size;
						int i;
						for(i = 0; i < STR_UNITS.Length; i++)
						{
							_size = size / 1024;
							if(_size < 1)
								break;
							size = _size;
						}
						str_unit = STR_UNITS[i];
					}
					return string.Format("{0} {1}", Math.Round(size), str_unit);
					//return string.Format("{0:N2} {1}", size, str_unit);
				}
			}
			public string LastWriteTime
			{
				get
				{
					if(bindingName == "..")
						return null;

					string str = "";
					if(linuxTVI.FileInfo != null)
					{
						str = linuxTVI.FileInfo.LastWriteTime.ToString();
					}
					return str;
				}
			}
			public string Type
			{
				get
				{
					if(isDirectory)
						return "Directory";
					else
					{
						return "File";
					}
				}
			}
			//public string Owner
			//{
			//	get
			//	{
			//		string str = "";

			//		return str;
			//	}
			//}

			private LinuxTreeViewItem linuxTVI = null;
			public LinuxTreeViewItem LinuxTVI { get { return linuxTVI; } set { linuxTVI = value; } }
		}

		private void OnCheckedDetail(object sender, RoutedEventArgs e)
		{
			if(sender == checkBox_linux_detail)
			{
				listView_linux_files.ItemsPanel = Resources["ItemsPanelTemplate_ListView_Detail"] as ItemsPanelTemplate;
				listView_linux_files.View = Resources["GridView_ListViewLinux_Detail"] as GridView;
			}
		}
		private void OnUncheckedDetail(object sender, RoutedEventArgs e)
		{
			if(sender == checkBox_linux_detail)
			{
				listView_linux_files.View = null;
				listView_linux_files.ItemsPanel = Resources["ItemsPanelTemplate_ListView_Icon"] as ItemsPanelTemplate;
			}
		}
		
		static List<LinuxListViewItem> list_LinuxListViewItem = new List<LinuxListViewItem>();
		public static LinuxTreeViewItem cur_LinuxTreeViewItem = null;
		private void OnClickLinuxFileEncrypt(object sender, RoutedEventArgs e)
		{
			ConfirmEncDec(listView_linux_files.SelectedItems.Cast<Object>(), true);
		}
		private void OnClickLinuxFileDecrypt(object sender, RoutedEventArgs e)
		{
			ConfirmEncDec(listView_linux_files.SelectedItems.Cast<Object>(), false);
		}
		
		bool CheckHaveDirectory(IEnumerable<Object> selected_list)
		{
			var enumerator = selected_list.GetEnumerator();
			bool haveDirectory = true;
			for(int i = 0; haveDirectory = enumerator.MoveNext(); i++)
			{
				LinuxTreeViewItem ltvi = enumerator.Current as LinuxTreeViewItem;

				CofileUI.UserControls.Cofile.LinuxListViewItem llvi = enumerator.Current as CofileUI.UserControls.Cofile.LinuxListViewItem;
				if(llvi != null)
					ltvi = llvi.LinuxTVI as LinuxTreeViewItem;

				if(ltvi == null)
					continue;

				if(ltvi.IsDirectory)
					break;
			}

			return haveDirectory;
		}
		string GetFileListString(IEnumerable<Object> selected_list)
		{
			string str = "";
			var enumerator = selected_list.GetEnumerator();
			for(int i = 0; enumerator.MoveNext(); i++)
			{
				LinuxTreeViewItem ltvi = enumerator.Current as LinuxTreeViewItem;

				CofileUI.UserControls.Cofile.LinuxListViewItem llvi = enumerator.Current as CofileUI.UserControls.Cofile.LinuxListViewItem;
				if(llvi != null)
					ltvi = llvi.LinuxTVI as LinuxTreeViewItem;

				if(ltvi == null)
					continue;

				str += ltvi.Path + "\n";
			}
			return str;
		}
		public void ConfirmEncDec(IEnumerable<Object> selected_list, bool isEncrypt)
		{
			string title = "", message = "";
			MessageDialogStyle dialog_style = MessageDialogStyle.AffirmativeAndNegative;
			WindowMain.CallBack affirmative_callback = delegate
			{
				//TextRange txt = new TextRange(Status.current.richTextBox_status.Document.ContentStart, Status.current.richTextBox_status.Document.ContentEnd);
				//txt.Text = "";
				SSHController.SendNRecvCofileCommand(selected_list, isEncrypt, true);
				//LinuxTreeViewItem.Refresh();
			};
			WindowMain.CallBack negative_callback = null;
			MetroDialogSettings settings = null;

			message += GetFileListString(selected_list);
			message += "\n";
			CofileType cofileType = this.GetSelectedType();
			if(cofileType == CofileType.undefined)
				return;

			if(isEncrypt 
				&& cofileType == CofileType.tail 
				&& !CheckHaveDirectory(selected_list))
			{
				SSHController.view_message_caption = "Encrypt";
				title += Resources["String.MainDialog.Encrypt.Title"];
				message += Resources["String.MainDialog.Encrypt.Message.Tail"];

				dialog_style = MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary;
				negative_callback = delegate
				{
					SSHController.SendNRecvCofileCommand(selected_list, isEncrypt, false);
				};
				settings = new MetroDialogSettings()
				{
					AffirmativeButtonText = "Yes"
					, NegativeButtonText = "No"
					, FirstAuxiliaryButtonText = "Cancel"
					//, ColorScheme = MetroDialogOptions.ColorScheme
				};
			}
			else if(isEncrypt)
			{
				SSHController.view_message_caption = "Encrypt";
				title += Resources["String.MainDialog.Encrypt.Title"];
				message += Resources["String.MainDialog.Encrypt.Message"];
			}
			else
			{
				SSHController.view_message_caption = "Decrypt";
				title += Resources["String.MainDialog.Decrypt.Title"];
				message += Resources["String.MainDialog.Decrypt.Message"];
			}
			message += "\n [ Type = " + cofileType.ToString().ToUpper() + " ]";

			WindowMain.current.ShowMessageDialog(title, message, dialog_style, affirmative_callback, negative_callback, settings: settings);
			
		}
	}








	public class HideStringToDoubleConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			if(value == null)
				return null;

			string str = value.ToString();
			if(str[0] == '.')
				return .5;
			return 1;
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			throw new Exception();
		}
	}
}
