using CofileUI.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
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
	/// Window_SelectJsonFile.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class Window_SelectJsonFile : Window
	{
		private string filePathRemote = null;
		public string FilePathRemote { get { return filePathRemote; } set { filePathRemote = value; } }
		public Window_SelectJsonFile(SftpFileTree root)
		{
			InitializeComponent();
			listView_linux_files.ItemsSource = root.children;
		}

		private void listView_linux_files_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			SftpFileTree sft = listView_linux_files.SelectedItem as SftpFileTree;
			if(sft == null)
				return;

			if(sft.File.IsDirectory)
				listView_linux_files.ItemsSource = sft.children;
			else
			{
				FilePathRemote = sft.File.FullName;
				this.DialogResult = true;
				this.Close();
			}
		}

		private void CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			if(sender == checkBox_linux_detail)
			{
				listView_linux_files.ItemsPanel = Resources["ItemsPanelTemplate_ListView_Detail"] as ItemsPanelTemplate;
				listView_linux_files.View = Resources["GridView_ListViewLinux_Detail"] as GridView;
			}
		}
		private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			if(sender == checkBox_linux_detail)
			{
				listView_linux_files.View = null;
				listView_linux_files.ItemsPanel = Resources["ItemsPanelTemplate_ListView_Icon"] as ItemsPanelTemplate;
			}
		}
	}

	public class HideStringToDoubleConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			string str = value.ToString();
			if(str[0] == '.')
				return .5;
			return 1;
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			throw new Exception();
		}
	}
}
