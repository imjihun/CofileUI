using CofileUI.Classes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MS.Internal.AppModel;
using MS.Win32;
using MahApps.Metro.IconPacks;
using Renci.SshNet.Sftp;
using System.Text.RegularExpressions;
using CofileUI.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Globalization;
using System.Windows.Controls.Primitives;
using Newtonsoft.Json.Linq;
using MahApps.Metro.Controls.Dialogs;
using CofileUI.UserControls.ConfigOptions;

namespace CofileUI.UserControls
{
	/// <summary>
	/// Cofile.xaml에 대한 상호 작용 논리
	/// </summary>
	public enum CofileType
	{
		undefined = 0
		, file
		, sam
		, tail
	}
	public partial class Cofile : UserControl
	{
		public static Cofile current;
		public bool bUpdated = false;
		public ConfigMenu configMenu;
		LinuxFileViewModel lfvm;

		public Cofile()
		{
			InitializeComponent();

			//root = new LinuxTreeViewItem("/", null, "/", true, null);
			//treeView_linux_directory.Items.Add(root);

			lfvm = new LinuxFileViewModel(this);
			DataContext = lfvm;
			current = this;
			this.Loaded += (sender, e) => {
				if(!bUpdated)
					Cofile.current.Refresh();
			};
			configMenu = new ConfigMenu() { DataContext = this };
			grid_config.Children.Add(configMenu);
			this.IsVisibleChanged += (sender, e) => { if(this.IsVisible && !this.bUpdated) this.Refresh(); };
		}
		public int Refresh()
		{
			if(WindowMain.current == null)
				return -1;

			if(WindowMain.current?.enableConnect?.sshManager?.IsConnected != true)
				return -2;
			// 추가
			// root 의 path 는 null 로 초기화
			string working_dir = WindowMain.current?.enableConnect?.sshManager?.WorkingDirectory;
			if(working_dir == null)
				return -1;

			configMenu.Refresh();
			lfvm.RefreshLinuxFileTree(working_dir);
			
			return 0;
		}
		public void Clear()
		{
			configMenu.Clear();
			lfvm.Clear();
		}
	}








	public class HideStringToDoubleConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			if(value == null)
				return null;

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
