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

	public partial class Decrypt : UserControl
	{
		public static Decrypt current;
		public bool bUpdated = false;

		public LinuxTreeView tv_linux;
		public Decrypt()
		{
			InitializeComponent();

			current = this;
			tv_linux = new LinuxTreeView();
			grid_treeView_linux_directory.Children.Add(tv_linux);

			this.Loaded += (sender, e) => {
				if(!bUpdated)
					Decrypt.current.Refresh();
			};
		}
		public int Refresh()
		{
			if(WindowMain.current == null)
				return -1;

			tv_linux?.Items.Clear();

			if(!SSHController.IsConnected)
				return -2;
			// 추가
			// root 의 path 는 null 로 초기화
			string working_dir = SSHController.WorkingDirectory;
			if(working_dir == null)
				return -1;

			int retval = 0;
			if((retval = tv_linux.ReLoadChild(working_dir)) < 0)
			{
				return retval;
			}
			Cofile.current.RefreshListView(tv_linux.last_refresh);
			Log.PrintLog("[refresh]", "UserControls.Cofile.Refresh");

			bUpdated = true;

			return 0;
			//LinuxTreeViewItem.ReconnectServer();
		}
	}







	
}
