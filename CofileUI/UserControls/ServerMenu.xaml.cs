using CofileUI.Classes;
using CofileUI.Windows;
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
	/// ServerMenu.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class ServerMenu : UserControl
	{
		ServerPanel panel_server;
		public ServerMenu()
		{
			InitializeComponent();
			InitServerTab();
			InitContextMenu();
		}
		#region Server Menu Class
		void InitServerTab()
		{
			// serverinfo.json 파일 로드
			FileInfo fi = new FileInfo(ServerInfo.PATH);
			if(fi.Exists)
			{
				string json = FileContoller.Read(ServerInfo.PATH);
				try
				{
					ServerInfo.jobj_root = JObject.Parse(json);
					panel_server = ServerInfo.ConvertFromJson(ServerInfo.jobj_root);

					grid_server.Children.Add(panel_server);
				}
				catch(Exception e)
				{
					Log.PrintError(e.Message, "UserControls.ServerMenu.InitServertab");
				}
			}
			else
			{
				try
				{
					ServerInfo.jobj_root = new JObject(new JProperty("Server", new JObject()));
					ServerPanel panel_server = ServerInfo.ConvertFromJson(ServerInfo.jobj_root);

					grid_server.Children.Add(panel_server);
				}
				catch(Exception e)
				{
					Log.PrintError(e.Message, "UserControls.ServerMenu.InitServertab");

				}
			}

			if(ServerMenuButton.group.Count > 0)
				ServerMenuButton.group[0].IsChecked = true;
		}

		#endregion

		private void InitContextMenu()
		{
			this.ContextMenu = new ContextMenu();
			MenuItem item;

			item = new MenuItem();
			item.Click += BtnAddServerMenu_Click;
			item.Header = "Add Server Group";
			item.Icon = new PackIconMaterial()
			{
				Kind = PackIconMaterialKind.FolderPlus,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			this.ContextMenu.Items.Add(item);
		}
		private void BtnAddServerMenu_Click(object sender, RoutedEventArgs e)
		{
			Window_AddServerMenu wms = new Window_AddServerMenu();
			Point pt = this.PointToScreen(new Point(0, 0));
			wms.Left = pt.X;
			wms.Top = pt.Y;
			if(wms.ShowDialog() == true)
			{
				string server_menu_name = wms.textBox_name.Text;


				if(ServerInfo.jobj_root == null)
					return;
				try
				{
					ServerInfo.jobj_root.Add(new JProperty(server_menu_name, new JObject()));

					if(!ServerInfo.save())
						return;

					ServerMenuButton smbtn = new ServerMenuButton(server_menu_name);
					ServerPanel.current.Children.Add(smbtn);
					ServerPanel.SubPanel.Children.Add(smbtn.child);
				}
				catch(Exception ex)
				{
					Log.ErrorIntoUI("서버 메뉴 이름이 중복됩니다.\r", "Add Server Menu", Status.current.richTextBox_status);
					Log.PrintError(ex.Message, "UserControls.ServerMenu.BtnAddServerMenu_Click");
				}
			}
		}
	}
}
