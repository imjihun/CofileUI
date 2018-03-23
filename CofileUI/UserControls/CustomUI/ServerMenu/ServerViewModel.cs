using CofileUI.Classes;
using CofileUI.Windows;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace CofileUI.UserControls
{
	public class ServerGroupModel : ModelBase
	{
		string groupName;
		public string GroupName { get { return groupName; } set { groupName = value; RaisePropertyChanged("GroupName"); } }
		ObservableCollection<ServerModel> servers = new ObservableCollection<ServerModel>();
		public ObservableCollection<ServerModel> Servers { get { return servers; } set { servers = value; RaisePropertyChanged("Servers"); } }
	}
	public class ServerViewModel : ViewModelBase
	{
		private JObject jobjRoot = new JObject();
		public JObject JobjRoot
		{
			get { return jobjRoot; }
			set
			{
				jobjRoot = value;
			}
		}


		public ObservableCollection<ServerGroupModel> serverGroups = new ObservableCollection<ServerGroupModel>();
		public ObservableCollection<ServerGroupModel> ServerGroups { get { return serverGroups; } set { serverGroups = value;  RaisePropertyChanged("ServerGroups"); } }

		public ServerViewModel()
		{
			// serverinfo.json 파일 로드
			FileInfo fi = new FileInfo(ServerModel.PATH);

			try
			{
				if(fi.Exists)
				{
					string json = FileContoller.Read(ServerModel.PATH);
					this.JobjRoot = JObject.Parse(json);
				}
				else
					this.JobjRoot = new JObject(new JProperty("Server", new JObject()));


				foreach(var jprop_server_group in this.JobjRoot.Properties())
				{
					JObject jobj_server_menu = jprop_server_group.Value as JObject;
					if(jobj_server_menu == null)
						continue;

					ServerGroupModel sgm = new ServerGroupModel() { GroupName = jprop_server_group.Name };
					ServerGroups.Add(sgm);

					foreach(var jprop_server in jobj_server_menu.Properties())
					{
						ServerModel serverinfo = new ServerModel(jprop_server);
						sgm.Servers.Add(serverinfo);
					}
				}
			}
			catch(Exception e)
			{
				Log.PrintError(e.Message, "UserControls.ServerViewModel.ServerViewModel");

			}
		}

		public bool Save()
		{
			if(!FileContoller.Write(ServerModel.PATH, this.JobjRoot.ToString()))
			{
				string caption = "save error";
				string message = "serverinfo.json 파일을 저장하는데 문제가 생겼습니다.";
				Log.PrintLog(message, "UserControls.ServerViewModel.Save");
				WindowMain.current.ShowMessageDialog(caption, message, MahApps.Metro.Controls.Dialogs.MessageDialogStyle.Affirmative);
				return false;
			}
			return true;
		}
		public int AddServerGroup(string _group_name)
		{
			try
			{
				this.JobjRoot?.Add(
					new JProperty(
						_group_name,
						new JObject()
					)
				);
				Save();
			}
			catch(Exception e)
			{
				Log.ErrorIntoUI("서버그룹을 추가하는데 실패하였습니다.\n" + e.Message, "Add Server", Status.current.richTextBox_status);
				Log.PrintError(e.Message, "UserControls.ServerViewModel.AddServerGroup");
				WindowMain.current.ShowMessageDialog(
					"Add Server",
					"서버그룹을 추가하는데 실패하였습니다.\n" + e.Message,
					MahApps.Metro.Controls.Dialogs.MessageDialogStyle.Affirmative);
				return -2;
			}

			return 0;
		}
		public int DeleteServerGroup(string _group_name)
		{
			try
			{
				this.JobjRoot?.Remove(_group_name);
				Save();
			}
			catch(Exception e)
			{
				Log.ErrorIntoUI("서버그룹을 삭제하는데 실패하였습니다.\n" + e.Message, "Delete Server Group", Status.current.richTextBox_status);
				Log.PrintError(e.Message, "UserControls.ServerViewModel.DeleteServerGroup");
				WindowMain.current.ShowMessageDialog(
					"Delete Server Group",
					"서버그룹을 삭제하는데 실패하였습니다.\n" + e.Message,
					MahApps.Metro.Controls.Dialogs.MessageDialogStyle.Affirmative);
				return -2;
			}

			return 0;
		}
		public int AddServer(string _group_name, string _name, string _ip, int _port)
		{
			try
			{
				(this.JobjRoot[_group_name] as JObject)?.Add(
					new JProperty(
						_name,
						new JObject(
							new object[] {
							new JProperty("ip", _ip),
							new JProperty("port", _port)
							}
						)
					)
				);
				Save();
			}
			catch(Exception e)
			{
				Log.ErrorIntoUI("서버를 추가하는데 실패하였습니다.\n" + e.Message, "Add Server", Status.current.richTextBox_status);
				Log.PrintError(e.Message, "UserControls.ServerViewModel.AddServer");
				WindowMain.current.ShowMessageDialog(
					"Add Server",
					"서버를 추가하는데 실패하였습니다.\n" + e.Message,
					MahApps.Metro.Controls.Dialogs.MessageDialogStyle.Affirmative);
				return -2;
			}

			return 0;
		}
		public int DeleteServer(string _group_name, string _name)
		{
			try
			{
				(this.JobjRoot[_group_name] as JObject)?.Remove(_name);
				Save();
			}
			catch(Exception e)
			{
				Log.ErrorIntoUI("서버를 삭제하는데 실패하였습니다.\n" + e.Message, "Delete Server", Status.current.richTextBox_status);
				Log.PrintError(e.Message, "UserControls.ServerViewModel.DeleteServer");
				WindowMain.current.ShowMessageDialog(
					"Delete Server",
					"서버를 삭제하는데 실패하였습니다.\n" + e.Message,
					MahApps.Metro.Controls.Dialogs.MessageDialogStyle.Affirmative);
				return -2;
			}

			return 0;
		}
		public int ModifyServer(ServerModel si, string _group_name, string _name, string _ip, int _port)
		{
			try
			{
				(this.JobjRoot[_group_name]?[si.Name] as JObject)?.Parent?.Replace(
					new JProperty(
						_name,
						new JObject(
							new object[] {
							new JProperty("ip", _ip),
							new JProperty("port", _port)
							}
						)
					)
				);
				Save();
			}
			catch(Exception e)
			{
				Log.ErrorIntoUI("서버정보를 변경하는데 실패하였습니다.\n" + e.Message, "Modify Server", Status.current.richTextBox_status);
				Log.PrintError(e.Message, "UserControls.ServerInfo.ModifyServer");
				WindowMain.current.ShowMessageDialog(
					"Modify Server",
					"서버정보를 변경하는데 실패하였습니다.\n" + e.Message,
					MahApps.Metro.Controls.Dialogs.MessageDialogStyle.Affirmative);
				return -2;
			}

			return 0;
		}
		
	}
}
