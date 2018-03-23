using MahApps.Metro.IconPacks;
using CofileUI.Classes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CofileUI.Windows;
using CofileUI.UserControls.ConfigOptions;
using System.Windows.Data;

namespace CofileUI.UserControls
{
	public class ConfigMenuTreeViewItem : TreeViewItem
	{
		ConfigMenuModel configIdx;
		public ConfigMenuModel ConfigIdx {
			get { return configIdx; }
			set {
				configIdx = value;

				if(configIdx == null)
					return;

				this.tb_dir.SetBinding(TextBlock.TextProperty, new Binding("input_dir") { Source = configIdx?.JPropInputDir?.Parent });
				if(configIdx?.ProcessIndex == null)
				{
					this.tb_title.Text = configIdx?.WorkName;
					item_delGroupConfig.Visibility = Visibility.Visible;
				}
				else
				{
					this.tb_title.Text = configIdx?.ProcessIndex;
					tb_dir.Visibility = Visibility.Visible;
					item_delConfig.Visibility = Visibility.Visible;
				}

				if(WindowMain.current.tabControl.SelectedIndex == 0)
					item_encrypt.Visibility = Visibility.Visible;
				else if(WindowMain.current.tabControl.SelectedIndex == 1)
					item_decrypt.Visibility = Visibility.Visible;
			}
		}
		public string WorkName { get { return this.ConfigIdx?.WorkName; } }
		public string ProcessIndex { get { return this.ConfigIdx?.ProcessIndex; } }
		ConfigMenuTreeView treeRoot;
		public ConfigMenuTreeView TreeRoot { get { return treeRoot; } set { treeRoot = value; } }
		
		private string strEnc = "";
		private string cofileEnc = "";

		private Grid grid;
		private TextBlock tb_title;
		private TextBlock tb_dir;
		private PackIconModern icon;
		private MenuItem item_delGroupConfig;
		private MenuItem item_delConfig;
		private MenuItem item_encrypt;
		private MenuItem item_decrypt;

		RelayCommand DelConfigWorkGroupCommand;
		RelayCommand DelConfigProcessCommand;
		RelayCommand RequestEncryptionCommand;

