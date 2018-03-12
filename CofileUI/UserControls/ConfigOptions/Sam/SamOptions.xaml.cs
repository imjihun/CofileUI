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

namespace CofileUI.UserControls.ConfigOptions.Sam
{
	/// <summary>
	/// SamOptions.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class SamOptions : UserControl
	{
		bool bInit = false;
		public static SamOptions current;
		JObject Root { get; set; }
		public SamOptions()
		{
			current = this;
			InitializeComponent();
			ConfigOptionManager.bChanged = false;
			this.Loaded += delegate
			{
				if(!bInit)
				{
					Root = DataContext as JObject;
					if(Root == null)
						return;

					grid1.Children.Add(new comm_option(Root["comm_option"] as JObject));
					ChangeSecondGrid();
					bInit = true;
				}
			};
		}
		public void ChangeSecondGrid()
		{
			JObject root = DataContext as JObject;
			if(root == null)
				return;
			JObject jobj = root["comm_option"] as JObject;
			if(jobj == null)
			{
				Log.PrintLog("NotFound Sam.comm_option", "UserControls.ConfigOptions.Sam.SamOptions.ChangedSecondGrid");
				return;
			}
			JValue jval = jobj["sam_type"] as JValue;
			if(jval == null)
			{
				Log.PrintLog("NotFound Sam.comm_option.sam_type", "UserControls.ConfigOptions.Sam.SamOptions.ChangedSecondGrid");
				return;
			}

			grid2.Children.Clear();

			if(Convert.ToInt64(jval.Value) == 0)
			{
				ChangeBySamType(root, "col_var", "col_fix");
				if(root["col_var"] == null)
				{
					Log.PrintLog("NotFound Sam.col_var", "UserControls.ConfigOptions.Sam.SamOptions.ChangedSecondGrid");
					return;
				}
				grid2.Children.Add(new col_var() { DataContext = root["col_var"].Parent });
			}
			else if(Convert.ToInt64(jval.Value) == 1)
			{
				ChangeBySamType(root, "col_fix", "col_var");
				if(root["col_fix"] == null)
				{
					Log.PrintLog("NotFound Sam.col_fix", "UserControls.ConfigOptions.Sam.SamOptions.ChangedSecondGrid");
					return;
				}
				grid2.Children.Add(new col_fix() { DataContext = root["col_fix"].Parent });
			}
		}
		static void ChangeBySamType(JObject root, string enableKey, string disableKey)
		{
			if(root == null || enableKey == null || disableKey == null)
				return;

			if(root[disableKey] != null)
			{
				root[disableKey].Parent.Replace(new JProperty(ConfigOptionManager.StartDisableProperty + disableKey, root[disableKey]));
				ConfigOptionManager.bChanged = true;
			}
			if(root[enableKey] == null)
			{
				if(root[ConfigOptionManager.StartDisableProperty + enableKey] != null)
					root[ConfigOptionManager.StartDisableProperty + enableKey].Parent.Replace(new JProperty(enableKey, root[ConfigOptionManager.StartDisableProperty + enableKey]));
				else
					root.Add(new JProperty(enableKey, new JArray()));
				ConfigOptionManager.bChanged = true;
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
