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
	public class ServerGroupButton : ToggleButton
	{
		public const double HEIGHT = 30;
		const double FONTSIZE = 13;

		private void InitStyle()
		{
			Style style = new Style(typeof(ServerGroupButton), (Style)App.Current.Resources["MetroToggleButton"]);
			Trigger trigger_selected = new Trigger() {Property = ToggleButton.IsCheckedProperty, Value = true };
			trigger_selected.Setters.Add(new Setter(ToggleButton.BackgroundProperty, (SolidColorBrush)App.Current.Resources["AccentColorBrush"]));
			trigger_selected.Setters.Add(new Setter(ToggleButton.ForegroundProperty, Brushes.White));
			style.Triggers.Add(trigger_selected);

			Trigger trigger_mouseover = new Trigger() {Property = ToggleButton.IsMouseOverProperty, Value = true };
			SolidColorBrush s = new SolidColorBrush(((SolidColorBrush)App.Current.Resources["AccentColorBrush"]).Color);
			s.Opacity = .5;
			trigger_mouseover.Setters.Add(new Setter(ToggleButton.BackgroundProperty, s));
			style.Triggers.Add(trigger_mouseover);

			this.Style = style;
		}
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

			item = new MenuItem();
			item.Click += BtnDelServerMenu_Click;
			item.Header = "Del Server Group";
			item.Icon = new PackIconMaterial()
			{
				Kind = PackIconMaterialKind.FolderRemove,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			this.ContextMenu.Items.Add(item);

			item = new MenuItem();
			item.Click += BtnAddServer_Click;
			item.Header = "Add Server";
			item.Icon = new PackIconMaterial()
			{
				Kind = PackIconMaterialKind.ServerPlus,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			this.ContextMenu.Items.Add(item);
		}
		public ServerGroupButton()
		{
			this.InitStyle();

			//this.Background = Brushes.White;
			this.Height = HEIGHT;
			this.HorizontalAlignment = HorizontalAlignment.Stretch;
			this.VerticalAlignment = VerticalAlignment.Bottom;
			this.FontSize = FONTSIZE;

			InitContextMenu();
		}
		private void BtnAddServer_Click(object sender, RoutedEventArgs e)
		{
			Window_AddServer wms = new Window_AddServer();
			Point pt = this.PointToScreen(new Point(0, 0));
			wms.Left = pt.X;
			wms.Top = pt.Y;
			if(wms.ShowDialog() == true)
			{
				string name = wms.ServerName;
				string ip = wms.Ip;
				int port = wms.Port;
				//string id = wms.textBox_id.Text;
				//string password = wms.textBox_password.Password;
				
				if(((this.Parent as ServerGroupPanel)?.Parent as ServerGroupRootPanel)?.ServerViewModel.AddServer(this.Content.ToString(), name, ip, port) != 0)
					return;
				
				ServerListBoxItem si = new ServerListBoxItem() { ServerModel = new ServerModel(((this.Parent as ServerGroupPanel)?.Parent as ServerGroupRootPanel)?.ServerViewModel.JobjRoot?[this.Content]?[name]?.Parent as JProperty) };
				(this.Parent as ServerGroupPanel)?.slb.Items.Add(si);
			}
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
				
				if(((this.Parent as ServerGroupPanel)?.Parent as ServerGroupRootPanel)?.ServerViewModel.AddServerGroup(server_menu_name) != 0)
					return;

				ServerGroupRootPanel sgrp = ((this.Parent as ServerGroupPanel)?.Parent as ServerGroupRootPanel);
				sgrp?.AddChild(new ServerGroupPanel() { Content = server_menu_name });
			}
		}
		private void BtnDelServerMenu_Click(object sender, RoutedEventArgs e)
		{
			WindowMain.current.ShowMessageDialog("Delete Server Menu", "해당 서버 메뉴를 정말 삭제하시겠습니까? 해당 서버 정보도 같이 삭제됩니다.", MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegative, DeleteServerMenuUI);
		}
		private void DeleteServerMenuUI()
		{
			if(((this.Parent as ServerGroupPanel)?.Parent as ServerGroupRootPanel)?.ServerViewModel.DeleteServerGroup(this.Content.ToString()) != 0)
				return;

			ServerGroupRootPanel sgrp = ((this.Parent as ServerGroupPanel)?.Parent as ServerGroupRootPanel);
			sgrp?.DeleteChild(this.Parent as ServerGroupPanel);
		}
		
		protected override void OnUnchecked(RoutedEventArgs e)
		{
			base.OnUnchecked(e);

			ServerListBox slb = (this.Parent as ServerGroupPanel)?.slb;
			if(slb != null)
				slb.Visibility = Visibility.Collapsed;

			UIElementCollection group = ((this.Parent as ServerGroupPanel)?.Parent as ServerGroupRootPanel)?.Children;
			if(group == null)
				return;

			int count = 0;
			for(int i = 0; i < group.Count; i++)
			{
				if((group[i] as ServerGroupPanel).IsChecked == true)
					count++;
			}
			if(count < 1)
				this.IsChecked = true;
		}
		protected override void OnChecked(RoutedEventArgs e)
		{
			base.OnChecked(e);
			UIElementCollection group = ((this.Parent as ServerGroupPanel)?.Parent as ServerGroupRootPanel)?.Children;
			if(group == null)
				return;

			int idx = group.IndexOf(this.Parent as ServerGroupPanel);
			if(idx < 0)
				return;

			int i;
			for(i = 0; i < idx; i++)
			{
				if(group[i] as ServerGroupPanel != null)
					(group[i] as ServerGroupPanel).VerticalAlignment = VerticalAlignment.Top;
				(group[i] as ServerGroupPanel).IsChecked = false;
			}
			if(group[i] as ServerGroupPanel != null)
				(group[i++] as ServerGroupPanel).VerticalAlignment = VerticalAlignment.Stretch;
			for(; i < group.Count; i++)
			{
				if(group[i] as ServerGroupPanel != null)
					(group[i] as ServerGroupPanel).VerticalAlignment = VerticalAlignment.Bottom;
				(group[i] as ServerGroupPanel).IsChecked = false;
			}


			ServerListBox slb = (this.Parent as ServerGroupPanel)?.slb;
			if(slb != null)
				slb.Visibility = Visibility.Visible;
		}
		protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseRightButtonDown(e);
			this.Focus();
		}
	}
}