		void DelConfigWorkGroup(object parameter)
		{
			if(this.ConfigIdx?.JobjRoot == null)
				return;

			WindowMain.current.ShowMessageDialog("Delete Config Work Group", "해당 Config Group 을 정말 삭제하시겠습니까? 하위 Config 정보도 모두 삭제됩니다.", MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegative,
				() =>
				{
					try
					{
						JObject cloneRoot = this.ConfigIdx?.JobjRoot?.DeepClone() as JObject;
						cloneRoot["work_group"]?[WorkName]?.Parent?.Remove();
						if(ConfigOptionManager.SaveOption(cloneRoot) != 0)
							return;

						this.ConfigIdx?.JobjRoot["work_group"]?[WorkName]?.Parent?.Remove();

						int? _cnt = this.TreeRoot?.Items.Count;
						int cnt = _cnt == null ? 0 : _cnt.Value;
						for(int i = 0; i < cnt; i++)
						{
							if(WorkName != null
							&& (this.TreeRoot.Items[i] as ConfigMenuTreeViewItem)?.tb_title.Text == WorkName)
							{
								this.TreeRoot.Items.RemoveAt(i);
								break;
							}
						}
					}
					catch(Exception ex)
					{
						Log.ErrorIntoUI(ex.Message, "Del Config Menu", Status.current.richTextBox_status);
						Log.PrintError(ex.Message, "UserControls.ConfigMenuButton.DeleteConfigMenuUI");
					}
				});
		}
		void DelConfigProcess(object parameter)
		{
			if(this.ConfigIdx?.JobjRoot == null)
				return;

			WindowMain.current.ShowMessageDialog("Delete Config", "해당 Config 를 정말 삭제하시겠습니까?", MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegative,
				() =>
				{
					try
					{
						JObject cloneRoot = this.ConfigIdx?.JobjRoot?.DeepClone() as JObject;
						cloneRoot["work_group"]?[WorkName]?["processes"]?[Int32.Parse(ProcessIndex)]?.Remove();
						if(ConfigOptionManager.SaveOption(cloneRoot) != 0)
							return;

						this.ConfigIdx?.JobjRoot["work_group"]?[WorkName]?["processes"]?[Int32.Parse(ProcessIndex)]?.Remove();

						int? _cnt = this.TreeRoot.Items.Count;
						int cnt = _cnt == null ? 0 : _cnt.Value;
						for(int i = 0; i < cnt; i++)
						{
							if(WorkName != null
							&& (this.TreeRoot.Items[i] as ConfigMenuTreeViewItem)?.tb_title.Text == WorkName)
							{
								int? _cnt2 = (this.TreeRoot.Items[i] as ConfigMenuTreeViewItem)?.Items.Count;
								int cnt2 = _cnt2 == null ? 0 : _cnt2.Value;
								int j;
								for(j = 0; j < cnt2; j++)
								{
									if(ProcessIndex != null
									&& ((this.TreeRoot.Items[i] as ConfigMenuTreeViewItem)?.Items[j] as ConfigMenuTreeViewItem)?.tb_title.Text == ProcessIndex)
									{
										(this.TreeRoot.Items[i] as ConfigMenuTreeViewItem)?.Items.RemoveAt(j);
										break;
									}
								}
								if(j != cnt2)
									break;
							}
						}
					}
					catch(Exception ex)
					{
						Log.ErrorIntoUI(ex.Message, "Del Config Menu", Status.current.richTextBox_status);
						Log.PrintError(ex.Message, "UserControls.ConfigMenuButton.DeleteConfigMenuUI");
					}
				});
		}
		void RequestEncryption(object parameter)
		{
			WindowMain.current.ShowMessageDialog(
				this.ConfigIdx?.Type + " " + strEnc,
				"해당항목들을 " + strEnc + " 하시겠습니까?",
				MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegative,
				() =>
				{
					bool? enc_retval = null;
					List<string> list_cofile_opt = parameter as List<string>;
					if(list_cofile_opt != null)
					{
						for(int i = 0; i < list_cofile_opt.Count; i++)
						{
							string command = "cofile " + this.ConfigIdx?.Type + " " + cofileEnc + " " + list_cofile_opt[i];
							enc_retval = WindowMain.current?.EnableConnect?.SshManager?.RunCofileCommand(command);
							if(enc_retval != true)
								break;
						}
					}
					else
					{
						//string config_info = this.work_name;
						//if(this.index != null)
						//	config_info += "_" + this.index;
						string cofile_config = WindowMain.current?.EnableConnect?.SshManager?.EnvCoHome + "/var/conf/test/" + this.ConfigIdx?.Type + ".json";

						string command = "cofile " + this.ConfigIdx?.Type + " " + cofileEnc + " -c " + cofile_config;
						enc_retval = WindowMain.current?.EnableConnect?.SshManager?.RunCofileCommand(command);
					}

					if(enc_retval == true)
					{
						WindowMain.current.ShowMessageDialog(
							this.ConfigIdx?.Type + " " + strEnc,
							strEnc + " 요청을 보냈습니다.",
							MahApps.Metro.Controls.Dialogs.MessageDialogStyle.Affirmative);
					}
					else
					{
						WindowMain.current.ShowMessageDialog(
							this.ConfigIdx?.Type + " " + strEnc,
							strEnc + " 요청에 실패하였습니다.",
							MahApps.Metro.Controls.Dialogs.MessageDialogStyle.Affirmative);
					}
				});
		}

