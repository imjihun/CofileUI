using CofileUI.Classes;
using CofileUI.UserControls.ConfigOptions;
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
using System.Windows.Shapes;

namespace CofileUI.Windows
{
	/// <summary>
	/// Window_Waiting.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class Window_Config : Window
	{
		public bool IsModal = false;
		public Window_Config(UserControl ui, bool _IsModal = false, string path = null)
		{
			InitializeComponent();

			IsModal = _IsModal;

			ConfigOptionManager.Path = path;
			if(ui != null)
				grid.Children.Add(ui);
		}

		public Window_Config(JObject jobj_config_root, string work_name = null, string index = null, bool _IsModal = false, string path = null)
			: this(ConfigOptionManager.CreateOption(jobj_config_root, work_name, index), _IsModal, path)
		{
		}
		
		private void OnClickOkButton(object sender, RoutedEventArgs e)
		{
			int retval = 0;
			try
			{
				if((retval = ConfigOptionManager.SaveOption()) < 0)
					Console.WriteLine("JHLIM_DEBUG : Window_Config " + retval);
			}
			catch(Exception ex)
			{
				Console.WriteLine("JHLIM_DEBUG : Window_Config " + ex.Message);
			}
			if(IsModal)
				this.DialogResult = true;
			this.Close();
		}
		private void OnClickCancelButton(object sender, RoutedEventArgs e)
		{
			if(IsModal)
				this.DialogResult = false;
			this.Close();
		}
	}
}
