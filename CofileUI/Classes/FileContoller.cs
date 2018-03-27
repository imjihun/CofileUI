using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CofileUI.Classes
{
	static class FileContoller
	{
		const int MAX_BUFFER = 4096;
		public static bool CreateDirectory(string path)
		{
			try
			{
				Directory.CreateDirectory(path);
				Log.PrintError("path = " + path, "Classes.FileController.CreateDirectory");
			}
			catch(Exception e)
			{
				Log.PrintError(e.Message + "<path = " + path + ">", "Classes.FileContoller.CreateDirectory");
				return false;
			}
			return true;
		}
		public static int DeleteFile(string path)
		{
			try
			{
				File.Delete(path);
			}
			catch(Exception e)
			{
				Log.PrintError(e.Message + "<path = " + path + ">", "Classes.FileContoller.DeleteFile");
			}
			return 0;
		}
		public static int DeleteDirectory(string path)
		{
			try
			{
				Directory.Delete(path, true);
			}
			catch(Exception e)
			{
				Log.PrintError(e.Message + "<path = " + path + ">", "Classes.FileContoller.DeleteDirectory");
			}
			return 0;
		}
		public static string Read(string path)
		{
			if(path == null)
				return null;

			StringBuilder ret_str = new StringBuilder("");
			try
			{
				FileStream fs = new FileStream(path, FileMode.Open);

				byte[] buffer = new byte[MAX_BUFFER];
				int size_read = 0;
				ret_str = new StringBuilder();

				while((size_read = fs.Read(buffer, 0, MAX_BUFFER)) > 0)
					ret_str.Append(Encoding.UTF8.GetString(buffer, 0, size_read));

				fs.Close();
			}
			catch(Exception e)
			{
				Log.PrintError(e.Message + "<path = " + path + ">", "Classes.FileContoller.Read");
			}

			return ret_str.ToString();
		}
		public static bool Write(string path, string str)
		{
			if(path == null || str == null)
				return false;
			try
			{
				// 경로에 디렉토리가 없으면 생성
				string dir = path;
				if(path[path.Length - 1] != '\\')
					dir = path.Substring(0, path.LastIndexOf('\\') + 1);
				if(!FileContoller.CreateDirectory(dir))
					return false;

				// 경로에 파일 쓰기.
				FileStream fs = new FileStream(path, FileMode.Create);

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
				Log.PrintError(e.Message + "<path = " + path + ">", "Classes.FileContoller.Write");
			}
			return false;
		}
		public static string[] LoadFile(string path, string searchPattern)
		{
			DirectoryInfo d = new DirectoryInfo(path);
			if(!d.Exists)
				return null;
			try
			{
				return Directory.GetFiles(path, searchPattern);
			}
			catch(Exception e)
			{
				Log.PrintError(e.Message + "<path = " + path + ">", "Classes.FileContoller.LoadFile");
				return null;
			}
		}
		public static int DeleteFilesInDirectory(string path_dir)
		{
			string[] files;

			DirectoryInfo d = new DirectoryInfo(path_dir);
			if(!d.Exists)
				return 0;

			try
			{
				files = Directory.GetFiles(path_dir);
			}
			catch(Exception e)
			{
				Log.PrintError(e.Message + "<path = " + path_dir + ">", "Classes.FileContoller.DeleteFilesInDirectory");
				return -1;
			}

			for(int i = 0; i < files.Length; i++)
			{
				try
				{
					File.Delete(files[i]);
				}
				catch(Exception e)
				{
					;
				}
			}
			return 0;
		}
	}
}
