using MahApps.Metro.Controls;
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
	/// Window_ViewImage.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class Window_ViewImage : MetroWindow
	{
		public Window_ViewImage(ImageSource source, string title)
		{
			InitializeComponent();
			this.Width = source.Width;
			this.Height = source.Height;
			image.Source = source;
			this.Title = title;
		}
	}
}
