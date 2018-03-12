using CofileUI.Classes;
using MahApps.Metro.Controls;
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
using static CofileUI.UserControls.ConfigOptions.ConfigOptionManager;

namespace CofileUI.UserControls.ConfigOptions.Tail
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
				Log.PrintLog("NotFound Tail.comm_option", "UserControls.ConfigOptions.Tail.comm_option.comm_option");
				return;
			}

			Root = root;
			DataContext = root;
			ConfigOptionManager.MakeUI(grid, root, detailOptions, groups, GetUIOptionKey, GetUIOptionValue);
		}
		
		public enum Options
		{
			// comm_option
			input_dir = 0
			, input_ext
			, output_dir
			, output_ext
			, sid
			, tail_type
			, interval
			, no_inform
			, input_filter
			//, shutdown_time
			, zero_byte_yn
			//, no_access_sentence
			, file_reserver_yn
			//, reg_yn
			, daemon_yn
			, limit_process
			, dir_recursive_yn
			, dir_recursive_max_depth
			, blacklist_filter

			, Length
		}
		public static string[] detailOptions = new string[(int)Options.Length]
			{
			// comm_option
				"암/복호화 할 대상 폴더"
				, "암/복호화 할 입력 로그파일의 확장자"
				, "암/복호화 후 출력 경로"
				, "암/복호화 후 덧붙일 확장자"
				, "DB SID 이름"
				, "Tail Type"
				, "폴더의 감시 주기"
				, "패턴 개수"
				, "암/복호화 대상 파일 필터 (정규표현식)"
				//, "자식 데몬 자동 종료 시간 (시간)"
				, "파일 크기가 0인 파일에 대한 암/복호화 유무 (true : 감시)"
				//, "복호화 권한이 없을 때, 출력 문구"
				, "원본 파일 유지 여부 (true : 유지)"
				//, "정규 표현식 사용 여부 (true : 사용)"
				, "데몬 모드 여부 (true : 데몬모드)"
				, "프로세스 제한"
				, "하위디렉토리 탐색 여부 (True : 탐색)"
				, "하위 디렉토리 탐색 깊이"
				, "암/복호화 제외 할 파일 패턴 (정규표현식)"
			};

		// Header 에 UI 를 빼던지, groups 를 static 변수로 선언 안하든지.
		Group[] groups = new Group[]
		{
			new Group()
			{
				Arr = new int[]
				{
					(int)Options.tail_type
					,(int)Options.no_inform
				}
			},
			new Group()
			{
				Header = new Label() {Content = "Basic" },
				Arr = new int[]
				{
					(int)Options.sid
					,(int)Options.input_dir
					,(int)Options.output_dir
					,(int)Options.output_ext
					,(int)Options.zero_byte_yn
					//,(int)Options.no_access_sentence
					,(int)Options.file_reserver_yn
					//,(int)Options.reg_yn
					,(int)Options.limit_process
					,(int)Options.blacklist_filter
				}
			},
			new Group()
			{
				Arr = new int[]
				{
					(int)Options.dir_recursive_yn
					,(int)Options.dir_recursive_max_depth
				}
			},
			new Group()
			{
				Header = new Label() {Content = "암/복호화 대상 규칙" },
				RadioButtonGroupName = "Input",
				Arr = new int[]
				{
					(int)Options.input_ext
					,(int)Options.input_filter
				}
			},
			new Group()
			{
				Arr = new int[]
				{
					(int)Options.daemon_yn
					,(int)Options.interval
					//,(int)Options.shutdown_time
				}
			}
		};
		static JProperty GetJProperty(Options opt, JObject root)
		{
			JProperty retval = null;
			try
			{
				if(root[(opt).ToString()] == null)
				{
					object value = "";
					switch(opt)
					{
						case Options.input_dir:
						case Options.input_ext:
						case Options.output_dir:
						case Options.output_ext:
						case Options.sid:
						case Options.input_filter:
						//case Options.no_access_sentence:
						case Options.blacklist_filter:
							value = "";
							break;
						case Options.zero_byte_yn:
						case Options.file_reserver_yn:
						//case Options.reg_yn:
						case Options.daemon_yn:
						case Options.dir_recursive_yn:
							value = false;
							break;
						case Options.tail_type:
						case Options.interval:
						case Options.no_inform:
						//case Options.shutdown_time:
						case Options.limit_process:
						case Options.dir_recursive_max_depth:
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
				Log.PrintError(e.Message + " (" + opt.ToString() + ")", "UserControls.ConfigOption.Tail.comm_option.GetJProperty");
			}
			return retval;
		}
		static FrameworkElement GetUIOptionKey(int opt, JObject root)
		{
			Options option = (Options)opt;
			FrameworkElement ret = null;
			try
			{
				string detail = detailOptions[(int)opt];
				switch(option)
				{
					// Basical
					case Options.input_dir:
					case Options.output_dir:
					case Options.output_ext:
					case Options.sid:
					case Options.tail_type:
					case Options.interval:
					case Options.no_inform:
					case Options.zero_byte_yn:
					case Options.file_reserver_yn:
					//case Options.reg_yn:
					case Options.daemon_yn:
					case Options.limit_process:
					case Options.dir_recursive_yn:
					case Options.dir_recursive_max_depth:
						{
							TextBlock tb = new TextBlock()
							{
								Text = detail
							};

							ret = tb;
						}
						break;

					// Optional
					//case Options.shutdown_time:
					//case Options.no_access_sentence:
					case Options.blacklist_filter:
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
					case Options.input_filter:
					case Options.input_ext:
						{
							StackPanel sp = new StackPanel() {Orientation = Orientation.Horizontal };

							RadioButton cb = new RadioButton();
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
					default:
						break;
				}
			}
			catch(Exception e)
			{
				Log.PrintError(e.Message + " (" + option.ToString() + ")", "UserControls.ConfigOption.Tail.comm_option.GetUIOptionKey");
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
			Options option = (Options)opt;

			JProperty jprop = GetJProperty(option, root);
			FrameworkElement ret = null;
			try
			{
				switch(option)
				{
					case Options.tail_type:
						{
							Dictionary<string, int> dic = new Dictionary<string, int>()
							{
								{ "LINE", 1 }
								, { "PATTERN", 2 }
							};
							ComboBox cb = new ComboBox() { SelectedIndex = 0 };
							var e = dic.GetEnumerator();
							while(e.MoveNext())
								cb.Items.Add(e.Current.Key);

							cb.DataContext = jprop.Parent;
							Binding bd = new Binding(option.ToString());
							bd.Mode = BindingMode.TwoWay;
							bd.Converter = new OnlyInt64TailTypeConverter();
							cb.SetBinding(ComboBox.SelectedIndexProperty, bd);

							cb.SelectedIndex = Convert.ToInt32(jprop.Value) - 1;

							cb.SelectionChanged += delegate {
								//((JValue)optionValue).Value = dic[cb.SelectedItem.ToString()];
								ConfigOptionManager.bChanged = true;
								TailOptions.current.ChangeSecondGrid();
							};
							ret = cb;
						}
						break;
					case Options.input_dir:
					case Options.input_ext:
					case Options.output_dir:
					case Options.output_ext:
					case Options.sid:
					case Options.input_filter:
					//case Options.no_access_sentence:
					case Options.blacklist_filter:
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
								ConfigOptionManager.bChanged = true;
							};
						}
						break;
					case Options.zero_byte_yn:
					case Options.file_reserver_yn:
					case Options.daemon_yn:
					case Options.dir_recursive_yn:
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
								ConfigOptionManager.bChanged = true;
							};
							ts.Unchecked += delegate
							{
								//((JValue)optionValue).Value = ts.IsChecked;
								ConfigOptionManager.bChanged = true;
							};
						}
						break;
					//case Options.reg_yn:
					//	{
					//		ToggleSwitch ts = new ToggleSwitch() { /*IsChecked = (bool)optionValue*/ };
					//		ts.Width = ConfigOptionSize.WIDTH_VALUE;
					//		ts.HorizontalAlignment = HorizontalAlignment.Left;

					//		ts.FontSize = 13;
					//		ts.OffLabel = "False";
					//		ts.OnLabel = "True";
					//		ts.Style = (Style)App.Current.Resources["MahApps.Metro.Styles.ToggleSwitch.Win10"];

					//		//if(panelDetailOption.RowDefinitions.Count > 0)
					//		//	Grid.SetRow(ts, panelDetailOption.RowDefinitions.Count - 1);
					//		//Grid.SetColumn(ts, 1);
					//		ret = ts;

					//		ts.DataContext = jprop.Parent;
					//		Binding bd = new Binding(option.ToString());
					//		bd.Mode = BindingMode.TwoWay;
					//		bd.Converter = new OnlyBooleanConverter();
					//		ts.SetBinding(ToggleSwitch.IsCheckedProperty, bd);

					//		ts.IsChecked = Convert.ToBoolean(jprop.Value);

					//		ts.Checked += delegate
					//		{
					//			//((JValue)optionValue).Value = ts.IsChecked;
					//			ConfigOptionManager.bChanged = true;
					//			TailOptions.current.ChangeSecondGrid();
					//		};
					//		ts.Unchecked += delegate
					//		{
					//			//((JValue)optionValue).Value = ts.IsChecked;
					//			ConfigOptionManager.bChanged = true;
					//			TailOptions.current.ChangeSecondGrid();
					//		};
					//	}
					//	break;
					case Options.interval:
					case Options.no_inform:
					//case Options.shutdown_time:
					case Options.limit_process:
					case Options.dir_recursive_max_depth:
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
								ConfigOptionManager.bChanged = true;
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
				Log.PrintError(e.Message + " (\"" + option.ToString() + "\" : \"" + jprop + "\")", "UserControls.ConfigOption.Tail.comm_option.GetUIOptionValue");
			}

			if(ret != null)
			{
				ret.Width = 150;
				ret.Margin = new Thickness(10, 3, 10, 3);
				ret.VerticalAlignment = VerticalAlignment.Center;
				ret.HorizontalAlignment = HorizontalAlignment.Left;
			}
			return ret;
		}

	}
}
