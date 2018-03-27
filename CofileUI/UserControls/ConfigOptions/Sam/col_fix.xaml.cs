using CofileUI.Classes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CofileUI.UserControls.ConfigOptions.Sam
{
	/// <summary>
	/// col_fix.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class col_fix : UserControl
	{
		public col_fix()
		{
			InitializeComponent();
		}

		enum Option
		{
			item = 0,
			start_pos,
			size,
			col_size,
			Length
		}
		string[] detailOptions = new string[(int)Option.Length] {
			"Item명",
			"암/복호화 대상 컬럼 시작 위치",
			"암/복호화 대상 컬럼 크기" ,
			"암/복호화 후 데이터 크기"
		};

		object[] initvalue = new object[(int)Option.Length] {"", (Int64)0, (Int64)0, (Int64)0 };
		private void OnClickAdd(object sender, RoutedEventArgs e)
		{
			try
			{
				JProperty jprop = this.DataContext as JProperty;
				if(jprop == null)
					return;
				JArray jarr = jprop.Value as JArray;
				if(jarr == null)
					return;
				Button btn = sender as Button;
				if(btn == null)
					return;

				object[] _initvalue = initvalue.Clone() as object[];
				Windows.Window_AddDataGridInConfig wa = new Windows.Window_AddDataGridInConfig(detailOptions, _initvalue);
				Point pt = this.PointToScreen(new Point(0, 0));
				wa.Left = pt.X + this.ActualWidth / 2 - wa.Width / 2;
				wa.Top = pt.Y + this.ActualHeight / 2 - wa.Height / 2;
				if(wa.ShowDialog() != true)
					return;

				JObject jobj = new JObject();
				for(int i = 0; i < wa.Value.Length; i++)
					jobj.Add(new JProperty(((Option)i).ToString(), wa.Value[i]));
				jarr.Add(jobj);

				ConfigOptionManager.IsChanged = true;
			}
			catch(Exception ex)
			{
				Log.PrintError(ex.Message, "UserControls.ConfigOption.Sam.col_fix.OnClickAdd");
			}
		}
		private void OnClickDelete(object sender, RoutedEventArgs e)
		{
			try
			{
				JProperty jprop = this.DataContext as JProperty;
				if(jprop == null)
					return;
				JArray jarr = jprop.Value as JArray;
				if(jarr == null)
					return;
				JObject jobj = dataGrid.SelectedItem as JObject;
				if(jobj == null)
					return;

				jarr.Remove(jobj);

				ConfigOptionManager.IsChanged = true;
			}
			catch(Exception ex)
			{
				JObject jobj = dataGrid.SelectedItem as JObject;
				Log.PrintError(ex.Message + " (" + jobj + ")", "UserControls.ConfigOption.Sam.col_fix.OnClickDelete");
			}
		}
		private void OnClickModify(object sender, RoutedEventArgs e)
		{
			try
			{
				JProperty jprop = this.DataContext as JProperty;
				if(jprop == null)
					return;
				JArray jarr = jprop.Value as JArray;
				if(jarr == null)
					return;
				JObject jobj = dataGrid.SelectedItem as JObject;
				if(jobj == null)
					return;
				object[] _initvalue = new object[initvalue.Length];
				for(int i = 0; i < _initvalue.Length; i++)
				{
					string key = ((Option)i).ToString();
					JValue jval = jobj[key] as JValue;
					if(jval != null)
						_initvalue[i] = jval.Value;
					else
						_initvalue[i] = initvalue[i];
				}

				Button btn = sender as Button;
				if(btn == null)
					return;

				Windows.Window_AddDataGridInConfig wa = new Windows.Window_AddDataGridInConfig(detailOptions, _initvalue);
				Point pt = this.PointToScreen(new Point(0, 0));
				wa.Left = pt.X + this.ActualWidth / 2 - wa.Width / 2;
				wa.Top = pt.Y + this.ActualHeight / 2 - wa.Height / 2;
				if(wa.ShowDialog() != true)
					return;


				for(int i = 0; i < wa.Value.Length; i++)
				{
					string key = ((Option)i).ToString();
					JValue jval = jobj[key] as JValue;
					if(jval == null)
					{
						jobj.Add(new JProperty(key, wa.Value[i]));

						jval = jobj[ConfigOptionManager.StartDisableProperty + key] as JValue;
						if(jval != null)
							jobj.Remove(ConfigOptionManager.StartDisableProperty + key);
					}
					else
						jval.Value = wa.Value[i];
				}

				DataContext = null;
				DataContext = jprop;

				ConfigOptionManager.IsChanged = true;
			}
			catch(Exception ex)
			{
				JObject jobj = dataGrid.SelectedItem as JObject;
				Log.PrintError(ex.Message + " (" + jobj + ")", "UserControls.ConfigOption.Sam.col_fix.OnClickModify");
			}
		}
	}
}
