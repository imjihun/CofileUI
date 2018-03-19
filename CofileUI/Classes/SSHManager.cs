using CofileUI.UserControls;
using CofileUI.Windows;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace CofileUI.Classes
{
	public class SSHManager
	{
		private const int NO_TIMEOUT = -1;
		private const double SECOND_TIMEOUT_READ = 2;

		public SshClient ssh;
		public SftpClient sftp;
		public ShellStream shell_stream;
		public StreamReader shell_stream_reader;
		public StreamWriter shell_stream_writer;
		public DispatcherTimer shell_stream_read_timer;
		int timeout_connect_ms = NO_TIMEOUT;
		private string envCoHome = null;
		public string EnvCoHome
		{
			set
			{
				envCoHome = value;
			}
			get
			{
				if(envCoHome == null)
					LoadEnvCoHome();
				return envCoHome;
			}
		}

		public string name;
		public string ip;
		public int port;
		public string id = null;

		public bool IsConnected
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

		public SSHManager(string _name, string _ip, int _port, string _id = null)
		{
			name = _name;
			ip = _ip;
			port = _port;
			id = _id;
		}

		#region Connect
		private bool CheckConnection(BaseClient client, string ip, int port, string id = null)
		{
			if((client == null || !client.IsConnected)
					|| (client.ConnectionInfo.Host != ip
						|| client.ConnectionInfo.Port != port
						|| (id != null && client.ConnectionInfo.Username != id)))
				return false;

			return true;
		}
		private bool CheckConnection(string ip, int port)
		{
			if(CheckConnection(sftp, ip, port) && CheckConnection(ssh, ip, port))
				return true;

			return false;
		}
		private bool Connect(string ip, int port, int timeout_ms = NO_TIMEOUT)
		{
			bool retval = true;

			Window_LogIn wl = new Window_LogIn();
			if(WindowMain.current != null)
			{
				Point pt = WindowMain.current.PointToScreen(new Point((WindowMain.current.Width - wl.Width)/2, (WindowMain.current.Height - wl.Height)/2));
				wl.Left = pt.X;
				wl.Top = pt.Y;
			}
			if(wl.ShowDialog() != true)
				return false;

			id = wl.Id;
			timeout_connect_ms = timeout_ms;
			string password = wl.Password;

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
					Log.PrintError(ex.Message, "Classes.SSHManager.Connect.th_popup");
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

					Log.PrintLog("ip = " + ip + ", port = " + port, "Classes.SSHManager.Connect.bw_connect");
					Log.PrintConsole("id = " + id, "Classes.SSHManager.Connect.bw_connect");
				}
				catch(Exception ex)
				{
					Log.PrintError(ex.Message, "Classes.SSHManager.Connect.bw_connect");
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
					WindowMain.current.ShowMessageDialog("Connect", "서버와 연결에 실패하였습니다.\n" + e.Result as string, MahApps.Metro.Controls.Dialogs.MessageDialogStyle.Affirmative);
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
				Log.PrintError("th_popup.Abort() : " + e.Message, "Classes.SSHManager.Connect");
			}


			//Log.PrintConsole("Connected", "Classes.SSHManager.ReConnect");

			//LinuxDirectoryViewer w = new LinuxDirectoryViewer(_PullListInDirectory("/home/cofile"));
			//w.Show();

			return retval;
		}

		private bool InitConnecting()
		{
			if(shell_stream_read_timer != null)
				shell_stream_read_timer.Stop();

			envCoHome = null;
			return true;
		}
		private bool InitConnected()
		{
			if(shell_stream_read_timer == null)
			{
				shell_stream_read_timer = new DispatcherTimer();
				shell_stream_read_timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
				shell_stream_read_timer.Tick += Shell_stream_read_timer_Tick;
			}

			ReadDummyMessageBlocking();

			if(shell_stream_read_timer != null)
				shell_stream_read_timer.Start();

			if(WindowMain.current != null)
			{
				WindowMain.current.Clear();
				WindowMain.current.ConnectedServerName = name;
				WindowMain.current.bUpdateInit(false);
			}


			//if(dsa.selected_serverinfo_panel != null
			//	&& dsa.selected_serverinfo_panel.Serverinfo != null)
			//{
			//	dsa.connected_serverinfo_panel = dsa.selected_serverinfo_panel;
			//	dsa.connected_serverinfo_panel.IsConnected = true;
			//}


			if(LoadEnvCoHome() == ReturnValue.Fail.LOAD_CO_HOME)
				;//return false;

			EditCoHome();
			return true;
		}
		public void EditCoHome()
		{
			Window_EnvSetting wms = new Window_EnvSetting();
			if(WindowMain.current != null)
			{
				Point pt = WindowMain.current.PointToScreen(new Point((WindowMain.current.Width - wms.Width)/2, (WindowMain.current.Height - wms.Height)/2));
				wms.Left = pt.X;
				wms.Top = pt.Y;
			}
			wms.textBox_cohome.Text = EnvCoHome;
			if(wms.ShowDialog() == true)
			{
				EnvCoHome = wms.textBox_cohome.Text;
			}
		}

		public bool ReConnect(int timeout_ms = NO_TIMEOUT)
		{
			bool retval = true;
			//if(serverinfo == null)
			//	return false;

			if(!CheckConnection(ip, port))
			{
				if(!InitConnecting())
					return false;

				retval = Connect(ip, port, timeout_ms);

				if(!IsConnected || !InitConnected())
				{
					retval = false;
				}
			}
			return retval;
		}
		public bool DisConnect()
		{
			envCoHome = null;
			if(ssh != null)
				ssh.Disconnect();
			if(sftp != null)
				sftp.Disconnect();
			if(WindowMain.current != null)
				WindowMain.current.Clear();
			return true;
		}
		#endregion

		#region ssh send, recv
		private int SendCommand(string command)
		{
			if(!IsConnected)
				return -1;

			try
			{
				// send
				if(shell_stream_reader != null)
				{
					shell_stream_writer.Write(command);
					shell_stream_writer.Write("\n");
					shell_stream_writer.Flush();
					Log.PrintLog(command, "Classes.SSHManager.sendCommand");

					return 0;
				}
			}
			catch(Exception ex)
			{
				Log.PrintError(ex.Message, "Classes.SSHManager.sendCommand");
			}
			return -2;
		}
		private void ReadTick()
		{
			if(!IsConnected)
				return;

			string read_line_ssh = "";
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
					int idx_newline = 0;
					if((idx_newline = read_line_ssh.IndexOf('\n')) >= 0
						|| (idx_newline = read_line_ssh.IndexOf('\r')) >= 0)
					{
						string line = read_line_ssh.Substring(0, idx_newline);
						Log.ViewMessage(line, "SSHManager", Status.current.richTextBox_status);


						Log.PrintConsole(line, "Classes.SSHManager.read");
						read_line_ssh = read_line_ssh.Substring(idx_newline + 1);
					}
				}

			}
			catch(Exception e)
			{
				Log.ErrorIntoUI(e.Message, "read", Status.current.richTextBox_status);
				Log.PrintError(e.Message, "Classes.SSHManager.read");
			}
			if(_read.Length > 0)
				Log.PrintConsole("[read] " + _read, "Classes.SSHManager.Shell_stream_read_timer_Tick");
		}
		private void ReadDummyMessageBlocking()
		{
			if(!IsConnected)
				return;

			int size_buffer = 4096;
			char[] buffer = new char[size_buffer];

			DateTime end = DateTime.Now.AddMilliseconds(3000);
			for(int i = 0; end > DateTime.Now; i++)
			{
				int cnt = shell_stream_reader.Read(buffer, 0, size_buffer);
			}
			return;
		}
		private string ReadLinesBlocking(string cmd, int cnt_read_outputline, double sec_timeout = SECOND_TIMEOUT_READ)
		{
			if(!IsConnected)
				return null;

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
					Log.PrintError("timeout", "Classes.SSHManager.ReadLinesBlocking");

				string[] split = str_read.Split('\n');

				int offset = split[0].Length;
				if(split.Length > cnt_read_outputline)
				{
					int length = 0;
					for(int i = 1; i < cnt_read_outputline + 1; i++)
					{
						length += split[i].Length + 1;
					}
					if(length > 0)
						length--;

					char[] newLines = new char[] {'\n', '\r' };

					string retval = str_read.Substring(offset, length).Trim(newLines);
					Log.PrintLog(retval, "Classes.SSHManager.ReadLinesBlocking");
					return retval;
				}

				return null;

			}
			catch(Exception e)
			{
				Log.PrintError(e.Message, "Classes.SSHManager.ReadLinesBlocking");
			}
			return null;
		}

		private void Shell_stream_read_timer_Tick(object sender, EventArgs e)
		{
			if(shell_stream_reader != null)
			{
				ReadTick();
			}
		}
		public string SendNReadBlocking(string command, int count_read_outputline, double sec_timeout_read = SECOND_TIMEOUT_READ)
		{
			string retval = null;
			if(shell_stream_read_timer != null)
				shell_stream_read_timer.Stop();

			if(SendCommand(command) == 0)
			{
				retval = ReadLinesBlocking(command, count_read_outputline, sec_timeout_read);
			}

			if(shell_stream_read_timer != null)
				shell_stream_read_timer.Start();
			return retval;
		}
		public bool RunCofileCommand(string command)
		{
			if(SendCommand(command) < 0)
				return false;
			//ssh.RunCommand(path_bin + command);
			return true;
		}
		#endregion

		#region Poll Linux Directory
		private SftpFile[] _PullListInDirectory(string Path)
		{
			IEnumerable<SftpFile> files = null;
			try
			{
				files = sftp.ListDirectory(Path).OrderBy(x => x.FullName);
			}
			catch(Exception e)
			{
				WindowMain.current.ShowMessageDialog("Open", e.Message, MahApps.Metro.Controls.Dialogs.MessageDialogStyle.Affirmative);
				Log.ErrorIntoUI(e.Message, "Pull Directory", Status.current.richTextBox_status);
				Log.PrintError(e.Message, "Classes.SSHManager._PullListInDirectory");
			}

			Log.PrintLog("Path = " + Path, "Classes.SSHManager._PullListInDirectory");
			if(files == null)
				return null;

			return files.ToArray();
		}
		public string WorkingDirectory
		{
			get
			{
				if(!IsConnected)
					return null;
				return sftp.WorkingDirectory;
			}
		}
		public SftpFile[] PullListInDirectory(string Path)
		{
			if(!IsConnected)
				return null;
			if(Path == null)
				return null;

			return _PullListInDirectory(Path);
		}
		#endregion

		#region sftp upload, download
		private bool CreateRemoteDirectory(string remote_dir_path)
		{
			if(!IsConnected)
				return false;

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
				Log.PrintError(ex.Message + "/ path = " + _path, "Classes.SSHManager.CreateRemoteDirectory");
				Log.ErrorIntoUI(ex.Message + "/ path = " + _path, "CreateRemoteDirectory", Status.current.richTextBox_status);
				return false;
			}
		}
		public string UploadFile(string local_path, string remote_directory, string remote_backup_dir = null)
		{
			//LinuxTreeViewItem.ReconnectServer();
			//LinuxTreeViewItem.ReConnect();
			if(!IsConnected)
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
								Log.PrintError("Create Directory Error", "Classes.SSHManager.UploadFile");
								return null;
							}

						}

						//sftp.UploadFile(fs, remote_file_path, true);
						string str = FileContoller.Read(local_path);
						string str1 = "echo \"" + str.Replace("\r", "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("$", "\\$") + "\" > \"" + remote_file_path + "\"";
						ssh.RunCommand(str1);
						//SendCommand(str1);

						Log.PrintLog(fi.Name + " => " + remote_file_path, "Classes.SSHManager.UploadFile");
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
					Log.PrintError("Not Exist File", "Classes.SSHManager.UploadFile");
					return null;
				}
			}
			catch(Exception e)
			{
				Log.ErrorIntoUI(e.Message + "/ " + local_path + " -> " + remote_directory, "UploadFile", Status.current.richTextBox_status);
				Log.PrintError(e.Message + "/ " + local_path + " -> " + remote_directory, "Classes.SSHManager.UploadFile");
				return null;
			}
			return remote_file_path;
		}
		public string UploadString(string str, string remote_file_path, string remote_backup_dir = null)
		{
			//LinuxTreeViewItem.ReconnectServer();
			//LinuxTreeViewItem.ReConnect();
			if(!IsConnected)
				return null;

			
			try
			{
				string[] arr_filename = remote_file_path.Split('/');
				string remote_directory = remote_file_path.Substring(0, remote_file_path.Length - arr_filename[arr_filename.Length - 1].Length);

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
							string remote_backup_file = remote_file_path + dt.ToString(".yyyy.MM.dd.hh.mm.ss") + ".backup";
							string com = @"cp '" + remote_file_path + "' '" + remote_backup_file + "'";
							ssh.RunCommand(com);
							//SendCommand(com);
						}
						else
						{
							//fs.Close();
							Log.PrintError("Create Directory Error", "Classes.SSHManager.UploadFile");
							return null;
						}

					}

					string str1 = "echo \"" + str.Replace("\r", "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("$", "\\$") + "\" > \"" + remote_file_path + "\"";
					ssh.RunCommand(str1);
					//SendCommand(str1);

					Log.PrintLog(" => " + remote_file_path, "Classes.SSHManager.UploadString");
				}
				else
				{
					remote_file_path = null;
				}
			}
			catch(Exception e)
			{
				Log.ErrorIntoUI(e.Message + "/ -> " + remote_file_path, "UploadString", Status.current.richTextBox_status);
				Log.PrintError(e.Message + "/ -> " + remote_file_path, "Classes.SSHManager.UploadString");
				return null;
			}
			return remote_file_path;
		}
		public bool DownloadFile(string local_path_folder, string remote_path_file, string local_file_name = null, string remote_filename = null, bool bLog = true)
		{
			//LinuxTreeViewItem.ReconnectServer();
			//LinuxTreeViewItem.ReConnect();
			if(!IsConnected)
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
					Log.PrintLog(remote_path_file + " => " + local, "Classes.SSHManager.DownloadFile");
				}
				else
				{
					Log.PrintLog("Not found file " + remote_path_file, "Classes.SSHManager.DownloadFile");
				}
				fs.Close();
			}
			catch(Exception e)
			{
				if(bLog)
				{
					Log.ErrorIntoUI(e.Message, "downloadFile", Status.current.richTextBox_status);
				}
				Log.PrintError(e.Message, "Classes.SSHManager.DownloadFile");
				return false;
			}
			return true;
		}
		public bool MoveFileToLocal(string local_path_folder, string remote_path_file, string local_file_name, double try_time_out_ms)
		{
			if(!IsConnected)
				return false;

			bool retval = false;
			if(try_time_out_ms != 0)
			{
				DateTime timeout = DateTime.Now.AddMilliseconds(try_time_out_ms);
				while(timeout > DateTime.Now && retval == false)
				{
					//tick();
					retval = DownloadFile(local_path_folder, remote_path_file, local_file_name, bLog: false);
					//if(!DownloadFile(local_path_folder, remote_path_file, local_file_name, bLog: false))
					//	return false;
				}
				if(retval == false)
				{
					Log.ErrorIntoUI(remote_path_file + " -> " + local_path_folder, "MoveFileToLocal", Status.current.richTextBox_status);
					Log.PrintError(remote_path_file + " -> " + local_path_folder, "Classes.SSHManager.MoveFileToLocal");
					return retval;
				}
				ssh.RunCommand("rm -rf '" + remote_path_file + "'");
				//SendCommand("rm -rf " + remote_path_file);

				return true;
			}
			else
			{
				//tick();
				retval = DownloadFile(local_path_folder, remote_path_file, local_file_name, bLog: false);
				if(retval == true)
				{
					ssh.RunCommand("rm -rf '" + remote_path_file + "'");
				}
				return retval;
			}

		}
		public bool DownloadDirectory(string local_folder_path, string remote_directory_path, Regex filter_file = null, Regex filter_except_dir = null)
		{
			//LinuxTreeViewItem.ReconnectServer();
			//LinuxTreeViewItem.ReConnect();
			if(!IsConnected)
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
				Log.PrintError(e.Message, "Classes.SSHManager.DownloadDirectory");
				return false;
			}
			return true;
		}
		#endregion


		private int LoadEnvCoHome()
		{
			string command = "echo $CO_HOME";
			string co_home = SendNReadBlocking(command, 1, 10);
			if(co_home == null || co_home == "")
			{
				Log.ErrorIntoUI("not defined $CO_HOME\r", "load $CO_HOME", Status.current.richTextBox_status);
				Log.PrintError("not defined $CO_HOME", "Classes.SSHManager._LoadEnvCoHome");
				return ReturnValue.Fail.LOAD_CO_HOME;
			}
			envCoHome = co_home;
			Log.PrintLog("$CO_HOME = " + co_home, "Classes.SSHManager._LoadEnvCoHome");
			return 0;
		}

		public string GetDataBase(string local_folder, string local_file)
		{
			string db_name = "cofile.db";
			string add_path_database = "/var/data/" + db_name;

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

		public int NewGetConfig(string local_dir)
		{
			if(!IsConnected)
				return -2;

			string[] filenames = { "file.json", "sam.json", "tail.json" };
			int i;
			for(i = 0; i < filenames.Length; i++)
			{
				if(!DownloadFile(local_dir, EnvCoHome + "/var/conf/" + id + "/" + filenames[i], filenames[i], filenames[i]))
					break;
			}
			if(i != filenames.Length)
				return -1;
			else
				return 0;
		}
		public int NewSetConfig(string local_dir)
		{
			if(!IsConnected)
				return -2;

			string[] filenames = { "file.json", "sam.json", "tail.json" };
			int i;
			for(i = 0; i < filenames.Length; i++)
			{
				if(null == UploadFile(local_dir + filenames[i], EnvCoHome + "/var/conf/" + id))
					break;
			}
			if(i != filenames.Length)
				return -1;
			else
				return 0;
		}
		
		public string GetEventLog(int n)
		{
			string env_co_home = EnvCoHome;
			string command = "tail -n" + n + " '" + env_co_home + "/var/log/event_log'";
			return SendNReadBlocking(command, n);
		}
	}
}
