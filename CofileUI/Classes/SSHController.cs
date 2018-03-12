using CofileUI.UserControls;
using CofileUI.Windows;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;

namespace CofileUI.Classes
{
	public static class SSHController
	{
		//static string[] IGNORE_FILENAME = new string[] {".", ".."};
		public static SshClient ssh;
		public static SftpClient sftp;
		public static ShellStream shell_stream;
		public static StreamReader shell_stream_reader;
		public static StreamWriter shell_stream_writer;
		public static DispatcherTimer shell_stream_read_timer;

		#region connect
		public static bool IsConnected
		{
			get
			{
				if(ssh != null && ssh.IsConnected
					&& sftp != null && sftp.IsConnected)
					return true;
				else
					return false;
			}
		}
		public static bool CheckConnection(string ip, int port)
		{
			if(CheckConnection(sftp, ip, port) && CheckConnection(ssh, ip, port))
				return true;

			return false;
		}
		private static bool CheckConnection(BaseClient client, string ip, int port, string id)
		{
			if((client == null || !client.IsConnected)
					|| (client.ConnectionInfo.Host != ip
						|| client.ConnectionInfo.Port != port
						|| client.ConnectionInfo.Username != id))
				return false;

			return true;
		}
		private static bool CheckConnection(BaseClient client, string ip, int port)
		{
			if((client == null || !client.IsConnected)
					|| (client.ConnectionInfo.Host != ip
						|| client.ConnectionInfo.Port != port))
				return false;

			return true;
		}

