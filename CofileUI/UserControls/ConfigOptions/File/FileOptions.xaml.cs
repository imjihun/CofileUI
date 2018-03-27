using CofileUI.Classes;
using MahApps.Metro.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CofileUI.UserControls.ConfigOptions.File
{
	/// <summary>
	/// FileOptions.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class FileOptions : UserControl
	{
		public FileOptions()
		{
			InitializeComponent();
			ConfigOptionManager.IsChanged = false;

			this.Loaded += FileOptions_Loaded;
		}

		bool bInit = false;
		JObject Root { get; set; }
		private void FileOptions_Loaded(object sender, RoutedEventArgs e)
		{
			if(!bInit)
			{
				Root = DataContext as JObject;
				if(Root == null)
					return;

				grid1.Children.Add(new comm_option(Root["comm_option"] as JObject));
				grid2.Children.Add(new enc_option(Root["enc_option"] as JObject));
				grid3.Children.Add(new dec_option(Root["dec_option"] as JObject));
				bInit = true;
			}
		}

		private void TitleMouseDown(object sender, MouseButtonEventArgs e)
		{
			if(e.ChangedButton == MouseButton.Left)
			{
				Window parentWindow = Window.GetWindow(this);
				parentWindow.DragMove();
			}
		}
	}
}