		public ConfigMenuTreeViewItem()
		{
			this.HorizontalAlignment = HorizontalAlignment.Stretch;

			grid = new Grid();
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
			DockPanel dp = new DockPanel();
			grid.Children.Add(dp);
			Grid.SetColumn(dp, 0);

			icon = new PackIconModern()
			{
				Kind = PackIconModernKind.Connect,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			icon.Margin = new Thickness(2, 0, 3, 0);
			icon.Visibility = Visibility.Hidden;
			dp.Children.Add(icon);

			tb_title = new TextBlock();
			tb_title.Foreground = Brushes.Black;
			dp.Children.Add(tb_title);

			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
			tb_dir = new TextBlock();
			tb_dir.Foreground = Brushes.Black;
			tb_dir.Margin = new Thickness(0, 0, 5, 0);
			tb_dir.Visibility = Visibility.Collapsed;
			grid.Children.Add(tb_dir);
			Grid.SetColumn(tb_dir, 1);

			this.Header = grid;
			this.AllowDrop = true;

			this.ContextMenu = new ContextMenu();

			DelConfigWorkGroupCommand = new RelayCommand(DelConfigWorkGroup);
			DelConfigProcessCommand = new RelayCommand(DelConfigProcess);
			RequestEncryptionCommand = new RelayCommand(RequestEncryption);

			item_delGroupConfig = new MenuItem();
			item_delGroupConfig.Command = DelConfigWorkGroupCommand;
			item_delGroupConfig.Header = "Del Config Work Group";
			item_delGroupConfig.Icon = new PackIconMaterial()
			{
				Kind = PackIconMaterialKind.FolderRemove,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			item_delGroupConfig.Visibility = Visibility.Collapsed;
			this.ContextMenu.Items.Add(item_delGroupConfig);

			item_delConfig = new MenuItem();
			item_delConfig.Command = DelConfigProcessCommand;
			item_delConfig.Header = "Del Config";
			item_delConfig.Icon = new PackIconMaterial()
			{
				Kind = PackIconMaterialKind.FolderRemove,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			item_delConfig.Visibility = Visibility.Collapsed;
			this.ContextMenu.Items.Add(item_delConfig);
			
			strEnc = "암호화";
			cofileEnc = "-e";
			item_encrypt = new MenuItem();
			item_encrypt.Command = RequestEncryptionCommand;
			item_encrypt.Header = "Encrypt";
			item_encrypt.Icon = new PackIconMaterial()
			{
				Kind = PackIconMaterialKind.Lock,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			item_encrypt.Visibility = Visibility.Collapsed;
			this.ContextMenu.Items.Add(item_encrypt);

			strEnc = "복호화";
			cofileEnc = "-d";
			item_decrypt = new MenuItem();
			item_decrypt.Command = RequestEncryptionCommand;
			item_decrypt.Header = "Decrypt";
			item_decrypt.Icon = new PackIconMaterial()
			{
				Kind = PackIconMaterialKind.Lock,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			item_decrypt.Visibility = Visibility.Collapsed;
			this.ContextMenu.Items.Add(item_decrypt);
		}

		protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseRightButtonDown(e);
			e.Handled = true;
			this.IsSelected = true;
			//if(this.Parent as ConfigList != null)
			//	this.IsSelected = true;
		}
		protected override void OnDragOver(DragEventArgs e)
		{
			base.OnDragOver(e);
			//var par = this.Parent as ConfigList;
			//if(par != null)
			//{
			//	this.Focus();
			//}
			this.Focus();
			e.Handled = true;
		}
		protected override void OnDrop(DragEventArgs e)
		{
			base.OnDrop(e);
			if(this.ConfigIdx?.JobjRoot == null)
				return;

			Object data_obj = (Object)e.Data.GetData("Object");
			List<LinuxTreeViewItem> list_ltvi = data_obj as List<LinuxTreeViewItem>;
			if(list_ltvi != null)
			{
				List<string> list_cofile_opt = new List<string>();
				int i;
				for(i = 0; i < list_ltvi.Count; i++)
				{
					LinuxTreeViewItem ltvi = list_ltvi[i];

					string tmp_index = "";
					bool? retval = false;
					if(this.WorkName != null && this.ProcessIndex == null && ltvi.IsDirectory)
					{
						JObject jobj;
						if(this.ConfigIdx?.Type == "file")
							jobj = new JObject(new JProperty("enc_option", new JObject(new JProperty("input_dir", ltvi.Path))));
						else
							jobj = new JObject(new JProperty("comm_option", new JObject(new JProperty("input_dir", ltvi.Path))));
						if(jobj != null)
						{
							JObject tmp_root = this.ConfigIdx?.JobjRoot.DeepClone() as JObject;
							(tmp_root?["work_group"]?[WorkName]?["processes"] as JArray)?.Add(jobj);
							tmp_index = this.Items.Count.ToString();

							Window_Config wc = new Window_Config(tmp_root, this.WorkName, tmp_index, true, ltvi.Path );
							Point pt = WindowMain.current.PointToScreen(new Point(WindowMain.current.Width - wc.Width, WindowMain.current.Height - wc.Height));
							wc.Left = pt.X;
							wc.Top = pt.Y;
							retval = wc.ShowDialog();
							if(retval == true)
							{
								(this.ConfigIdx?.JobjRoot["work_group"]?[WorkName]?["processes"] as JArray)?.Add(jobj);
								ConfigMenuTreeViewItem new_cip = new ConfigMenuTreeViewItem() {
									TreeRoot = this.TreeRoot,
									ConfigIdx = new ConfigMenuModel(this.ConfigIdx?.JobjRoot) { WorkName = this.WorkName, ProcessIndex = tmp_index }
								};
								this.Items.Add(new_cip);

								//string cofile_type = this.root["type"].ToString();
								//string config_info = WindowMain.current?.enableConnect?.sshManager?.EnvCoHome + "/var/conf/test/" + cofile_type + ".json";
								//WindowMain.current?.enableConnect?.sshManager?.NewSendNRecvCofileCommand(list_ltvi, true, config_info, cofile_type);
								Console.WriteLine("JHLIM_DEBUG : Encrypt {0} {1} {2} [{3}]", this.ConfigIdx?.Type, this.WorkName, tmp_index, ltvi.Path);

								//string config_info = new_cip.work_name;
								//if(new_cip.index != null)
								//	config_info += "_" + new_cip.index;
								string config_info = "/home/test/var/conf/test/" + this.ConfigIdx?.Type + ".json";
								list_cofile_opt.Add("-c " + config_info);
							}
							else
								break;
						}
					}
					else
					{
						//string config_info = this.work_name;
						//if(this.index != null)
						//	config_info += "_" + this.index;
						string config_info = "/home/test/var/conf/test/" + this.ConfigIdx?.Type + ".json";
						if(ltvi.IsDirectory)
						{
							list_cofile_opt.Add("-c " + config_info + " -id " + ltvi.Path + " -od " + ltvi.Path);
						}
						else
						{
							string dir = ltvi.Path.Substring(0, ltvi.Path.Length - ltvi.FileName.Length);
							list_cofile_opt.Add("-c " + config_info + " -id " + dir + " -od " + dir + " -f " + ltvi.FileName);
						}
					}
				}
				if(i == list_ltvi.Count)
				{
					RequestEncryption(list_cofile_opt);
				}
			}
			e.Handled = true;
		}
		protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
		{
			base.OnMouseDoubleClick(e);
			if(this.IsSelected && e.ChangedButton == MouseButton.Left)
			{
				Window_Config wc = new Window_Config(this.ConfigIdx?.JobjRoot, this.WorkName, this.ProcessIndex, path: this.ConfigIdx?.Path);
				Point pt = WindowMain.current.PointToScreen(new Point(WindowMain.current.Width - wc.Width, WindowMain.current.Height - wc.Height));
				wc.Left = pt.X;
				wc.Top = pt.Y;
				wc.Show();
				e.Handled = true;
			}
		}
	}

}
