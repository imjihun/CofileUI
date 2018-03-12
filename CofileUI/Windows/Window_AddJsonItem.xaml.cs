using Newtonsoft.Json.Linq;
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
	/// popup_AddJsonItem.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class Window_AddJsonItem : Window
	{
		public string key = "";
		public JToken value = "";
		JToken[] value_type = new JToken[5];
		public Window_AddJsonItem()
		{
			InitializeComponent();

			value_type[0] = new JValue("");
			value_type[1] = new JValue(false);
			value_type[2] = new JValue(0);
			value_type[3] = new JObject();
			value_type[4] = new JArray(new JObject());

			btn_ok.Click += Btn_ok_Click;
			btn_cancel.Click += Btn_cancel_Click;

			textBox_key.KeyDown += TextBox_KeyDown;

			textBox_key.Focus();
		}

		private void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key != Key.Enter)
				return;

			this.DialogResult = true;
			this.Close();
		}
		private void Btn_ok_Click(object sender, RoutedEventArgs e)
		{
			key = textBox_key.Text;
			value = value_type[comboBox_type.SelectedIndex];
			this.DialogResult = true;
			this.Close();
		}

		private void Btn_cancel_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
			this.Close();
		}
	}
}
