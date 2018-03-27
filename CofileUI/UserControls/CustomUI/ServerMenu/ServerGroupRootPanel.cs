using MahApps.Metro;
using MahApps.Metro.IconPacks;
using CofileUI.Classes;
using CofileUI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using CofileUI.Windows;
using System.Windows.Data;

namespace CofileUI.UserControls
{
	public class ServerGroupRootPanel : Grid
	{
		public ServerViewModel serverViewModel;
		public ServerViewModel ServerViewModel {
			get { return serverViewModel; }
			set {
				serverViewModel = value;
				if(this.serverViewModel?.JobjRoot == null)
					return;

				try
				{
					foreach(var jprop_server_group in this.serverViewModel.JobjRoot.Properties())
					{
						JObject jobj_server_menu = jprop_server_group.Value as JObject;
						if(jobj_server_menu == null)
							continue;

						ServerGroupPanel sgp = new ServerGroupPanel() { Content = jprop_server_group.Name };
						this.AddChild(sgp);

						foreach(var jprop_server in jobj_server_menu.Properties())
						{
							ServerModel serverinfo = new ServerModel(jprop_server);
							sgp.slb.Items.Add(new ServerListBoxItem() { ServerModel = serverinfo });
						}
					}
				}
				catch(Exception e)
				{
					Log.PrintError(e.Message, "UserControls.ServerViewModel.RefreshUI");
				}
			}
		}

		public ServerGroupRootPanel()
		{
			this.Background = null;

			ServerViewModel = new ServerViewModel();

			if(this.Children.Count > 0
				&& this.Children[0] is ServerGroupPanel)
				(this.Children[0] as ServerGroupPanel).IsChecked = true;
		}

		public void AddChild(ServerGroupPanel sgp)
		{
			this.Children.Add(sgp);
			for(int i = 0; i < this.Children.Count; i++)
			{
				ServerGroupPanel _sgp = this.Children[i] as ServerGroupPanel;
				if(_sgp != null)
					_sgp.Margin = new Thickness(0, i * ConfigMenuPanel.HEIGHT, 0, (this.Children.Count - (i + 1)) * ConfigMenuPanel.HEIGHT);
			}
		}
		public void DeleteChild(ServerGroupPanel sgp)
		{
			this.Children.Remove(sgp);
			for(int i = 0; i < this.Children.Count; i++)
			{
				ServerGroupPanel _sgp = this.Children[i] as ServerGroupPanel;
				if(_sgp != null)
					_sgp.Margin = new Thickness(0, i * ConfigMenuPanel.HEIGHT, 0, (this.Children.Count - (i + 1)) * ConfigMenuPanel.HEIGHT);
			}
		}
	}
}
