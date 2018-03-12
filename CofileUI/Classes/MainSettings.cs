using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CofileUI.Classes
{
	public static class MainSettings
	{
		private static bool isTimeOut = true;
		public static bool IsTimeOut { get { return isTimeOut; } set { isTimeOut = value; } }
		private static int sessionTimeOut = 5;
		public static int SessionTimeOut { get { return sessionTimeOut; } set { sessionTimeOut = value; } }

		public static class Path
		{
			private static string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

			public static string PathDirPreviewFile = BaseDirectory + @"tmp\";

			public static string PathDirConfigFile = BaseDirectory + @"config\";

			public static string PathDirServerInfo = BaseDirectory + @"system\";
			public static string FileNameServerInfo = @"serverinfo.json";

			public static string PathDirCofileDB = BaseDirectory + @"system\tmp\";
		}
	}
}
