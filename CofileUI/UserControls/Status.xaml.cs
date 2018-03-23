using CofileUI.Classes;
using CofileUI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CofileUI.UserControls
{
	/// <summary>
	/// Status.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class Status : UserControl
	{
		public static Status current;
		public Status()
		{
			current = this;

			InitializeComponent();
		}
		private void TextBox_TextChanged_ScrollToEnd(object sender, TextChangedEventArgs e)
		{
			//textBox_status.ScrollToEnd();
			TextBoxBase tb = sender as TextBoxBase;
			if(tb == null)
				return;
			tb.ScrollToEnd();
		}
		public void Clear()
		{
			TextRange txt = new TextRange(richTextBox_status.Document.ContentStart, richTextBox_status.Document.ContentEnd);
			txt.Text = "";

			textBox_serverLog.Text = "";
		}

		private void OnClickServerLogRefresh(object sender, RoutedEventArgs e)
		{
			//TextRange rangeOfWord = new TextRange(richTextBox_serverLog.Document.ContentStart, richTextBox_serverLog.Document.ContentEnd);
			//rangeOfWord.Text = WindowMain.current?.enableConnect?.sshManager?.GetEventLog();
			textBox_serverLog.Text = WindowMain.current?.EnableConnect?.SshManager?.GetEventLog((int)numericUpDown_count.Value);
			//rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
		}
		private void OnKeyDownNumericUpDown(object sender, KeyEventArgs e)
		{
			textBox_serverLog.Text = WindowMain.current?.EnableConnect?.SshManager?.GetEventLog((int)numericUpDown_count.Value);
		}

		const int MAX = 5000;
		private void numericUpDown_count_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
		{
			if(numericUpDown_count.Value < 0)
				numericUpDown_count.Value = 0;
			else if(numericUpDown_count.Value > MAX)
				numericUpDown_count.Value = MAX;
		}

		//public void PrintLog(string message, string caption)
		//{
		//	string str = "[Error] ";

		//	if(caption != null)
		//		str += "[" + caption + "] ";

		//	str += message;

		//	//str += "\n";
		//	CheckNewLine(ref str);
		//	PrintErrorLogFile(message, caption);

		//	if(output_ui != null)
		//	{
		//		TextBox tb = output_ui as TextBox;
		//		if(tb != null)
		//			tb.Text += str;


		//		RichTextBox rtb = output_ui as RichTextBox;
		//		if(rtb != null)
		//		{
		//			TextRange rangeOfWord = new TextRange(rtb.Document.ContentEnd, rtb.Document.ContentEnd);
		//			rangeOfWord.Text = str;
		//			rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
		//			//rangeOfWord.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Regular);
		//		}
		//	}
		//}
	}
}
