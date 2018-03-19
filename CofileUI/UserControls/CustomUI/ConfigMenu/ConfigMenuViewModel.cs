using CofileUI.Classes;
using CofileUI.Windows;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace CofileUI.UserControls.CustomUI.ConfigMenu
{
	class ConfigMenuViewModel : ViewModelBase
	{
		ConfigMenuModel fileConfig;
		public ConfigMenuModel FileConfig { get { return fileConfig; } set { fileConfig = value; RaisePropertyChanged("FileConfig"); } }

		ConfigMenuModel samConfig;
		public ConfigMenuModel SamConfig { get { return samConfig; } set { samConfig = value; RaisePropertyChanged("SamConfig"); } }

		ConfigMenuModel tailConfig;
		public ConfigMenuModel TailConfig { get { return tailConfig; } set { tailConfig = value; RaisePropertyChanged("TailConfig"); } }

		public ConfigMenuViewModel()
		{
			string local_dir = MainSettings.Path.PathDirConfigFile + "\\" + WindowMain.current.enableConnect.sshManager.name + "\\" + WindowMain.current.enableConnect.sshManager.id;
			WindowMain.current.enableConnect.sshManager.GetConfig(local_dir);

			string file_json = FileContoller.Read(local_dir + "\\" + "file.json");
			string sam_json = FileContoller.Read(local_dir + "\\" + "sam.json");
			string tail_json = FileContoller.Read(local_dir + "\\" + "tail.json");

			JObject obj_file_root = JObject.Parse(file_json);
			JObject obj_sam_root = JObject.Parse(sam_json);
			JObject obj_tail_root = JObject.Parse(tail_json);

			FileConfig = new ConfigMenuModel(obj_file_root);
			SamConfig = new ConfigMenuModel(obj_sam_root);
			TailConfig = new ConfigMenuModel(obj_tail_root);
		}
	}
}
