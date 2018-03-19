using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Collections.Specialized;
using Renci.SshNet;
using System.Reflection;
using Renci.SshNet.Sftp;
using MahApps.Metro.Controls;
using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using CofileUI.Classes;
using CofileUI.UserControls;

namespace CofileUI.Windows
{
	/// <summary>
	/// test4.xaml에 대한 상호 작용 논리
	/// </summary>
	public static class style
	{
		public static Accent currentAccent = ThemeManager.GetAccent("Blue");
		public static AppTheme currentAppTheme = ThemeManager.GetAppTheme("White");
	}
	public partial class WindowMain : MetroWindow
	{
		public static WindowMain current;
		public ServerInfo enableConnect;
		public WindowMain()
		{
			//string[] dll_path = new string[] { @"bin\EntityFramework.dll"
			//									,@"bin\EntityFramework.SqlServer.dll"
			//									,@"bin\MahApps.Metro.dll"
			//									,@"bin\MahApps.Metro.IconPacks.dll"
			//									,@"bin\Newtonsoft.Json.dll"
			//									,@"bin\Renci.SshNet.dll"
			//									,@"bin\System.Data.SQLite.dll"
			//									,@"bin\System.Data.SQLite.EF6.dll"
			//									,@"bin\System.Data.SQLite.Linq.dll"
			//									,@"bin\System.Windows.Interactivity.dll"};

			current = this;
			InitializeComponent();
			this.Closed += test4_Closed;

			this.PreviewKeyDown += WindowMain_PreviewKeyDown;
			this.PreviewMouseDown += WindowMain_PreviewMouseDown;
			this.PreviewMouseWheel += WindowMain_PreviewMouseWheel;
			DispatcherTimer DisconnectTimeout = new DispatcherTimer();
			DisconnectTimeout.Interval = new TimeSpan(0, 0, 0, 5);
			DisconnectTimeout.Tick += DisconnectTimeout_Tick;
			DisconnectTimeout.Start();
		}
		
		static DateTime LastInputTime = DateTime.Now;
		private void WindowMain_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			LastInputTime = DateTime.Now;
		}
		private void WindowMain_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			LastInputTime = DateTime.Now;
		}
		private void WindowMain_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			LastInputTime = DateTime.Now;
		}
		
		private void DisconnectTimeout_Tick(object sender, EventArgs e)
		{
			if(MainSettings.IsTimeOut && enableConnect?.sshManager?.IsConnected == true
				&& LastInputTime.AddMinutes(MainSettings.SessionTimeOut) < DateTime.Now)
			//&& LastInputTime.AddSeconds(5) < DateTime.Now)
			{
				WindowMain.current.ShowMessageDialog("Session Timeout", MainSettings.SessionTimeOut + "분 간 입력이 없어 연결이 종료됩니다.", MessageDialogStyle.Affirmative, DisconnectTimeout);
			}
			//Console.WriteLine("LastInputTime = " + LastInputTime);
		}
		private void DisconnectTimeout()
		{
			enableConnect?.sshManager?.DisConnect();
		}

		public static bool bCtrl = false;
		public static bool bShift = false;
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if(e.Key == Key.LeftCtrl)
				bCtrl = true;
			else if(e.Key == Key.LeftShift)
				bShift = true;
		}
		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);
			if(e.Key == Key.LeftCtrl)
				bCtrl = false;
			else if(e.Key == Key.LeftShift)
				bShift = false;
		}
		
		private void test4_Closed(object sender, EventArgs e)
		{
			Application.Current.Shutdown();
		}


		#region View Update
		public void bUpdateInit(bool val)
		{
			Cofile.current.bUpdated = val;
			Decrypt.current.bUpdated = val;
			DataBaseInfo.bUpdated = val;
		}
		string connectedServerName = "";
		public string ConnectedServerName { get { return connectedServerName; }
			set
			{
				connectedServerName = value;
				if(value == "")
				{
					label_serverinfo.Content = "";

				}
				else
				{
					label_serverinfo.Content = "[ " + connectedServerName + " ] is Connected from [ " + enableConnect?.sshManager?.id + " ]";
				}
			}
		}
		
		// 서버메뉴리스트에서 서버를 컨넥팅 동작을 할 때 작동
		public void Refresh(ServerInfo si)
		{
			this.enableConnect = si;

			if(tabControl.SelectedIndex == 0) Cofile.current.Refresh();
			else if(tabControl.SelectedIndex == 1) Decrypt.current.Refresh();
			else if(tabControl.SelectedIndex == 2) DataBaseInfo.RefreshUi();
			else if(tabControl.SelectedIndex == 3) DataBaseInfo.RefreshUi();

			if(this.enableConnect?.sshManager?.IsConnected == true)
			{
				ConnectedServerName = this.enableConnect?.sshManager?.name;
			}
		}
		public void Clear()
		{
			if(Cofile.current != null)
				Cofile.current.Clear();
			if(Decrypt.current != null)
				Decrypt.current.Clear();
			if(Sqlite_LogTable.current != null)
				Sqlite_LogTable.current.Clear();
			if(Sqlite_StatusTable.current != null)
				Sqlite_StatusTable.current.Clear();
			if(Status.current != null)
				Status.current.Clear();

			ConnectedServerName = "";
		}
		#endregion

		public delegate void CallBack();
		public void ShowMessageDialog(string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative, CallBack affirmative_callback = null, CallBack negative_callback = null, CallBack alwayse_callback = null, MetroDialogSettings settings = null)
		{
			//MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Theme;
			//var mySettings = new MetroDialogSettings()
			//{
			//	AffirmativeButtonText = "Ok",
			//	//NegativeButtonText = "Go away!",
			//	FirstAuxiliaryButtonText = "Cancel",
			//	//ColorScheme = UseAccentForDialogsMenuItem.IsChecked ? MetroDialogColorScheme.Accented : MetroDialogColorScheme.Theme
			//};

			MessageDialogResult result = this.ShowModalMessageExternal(title, message, style, settings);
			//MessageDialogResult result = await this.ShowMessageAsync(title, message, style);

			if(affirmative_callback != null && result == MessageDialogResult.Affirmative)
				affirmative_callback();

			if(negative_callback != null && result == MessageDialogResult.Negative)
				negative_callback();

			if(alwayse_callback != null)
				alwayse_callback();
		}

		private void OnClickMainSetting(object sender, RoutedEventArgs e)
		{
			Window_MainSetting wm = new Window_MainSetting();
			Point pt = this.PointToScreen(new Point(0, 0));
			wm.Left = pt.X + this.ActualWidth / 2 - wm.Width / 2;
			wm.Top = pt.Y + this.ActualHeight / 2 - wm.Height / 2;
			if(wm.ShowDialog() == true)
				;
		}
	}
}