using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
using System.Globalization;
using MahApps.Metro.Controls;
using CofileUI.Classes;
using static CofileUI.UserControls.ConfigOptions.ConfigOptionManager;

namespace CofileUI.UserControls.ConfigOptions.File
{
	/// <summary>
	/// comm_option.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class comm_option : UserControl
	{		
		static JObject Root { get; set; }
		public comm_option(JObject root)
		{
			InitializeComponent();

			if(root == null)
			{
				Log.PrintLog("NotFound File.comm_option", "UserControls.ConfigOptions.File.comm_option.comm_option");
				return;
			}

			Root = root;
			DataContext = root;
			ConfigOptionManager.MakeUI(grid, root, groups, GetUIOptionKey, GetUIOptionValue);
		}

		public enum Option
		{
			sid = 0
			, item
			, encode_type
			, log_console_yn
			, header_file_save_yn
			, file_reserver_yn
			, dir_monitoring_yn
			, dir_monitoring_term
			, verify_yn
			, schedule_time
			, result_log_yn
			, dir_recursive_yn
			, dir_recursive_max_depth
			, delay_time

			, Length
		}
		public static string[] detailOptions = new string[(int)Option.Length]
			{
				// comm_option
				"DB SID 이름"
				, "Item 명"
				, "암호화 인코딩 타입"
				, "로그출력 여부 (True : stdout)"
				, "header 파일 생성 여부 (True : 생성)"
				, "원본 파일 유지 여부 (True : 유지)"
				, "폴더 감시 모드 (daemon)"
				, "폴더 감시 주기 (초)"
				, "암호화후 데이터 검증 여부 (True : 검증)"
				, "스케쥴 시간 (00:00 ~ 23:59)"
				, "로그 결과 저장 여부 (True : 저장)"
				, "하위디렉토리 탐색 여부 (True : 탐색)"
				, "하위 디렉토리 탐색 깊이"
				, "지연시간 (*d*h*m*s)"
			};

		// Header 에 UI 를 빼던지, groups 를 static 변수로 선언 안하든지.
		Group[] groups = new Group[]
		{
			new Group()
			{
				Header = new Label() {Content = "Basic" },
				ArrGroupBodyOption = new int[]
				{
					(int)Option.sid
					, (int)Option.item
					, (int)Option.encode_type
					, (int)Option.log_console_yn
					, (int)Option.header_file_save_yn
					, (int)Option.file_reserver_yn
					, (int)Option.verify_yn
					, (int)Option.result_log_yn
					, (int)Option.delay_time
				}
			},
			new Group()
			{
				ArrGroupBodyOption = new int[]
				{
					(int)Option.dir_recursive_yn
					, (int)Option.dir_recursive_max_depth
				}
			},
			new Group()
			{
				RadioButtonGroupName = "Monitoring",
				ArrGroupBodyOption = new int[]
				{
					(int)Option.dir_monitoring_yn
					, (int)Option.dir_monitoring_term
					, (int)Option.schedule_time
				}
			}
		};
		static JProperty GetJProperty(Option opt, JObject root)
		{
			JProperty retval = null;
			try
			{
				if(root[(opt).ToString()] == null)
				{
					object value = "";
					switch(opt)
					{
						case Option.sid:
						case Option.item:
						case Option.encode_type:
						case Option.schedule_time:
						case Option.delay_time:
							value = "";
							break;
						case Option.log_console_yn:
						case Option.header_file_save_yn:
						case Option.file_reserver_yn:
						case Option.dir_monitoring_yn:
						case Option.verify_yn:
						case Option.result_log_yn:
						case Option.dir_recursive_yn:
							value = false;
							break;
						case Option.dir_monitoring_term:
						case Option.dir_recursive_max_depth:
							value = (Int64)0;
							break;
					}
					if(root[ConfigOptionManager.StartDisableProperty + (opt).ToString()] != null)
					{
						JProperty jprop = root[ConfigOptionManager.StartDisableProperty + (opt).ToString()].Parent as JProperty;
						if(jprop != null)
						{
							retval = jprop;
							//jprop.Replace(new JProperty((opt).ToString(), jprop.Value));
						}
						else
						{
							retval = new JProperty((opt).ToString(), value);
							root.Add(retval);
						}
					}
					else
					{
						retval = new JProperty((opt).ToString(), value);
						root.Add(retval);
					}
				}
				else
					retval = root[(opt).ToString()].Parent as JProperty;
			}
			catch(Exception e)
			{
				Log.PrintError(e.Message + " (" + opt.ToString() + ")", "UserControls.ConfigOption.File.comm_option.GetJProperty");
			}
			return retval;
		}
		static FrameworkElement GetUIOptionKey(int opt, JObject root)
		{
			Option option = (Option)opt;
			FrameworkElement ret = null;
			try
			{
				string detail = detailOptions[(int)opt];
				switch(option)
				{
					// Basical
					case Option.sid:
					case Option.item:
					case Option.encode_type:
					case Option.log_console_yn:
					case Option.header_file_save_yn:
					case Option.file_reserver_yn:
					case Option.dir_monitoring_yn:
					case Option.verify_yn:
					case Option.result_log_yn:
					case Option.dir_recursive_yn:
					case Option.dir_recursive_max_depth:

						{
							TextBlock tb = new TextBlock()
							{
								Text = detail
							};

							ret = tb;
						}
						break;

					// Optional
					//case Option.thread_count:
					case Option.delay_time:
						{
							StackPanel sp = new StackPanel() {Orientation = Orientation.Horizontal };

							CheckBox cb = new CheckBox();
							JProperty jprop = GetJProperty(option, root);
							cb.IsChecked = !(jprop.Name[0] == ConfigOptionManager.StartDisableProperty);
							sp.Children.Add(cb);
							TextBlock tb = new TextBlock()
							{
								Text = detail
							};
							sp.Children.Add(tb);

							// delegate 에 지역변수를 사용하면 지역변수를 메모리에서 계속 잡고있는다. (전역변수 화 (어디 소속으로 전역변수 인지 모르겠다.))
							cb.Checked += delegate
							{
								ConfigOptionManager.CheckedKey(ref jprop);
							};
							cb.Unchecked += delegate
							{
								ConfigOptionManager.UncheckedKey(ref jprop);
							};
							ret = sp;
						}
						break;
					case Option.schedule_time:
					case Option.dir_monitoring_term:
						{
							StackPanel sp = new StackPanel() {Orientation = Orientation.Horizontal };

							RadioButton rb = new RadioButton();
							JProperty jprop = GetJProperty(option, root);
							rb.IsChecked = !(jprop.Name[0] == ConfigOptionManager.StartDisableProperty);
							sp.Children.Add(rb);
							TextBlock tb = new TextBlock()
							{
								Text = detail
							};
							sp.Children.Add(tb);

							// delegate 에 지역변수를 사용하면 지역변수를 메모리에서 계속 잡고있는다. (전역변수 화 (어디 소속으로 전역변수 인지 모르겠다.))
							rb.Checked += delegate
							{
								ConfigOptionManager.CheckedKey(ref jprop);
							};
							rb.Unchecked += delegate
							{
								ConfigOptionManager.UncheckedKey(ref jprop);
							};
							ret = sp;
						}
						break;
					default:
						break;
				}
			}
			catch(Exception e)
			{
				Log.PrintError(e.Message + " (" + option.ToString() + ")", "UserControls.ConfigOption.File.comm_option.GetUIOptionKey");
			}

			if(ret != null)
			{
				ret.Margin = new Thickness(10, 3, 10, 3);
				ret.VerticalAlignment = VerticalAlignment.Center;
				ret.HorizontalAlignment = HorizontalAlignment.Left;
			}
			return ret;
		}
		static FrameworkElement GetUIOptionValue(int opt, JObject root)
		{
			Option option = (Option)opt;

			JProperty jprop = GetJProperty(option, root);
			FrameworkElement ret = null;
			try
			{
				switch(option)
				{
					case Option.encode_type:
						{
							Dictionary<string, int> dic = new Dictionary<string, int>()
							{
								{ "binary", 0}
								, { "ASCII", 1 }
							};
							ComboBox cb = new ComboBox() { SelectedIndex = 0 };
							var e = dic.GetEnumerator();
							while(e.MoveNext())
								cb.Items.Add(e.Current.Key);


							cb.DataContext = jprop.Parent;
							Binding bd = new Binding(option.ToString());
							bd.Mode = BindingMode.TwoWay;
							bd.Converter = new StringToInt64Converter();
							cb.SetBinding(ComboBox.SelectedIndexProperty, bd);

							cb.SelectedIndex = Convert.ToInt32(jprop.Value);

							cb.SelectionChanged += delegate
							{
								ConfigOptionManager.IsChanged = true;
							};
							ret = cb;
						}
						break;
					case Option.sid:
					case Option.item:
					case Option.schedule_time:
					case Option.delay_time:
						{
							TextBox tb = new TextBox() {/*Text = optionValue.ToString()*/ };
							tb.Width = ConfigOptionSize.WIDTH_VALUE;
							tb.HorizontalAlignment = HorizontalAlignment.Left;
							ret = tb;

							tb.DataContext = jprop.Parent;
							Binding bd = new Binding(option.ToString());
							bd.Mode = BindingMode.TwoWay;
							// TextBox.Text 의 UpdateSourceTrigger 의 기본속성은 LostFocus 이다.
							bd.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
							tb.SetBinding(TextBox.TextProperty, bd);

							tb.Text = Convert.ToString(jprop.Value);

							tb.TextChanged += delegate
							{
								//((JValue)optionValue).Value = tb.Text;
								ConfigOptionManager.IsChanged = true;
							};
						}
						break;
					case Option.log_console_yn:
					case Option.header_file_save_yn:
					case Option.file_reserver_yn:
					case Option.dir_monitoring_yn:
					case Option.verify_yn:
					case Option.result_log_yn:
					case Option.dir_recursive_yn:
						{
							ToggleSwitch ts = new ToggleSwitch() { /*IsChecked = (bool)optionValue*/ };
							ts.Width = ConfigOptionSize.WIDTH_VALUE;
							ts.HorizontalAlignment = HorizontalAlignment.Left;

							ts.FontSize = 13;
							ts.OffLabel = "False";
							ts.OnLabel = "True";
							ts.Style = (Style)App.Current.Resources["MahApps.Metro.Styles.ToggleSwitch.Win10"];

							//if(panelDetailOption.RowDefinitions.Count > 0)
							//	Grid.SetRow(ts, panelDetailOption.RowDefinitions.Count - 1);
							//Grid.SetColumn(ts, 1);
							ret = ts;

							ts.DataContext = jprop.Parent;
							Binding bd = new Binding(option.ToString());
							bd.Mode = BindingMode.TwoWay;
							bd.Converter = new OnlyBooleanConverter();
							ts.SetBinding(ToggleSwitch.IsCheckedProperty, bd);

							ts.IsChecked = Convert.ToBoolean(jprop.Value);

							ts.Checked += delegate
							{
								//((JValue)optionValue).Value = ts.IsChecked;
								ConfigOptionManager.IsChanged = true;
							};
							ts.Unchecked += delegate
							{
								//((JValue)optionValue).Value = ts.IsChecked;
								ConfigOptionManager.IsChanged = true;
							};
						}
						break;
					case Option.dir_monitoring_term:
					case Option.dir_recursive_max_depth:
						{
							NumericUpDown tb_integer = new NumericUpDown() {/*Value = (System.Int64)optionValue*/ };
							tb_integer.Width = ConfigOptionSize.WIDTH_VALUE;
							tb_integer.HorizontalAlignment = HorizontalAlignment.Left;

							//if(panelDetailOption.RowDefinitions.Count > 0)
							//	Grid.SetRow(tb_integer, panelDetailOption.RowDefinitions.Count - 1);
							//Grid.SetColumn(tb_integer, 1);
							ret = tb_integer;

							tb_integer.DataContext = jprop.Parent;
							Binding bd = new Binding(option.ToString());
							bd.Mode = BindingMode.TwoWay;
							bd.Converter = new OnlyInt64Converter();
							tb_integer.SetBinding(NumericUpDown.ValueProperty, bd);

							tb_integer.Value = Convert.ToInt64(jprop.Value);

							tb_integer.ValueChanged += delegate
							{
								//((JValue)optionValue).Value = (System.Int64)tb_integer.Value;
								ConfigOptionManager.IsChanged = true;
							};

						}
						break;
					default:
						break;
				}

				if(jprop != null && jprop.Name[0] == ConfigOptionManager.StartDisableProperty
					&& Root[jprop.Name.TrimStart(ConfigOptionManager.StartDisableProperty)] != null
					&& Root[jprop.Name.TrimStart(ConfigOptionManager.StartDisableProperty)].Parent != null)
				{
					Root[jprop.Name.TrimStart(ConfigOptionManager.StartDisableProperty)].Parent.Remove();
				}
			}
			catch(Exception e)
			{
				Log.PrintError(e.Message + " (\"" + option.ToString() + "\" : \"" + jprop + "\")", "UserControls.ConfigOption.File.comm_option.GetUIOptionValue");
			}

			if(ret != null)
			{
				ret.Width = ConfigOptionSize.WIDTH_VALUE;
				ret.Margin = new Thickness(10, 3, 10, 3);
				ret.VerticalAlignment = VerticalAlignment.Center;
				ret.HorizontalAlignment = HorizontalAlignment.Left;
			}
			return ret;
		}
	}
}
