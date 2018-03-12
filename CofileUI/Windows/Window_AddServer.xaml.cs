using CofileUI.Classes;
using CofileUI.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
	/// Window_MakeSession.xaml에 대한 상호 작용 논리
	/// </summary>
	
	public partial class Window_AddServer : Window
	{
		private string servername = "";
		public string ServerName { get { return servername; } set { servername = value; textBox_name.Text = value; } }
		private string ip = "";
		public string Ip { get { return ip; } set { ip = value; textBox_ip.Text = value; } }
		private int port = 22;
		public int Port { get { return port; } set { port = value; textBox_port.Text = value.ToString(); } }

		public Window_AddServer(ServerInfo si = null)
		{
			InitializeComponent();
			textBox_name.Focus();
			textBox_name.KeyDown += TextBox_KeyDown;
			textBox_ip.KeyDown += TextBox_KeyDown;
			textBox_port.KeyDown += TextBox_KeyDown;
			//textBox_id.KeyDown += TextBox_KeyDown;
			//textBox_password.KeyDown += TextBox_KeyDown;

			//textBox_password.PasswordChanged += TextBox_password_PasswordChanged;
			Port = 22;
			if(si != null)
			{
				ServerName = si.name;
				Ip = si.ip;
				Port = si.port;
			}
		}

		//private void TextBox_password_PasswordChanged(object sender, RoutedEventArgs e)
		//{
		//	if(textBox_password.Password.Length == 0)
		//	{
		//		textBlock_password_hint.Visibility = Visibility.Visible;
		//	}
		//	else
		//	{
		//		textBlock_password_hint.Visibility = Visibility.Hidden;
		//	}
		//}
		private bool SaveValue()
		{
			ServerName = textBox_name.Text;
			Ip = textBox_ip.Text;
			try
			{
				Port = Convert.ToInt32(textBox_port.Text);
			}
			catch(Exception ex)
			{
				Log.PrintError(ex.Message, "Windows.Window_AddServer.SaveValue");
				return false;
			}
			return true;
		}
		private void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key != Key.Enter)
				return;
			if(SaveValue())
			{
			}

			this.DialogResult = true;
			this.Close();
		}

		private void OnButtonClickOk(object sender, RoutedEventArgs e)
		{
			if(SaveValue())
			{
			}

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
