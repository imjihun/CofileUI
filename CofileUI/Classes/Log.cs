using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;

namespace CofileUI.Classes
{
	static class Log
	{
		const string ADD_LOG_DIR = @"system\log\";
		const string ADD_ERR_DIR = @"system\err\";

		//// time, namespace, class, function, message
		//public static void PrintLogFile(string namesp, string cla, string func, string message, string action)
		//{
		//	DateTime dt = DateTime.Now;
		//	string filename = dt.ToString("yyyy.MM.dd.hh") + ".log";
		//	string log = dt.ToString("[yyyy.MM.dd.hh.mm.ss]");
		//	log += "[" + namesp + "." + cla + "." + func + "]";
		//	log += "[" + action + "]";
		//	log += " " + message + "\r\n";
		//	FileContoller.Write(AppDomain.CurrentDomain.BaseDirectory + ADD_LOG_DIR + filename, log);
		//}
		private static bool Write(string path, string str)
		{
			if(path == null || str == null)
				return false;
			try
			{
				// 경로에 디렉토리가 없으면 생성
				string dir = path;
				if(path[path.Length - 1] != '\\')
					dir = path.Substring(0, path.LastIndexOf('\\') + 1);

				Directory.CreateDirectory(dir);
				//if(!FileContoller.CreateDirectory(dir))
				//	return false;

				// 경로에 파일 쓰기.
				FileStream fs = new FileStream(path, FileMode.Append);

				byte[] buffer/* = new byte[MAX_BUFFER]*/;
				int size_write = 0;

				buffer = Encoding.UTF8.GetBytes(str);
				size_write = buffer.Length;

				fs.Write(buffer, 0, size_write);

				fs.Close();
				return true;
			}
			catch(Exception e)
			{
				Console.WriteLine("[Classes.Log.Write] " + e.Message);
			}
			return false;
		}
		public static void PrintLog(string message, string caption)
		{
			DateTime dt = DateTime.Now;
			string filename = dt.ToString("yyyy.MM.dd.hh") + ".log";
			string log = dt.ToString("[yyyy.MM.dd.hh.mm.ss]");
			log += "[" + caption + "]";
			log += " " + message + System.Environment.NewLine + System.Environment.NewLine;
			Write(AppDomain.CurrentDomain.BaseDirectory + ADD_LOG_DIR + filename, log);
			Console.Write(log);
		}
		public static void PrintError(string message, string caption)
		{
			DateTime dt = DateTime.Now;
			string filename = dt.ToString("yyyy.MM.dd.hh") + ".err.log";
			string log = dt.ToString("[yyyy.MM.dd.hh.mm.ss]");
			log += "[" + caption + "]";
			log += " " + message + System.Environment.NewLine + System.Environment.NewLine;
			Write(AppDomain.CurrentDomain.BaseDirectory + ADD_ERR_DIR + filename, log);
			Console.Write(log);
		}

		public static void PrintConsole(string message, string caption)
		{
			string str = System.Environment.NewLine;

			if(caption != null)
				str += "[" + caption + "] ";

			str += message;

			//PrintLogFile(message, caption);
			Console.WriteLine(str);
		}

		#region Print UI
		private static void CheckNewLine(ref string str)
		{
			if(str[str.Length - 1] == '\n')
				str = str.TrimEnd('\n');

			if(str[str.Length - 1] != '\r')
				str += '\r';
		}
		public static void _ViewMessage(string message, string caption, TextBoxBase output_ui)
		{
			//string str = System.Environment.NewLine;
			string str = "";

			if(caption != null && caption != "")
				str += "[" + caption + "] ";

			str += message;

			CheckNewLine(ref str);
			//str += System.Environment.NewLine;

			if(output_ui != null)
			{
				TextBox tb = output_ui as TextBox;
				if(tb != null)
					tb.Text += str;


				RichTextBox rtb = output_ui as RichTextBox;
				if(rtb != null)
				{
					TextRange rangeOfWord = new TextRange(rtb.Document.ContentEnd, rtb.Document.ContentEnd);
					rangeOfWord.Text = str;
					rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
					//rtb.AppendText(str);
					//rtb.Document.Blocks.Add(new Paragraph(new Inline()))
				}
			}
		}
		public static void ViewMessage(string message, string caption, TextBoxBase output_ui)
		{
			if(message == null || message == "")
				return;

			// 한줄씩 실행
			string[] split = message.Split('\n');
			for(int i = 0; i < split.Length; i++)
			{
				if(split[i].Length > 0)
					_ViewMessage(split[i], caption, output_ui);
			}

			// 마지막에 문단 나누기 추가
			//string str = "\r\n";
			string str = "";
			if(output_ui != null)
			{
				TextBox tb = output_ui as TextBox;
				if(tb != null)
					tb.Text += str;


				RichTextBox rtb = output_ui as RichTextBox;
				if(rtb != null)
				{
					TextRange rangeOfWord = new TextRange(rtb.Document.ContentEnd, rtb.Document.ContentEnd);
					rangeOfWord.Text = str;
					rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
					//rtb.AppendText(str);
					//rtb.Document.Blocks.Add(new Paragraph(new Inline()))
				}
			}
			PrintLog("[" + caption + "]" + message, "Classes.Log.ViewMessage");
		}

		public static void ViewUndefine(string message, string caption, TextBoxBase output_ui)
		{
			//string str = System.Environment.NewLine;
			string str = "";

			if(caption != null)
				str += "[" + caption + "] ";

			str += message;

			//_print(str);
			CheckNewLine(ref str);
			//str += System.Environment.NewLine;

			if(output_ui != null)
			{
				TextBox tb = output_ui as TextBox;
				if(tb != null)
					tb.Text += str;


				RichTextBox rtb = output_ui as RichTextBox;
				if(rtb != null)
				{
					TextRange rangeOfWord = new TextRange(rtb.Document.ContentEnd, rtb.Document.ContentEnd);
					rangeOfWord.Text = str;
					rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.LightGray);
					//rtb.AppendText(str);
					//rtb.Document.Blocks.Add(new Paragraph(new Inline()))
				}
			}
		}
		public static void ErrorIntoUI(string message, string caption, TextBoxBase output_ui)
		{
			string str = "[Error] ";

			if(caption != null)
				str += "[" + caption + "] ";

			str += message;

			//str += "\n";
			CheckNewLine(ref str);
			PrintError(message, caption);

			if(output_ui != null)
			{
				TextBox tb = output_ui as TextBox;
				if(tb != null)
					tb.Text += str;


				RichTextBox rtb = output_ui as RichTextBox;
				if(rtb != null)
				{
					TextRange rangeOfWord = new TextRange(rtb.Document.ContentEnd, rtb.Document.ContentEnd);
					rangeOfWord.Text = str;
					rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
					//rangeOfWord.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Regular);
				}
			}
		}
		#endregion
	}
}