		private const int NO_TIMEOUT = -1;
		private static int timeout_connect_ms = NO_TIMEOUT;
		private static bool Connect(string ip, int port, int timeout_ms = NO_TIMEOUT)
		{
			bool retval = true;

			Window_LogIn wl = new Window_LogIn();
			if(ServerList.selected_serverinfo_panel != null)
			{
				Point pt = ServerList.selected_serverinfo_panel.PointToScreen(new Point(0, 0));
				wl.Left = pt.X;
				wl.Top = pt.Y;
			}
			if(wl.ShowDialog() != true)
				return false;

			string id = wl.Id;
			string password = wl.Password;
			ServerList.selected_serverinfo_panel.Serverinfo.id = id;
			timeout_connect_ms = timeout_ms;

			System.Threading.Thread th_popup = new System.Threading.Thread(delegate(object obj)
					{
						try
						{
							Windows.Window_Waiting ww = new Window_Waiting("연결 중입니다..");
							double[] arr = obj as double[];
							if(arr != null && arr.Length >= 2)
							{
								ww.Left = arr[0] - ww.Width / 2;
								ww.Top = arr[1] - ww.Height / 2;
							}
							ww.Topmost = true;
							ww.Show();
							System.Windows.Threading.Dispatcher.Run();
						}
						catch(Exception ex)
						{
							Log.PrintError(ex.Message, "Classes.SSHController.Connect.th_popup");
						}
					}
				);
			th_popup.SetApartmentState(System.Threading.ApartmentState.STA);
			th_popup.IsBackground = true;
			Point _pt = WindowMain.current.PointToScreen(new Point(0, 0));
			th_popup.Start(new double[] { _pt.X + WindowMain.current.ActualWidth / 2, _pt.Y + WindowMain.current.ActualHeight / 2 });

			System.Threading.AutoResetEvent resetEvent_connect = new System.Threading.AutoResetEvent(false);
			System.ComponentModel.BackgroundWorker bw_connect = new System.ComponentModel.BackgroundWorker();
			bw_connect.DoWork += delegate (object sender, System.ComponentModel.DoWorkEventArgs e)
			{
				try
				{
					ssh = new SshClient(ip, port, id, password);
					if(timeout_connect_ms != NO_TIMEOUT)
						ssh.ConnectionInfo.Timeout = new TimeSpan(0, 0, 0, 0, timeout_connect_ms);
					ssh.Connect();
					sftp = new SftpClient(ip, port, id, password);
					if(timeout_connect_ms != NO_TIMEOUT)
						sftp.ConnectionInfo.Timeout = new TimeSpan(0, 0, 0, 0, timeout_connect_ms);
					sftp.Connect();

					if(ssh.IsConnected)
					{
						shell_stream = ssh.CreateShellStream("customCommand", 80, 24, 800, 600, 4096);
						shell_stream_reader = new StreamReader(shell_stream);
						shell_stream_writer = new StreamWriter(shell_stream);
					}

					Log.PrintLog("ip = " + ip + ", port = " + port, "Classes.SSHController.Connect.bw_connect");
					Log.PrintConsole("id = " + id, "Classes.SSHController.Connect.bw_connect");
				}
				catch(Exception ex)
				{
					Log.PrintError(ex.Message, "Classes.SSHController.Connect.bw_connect");
					//Log.ErrorIntoUI(ex.Message, "Connect", Status.current.richTextBox_status);
					e.Result = ex.Message;
				}
				resetEvent_connect.Set();
				return;
			};
			bw_connect.RunWorkerCompleted += delegate (object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
			{
				if(e.Result as string != null)
				{
					Log.ErrorIntoUI(e.Result as string, "Connect", Status.current.richTextBox_status);
				}
			};
			bw_connect.RunWorkerAsync("MyName");
			resetEvent_connect.WaitOne();

			try
			{
				th_popup.Abort();
			}
			catch(Exception e)
			{
				Log.PrintError("th_popup.Abort() : " + e.Message, "Classes.SSHController.Connect");
			}


			//Log.PrintConsole("Connected", "Classes.SSHController.ReConnect");

			//LinuxDirectoryViewer w = new LinuxDirectoryViewer(_PullListInDirectory("/home/cofile"));
			//w.Show();

			return retval;
		}
		#endregion
		#region sftp upload, download
		private static bool CreateRemoteDirectory(string remote_dir_path)
		{
			string _path = "/";
			try
			{
				string[] split = remote_dir_path.Split('/');

				for(int i = 0; i < split.Length; i++)
				{
					_path += split[i] + "/";
					if(!sftp.Exists(_path))
					{
						string com = "mkdir '" + _path + "'";
						//SendCommand(com);
						ssh.RunCommand(com);
					}
					//sftp.CreateDirectory(_path);
				}
				return true;
			}
			catch(Exception ex)
			{
				Log.PrintError(ex.Message + "/ path = " + _path, "Classes.SSHController.CreateRemoteDirectory");
				Log.ErrorIntoUI(ex.Message + "/ path = " + _path, "CreateRemoteDirectory", Status.current.richTextBox_status);
				return false;
			}
		}
		public static string UploadFile(string local_path, string remote_directory, string remote_backup_dir = null)
		{
			//LinuxTreeViewItem.ReconnectServer();
			//LinuxTreeViewItem.ReConnect();
			if(!SSHController.ReConnect(timeout_connect_ms))
				return null;


			string remote_file_path = null;
			try
			{
				FileInfo fi = new FileInfo(local_path);
				if(fi.Exists)
				{
					//FileStream fs = File.Open(local_path, FileMode.Open, FileAccess.Read);
					remote_file_path = remote_directory + fi.Name;
					//if(isOverride)
					//{
					//	sftp.UploadFile(fs, remote_file_path);
					//	Log.PrintConsole(fi.Name + " => " + remote_file_path, "upload file"/*, test4.m_wnd.richTextBox_status*/);
					//}

					if(CreateRemoteDirectory(remote_directory))
					{
						if(remote_backup_dir != null && sftp.Exists(remote_file_path))
						{
							if(CreateRemoteDirectory(remote_backup_dir))
							{
								DateTime dt;
								//dt = DateTime.Now;

								// 원래는 서버시간으로 생성해야함.
								// 서버마다 시간을 알수있는 함수가 다를수 있으므로 sftp를 사용
								// 위 if 문의 sftp.Exists(remote_file_path) 에서 엑세스한 시간을 가져옴.
								dt = sftp.GetLastAccessTime(remote_file_path);

								// '파일 명'.'연도'.'달'.'날짜'.'시간'.'분'.'초'.backup 형식으로 백업파일 생성
								string remote_backup_file = remote_backup_dir + fi.Name + dt.ToString(".yyyy.MM.dd.hh.mm.ss") + ".backup";
								string com = @"cp '" + remote_file_path + "' '" + remote_backup_file + "'";
								ssh.RunCommand(com);
								//SendCommand(com);
							}
							else
							{
								//fs.Close();
								Log.PrintError("Create Directory Error", "Classes.SSHController.UploadFile");
								return null;
							}

						}

						//sftp.UploadFile(fs, remote_file_path, true);
						string str = FileContoller.Read(local_path);
						string str1 = "echo \"" + str.Replace("\r", "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("$", "\\$") + "\" > \"" + remote_file_path + "\"";
						ssh.RunCommand(str1);
						//SendCommand(str1);

						Log.PrintLog(fi.Name + " => " + remote_file_path, "Classes.SSHController.UploadFile");
					}
					else
					{
						remote_file_path = null;
					}
					//fs.Close();
				}
				else
				{
					Log.ErrorIntoUI("Not Exist File", "upload file", Status.current.richTextBox_status);
					Log.PrintError("Not Exist File", "Classes.SSHController.UploadFile");
					return null;
				}
			}
			catch(Exception e)
			{
				Log.ErrorIntoUI(e.Message + "/ " + local_path + " -> " + remote_directory, "UploadFile", Status.current.richTextBox_status);
				Log.PrintError(e.Message + "/ " + local_path + " -> " + remote_directory, "Classes.SSHController.UploadFile");
				return null;
			}
			return remote_file_path;
		}
		public static bool DownloadFile(string local_path_folder, string remote_path_file, string local_file_name = null, string remote_filename = null, bool bLog = true)
		{
			//LinuxTreeViewItem.ReconnectServer();
			//LinuxTreeViewItem.ReConnect();
			if(!SSHController.ReConnect(timeout_connect_ms))
				return false;

			try
			{
				string local;
				if(local_file_name != null)
					local = local_path_folder + local_file_name;
				else
				{
					if(remote_filename == null)
					{
						string[] split = remote_path_file.Split('/');
						remote_filename = split[split.Length - 1];
					}
					local = local_path_folder + remote_filename;
				}

				if(!FileContoller.CreateDirectory(local_path_folder))
					return false;

				FileStream fs = new FileStream(local, FileMode.Create);
				if(sftp.Exists(remote_path_file))
				{
					sftp.DownloadFile(remote_path_file, fs);
					Log.PrintLog(remote_path_file + " => " + local, "Classes.SSHController.DownloadFile");
				}
				else
				{
					Log.PrintLog("Not found file " + remote_path_file, "Classes.SSHController.DownloadFile");
				}
				fs.Close();
			}
			catch(Exception e)
			{
				if(bLog)
				{
					Log.ErrorIntoUI(e.Message, "downloadFile", Status.current.richTextBox_status);
				}
				Log.PrintError(e.Message, "Classes.SSHController.DownloadFile");
				return false;
			}
			return true;
		}
		public static bool MoveFileToLocal(string local_path_folder, string remote_path_file, string local_file_name, double try_time_out_ms)
		{
			bool retval = false;
			if(try_time_out_ms != 0)
			{
				DateTime timeout = DateTime.Now.AddMilliseconds(try_time_out_ms);
				while(timeout > DateTime.Now && retval == false)
				{
					tick();
					retval = DownloadFile(local_path_folder, remote_path_file, local_file_name, bLog: false);
					//if(!DownloadFile(local_path_folder, remote_path_file, local_file_name, bLog: false))
					//	return false;
				}
				if(retval == false)
				{
					Log.ErrorIntoUI(remote_path_file + " -> " + local_path_folder, "MoveFileToLocal", Status.current.richTextBox_status);
					Log.PrintError(remote_path_file + " -> " + local_path_folder, "Classes.SSHController.MoveFileToLocal");
					return retval;
				}
				ssh.RunCommand("rm -rf '" + remote_path_file + "'");
				//SendCommand("rm -rf " + remote_path_file);

				return true;
			}
			else
			{
				tick();
				retval = DownloadFile(local_path_folder, remote_path_file, local_file_name, bLog: false);
				if(retval == true)
				{
					ssh.RunCommand("rm -rf '" + remote_path_file + "'");
				}
				return retval;
			}

		}
		public static bool DownloadDirectory(string local_folder_path, string remote_directory_path, Regex filter_file = null, Regex filter_except_dir = null)
		{
			//LinuxTreeViewItem.ReconnectServer();
			//LinuxTreeViewItem.ReConnect();
			if(!SSHController.ReConnect(timeout_connect_ms))
				return false;

			try
			{
				if(!FileContoller.CreateDirectory(local_folder_path))
					return false;

				SftpFile[] files = PullListInDirectory(remote_directory_path);
				if(files == null)
					return false;
				for(int i = 0; i < files.Length; i++)
				{
					if(files[i].Name == "." || files[i].Name == "..")
						continue;

					if(files[i].IsDirectory
						&& (filter_except_dir == null || !filter_except_dir.IsMatch(files[i].Name)))
					{
						string re_local_folder_path = local_folder_path + files[i].Name + @"\";
						DownloadDirectory(re_local_folder_path, files[i].FullName, filter_file, filter_except_dir);
						continue;
					}

					if(filter_file != null && !filter_file.IsMatch(files[i].Name))
						continue;

					DownloadFile(local_folder_path, files[i].FullName, files[i].Name, files[i].Name);
				}
			}
			catch(Exception e)
			{
				Log.ErrorIntoUI(e.Message, "DownloadDirectory", Status.current.richTextBox_status);
				Log.PrintError(e.Message, "Classes.SSHController.DownloadDirectory");
				return false;
			}
			return true;
		}
		#endregion
		#region ssh send, recv
		private const double SECOND_TIMEOUT_READ = 2;
		public static string SendNReadBlocking(string command, int count_read_outputline, double sec_timeout_read = SECOND_TIMEOUT_READ)
		{
			string retval = null;
			if(shell_stream_read_timer != null)
				shell_stream_read_timer.Stop();

			if(SendCommand(command))
			{
				retval = ReadLinesBlocking(command, count_read_outputline, sec_timeout_read);
			}

			if(shell_stream_read_timer != null)
				shell_stream_read_timer.Start();
			return retval;
		}
		private static bool SendCommand(string command)
		{
			//LinuxTreeViewItem.ReconnectServer();
			//LinuxTreeViewItem.ReConnect();
			if(!SSHController.ReConnect(timeout_connect_ms))
				return false;

			try
			{
				// send
				if(shell_stream_reader != null)
				{
					//readDummyMessageBlocking(null);

					shell_stream_writer.Write(command);
					shell_stream_writer.Write("\n");
					shell_stream_writer.Flush();
					Log.PrintLog(command, "Classes.SSHController.sendCommand");

					bStart = false;

					return true;
				}
			}
			catch(Exception ex)
			{
				Log.ErrorIntoUI(ex.Message, "send command", Status.current.richTextBox_status);
				Log.PrintError(ex.Message, "Classes.SSHController.sendCommand");
				if(ServerList.selected_serverinfo_panel == null)
					return false;

				string ip = ServerList.selected_serverinfo_panel.Serverinfo.ip;
				//string id = ServerList.selected_serverinfo_textblock.serverinfo.id;
				//string password = ServerList.selected_serverinfo_textblock.serverinfo.password;
				int port = ServerList.selected_serverinfo_panel.Serverinfo.port;

				//DisConnect();
				SSHController.Connect(ip, port, timeout_connect_ms);
				if(IsConnected)
					readDummyMessageBlocking();
			}
			return false;
		}
		private static string ReadLinesBlocking(string cmd, int cnt_read_outputline, double sec_timeout = SECOND_TIMEOUT_READ)
		{
			char newLine = '\n';
			int size_buffer = 4096;
			char[] buffer = new char[size_buffer];

			string str_read = "";
			try
			{
				DateTime now = DateTime.Now;
				DateTime timeout = DateTime.Now.AddSeconds(sec_timeout);
				while(timeout > (now = DateTime.Now) &&
					(str_read.Length < cmd.Length
					|| str_read.LastIndexOf(cmd) < 0
					// cnt_read_line + 2 => cmd (newLine) 아웃풋 (newLine) [asd@local]$
					|| str_read.Split(newLine).Length < cnt_read_outputline + 2))
				{
					int cnt = shell_stream_reader.Read(buffer, 0, size_buffer);
					if(cnt > 0)
					{
						string _read = new string(buffer, 0, cnt);
						str_read += _read;
						timeout = DateTime.Now.AddSeconds(sec_timeout);
					}
				}
				if(timeout < now)
					Log.PrintError("timeout", "Classes.SSHController.ReadLinesBlocking");
				//if(timeout < now)
				//{
				//	string[] _split = str_read.Split('\n');
				//	if(_split.Length >= 2)
				//	{
				//		return _split[_split.Length - 2];
				//	}
				//	else
				//		return null;
				//}

				string[] split = str_read.Split('\n');

				int offset = split[0].Length;
				//int length = str_read.Length - offset - split[split.Length - 1].Length;
				if(split.Length > cnt_read_outputline)
				{
					//int length = split[1].Length;
					int length = 0;
					for(int i = 1; i < cnt_read_outputline + 1; i++)
					{
						length += split[i].Length + 1;
					}
					if(length > 0)
						length--;

					char[] newLines = new char[] {'\n', '\r' };

					string retval = str_read.Substring(offset, length).Trim(newLines);
					Log.PrintLog(retval, "Classes.SSHController.ReadLinesBlocking");
					return retval;
				}

				return null;

			}
			catch(Exception e)
			{
				Log.PrintError(e.Message, "Classes.SSHController.ReadLinesBlocking");
			}
			return null;
		}
		#endregion
		#region Not Portable
		private static bool InitConnecting()
		{
			if(shell_stream_read_timer != null)
				shell_stream_read_timer.Stop();

			envCoHome = null;
			return true;
		}
		private static bool InitConnected()
		{
			if(shell_stream_read_timer == null)
			{
				shell_stream_read_timer = new DispatcherTimer();
				shell_stream_read_timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
				shell_stream_read_timer.Tick += Shell_stream_read_timer_Tick;
			}

			if(WindowMain.current != null)
				WindowMain.current.Clear();

			readDummyMessageBlocking();

			if(shell_stream_read_timer != null)
				shell_stream_read_timer.Start();

			if(WindowMain.current != null)
			{
				WindowMain.current.Changed_server_name = ServerList.selected_serverinfo_panel.Serverinfo.name;
				WindowMain.current.bUpdateInit(false);
			}


			if(ServerList.selected_serverinfo_panel != null
				&& ServerList.selected_serverinfo_panel.Serverinfo != null)
			{
				ServerList.connected_serverinfo_panel = ServerList.selected_serverinfo_panel;
				ServerList.connected_serverinfo_panel.IsConnected = true;
			}

			
			if(LoadEnvCoHome() == ReturnValue.Fail.LOAD_CO_HOME)
				;//return false;

			EditCoHome();
			//MahApps.Metro.Controls.Dialogs.MetroDialogSettings settings = new MahApps.Metro.Controls.Dialogs.MetroDialogSettings()
			//{
			//	AffirmativeButtonText = "Yes",
			//	NegativeButtonText = "No"
			//};
			//WindowMain.current.ShowMessageDialog("$CO_HOME", "$CO_HOME = " + EnvCoHome + "\n환경변수를 수정하시겠습니까?", MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegative, settings: settings, affirmative_callback: EditCoHome);
			return true;
		}
		public static void EditCoHome()
		{
			Window_EnvSetting wms = new Window_EnvSetting();
			if(ServerList.selected_serverinfo_panel != null)
			{
				Point pt = ServerList.selected_serverinfo_panel.PointToScreen(new Point(0, 0));
				wms.Left = pt.X;
				wms.Top = pt.Y;
			}
			wms.textBox_cohome.Text = EnvCoHome;
			if(wms.ShowDialog() == true)
			{
				EnvCoHome = wms.textBox_cohome.Text;
			}
		}

		public static bool ReConnect(int timeout_ms = NO_TIMEOUT)
		{
			bool retval = true;

			if(ServerList.selected_serverinfo_panel == null)
				return false;

			string ip = ServerList.selected_serverinfo_panel.Serverinfo.ip;
			//string id = ServerList.selected_serverinfo_textblock.serverinfo.id;
			//string password = ServerList.selected_serverinfo_textblock.serverinfo.password;
			int port = ServerList.selected_serverinfo_panel.Serverinfo.port;

			if(!CheckConnection(ip, port))
			{
				if(!InitConnecting())
					return false;

				retval = Connect(ip, port, timeout_ms);

				if(!IsConnected || !InitConnected())
					retval = false;
			}
			return retval;
		}
		public static bool DisConnect()
		{
			envCoHome = null;
			if(ssh != null)
				ssh.Disconnect();
			if(sftp != null)
				sftp.Disconnect();
			if(WindowMain.current != null)
				WindowMain.current.Clear();
			if(ServerList.selected_serverinfo_panel != null
				&& ServerList.selected_serverinfo_panel.Serverinfo != null)
				ServerList.connected_serverinfo_panel.IsConnected = false;
			return true;
		}
		#endregion

		#region Read (ssh)
		static string read_line_ssh = "";
		static string[] view_log_start = new string[] {"inform"};
		static string[] view_error_start = new string[] {"error" };
		public static string view_message_caption = "";
		static bool bstart = false;
		static bool bStart
		{
			get { return bstart; }
			set
			{
				bstart = value;
				//if(!value)
				//	read_line_ssh = "";
			}
		}
		private static string read()
		{
			int size_buffer = 4096;
			char[] buffer = new char[size_buffer];

			string _read = "";
			try
			{
				int cnt = shell_stream_reader.Read(buffer, 0, size_buffer);

				_read = new string(buffer, 0, cnt);
				read_line_ssh += _read;/* Encoding.UTF8.GetString(buffer, 0, cnt);*/
				if(read_line_ssh.Length > 0)
				{
					//Log.Print(read_line_ssh, "read"/*, test4.m_wnd.richTextBox_status*/);
					int idx_newline = 0;
					if((idx_newline = read_line_ssh.IndexOf('\n')) >= 0
						|| (idx_newline = read_line_ssh.IndexOf('\r')) >= 0)
					{
						string line = read_line_ssh.Substring(0, idx_newline);
						//int i;
						//for(i = 0; i < view_log_start.Length; i++)
						//{
						//	if(line.Length > view_log_start[i].Length && line.Substring(0, view_log_start[i].Length).ToLower() == view_log_start[i])
						//	{
						//		Log.ViewMessage(line, view_message_caption, Status.current.richTextBox_status);
						//	}
						//}
						//if(i == view_log_start.Length)
						//{
						//	for(i = 0; i < view_error_start.Length; i++)
						//	{
						//		if(line.Length > view_error_start[i].Length && line.Substring(0, view_error_start[i].Length).ToLower() == view_error_start[i])
						//			Log.PrintError(line, view_message_caption, Status.current.richTextBox_status);
						//	}
						//	if(i == view_error_start.Length)
						//		Log.ViewUndefine(line, "__undefined][" + view_message_caption, Status.current.richTextBox_status);
						//}

						//if(line.IndexOf("Copyright (c) 2004-2016, eGlobal Systems, Co., Ltd.") >= 0)
						//{
						//	bStart = true;
						//}
						//else if(bStart)
						//{
						//	Log.ViewMessage(line, view_message_caption, Status.current.richTextBox_status);
						//}

						//if(line.IndexOf("\r") != line.LastIndexOf("\r") || line.IndexOf("\n") != line.LastIndexOf("\n"))
						//{ }
						//else
						Log.ViewMessage(line, view_message_caption, Status.current.richTextBox_status);


						Log.PrintConsole(line, "Classes.SSHController.read");
						read_line_ssh = read_line_ssh.Substring(idx_newline + 1);
					}
				}

			}
			catch(Exception e)
			{
				Log.ErrorIntoUI(e.Message, "read", Status.current.richTextBox_status);
				Log.PrintError(e.Message, "Classes.SSHController.read");
			}
			return _read;
		}
		private static void readDummyMessageBlocking()
		{
			// 수정중..

			int size_buffer = 4096;
			char[] buffer = new char[size_buffer];
			//shell_stream.Flush();
			DateTime end = DateTime.Now.AddMilliseconds(3000);
			for(int i = 0; /*i < 1000 && */end > DateTime.Now; i++)
			{
				int cnt = shell_stream_reader.Read(buffer, 0, size_buffer);
				//if(cnt > 0)
				//	end = DateTime.Now.AddMilliseconds(1000);
			}
			return;

			//shell_stream_read_timer.Stop();

			//int size_buffer = 4096;
			//char[] buffer = new char[size_buffer];
			//string message = "";
			//string ret_message = null;
			//try
			//{
			//	// '$' or '#' 의 값이 나올떄까지 리드
			//	// 대신 output 에 $ 이나 # 이 있으면 안됨.
			//	string str_recv = "";
			//	while(str_recv.Length < 1 || (str_recv.LastIndexOf('$') < 0 && str_recv.LastIndexOf('#') < 0))
			//	{
			//		int cnt = shell_stream_reader.Read(buffer, 0, size_buffer);
			//		str_recv += new string(buffer, 0, cnt);/* Encoding.UTF8.GetString(buffer, 0, cnt);*/
			//	}
			//	int l = str_recv.Length;
			//	Log.PrintConsole(str_recv, "readMessageBlocking][" + l);
			//	//Log.ViewMessage(str_recv, "readMessageBlocking][" + l, Status.current.richTextBox_status);
			//	if(cmd_send == null || cmd_send == "")
			//		return cmd_send;

			//	// 명령어 전까지 자르기.(dummy 제거)
			//	//if(str_recv.LastIndexOf(cmd_send) >= 0)
			//	//	message = str_recv.Substring(str_recv.LastIndexOf(cmd_send));
			//	// 명령어가 이상한 값이 들어가므로.. 명령어 라인까지 자르기 (전제조건 : 명령어 부터 리시브를 받는다)
			//	if(str_recv.IndexOf('\n') >= 0 && str_recv.Length > str_recv.IndexOf('\n') + 1)
			//	{
			//		message = str_recv.Substring(str_recv.IndexOf('\n') + 1);
			//	}

			//	// 명령어 라인과 
			//	// 마지막라인 '[~] $' 제거
			//	if(message.IndexOf('\n') >= 0 && message.IndexOf('\n') + 1 < message.Length)
			//	{
			//		// 명령어는 이미 잘려있기 때문에
			//		//int idx = message.IndexOf('\n');
			//		int idx = 0;

			//		int len = message.LastIndexOf('\n') - idx;
			//		if(len > 0)
			//			ret_message = message.Substring(idx + 1, len);
			//	}


			//}
			//catch(Exception e)
			//{
			//	Log.PrintError(e.Message, "readMessageBlocking", Status.current.richTextBox_status);
			//}

			//shell_stream_read_timer.Start();
			//return ret_message;
		}
		private static string readCofileMessageBlocking()
		{
			shell_stream_read_timer.Stop();

			int size_buffer = 4096;
			char[] buffer = new char[size_buffer];
			string message = "";
			string ret_message = null;
			try
			{
				// '$' 의 값이 나올떄까지 리드
				string str_recv = "";
				while(str_recv.Length < 1 || str_recv.LastIndexOf('$') < 0)
				{
					int cnt = shell_stream_reader.Read(buffer, 0, size_buffer);
					str_recv += new string(buffer, 0, cnt);/* Encoding.UTF8.GetString(buffer, 0, cnt);*/
				}
				int l = str_recv.Length;
				Log.PrintConsole(str_recv, "Classes.SSHController.readCofileMessageBlocking");

				// Copyright 부분 다음 라인까지 자르기
				string Copyright = "Copyright";
				if(str_recv.LastIndexOf(Copyright) >= 0)
				{
					str_recv = str_recv.Substring(str_recv.LastIndexOf(Copyright));
					if(str_recv.IndexOf('\n') >= 0 && str_recv.Length > str_recv.IndexOf('\n') + 1)
						str_recv = str_recv.Substring(str_recv.IndexOf('\n') + 1);
					if(str_recv.IndexOf('\n') >= 0 && str_recv.Length > str_recv.IndexOf('\n') + 1)
						message = str_recv.Substring(str_recv.IndexOf('\n') + 1);
				}

				// 마지막라인 '[~] $' 제거
				if(message.IndexOf('\n') >= 0 && message.IndexOf('\n') + 1 < message.Length)
				{
					int len = message.LastIndexOf('\n');
					if(len > 0)
						ret_message = message.Substring(0, len + 1);
				}
			}
			catch(Exception e)
			{
				Log.ErrorIntoUI(e.Message, "readMessageBlocking", Status.current.richTextBox_status);
				Log.PrintError(e.Message, "Classes.SSHController.readCofileMessageBlocking");
			}

			shell_stream_read_timer.Start();
			return ret_message;
		}

		private static void Shell_stream_read_timer_Tick(object sender, EventArgs e)
		{
			tick();
		}
		private static void tick()
		{
			if(shell_stream_reader != null)
			{
				string str = read();
				if(str.Length > 0)
					Log.PrintConsole("[read] " + str, "Classes.SSHController.Shell_stream_read_timer_Tick");
			}
		}
		//private static string readCoHomeBlocking(string cmd_send)
		//{
		//	string newLine = System.Environment.NewLine;
		//	cmd_send += newLine;
		//	int cmd_length = cmd_send.Length;
		//	shell_stream_read_timer.Stop();
		//	int size_buffer = 4096;
		//	char[] buffer = new char[size_buffer];
		//	string prevstr = "";
		//	string line = "";
		//	string env_co_home = null;
		//	try
		//	{
		//		DateTime now = DateTime.Now;
		//		DateTime timeout = now.AddSeconds(3);
		//		while(timeout > (now = DateTime.Now)
		//				// 보낸 명령어 길이보다 많이 수신해야함.
		//			&& (prevstr.Length < cmd_length 
		//				// 보낸 명령어를 수신했는지 검사
		//				|| prevstr.LastIndexOf(cmd_send) < 0 
		//				// 보낸 명령어를 수신한 뒤에 개행을 수신했는지 검사
		//				|| prevstr.IndexOf(newLine, prevstr.LastIndexOf(cmd_send) + cmd_length) < 0))
		//		{
		//			int cnt = shell_stream_reader.Read(buffer, 0, size_buffer);
		//			prevstr += new string(buffer, 0, cnt);/* Encoding.UTF8.GetString(buffer, 0, cnt);*/
		//		}
		//		if(timeout < now)
		//		{
		//			return null;
		//		}

		//		int startline = 0;
		//		startline = prevstr.LastIndexOf(cmd_send);
		//		line = prevstr.Substring(startline + cmd_length);
		//		//if(startline > 2 && prevstr[startline - 2] != '$')
		//		//{
		//		//	prevstr = line;
		//		//	while((prevstr.Length < cmd_length || prevstr.IndexOf(cmd_send) < 0 || prevstr.Length <= prevstr.IndexOf(cmd_send) + cmd_length))
		//		//	{
		//		//		int cnt = shell_stream_reader.Read(buffer, 0, size_buffer);
		//		//		prevstr += new string(buffer, 0, cnt);/* Encoding.UTF8.GetString(buffer, 0, cnt);*/
		//		//	}
		//		//	startline = 0;
		//		//	startline = prevstr.IndexOf(cmd_send);
		//		//	line = prevstr.Substring(startline + cmd_length);
		//		//}

		//		//bool bGet_end = false;
		//		while(line.Length < cmd_length || line.IndexOf('\n') < 0)
		//		{
		//			int cnt = shell_stream_reader.Read(buffer, 0, size_buffer);
		//			line += new string(buffer, 0, cnt);/* Encoding.UTF8.GetString(buffer, 0, cnt);*/
		//		}
		//		string str = line.ToString();
		//		int idx_end = str.IndexOf('\r');

		//		if(idx_end > 0)
		//		{
		//			env_co_home = str.Substring(0, idx_end);
		//		}
		//		else
		//			env_co_home = "";

		//		Log.PrintLog(str, "Classes.SSHController.readCoHomeBlocking");

		//	}
		//	catch(Exception e)
		//	{
		//		Log.ErrorIntoUI(e.Message, "readCoHomeBlocking", Status.current.richTextBox_status);
		//		Log.PrintError(e.Message, "Classes.SSHController.readCoHomeBlocking");
		//	}

		//	shell_stream_read_timer.Start();
		//	return env_co_home;
		//}
		#endregion


		#region Poll Linux Directory
		private static SftpFile[] _PullListInDirectory(string Path)
		{
			IEnumerable<SftpFile> files = null;
			try
			{
				files = sftp.ListDirectory(Path).OrderBy(x => x.FullName);
			}
			catch(Exception e)
			{
				Log.ErrorIntoUI(e.Message, "Pull Directory", Status.current.richTextBox_status);
				Log.PrintError(e.Message, "Classes.SSHController._PullListInDirectory");
			}

			Log.PrintLog("Path = " + Path, "Classes.SSHController._PullListInDirectory");
			if(files == null)
				return null;

			return files.ToArray();
		}
		public static string WorkingDirectory
		{
			get
			{
				if(!SSHController.ReConnect(timeout_connect_ms))
					return null;
				return sftp.WorkingDirectory;
			}
		}
		public static SftpFile[] PullListInDirectory(string Path)
		{
			if(!SSHController.ReConnect(timeout_connect_ms))
				return null;
			if(Path == null)
				return null;

			return _PullListInDirectory(Path);
		}
		#endregion

		#region Excute Cofile Command
		private static string envCoHome = null;
		public static string EnvCoHome
		{
			set
			{
				envCoHome = value;
			}
			get
			{
				if(ReConnect(timeout_connect_ms) && envCoHome == null)
					LoadEnvCoHome();
				return envCoHome;
			}
		}
		private static int LoadEnvCoHome()
		{
			string command = "echo $CO_HOME";
			string co_home = SendNReadBlocking(command, 1, 10);
			if(co_home == null || co_home == "")
			{
				Log.ErrorIntoUI("not defined $CO_HOME\r", "load $CO_HOME", Status.current.richTextBox_status);
				Log.PrintError("not defined $CO_HOME", "Classes.SSHController._LoadEnvCoHome");
				return ReturnValue.Fail.LOAD_CO_HOME;
			}
			envCoHome = co_home;
			Log.PrintLog("$CO_HOME = " + co_home, "Classes.SSHController._LoadEnvCoHome");
			return 0;
		}

		public static string add_path_config_upload = "/var/conf/";
		public static string add_path_bin = "/bin/";
		public static string add_path_run_cofile = add_path_bin + "cofile";
		public static string MakeCommandRunCofile(string path_run, CofileType type, bool isEncrypt, string path, string configname, bool isDirectory)
		{
			string str = path_run;
			str += " " + type.ToString().ToLower();

			if(isEncrypt)
				str += " -e";
			else
				str += " -d";

			string[] split = path.Split('/');
			string filename = split[split.Length - 1];

			if(!isDirectory)
				str += " -f '" + filename + "'";

			if(configname != null)
				str += " -c '" + configname + "'";

			if(isDirectory)
				str += " -id '" + path + "'";
			else
				str += " -id '" + path.Substring(0, path.Length - filename.Length) + "'";

			return str;
		}
		public static string MakeCommandRunCofilePreview(string path_run, CofileType type, bool isEncrypt, string path, string configname, bool isDirectory)
		{
			string str = MakeCommandRunCofile(path_run, type, isEncrypt, path, configname, isDirectory);

			string[] split = path.Split('/');
			string filename = split[split.Length - 1];

			if(isDirectory)
				str += " -od '" + path + "'";
			else
				str += " -od '" + path.Substring(0, path.Length - filename.Length) + "'";

			str += " -oe " + Cofile.PreviewExtension;
			return str;
		}
		public static string ConvertPathLocalToRemoteIfExist(string local_path_configfile, string cur_local_root_path)
		{
			if(local_path_configfile == null)
				return null;

			string env_co_home = EnvCoHome;
			string remote_path_configfile_dir = env_co_home + add_path_config_upload + ServerList.selected_serverinfo_panel.Serverinfo.id + "/";

			string remote_path_configfile = remote_path_configfile_dir;
			string retval = null;
			try
			{
				if(local_path_configfile.Length > cur_local_root_path.Length
					&& local_path_configfile.Substring(0, cur_local_root_path.Length) == cur_local_root_path)
				{
					remote_path_configfile += local_path_configfile.Substring(cur_local_root_path.Length).Replace('\\', '/');
					if(sftp.Exists(remote_path_configfile))
						retval = remote_path_configfile;
				}
				else
				{
					string[] split = local_path_configfile.Split('\\');
					remote_path_configfile = remote_path_configfile_dir + split[split.Length - 1];
					if(sftp.Exists(remote_path_configfile))
						retval = remote_path_configfile;
				}
			}
			catch(Exception e)
			{
				Log.ErrorIntoUI(e.Message, "Config File Check", Status.current.richTextBox_status);
				Log.PrintError(e.Message, "Classes.SSHController.ConvertPathLocalToRemote");
			}
			if(retval == null)
			{
				Log.ErrorIntoUI("Check the Path Config file [path = " + Cofile.current.SelectedConfigLocalPath + "]"
					, "Config Path Error", Status.current.richTextBox_status);

				string message = "Check the Path Config file" + Environment.NewLine + "path = " + Cofile.current.SelectedConfigLocalPath;
				WindowMain.current.ShowMessageDialog("Config Path Error", message);
				Log.PrintError(message, "Classes.SSHController.ConvertPathLocalToRemote");
			}
			return retval;
		}
		public static bool SendNRecvCofileCommand(IEnumerable<Object> selected_list, bool isEncrypt, bool is_tail_deamon)
		{
			Status.current.Clear();

			if(Cofile.current.SelectedConfigLocalPath == null)
				return false;

			string remote_configfile_path = "";
			//string remote_configfile_path = ConvertPathLocalToRemoteIfExist(Cofile.current.SelectedConfigLocalPath, ConfigOption.CurRootPathLocal);
			//if(remote_configfile_path == null)
			//	return false;

			//string remote_file_path = UploadFile(Cofile.current.Selected_config_file_path, remote_directory);
			//if(remote_file_path == null)
			//	return false;

			string env_co_home = EnvCoHome;
			List <LinuxTreeViewItem> parents = new List<LinuxTreeViewItem>();
			var enumerator = selected_list.GetEnumerator();
			for(int i = 0; enumerator.MoveNext(); i++)
			{
				LinuxTreeViewItem ltvi = enumerator.Current as LinuxTreeViewItem;

				CofileUI.UserControls.Cofile.LinuxListViewItem llvi = enumerator.Current as CofileUI.UserControls.Cofile.LinuxListViewItem;
				if(llvi != null)
					ltvi = llvi.LinuxTVI as LinuxTreeViewItem;

				if(ltvi == null)
					break;

				//string str = LinuxTreeViewItem.selected_list[i].Path.Substring(env_co_home.Length);
				string send_cmd;
				send_cmd = MakeCommandRunCofile(env_co_home + add_path_run_cofile, Cofile.current.GetSelectedType(), isEncrypt, ltvi.Path, remote_configfile_path, ltvi.IsDirectory);
				//SendCommandOnce(send_cmd);
				if(!is_tail_deamon)
					send_cmd += " -1";

				if(!SendCommand(send_cmd))
					return false;
				//string ret = readMessageBlocking(send_cmd);

				//string ret = readCofileMessageBlocking();
				//string caption = isEncrypt ? "Encrypt" : "Decrypt";
				//Log.ViewMessage(ret, caption, Status.current.richTextBox_status);

				LinuxTreeViewItem parent = ltvi.Parent;
				if(parent != null && parents.IndexOf(parent) < 0)
					parents.Add(parent);
			}
			WindowMain.current.progressBar.Value = 0;
			for(int i = 0; i < parents.Count; i++)
				parents[i].RefreshChild();
			Cofile.current.RefreshListView(Cofile.cur_LinuxTreeViewItem);
			return true;
		}
		public static bool SendNRecvCofileCommandPreview(IEnumerable<Object> selected_list, bool isEncrypt)
		{
			Status.current.Clear();

			if(Cofile.current.SelectedConfigLocalPath == null)
				return false;

			string remote_configfile_path = "";
			//string remote_configfile_path = ConvertPathLocalToRemoteIfExist(Cofile.current.SelectedConfigLocalPath, ConfigOption.CurRootPathLocal);
			//if(remote_configfile_path == null)
			//	return false;

			string env_co_home = EnvCoHome;
			List <LinuxTreeViewItem> parents = new List<LinuxTreeViewItem>();
			var enumerator = selected_list.GetEnumerator();
			for(int i = 0; enumerator.MoveNext(); i++)
			{
				LinuxTreeViewItem ltvi = enumerator.Current as LinuxTreeViewItem;

				CofileUI.UserControls.Cofile.LinuxListViewItem llvi = enumerator.Current as CofileUI.UserControls.Cofile.LinuxListViewItem;
				if(llvi != null)
					ltvi = llvi.LinuxTVI as LinuxTreeViewItem;

				if(ltvi == null)
					break;

				string send_cmd = MakeCommandRunCofilePreview(env_co_home + add_path_run_cofile, Cofile.current.GetSelectedType(), isEncrypt, ltvi.Path, remote_configfile_path, ltvi.IsDirectory);

				if(!SendCommand(send_cmd))
					return false;
				//read_line_ssh = "";
				//DateTime dt = DateTime.Now.AddSeconds(5);
				//while(dt > DateTime.Now && read_line_ssh.Length <= 0)
				//	yield return null;


				//string ret = readCofileMessageBlocking();
				//string caption = isEncrypt ? "Encrypt" : "Decrypt";
				//Log.ViewMessage(ret, caption, Status.current.richTextBox_status);

				LinuxTreeViewItem parent = ltvi.Parent;
				if(parent != null && parents.IndexOf(parent) < 0)
					parents.Add(parent);
			}
			WindowMain.current.progressBar.Value = 0;
			for(int i = 0; i < parents.Count; i++)
				parents[i].RefreshChild();
			Cofile.current.RefreshListView(Cofile.cur_LinuxTreeViewItem);
			return true;
		}
		public static bool RunCofileCommand(string command)
		{
			string env_co_home = EnvCoHome;
			string path_bin = env_co_home + add_path_bin;
			if(!SendCommand(command))
				return false;
			//ssh.RunCommand(path_bin + command);
			return true;
		}
		#endregion


		#region Get DataBase (SFTP) (Cofile.db)
		static string db_name = "cofile.db";
		static string add_path_database = "/var/data/" + db_name;
		public static string GetDataBase(string local_folder, string local_file)
		{
			string remote_directory = EnvCoHome;
			if(remote_directory == null)
				return null;

			string remote_path_file = remote_directory + add_path_database;
			if(DownloadFile(local_folder, remote_path_file, local_file, db_name))
			{
				DownloadFile(local_folder, remote_path_file + "-shm", local_file + "-shm", db_name + "-shm");
				DownloadFile(local_folder, remote_path_file + "-wal", local_file + "-wal", db_name + "-wal");
				return local_folder + local_file;
			}
			return null;
		}
		#endregion


		#region Get Event Log (SSH)
		public static string GetEventLog(int n)
		{
			string env_co_home = EnvCoHome;
			string command = "tail -n" + n + " '" + env_co_home + "/var/log/event_log'";
			return SendNReadBlocking(command, n);
		}
		#endregion

		#region Get/Set Config File (SFTP)
		static string add_path_config_dir = @"/var/conf/";
		public static string GetConfig(string local_folder)
		{
			string remote_directory = EnvCoHome;
			if(remote_directory == null)
				return null;

			string remote_path_dir = remote_directory + add_path_config_dir;
			remote_path_dir += ServerList.selected_serverinfo_panel.Serverinfo.id + "/";
			if(!CreateRemoteDirectory(remote_path_dir))
				return null;

			//if(downloadDirectory(local_folder, remote_path_dir, new Regex(@"\.json$"), new Regex(@"^\.backup")))
			if(DownloadDirectory(local_folder, remote_path_dir, new Regex(@"\.json$")))
			{
				//JsonTreeViewItem.RemotePathRootDir = remote_path_dir;
				return local_folder;
			}
			return null;
		}
		public static string SetConfig(string local_file_path, string cur_local_root_path)
		{
			string remote_directory = EnvCoHome;
			if(remote_directory == null)
				return null;

			string remote_path_dir = remote_directory + add_path_config_dir + ServerList.selected_serverinfo_panel.Serverinfo.id + "/";
			if(local_file_path.Length > cur_local_root_path.Length
				&& local_file_path.Substring(0, cur_local_root_path.Length) == cur_local_root_path)
			{
				string added_path = local_file_path.Substring(cur_local_root_path.Length).Replace('\\', '/');
				string[] split = added_path.Split('/');
				added_path = added_path.Substring(0, added_path.Length - split[split.Length - 1].Length);

				remote_path_dir += added_path;
			}
			//string remote_path = ConvertPathLocalToRemote(local_file_path, cur_local_root_path);
			string remote_backup_path_dir = remote_directory + add_path_config_dir + @".backup/" + ServerList.selected_serverinfo_panel.Serverinfo.id + "/";
			return UploadFile(local_file_path, remote_path_dir, remote_backup_path_dir);
		}
		#endregion


		public static SftpFileTree GetListConfigFile()
		{
			string remote_directory = EnvCoHome;
			if(remote_directory == null)
				return null;

			string remote_path_dir = remote_directory + add_path_config_dir + ServerList.selected_serverinfo_panel.Serverinfo.id + "/";

			SftpFileTree.MakeTree(SftpFileTree.root, remote_path_dir);
			return SftpFileTree.root;
		}
	}

	public class SftpFileTree
	{
		public static SftpFileTree root = new SftpFileTree();
		private SftpFile file;
		public SftpFile File { get { return file; } set { file = value; } }
		public SftpFileTree parent = null;
		public List<SftpFileTree> children = new List<SftpFileTree>();
		private SftpFileTree() { parent = this; }
		public SftpFileTree(SftpFile _file)
		{
			File = _file;
		}

		public static void MakeTree(SftpFileTree cur, string remote_path_dir)
		{
			cur.children.Clear();

			SftpFile[] files = SSHController.PullListInDirectory(remote_path_dir);
			for(int i = 0; i < files.Length; i++)
			{
				if(files[i].Name == ".")
					continue;

				SftpFileTree child = new SftpFileTree(files[i]);

				cur.children.Add(child);
				child.parent = cur;

				if(files[i].IsDirectory)
				{
					if(files[i].Name == "..")
					{
						if(cur.parent != null)
							child.children = cur.parent.children;
					}
					else
						MakeTree(child, files[i].FullName);
				}
			}
		}
	}
}
