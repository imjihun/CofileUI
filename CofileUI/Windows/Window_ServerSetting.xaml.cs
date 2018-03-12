using CofileUI.Classes;
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
	/// Window_ServerSetting.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class Window_ServerSetting : Window
	{
		public Window_ServerSetting()
		{
			InitializeComponent();
			numericUpDown_sessionTimeOut.KeyDown += TextBox_KeyDown;
			numericUpDown_sessionTimeOut.ValueChanged += delegate
			{
				if(numericUpDown_sessionTimeOut.Value < 0)
					numericUpDown_sessionTimeOut.Value = 0;
				numericUpDown_sessionTimeOut.Value = Convert.ToInt32(numericUpDown_sessionTimeOut.Value);
			};
			numericUpDown_sessionTimeOut.Value = MainSettings.SessionTimeOut;
			numericUpDown_sessionTimeOut.Focus();
		}
		private void SaveVariable()
		{
			MainSettings.SessionTimeOut = Convert.ToInt32(numericUpDown_sessionTimeOut.Value);
		}
		private void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key != Key.Enter)
				return;

			this.DialogResult = true;
			this.Close();
		}

		private void OnButtonClickOk(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
			this.Close();
		}
		private void OnButtonClickCancel(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
			this.Close();
		}
	}
}
