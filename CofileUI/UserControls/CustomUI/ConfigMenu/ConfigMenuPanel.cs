using CofileUI.Classes;
using CofileUI.UserControls.ConfigOptions;
using CofileUI.Windows;
using MahApps.Metro.IconPacks;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace CofileUI.UserControls
{
	public class ConfigMenuPanel : StackPanel
	{
		ConfigMenuButton button;
		ConfigMenuTreeView detailView;
		public ConfigMenuTreeView DetailView { get { return detailView; } }

		public string Header { get { return button.Content.ToString(); } set { button.Content = value; } }
		public bool? IsChecked { get { return button.IsChecked; } set { button.IsChecked = value; } }
		public static double HEIGHT { get { return ConfigMenuButton.HEIGHT; } }
		private JObject root = null;
		public JObject Root
		{
			get { return root; }
			set
			{
				root = value;
				button.Root = value;
				RefreshChild();
			}
		}

		void RefreshChild()
		{
			this.detailView?.Items.Clear();

			JObject jobj_work_group_root = this.Root?["work_group"] as JObject;
			if(jobj_work_group_root == null)
				return;

			foreach(var work in jobj_work_group_root.Properties())
			{
				JObject jobj_config_menu = work.Value as JObject;
				if(jobj_config_menu == null)
					continue;

				ConfigMenuTreeViewItem ui_config_group = new ConfigMenuTreeViewItem() {
					TreeRoot = this.DetailView,
					ConfigIdx = new ConfigMenuModel(this.Root) { WorkName = work.Name }
				};
				ui_config_group.IsExpanded = true;
				this.detailView?.Items.Add(ui_config_group);

				JArray jarr_processes = jobj_config_menu?["processes"] as JArray;
				if(jarr_processes == null)
					continue;
				int i = 0;
				foreach(var jprop_config_info in jarr_processes)
				{
					JObject jobj_process_info = jprop_config_info as JObject;
					if(jobj_process_info == null)
						continue;
					string dir = null;
					if(this.Root["type"].ToString() == "file")
						dir = (jobj_process_info["enc_option"] as JObject)?["input_dir"]?.ToString();
					else
						dir = (jobj_process_info["comm_option"] as JObject)?["input_dir"]?.ToString();
					ui_config_group.Items.Add(new ConfigMenuTreeViewItem() {
						TreeRoot = this.DetailView,
						ConfigIdx = new ConfigMenuModel(this.Root) { WorkName = work.Name, ProcessIndex = i.ToString(), }
					});

					i++;
				}
			}
		}

		RelayCommand AddConfigWorkGroupCommand;
		void AddConfigWorkGroup(object parameter)
		{
			if(WindowMain.current?.EnableConnect?.SshManager?.IsConnected != true)
				return;

			Window_AddConfigWorkGroup wms = new Window_AddConfigWorkGroup();
			Point pt = this.PointToScreen(new Point(0, 0));
			wms.Left = pt.X;
			wms.Top = pt.Y;
			if(wms.ShowDialog() == true)
			{
				try
				{
					string work_group_name = wms.textBox_name.Text;
					JObject cloneRoot = this.Root?.DeepClone() as JObject;
					JProperty jprop_new = new JProperty(work_group_name, new JObject(new JProperty("processes", new JArray())));
					if(cloneRoot?["work_group"] as JObject == null)
						return;
					(cloneRoot?["work_group"] as JObject).Add(jprop_new);
					if(ConfigOptionManager.SaveOption(cloneRoot) != 0)
						return;

					(this.Root?["work_group"] as JObject)?.Add(jprop_new);

					ConfigMenuTreeViewItem ui_config_group = new ConfigMenuTreeViewItem()
					{
						TreeRoot = this.DetailView,
						ConfigIdx = new ConfigMenuModel(this.Root) { WorkName = work_group_name }
					};
					ui_config_group.IsExpanded = true;
					this.DetailView?.Items.Add(ui_config_group);
				}
				catch(Exception ex)
				{
					Log.ErrorIntoUI("config 그룹명이 중복됩니다.\r", "Add Config Group Name", Status.current.richTextBox_status);
					Log.PrintError(ex.Message, "UserControls.ConfigOptions.ConfigPanel.ConfigInfoPanel");
					Console.WriteLine("JHLIM_DEBUG : " + ex.Message);
					WindowMain.current?.ShowMessageDialog("Add Config Fail", "그룹명이 중복됩니다.\n" + ex.Message, MahApps.Metro.Controls.Dialogs.MessageDialogStyle.Affirmative);
				}
			}
		}
		public ConfigMenuPanel()
		{
			this.Orientation = Orientation.Vertical;

			button = new ConfigMenuButton();
			this.Children.Add(button);

			detailView = new ConfigMenuTreeView();
			this.Children.Add(detailView);

			AddConfigWorkGroupCommand = new RelayCommand(AddConfigWorkGroup);
			this.ContextMenu = new ContextMenu();
			MenuItem item;

			item = new MenuItem();
			item.Command = AddConfigWorkGroupCommand;
			item.Header = "Add Config Work Group";
			item.Icon = new PackIconMaterial()
			{
				Kind = PackIconMaterialKind.FolderPlus,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			this.ContextMenu.Items.Add(item);
			this.Background = Brushes.White;
		}
	}
}
