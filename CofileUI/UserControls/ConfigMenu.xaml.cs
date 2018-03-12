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
		LinuxTreeViewItem root = null;
		ConfigPanel servergrid = null;

		public ConfigMenu()
		{
			InitializeComponent();
			root = (DataContext as LinuxTreeViewItem);
			InitConfigTab();
		}
		#region Config Menu Class
		public ConfigPanel ConvertFromJson(JObject jobj_root)
		{
			if(jobj_root == null)
				return null;

			servergrid = new ConfigPanel();
			try
			{
				foreach(var v in jobj_root.Properties())
				{
					JObject jobj_config_root = v.Value as JObject;
					if(jobj_config_root == null)
						continue;

					ConfigMenuButton smbtn = new ConfigMenuButton(servergrid, jobj_config_root, v.Name);
					servergrid.Children.Add(smbtn);
					servergrid.SubPanel.Children.Add(smbtn.child);

					JObject jobj_work_group_root = jobj_config_root.GetValue("work_group") as JObject;
					if(jobj_work_group_root == null)
						continue;

					foreach(var work in jobj_work_group_root.Properties())
					{
						JObject jobj_server_menu = work.Value as JObject;
						if(jobj_server_menu == null)
							continue;

						ConfigInfoPanel ui_config_group = new ConfigInfoPanel(smbtn, jobj_config_root, work.Name);
						ui_config_group.IsExpanded = true;
						smbtn.child.Items.Add(ui_config_group);

						JArray jarr_processes = jobj_server_menu.GetValue("processes") as JArray;
						if(jarr_processes == null)
							continue;
						int i = 0;
						foreach(var jprop_server_info in jarr_processes)
						{
							JObject jobj_process_info = jprop_server_info as JObject;
							if(jobj_process_info == null)
								continue;
							string dir = null;
							string daemon_yn = null;
							if(jobj_config_root.GetValue("type").ToString() == "file")
								dir = (jobj_process_info.GetValue("enc_option") as JObject)?.GetValue("input_dir")?.ToString();
							else
								dir = (jobj_process_info.GetValue("comm_option") as JObject)?.GetValue("input_dir")?.ToString();
							ui_config_group.Items.Add(new ConfigInfoPanel(smbtn, jobj_config_root, work.Name, i.ToString(), dir));

							string daemon_keyword = "dir_monitoring_yn";
							if(jobj_config_root.GetValue("type").ToString() == "tail")
								daemon_keyword = "daemon_yn";

							JToken jcur = jobj_process_info;
							while(jcur != null
								&& daemon_yn == null)
							{
								daemon_yn = (jcur["comm_option"] as JObject)?.GetValue(daemon_keyword)?.ToString();
								jcur = jcur.Parent;
								while(jcur != null
									&& jcur as JObject == null)
									jcur = jcur.Parent;
							}

							if(daemon_yn == "True")
							{
								LinuxTreeViewItem.ChangeColor(root, dir, jobj_config_root["type"] + "-" + work.Name + "-" + i);
							}
							i++;
						}
					}
				}
				return servergrid;
			}
			catch(Exception e)
			{
				Log.PrintError(e.Message, "UserControls.ServerInfo.ConvertFromJson");
			}
			return null;
		}
		void InitConfigTab()
		{
			try
			{
				//ConfigInfo.jobj_root = JObject.Parse(json);
				JObject root = JObject.Parse("{ \"File Config\" : " + Properties.Resources.file_config_default + ", \"Sam Config\" : " + Properties.Resources.sam_config_default + ", \"Tail Config\" : " + Properties.Resources.tail_config_default + " }");
				ConfigPanel panel_server = ConvertFromJson(root);

				grid.Children.Add(panel_server);
			}
			catch(Exception e)
			{
				Log.PrintError(e.Message, "UserControls.ConfigMenu.InitConfigtab");
			}
			if(servergrid.btn_group.Count > 0)
				servergrid.btn_group[0].IsChecked = true;
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);
			Console.WriteLine("JHLIM_DEBUG : " + servergrid.btn_group[0]?.Root["work_group"]?["test3"]);

			JObject root = JObject.Parse("{ \"File Config\" : " + servergrid.btn_group[0].Root + ", \"Sam Config\" : " + servergrid.btn_group[1].Root + ", \"Tail Config\" : " + servergrid.btn_group[2].Root + " }");
			servergrid.btn_group.Clear();
			ConfigPanel panel_server = ConvertFromJson(root);

			grid.Children.Clear();
			grid.Children.Add(panel_server);
			if(servergrid.btn_group.Count > 0)
				servergrid.btn_group[0].IsChecked = true;
		}

		#endregion
	}
}
