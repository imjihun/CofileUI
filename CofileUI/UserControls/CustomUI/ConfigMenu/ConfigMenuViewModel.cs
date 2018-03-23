using CofileUI.Classes;
using CofileUI.Windows;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace CofileUI.UserControls
{
	class ConfigMenuViewModel : ViewModelBase
	{
		JObject jobjFileRoot;
		public JObject JobjFileRoot { get { return jobjFileRoot; } set { jobjFileRoot = value; RaisePropertyChanged("JobjFileRoot"); } }
		JObject jobjSamRoot;
		public JObject JobjSamRoot { get { return jobjSamRoot; } set { jobjSamRoot = value; RaisePropertyChanged("JobjSamRoot"); } }
		JObject jobjTailRoot;
		public JObject JobjTailRoot { get { return jobjTailRoot; } set { jobjTailRoot = value; RaisePropertyChanged("JobjTailRoot"); } }

		//ObservableCollection<ConfigMenuModel> fileConfig = new ObservableCollection<ConfigMenuModel>();
		//public ObservableCollection<ConfigMenuModel> FileConfig { get { return fileConfig; } set { fileConfig = value; RaisePropertyChanged("FileConfig"); } }
		//ObservableCollection<ConfigMenuModel> samConfig = new ObservableCollection<ConfigMenuModel>();
		//public ObservableCollection<ConfigMenuModel> SamConfig { get { return samConfig; } set { samConfig = value; RaisePropertyChanged("SamConfig"); } }
		//ObservableCollection<ConfigMenuModel> tailConfig = new ObservableCollection<ConfigMenuModel>();
		//public ObservableCollection<ConfigMenuModel> TailConfig { get { return tailConfig; } set { tailConfig = value; RaisePropertyChanged("TailConfig"); } }

		public ConfigMenuViewModel()
		{
			Refresh();
		}
		public void Refresh()
		{
			if(WindowMain.current?.EnableConnect?.SshManager == null)
				return;

			string local_dir = MainSettings.Path.PathDirConfigFile + "\\" + WindowMain.current.EnableConnect.Name + "\\" + WindowMain.current.EnableConnect.Id;
			WindowMain.current.EnableConnect.SshManager.GetConfig(local_dir);

			string file_json = FileContoller.Read(local_dir + "\\" + "file.json");
			string sam_json = FileContoller.Read(local_dir + "\\" + "sam.json");
			string tail_json = FileContoller.Read(local_dir + "\\" + "tail.json");

			jobjFileRoot = JObject.Parse(file_json);
			jobjSamRoot = JObject.Parse(sam_json);
			jobjTailRoot = JObject.Parse(tail_json);

			//JObject jobj_work_group_root = jobjFileRoot["work_group"] as JObject;
			//if(jobj_work_group_root != null)
			//{
			//	foreach(JProperty jprop_work_group in jobj_work_group_root.Properties())
			//	{
			//		this.FileConfig.Add(new ConfigMenuModel(jobjFileRoot, jprop_work_group.Name));
			//	}
			//}
		}
	}
}
