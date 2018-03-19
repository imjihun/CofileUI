using CofileUI.Classes;
using CofileUI.Windows;
using CofileUI.UserControls.ConfigOptions;
using MahApps.Metro.IconPacks;
using Newtonsoft.Json.Linq;
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

namespace CofileUI.UserControls
{
	/// <summary>
	/// ConfigMenu.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class ConfigMenu : UserControl
	{
		ConfigPanel configgrid = null;

		public ConfigMenuButton btnFileConfig = null;
		public ConfigMenuButton btnSamConfig = null;
		public ConfigMenuButton btnTailConfig = null;

		public ConfigMenu()
		{
			InitializeComponent();
			try
			{
				configgrid = new ConfigPanel();
				grid.Children.Add(configgrid);

				btnFileConfig = new ConfigMenuButton(configgrid, null, "File Config");
				configgrid.Children.Add(btnFileConfig);
				configgrid.SubPanel.Children.Add(btnFileConfig.child);

				btnSamConfig = new ConfigMenuButton(configgrid, null, "Sam Config");
				configgrid.Children.Add(btnSamConfig);
				configgrid.SubPanel.Children.Add(btnSamConfig.child);

				btnTailConfig = new ConfigMenuButton(configgrid, null, "Tail Config");
				configgrid.Children.Add(btnTailConfig);
				configgrid.SubPanel.Children.Add(btnTailConfig.child);

				Refresh();
			}
			catch(Exception e)
			{
				Log.PrintError(e.Message, "UserControls.ConfigMenu");
			}
			if(configgrid?.btn_group != null && configgrid.btn_group.Count > 0)
				configgrid.btn_group[0].IsChecked = true;
		}

		public void Refresh()
		{
			//string file_json = Properties.Resources.file_config_default;
			//string sam_json = Properties.Resources.sam_config_default;
			//string tail_json = Properties.Resources.tail_config_default;
			//btnFileConfig.Root = JObject.Parse(file_json);
			//btnSamConfig.Root = JObject.Parse(sam_json);
			//btnTailConfig.Root = JObject.Parse(tail_json);

			if(WindowMain.current?.enableConnect?.sshManager?.NewGetConfig(MainSettings.Path.PathDirConfigFile) == 0)
			{
				string file_json = FileContoller.Read(MainSettings.Path.PathDirConfigFile + "/file.json");
				if(file_json == null || file_json.Length == 0)
					file_json = Properties.Resources.file_config_default;
				string sam_json = FileContoller.Read(MainSettings.Path.PathDirConfigFile + "/sam.json");
				if(file_json == null || file_json.Length == 0)
					file_json = Properties.Resources.sam_config_default;
				string tail_json = FileContoller.Read(MainSettings.Path.PathDirConfigFile + "/tail.json");
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
				}
			}
		}
		public void Clear()
		{
			try
			{
				btnFileConfig.Root = null;
				btnSamConfig.Root = null;
				btnTailConfig.Root = null;
			}
			catch(Exception e)
			{
				Console.WriteLine("JHLIM_DEBUG : ConfigMenu JObject.Parse " + e.Message);
			}
		}
	}
}
 