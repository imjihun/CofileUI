using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CofileUI.UserControls.CustomUI.ConfigMenu
{
	class ConfigMenuModel : INotifyPropertyChanged
	{
		JObject configRoot;
		public JObject ConfigRoot { get { return configRoot; } set { configRoot = value; RaisePropertyChanged("ConfigRoot"); } }

		string workGroupName;
		public string WorkGroupName { get { return workGroupName; } set { workGroupName = value; RaisePropertyChanged("WorkGroupName"); } }

		string processIndex;
		public string ProcessIndex { get { return processIndex; } set { processIndex = value; RaisePropertyChanged("ProcessIndex"); } }

		private ObservableCollection<ConfigMenuModel> children = new ObservableCollection<ConfigMenuModel>();
		public ObservableCollection<ConfigMenuModel> Children { get { return children; } set { children = value; RaisePropertyChanged("Children"); } }

		public ConfigMenuModel(JObject _configRoot, string _workGroupName = null, string _processIndex = null)
		{
			ConfigRoot = _configRoot;
			WorkGroupName = _workGroupName;
			ProcessIndex = _processIndex;

			if(WorkGroupName == null)
			{
				JObject jobj_work_group_root = ConfigRoot["work_group"] as JObject;
				if(jobj_work_group_root != null)
				{
					foreach(JProperty jprop_work_group in jobj_work_group_root.Properties())
					{
						this.Children.Add(new ConfigMenuModel(ConfigRoot, jprop_work_group.Name));
					}
				}
			}
			else if(ProcessIndex == null)
			{
				JArray jarr_processes_root = ConfigRoot["work_group"]?["processes"] as JArray;
				if(jarr_processes_root != null)
				{
					int process_index = 0;
					foreach(JObject jobj_process in jarr_processes_root)
					{
						this.Children.Add(new ConfigMenuModel(ConfigRoot, WorkGroupName, process_index++.ToString()));
					}
				}
			}
		}

		void RaisePropertyChanged(string prop)
		{
			if(PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }
		}
		public event PropertyChangedEventHandler PropertyChanged;
	}
}
