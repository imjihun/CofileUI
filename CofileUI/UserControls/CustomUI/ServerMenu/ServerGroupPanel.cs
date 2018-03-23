using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace CofileUI.UserControls
{
	public class ServerGroupPanel : StackPanel
	{
		public ServerGroupButton sgb;
		public ServerListBox slb;
		public object Content { get { return sgb.Content; } set { sgb.Content = value; } }
		public bool? IsChecked { get { return sgb.IsChecked; } set { sgb.IsChecked = value; } }
		public ServerGroupPanel()
		{
			this.Orientation = Orientation.Vertical;

			sgb = new ServerGroupButton();
			this.Children.Add(sgb);

			slb = new ServerListBox();
			this.Children.Add(slb);

			this.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
		}
	}
}
