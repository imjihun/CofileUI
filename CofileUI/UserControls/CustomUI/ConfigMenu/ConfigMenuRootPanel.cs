using MahApps.Metro.IconPacks;
using CofileUI.Classes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using CofileUI.Windows;
using CofileUI.UserControls.ConfigOptions;

namespace CofileUI.UserControls
{
	/// <summary>
	/// ConfigPanel	-> ConfigMenuButton
	///				-> SubPanel -> ConfigList -> ConfigInfoTextBlock(ConfigInfo)
	/// </summary>
	
	public class ConfigMenuRootPanel : Grid
	{
		public ConfigMenuButton btnFileConfig = null;
		public ConfigMenuButton btnSamConfig = null;
		public ConfigMenuButton btnTailConfig = null;

		public ConfigMenuButton btn_selected = null;
		public List<ConfigMenuButton> btn_group = new List<ConfigMenuButton>();
		
		public Grid SubPanel;
		RelayCommand AddConfigWorkGroupCommand;

		void AddConfigWorkGroup(object parameter)
		{
			Window_AddConfigWorkGroup wms = new Window_AddConfigWorkGroup();
			Point pt = this.PointToScreen(new Point(0, 0));
			wms.Left = pt.X;
			wms.Top = pt.Y;
			if(wms.ShowDialog() == true)
			{
				try
				{
					if(this.btn_selected == null
					|| this.btn_selected.Root?["work_group"] as JObject == null)
						return;

					string work_group_name = wms.textBox_name.Text;
					(this.btn_selected.Root?["work_group"] as JObject)?.Add(new JProperty(work_group_name, new JObject(new JProperty("processes", new JArray()))));
					ConfigOptionManager.SaveOption(this.btn_selected.Root);

					ConfigMenuTreeViewItem ui_config_group = new ConfigMenuTreeViewItem(this.btn_selected, this.btn_selected.Root, work_group_name);
					ui_config_group.IsExpanded = true;
					this.btn_selected.child.Items.Add(ui_config_group);
				}
				catch(Exception ex)
				{
					Log.ErrorIntoUI("config 그룹명이 중복됩니다.\r", "Add Config Group Name", Status.current.richTextBox_status);
					Log.PrintError(ex.Message, "UserControls.ConfigOptions.ConfigPanel.ConfigInfoPanel");
				}
			}
		}

		public ConfigMenuRootPanel()
		{
			this.Background = null;

			SubPanel = new Grid();
			SubPanel.Background = Brushes.White;
			AddConfigWorkGroupCommand = new RelayCommand(AddConfigWorkGroup);
			SubPanel.ContextMenu = new ContextMenu();
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
			SubPanel.ContextMenu.Items.Add(item);
			this.Children.Add(SubPanel);



			btnFileConfig = new ConfigMenuButton(this, null, "File Config");
			this.Children.Add(btnFileConfig);
			this.SubPanel.Children.Add(btnFileConfig.child);

			btnSamConfig = new ConfigMenuButton(this, null, "Sam Config");
			this.Children.Add(btnSamConfig);
			this.SubPanel.Children.Add(btnSamConfig.child);

			btnTailConfig = new ConfigMenuButton(this, null, "Tail Config");
			this.Children.Add(btnTailConfig);
			this.SubPanel.Children.Add(btnTailConfig.child);

			if(this?.btn_group != null && this.btn_group.Count > 0)
				this.btn_group[0].IsChecked = true;
		}

		public int Refresh()
		{
			if(WindowMain.current?.enableConnect?.Name == null
				|| WindowMain.current?.enableConnect?.sshManager?.id == null)
				return -1;

			string local_dir = MainSettings.Path.PathDirConfigFile + WindowMain.current.enableConnect.Name  + "\\" + WindowMain.current.enableConnect.sshManager.id;
			if(WindowMain.current?.enableConnect?.sshManager?.GetConfig(local_dir) == 0)
			{
				string file_json = FileContoller.Read(local_dir + "/file.json");
				if(file_json == null || file_json.Length == 0)
					file_json = Properties.Resources.file_config_default;
				string sam_json = FileContoller.Read(local_dir + "/sam.json");
				if(file_json == null || file_json.Length == 0)
					file_json = Properties.Resources.sam_config_default;
				string tail_json = FileContoller.Read(local_dir + "/tail.json");
				if(file_json == null || file_json.Length == 0)
					file_json = Properties.Resources.tail_config_default;

				try
				{
					btnFileConfig.Root = JObject.Parse(file_json);
					btnSamConfig.Root = JObject.Parse(sam_json);
					btnTailConfig.Root = JObject.Parse(tail_json);
				}
				catch(Exception e)
				{
					Console.WriteLine("JHLIM_DEBUG : ConfigMenu JObject.Parse " + e.Message);
					return -2;
				}
				return 0;
			}
			return -1;
		}
		public void Clear()
		{
			btnFileConfig.Root = null;
			btnSamConfig.Root = null;
			btnTailConfig.Root = null;
		}
	}
}
