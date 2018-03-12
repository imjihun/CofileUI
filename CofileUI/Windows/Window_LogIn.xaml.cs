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
	/// Window_LogIn.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class Window_LogIn : Window
	{
		public string Id = "";
		public string Password = "";

		public Window_LogIn()
		{
			InitializeComponent();
			textBox_id.KeyDown += TextBox_id_KeyDown;
			passwordBox_password.KeyDown += PasswordBox_password_KeyDown; ;

			textBox_id.Focus();
		}

		private void SaveValue()
		{
			Id = textBox_id.Text;
			Password = passwordBox_password.Password;
		}

		private void TextBox_id_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key != Key.Enter)
				return;

			passwordBox_password.Focus();
		}
		private void PasswordBox_password_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key != Key.Enter)
				return;

			SaveValue();
			this.DialogResult = true;
			this.Close();
		}

		private void OnButtonClickOk(object sender, RoutedEventArgs e)
		{
			SaveValue();
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
