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
	public class ServerListBox : ListBox
	{
		MenuItem item_delete;
		MenuItem item_connect;
		MenuItem item_disconect;
		MenuItem item_modify;

		RelayCommand DeleteServerCommand;
		RelayCommand ConnectServerCommand;
		RelayCommand DisConnectServerCommand;
		RelayCommand ModifyServerCommand;

		private void DeleteServer(object parameter)
		{
			WindowMain.current.ShowMessageDialog(
				"Delete Server",
				"해당 서버 정보를 정말 삭제하시겠습니까?",
				MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegative,
				() => {
					ServerListBoxItem slbi = this.SelectedItem as ServerListBoxItem;
					if(slbi == null)
						return;
					if(((this.Parent as ServerGroupPanel)?.Parent as ServerGroupRootPanel)?.ServerViewModel.DeleteServer((this.Parent as ServerGroupPanel)?.Content.ToString(), slbi.Serverinfo.Name) != 0)
						return;

					this.Items.Remove(slbi);
				});
		}
		private void ConnectServer(object parameter)
		{
			ServerListBoxItem sip = this.SelectedItem as ServerListBoxItem;
			if(sip == null)
				return;

			if(sip.Serverinfo.SshManager?.ReConnect() == true)
				;

			WindowMain.current.Refresh(sip.Serverinfo);
		}
		private void DisConnectServer(object parameter)
		{
			ServerListBoxItem sip = this.SelectedItem as ServerListBoxItem;
			if(sip == null)
				return;

			sip.Serverinfo.SshManager?.DisConnect();

			if(WindowMain.current != null)
				WindowMain.current.Clear();
		}
		private void ModifyServer(object parameter)
		{
			ServerListBoxItem sip = this.SelectedItem as ServerListBoxItem;
			if(sip == null)
				return;

			Window_AddServer wms = new Window_AddServer(sip.Serverinfo);
			//wms.textBox_password.Password = sitb.serverinfo.password;

			Point pt = this.PointToScreen(new Point(0, 0));
			wms.Left = pt.X;
			wms.Top = pt.Y;
			if(wms.ShowDialog() == true)
			{
				string name = wms.ServerName;
				string ip = wms.Ip;
				int port = wms.Port;

				if(((this.Parent as ServerGroupPanel)?.Parent as ServerGroupRootPanel)?.ServerViewModel.ModifyServer(sip.Serverinfo, (this.Parent as ServerGroupPanel)?.Content.ToString(), name, ip, port) != 0)
					return;
			}
		}

		public ServerListBox()
		{
			this.Margin = new Thickness(20, 0, 0, 0);
			this.BorderBrush = null;

			this.Visibility = Visibility.Collapsed;

			DeleteServerCommand = new RelayCommand(DeleteServer);
			ConnectServerCommand = new RelayCommand(ConnectServer);
			DisConnectServerCommand = new RelayCommand(DisConnectServer);
			ModifyServerCommand = new RelayCommand(ModifyServer);

			this.ContextMenu = new ContextMenu();

			item_delete = new MenuItem();
			item_delete.Command = DeleteServerCommand;
			item_delete.Header = "Delete Server";
			item_delete.Icon = new PackIconMaterial()
			{
				Kind = PackIconMaterialKind.ServerMinus,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			this.ContextMenu.Items.Add(item_delete);

			item_connect = new MenuItem();
			item_connect.Command = ConnectServerCommand;
			item_connect.Header = "Connect Server";
			item_connect.Icon = new PackIconModern()
			{
				Kind = PackIconModernKind.Connect,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			this.ContextMenu.Items.Add(item_connect);

			item_disconect = new MenuItem();
			item_disconect.Command = DisConnectServerCommand;
			item_disconect.Header = "DisConnect Server";
			item_disconect.Icon = new PackIconModern()
			{
				Kind = PackIconModernKind.Disconnect,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			this.ContextMenu.Items.Add(item_disconect);

			item_modify = new MenuItem();
			item_modify.Command = ModifyServerCommand;
			item_modify.Header = "Modify Server";
			item_modify.Icon = new PackIconMaterial()
			{
				Kind = PackIconMaterialKind.Settings,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			this.ContextMenu.Items.Add(item_modify);

			this.ContextMenu.Opened += ContextMenu_Opened;
		}

		private void ContextMenu_Opened(object sender, RoutedEventArgs e)
		{
			ServerListBoxItem sip = this.SelectedItem as ServerListBoxItem;
			if(sip == null)
				return;

			if(sip.IsConnected)
			{
				item_delete.IsEnabled = false;
				item_connect.IsEnabled = false;
				item_disconect.IsEnabled = true;
				item_modify.IsEnabled = false;
			}
			else
			{
				item_delete.IsEnabled = true;
				item_connect.IsEnabled = true;
				item_disconect.IsEnabled = false;
				item_modify.IsEnabled = true;
			}
		}
		protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
		{
			base.OnMouseDoubleClick(e);
			if(e.ChangedButton == MouseButton.Left)
			{
				this.ConnectServerCommand.Execute(null);
			}
		}
	}

}
