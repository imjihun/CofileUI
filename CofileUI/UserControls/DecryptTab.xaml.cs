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

	public partial class DecryptTab : UserControl
	{
		public static DecryptTab current;
		public bool bUpdated = false;

		public ConfigMenuRootPanel configMenu;
		public LinuxTreeView tv_linux;

		public DecryptTab()
		{
			InitializeComponent();

			current = this;

			tv_linux = new LinuxTreeView();
			grid_treeView_linux_directory.Children.Add(tv_linux);

			configMenu = new ConfigMenuRootPanel();
			grid_config_menu.Children.Add(configMenu);

			this.Loaded += (sender, e) => {
				if(!bUpdated)
					DecryptTab.current.Refresh();
			};
			this.IsVisibleChanged += (sender, e) => { if(this.IsVisible && !this.bUpdated) this.Refresh(); };
			//this.IsEnabledChanged += (sender, e) => { Console.WriteLine("JHLIM_DEBUG IsEnabledChanged"); };
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

			int retval = 0;
			if((retval = configMenu.Refresh()) < 0)
				return retval;
			if((retval = tv_linux.Refresh(working_dir)) < 0)
				return retval;

			Log.PrintLog("[refresh]", "UserControls.Cofile.Refresh");

			bUpdated = true;

			return 0;
			//LinuxTreeViewItem.ReconnectServer();
		}
		public void Clear()
		{
			tv_linux.Items.Clear();
			configMenu.Clear();
		}
	}
}
