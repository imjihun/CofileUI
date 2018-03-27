using System.Windows;
using System.Windows.Controls;
using CofileUI.Windows;
using System.Windows.Data;

namespace CofileUI.UserControls
{
	public class ConfigMenuTreeView : TreeView
	{		
		public ConfigMenuTreeView()
		{
			this.Margin = new Thickness(20, 0, 0, 0);
			this.BorderBrush = null;

			this.HorizontalContentAlignment = HorizontalAlignment.Stretch;

			this.Visibility = Visibility.Collapsed;

			//this.SetBinding(TreeView.ItemsSourceProperty, "FileConfig");
		}
	}
}
