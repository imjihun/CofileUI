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
		private ConfigMenuPanel panFileConfig = null;
		private ConfigMenuPanel panSamConfig = null;
		private ConfigMenuPanel panTailConfig = null;

		public ConfigMenuRootPanel()
		{
			this.Background = null;

			panFileConfig = new ConfigMenuPanel() { Header = "File Config" };
			this.Children.Add(panFileConfig);

			panSamConfig = new ConfigMenuPanel() { Header = "Sam Config" };
			this.Children.Add(panSamConfig);

			panTailConfig = new ConfigMenuPanel() { Header = "Tail Config" };
			this.Children.Add(panTailConfig);

			for(int i = 0; i < this.Children.Count; i++)
			{
				ConfigMenuPanel btn = this.Children[i] as ConfigMenuPanel;
				if(btn != null)
					btn.Margin = new Thickness(0, i * ConfigMenuPanel.HEIGHT, 0, (this.Children.Count - (i + 1)) * ConfigMenuPanel.HEIGHT);
			}

			if(this?.Children != null && this.Children.Count > 0
				&& (this.Children[0] as ConfigMenuPanel) != null)
				(this.Children[0] as ConfigMenuPanel).IsChecked = true;

			//this.IsVisibleChanged += (sender, e) =>
			//{
			//	if(this.IsVisible)
			//		Refresh();
			//};
		}

		public int Refresh()
		{
			if(WindowMain.current?.EnableConnect?.Name == null
				|| WindowMain.current?.EnableConnect?.Id == null)
				return -1;

			string local_dir = MainSettings.Path.PathDirConfigFile + WindowMain.current.EnableConnect.Name  + "\\" + WindowMain.current.EnableConnect.Id;
			
			if(WindowMain.current?.EnableConnect?.SshManager?.GetConfig(local_dir) != 0)
			{
				FileContoller.Write(local_dir + "/file.json", Properties.Resources.file_config_default);
				FileContoller.Write(local_dir + "/sam.json", Properties.Resources.file_config_default);
				FileContoller.Write(local_dir + "/tail.json", Properties.Resources.file_config_default);
			}
			string file_json = FileContoller.Read(local_dir + "/file.json");
			string sam_json = FileContoller.Read(local_dir + "/sam.json");
			string tail_json = FileContoller.Read(local_dir + "/tail.json");

			if(file_json == null || file_json.Length == 0
				|| sam_json == null || sam_json.Length == 0
				|| tail_json == null || tail_json.Length == 0)
				return -3;

			try
			{
				if(file_json != null && file_json.Length > 0)
					panFileConfig.Root = JObject.Parse(file_json);
				if(sam_json != null && sam_json.Length > 0)
					panSamConfig.Root = JObject.Parse(sam_json);
				if(tail_json != null && tail_json.Length > 0)
					panTailConfig.Root = JObject.Parse(tail_json);
			}
			catch(Exception e)
			{
				Console.WriteLine("JHLIM_DEBUG : ConfigMenu JObject.Parse " + e.Message);
				return -2;
			}
			return 0;
		}
		public void Clear()
		{
			panFileConfig.Root = null;
			panSamConfig.Root = null;
			panTailConfig.Root = null;
		}
	}
}
