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
	public class ServerListBoxItem : ListBoxItem
	{
		private ServerModel serverModel;
		public ServerModel ServerModel {
			get { return serverModel; }
			set {
				serverModel = value;
				icon.SetBinding(
					PackIconModern.VisibilityProperty,
					new Binding("SshManager.IsConnected")
					{
						Source = this.ServerModel,
						Converter = new BooleanToVisibilityConverter2()
					}
				);
				tb.SetBinding(
					TextBlock.TextProperty, 
					new Binding("Name") { Source = this.ServerModel }
				);
			} }
		
		public bool IsConnected
		{
			get
			{
				if(ServerModel?.SshManager?.IsConnected != true)
					return false;
				else
					return true;
			}
		}

		private TextBlock tb;
		private PackIconModern icon;
		public ServerListBoxItem()
		{
			this.HorizontalAlignment = HorizontalAlignment.Stretch;

			StackPanel sp = new StackPanel();

			sp.Orientation = Orientation.Horizontal;


			icon = new PackIconModern()
			{
				Kind = PackIconModernKind.Connect,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			icon.Margin = new Thickness(2, 0, 3, 0);
			sp.Children.Add(icon);

			tb = new TextBlock();
			//this.tb.Foreground = Brushes.Black;
			sp.Children.Add(tb);
			//this.Text = si.Name;

			this.Content = sp;
		}
	}
}
