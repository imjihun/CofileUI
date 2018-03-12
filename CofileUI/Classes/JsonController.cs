using CofileUI.Classes;
using CofileUI.Windows;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CofileUI.Classes
{
	class JsonController
	{
		//static string error_message = "";
		//public static string Error_message { get { return error_message; } }
		public static JToken ParseJsonUI(string json)
		{
			if(json == null)
				return null;
			JToken obj = null;
			try
			{
				obj = JToken.Parse(json);
				//Log.Print(obj);
			}
			catch(Exception e)
			{
				//error_message = e.Message;
				Log.PrintError(e.Message, "Classes.JsonController.ParseJson");
				WindowMain.current.ShowMessageDialog("Json Context Error!!", e.Message, MahApps.Metro.Controls.Dialogs.MessageDialogStyle.Affirmative);
			}

			return obj;
		}
		public static JToken ParseJson(string json)
		{
			if(json == null)
				return null;
			JToken obj = null;
			try
			{
				obj = JToken.Parse(json);
				//Log.Print(obj);
			}
			catch(Exception e)
			{
				//error_message = e.Message;
				Log.PrintError(e.Message, "Classes.JsonController.ParseJson");
			}

			return obj;
		}
	}
}
