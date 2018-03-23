using System.Windows;
using System.Windows.Controls;
using CofileUI.Windows;
using System.Windows.Data;

namespace CofileUI.UserControls
{
	public class ConfigMenuTreeView : TreeView
	{
		public ConfigMenuButton parent;

		private void OnClickEnvSetting(object sender, RoutedEventArgs e)
		{
			if(WindowMain.current?.EnableConnect?.SshManager == null)
				return;

			Window_EnvSetting wms = new Window_EnvSetting();
			Point pt = this.PointToScreen(new Point(0, 0));
			wms.Left = pt.X;
			wms.Top = pt.Y;
			wms.textBox_cohome.Text = WindowMain.current.EnableConnect.SshManager.EnvCoHome;
			if(wms.ShowDialog() == true)
			{
				WindowMain.current.EnableConnect.SshManager.EnvCoHome = wms.textBox_cohome.Text;
			}
		}

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
