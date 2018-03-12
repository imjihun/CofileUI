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
	public partial class Window_Waiting : Window
	{
		public Window_Waiting(string Message)
		{
			InitializeComponent();
			textBlock.Text = Message;
		}
	}
}
