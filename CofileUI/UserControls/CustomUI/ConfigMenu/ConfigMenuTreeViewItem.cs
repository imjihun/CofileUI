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

namespace CofileUI.UserControls
{

	public class ConfigMenuTreeViewItem : TreeViewItem
	{
		public JObject root;
		public string work_name;
		public string index;
		public string path;

		private string cofile_type = "";
		private string str_enc = "";
		private string cofile_enc = "";

		public Grid grid;
		public TextBlock tb_title;
		private TextBlock tb_dir;
		public PackIconModern icon;

		public ConfigMenuButton bnt_parent;

		private void CreateMember()
		{
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

			if(index != null)
			{
				grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
				tb_dir = new TextBlock();
				tb_dir.Foreground = Brushes.Black;
				tb_dir.Margin = new Thickness(0, 0, 5, 0);
				grid.Children.Add(tb_dir);
				Grid.SetColumn(tb_dir, 1);
			}
			this.Header = grid;
		}
		
		RelayCommand DelConfigWorkGroupCommand;
		void DelConfigWorkGroup(object parameter)
		{
			if(this.root == null)
				return;

			WindowMain.current.ShowMessageDialog("Delete Config Work Group", "해당 Config Group 을 정말 삭제하시겠습니까? 하위 Config 정보도 모두 삭제됩니다.", MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegative,
				() =>
				{
					try
					{
						this.root["work_group"]?[work_name]?.Parent?.Remove();
						ConfigOptionManager.SaveOption(this.root);
						int? _cnt = this.bnt_parent?.pan_parent?.btn_selected.child.Items.Count;
						int cnt = _cnt == null ? 0 : _cnt.Value;
						for(int i = 0; i < cnt; i++)
						{
							if(work_name != null
							&& (this.bnt_parent?.pan_parent?.btn_selected.child.Items[i] as ConfigMenuTreeViewItem)?.tb_title.Text == work_name)
							{
								this.bnt_parent?.pan_parent?.btn_selected.child.Items.RemoveAt(i);
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
		RelayCommand DelConfigProcessCommand;
		void DelConfigProcess(object parameter)
		{
			if(this.root == null)
				return;

			WindowMain.current.ShowMessageDialog("Delete Config", "해당 Config 를 정말 삭제하시겠습니까?", MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegative,
				() =>
				{
					try
					{
						this.root["work_group"]?[work_name]?["processes"]?[Int32.Parse(index)]?.Remove();
						ConfigOptionManager.SaveOption(this.root);
						int? _cnt = this.bnt_parent?.pan_parent?.btn_selected.child.Items.Count;
						int cnt = _cnt == null ? 0 : _cnt.Value;
						for(int i = 0; i < cnt; i++)
						{
							if(work_name != null
							&& (this.bnt_parent?.pan_parent?.btn_selected.child.Items[i] as ConfigMenuTreeViewItem)?.tb_title.Text == work_name)
							{
								int? _cnt2 = (this.bnt_parent?.pan_parent?.btn_selected.child.Items[i] as ConfigMenuTreeViewItem)?.Items.Count;
								int cnt2 = _cnt2 == null ? 0 : _cnt2.Value;
								int j;
								for(j = 0; j < cnt2; j++)
								{
									if(index != null
									&& ((this.bnt_parent?.pan_parent?.btn_selected.child.Items[i] as ConfigMenuTreeViewItem)?.Items[j] as ConfigMenuTreeViewItem)?.tb_title.Text == index)
									{
										(this.bnt_parent?.pan_parent?.btn_selected.child.Items[i] as ConfigMenuTreeViewItem)?.Items.RemoveAt(j);
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
		RelayCommand RequestEncryptionCommand;
		void RequestEncryption(object parameter)
		{
			WindowMain.current.ShowMessageDialog(
				this.root["type"] + " " + str_enc,
				"해당항목들을 " + str_enc + " 하시겠습니까?",
				MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegative,
				() =>
				{
					bool? enc_retval = null;
					List<string> list_cofile_opt = parameter as List<string>;
					if(list_cofile_opt != null)
					{
						for(int i = 0; i < list_cofile_opt.Count; i++)
						{
							string command = "cofile " + cofile_type + " " + cofile_enc + " " + list_cofile_opt[i];
							enc_retval = WindowMain.current?.enableConnect?.sshManager?.RunCofileCommand(command);
							if(enc_retval != true)
								break;
						}
					}
					else
					{
						//string config_info = this.work_name;
						//if(this.index != null)
						//	config_info += "_" + this.index;
						string cofile_config = WindowMain.current?.enableConnect?.sshManager?.EnvCoHome + "/var/conf/test/" + cofile_type + ".json";

						string command = "cofile " + cofile_type + " " + cofile_enc + " -c " + cofile_config;
						enc_retval = WindowMain.current?.enableConnect?.sshManager?.RunCofileCommand(command);
					}

					if(enc_retval == true)
					{
						WindowMain.current.ShowMessageDialog(
							this.root["type"] + " " + str_enc,
							str_enc + " 요청을 보냈습니다.",
							MahApps.Metro.Controls.Dialogs.MessageDialogStyle.Affirmative);
					}
					else
					{
						WindowMain.current.ShowMessageDialog(
							this.root["type"] + " " + str_enc,
							str_enc + " 요청에 실패하였습니다.",
							MahApps.Metro.Controls.Dialogs.MessageDialogStyle.Affirmative);
					}
				});
		}

		public ConfigMenuTreeViewItem(ConfigMenuButton _bnt_parent, JObject _root, string _work_name, string _index = null, string _path = null)
		{
			this.bnt_parent = _bnt_parent;
			this.root = _root;
			this.work_name = _work_name;
			this.index = _index;
			this.path = _path;
			CreateMember();

			this.HorizontalAlignment = HorizontalAlignment.Stretch;
			if(_index == null)
				this.tb_title.Text = _work_name;
			else
			{
				this.tb_title.Text = _index;
				this.tb_dir.Text = _path;
			}
			this.AllowDrop = true;

			this.ContextMenu = new ContextMenu();
			MenuItem item;
			if(this.index == null)
			{
				DelConfigWorkGroupCommand = new RelayCommand(DelConfigWorkGroup);
				item = new MenuItem();
				item.Command = DelConfigWorkGroupCommand;
				item.Header = "Del Config Work Group";
				item.Icon = new PackIconMaterial()
				{
					Kind = PackIconMaterialKind.FolderRemove,
					VerticalAlignment = VerticalAlignment.Center,
					HorizontalAlignment = HorizontalAlignment.Center
				};
				this.ContextMenu.Items.Add(item);
			}
			else
			{
				DelConfigProcessCommand = new RelayCommand(DelConfigProcess);
				item = new MenuItem();
				item.Command = DelConfigProcessCommand;
				item.Header = "Del Config";
				item.Icon = new PackIconMaterial()
				{
					Kind = PackIconMaterialKind.FolderRemove,
					VerticalAlignment = VerticalAlignment.Center,
					HorizontalAlignment = HorizontalAlignment.Center
				};
				this.ContextMenu.Items.Add(item);
			}

			cofile_type = this.root["type"].ToString();
			if(WindowMain.current.tabControl.SelectedIndex == 0)
			{
				str_enc = "암호화";
				cofile_enc = "-e";
				RequestEncryptionCommand = new RelayCommand(RequestEncryption);
				item = new MenuItem();
				item.Command = RequestEncryptionCommand;
				item.Header = "Encrypt";
				item.Icon = new PackIconMaterial()
				{
					Kind = PackIconMaterialKind.Lock,
					VerticalAlignment = VerticalAlignment.Center,
					HorizontalAlignment = HorizontalAlignment.Center
				};
				this.ContextMenu.Items.Add(item);
			}
			else if(WindowMain.current.tabControl.SelectedIndex == 1)
			{
				str_enc = "복호화";
				cofile_enc = "-d";
				RequestEncryptionCommand = new RelayCommand(RequestEncryption);
				item = new MenuItem();
				item.Command = RequestEncryptionCommand;
				item.Header = "Decrypt";
				item.Icon = new PackIconMaterial()
				{
					Kind = PackIconMaterialKind.Lock,
					VerticalAlignment = VerticalAlignment.Center,
					HorizontalAlignment = HorizontalAlignment.Center
				};
				this.ContextMenu.Items.Add(item);
			}
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
					if(this.work_name != null && this.index == null && ltvi.IsDirectory)
					{
						JObject jobj;
						if(this.root["type"].ToString() == "file")
							jobj = new JObject(new JProperty("enc_option", new JObject(new JProperty("input_dir", ltvi.Path))));
						else
							jobj = new JObject(new JProperty("comm_option", new JObject(new JProperty("input_dir", ltvi.Path))));
						if(jobj != null)
						{
							JObject tmp_root = this.root.DeepClone() as JObject;
							(tmp_root?["work_group"]?[work_name]?["processes"] as JArray)?.Add(jobj);
							tmp_index = this.Items.Count.ToString();

							Window_Config wc = new Window_Config(tmp_root, this.work_name, tmp_index, true, ltvi.Path );
							Point pt = WindowMain.current.PointToScreen(new Point(WindowMain.current.Width - wc.Width, WindowMain.current.Height - wc.Height));
							wc.Left = pt.X;
							wc.Top = pt.Y;
							retval = wc.ShowDialog();
							if(retval == true)
							{
								(this.root?["work_group"]?[work_name]?["processes"] as JArray)?.Add(jobj);
								ConfigMenuTreeViewItem new_cip = new ConfigMenuTreeViewItem(this.bnt_parent, this.root, this.work_name, tmp_index, ltvi.Path);
								this.Items.Add(new_cip);

								//string cofile_type = this.root["type"].ToString();
								//string config_info = WindowMain.current?.enableConnect?.sshManager?.EnvCoHome + "/var/conf/test/" + cofile_type + ".json";
								//WindowMain.current?.enableConnect?.sshManager?.NewSendNRecvCofileCommand(list_ltvi, true, config_info, cofile_type);
								Console.WriteLine("JHLIM_DEBUG : Encrypt {0} {1} {2} [{3}]", this.root["type"], this.work_name, tmp_index, ltvi.Path);

								//string config_info = new_cip.work_name;
								//if(new_cip.index != null)
								//	config_info += "_" + new_cip.index;
								string config_info = "/home/test/var/conf/test/" + this.root["type"].ToString() + ".json";
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
						string config_info = "/home/test/var/conf/test/" + this.root["type"].ToString() + ".json";
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
				Window_Config wc = new Window_Config(root, work_name, index, path: path);
				Point pt = WindowMain.current.PointToScreen(new Point(WindowMain.current.Width - wc.Width, WindowMain.current.Height - wc.Height));
				wc.Left = pt.X;
				wc.Top = pt.Y;
				wc.Show();
				e.Handled = true;
			}
		}
	}

}
