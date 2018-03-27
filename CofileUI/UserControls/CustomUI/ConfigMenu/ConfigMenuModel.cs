using CofileUI.Classes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CofileUI.UserControls
{
	public class ConfigMenuModel : ModelBase
	{
		JObject jobjRoot;
		public JObject JobjRoot { get { return jobjRoot; } }
		public string Type { get { return this.jobjRoot?["type"]?.ToString(); } }
		public JProperty JPropInputDir
		{
			get
			{
				if(WorkName != null && ProcessIndex != null)
				{
					if(this.Type == "file")
						return (this.JobjRoot["work_group"]?[WorkName]?["processes"]?[Int32.Parse(ProcessIndex)]?["enc_option"] as JObject)?["input_dir"]?.Parent as JProperty;
					else if(this.Type == "sam" || this.Type == "tail")
						return (this.JobjRoot["work_group"]?[WorkName]?["processes"]?[Int32.Parse(ProcessIndex)]?["comm_option"] as JObject)?["input_dir"]?.Parent as JProperty;
				}
				return null;
			}
		}
		public string Path
		{
			get
			{
				return JPropInputDir?.Value?.ToString();
			}
		}

		string workName;
		public string WorkName { get { return workName; } set { workName = value; RaisePropertyChanged("WorkName"); } }
		string processIndex;
		public string ProcessIndex { get { return processIndex; } set { processIndex = value; RaisePropertyChanged("ProcessIndex"); } }

		public ConfigMenuModel(JObject _root)
		{
			jobjRoot = _root;
		}
	}
}
