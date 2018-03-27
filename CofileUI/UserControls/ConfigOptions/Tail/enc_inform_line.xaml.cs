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
	/// enc_inform_line.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class enc_inform_line : UserControl
	{
		static JObject Root { get; set; }
		public enc_inform_line(JObject root)
		{
			InitializeComponent();

			if(root == null)
			{
				Log.PrintLog("NotFound Tail.enc_inform", "UserControls.ConfigOptions.Tail.enc_inform_line.enc_inform_line");
				return;
			}

			Root = root;
			DataContext = root;
			ConfigOptionManager.MakeUI(grid, root, groups, GetUIOptionKey, GetUIOptionValue);
		}

		public enum Options
		{
			// comm_option
			item = 0
			, delimiter

			, Length
		}
		// Header 에 UI 를 빼던지, groups 를 static 변수로 선언 안하든지.
		Group[] groups = new Group[]
		{
			new Group()
			{
				Header = new Label() {Content = "Basic" },
				ArrGroupBodyOption = new int[]
				{
					(int)Options.item,
					(int)Options.delimiter
				}
			}
		};

		public static string[] detailOptions = new string[(int)Options.Length]
			{
			// comm_option
				"암/복호화에 사용할 ITEM 명",
				"구분자"
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
						case Options.item:
						case Options.delimiter:
							value = "";
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
				Log.PrintError(e.Message + " (" + opt.ToString() + ")", "UserControls.ConfigOption.Tail.enc_inform_line.GetJProperty");
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
					case Options.item:
					case Options.delimiter:
						{
							TextBlock tb = new TextBlock()
							{
								Text = detail
							};

							ret = tb;
						}
						break;
					default:
						break;
				}
			}
			catch(Exception e)
			{
				Log.PrintError(e.Message + " (" + option.ToString() + ")", "UserControls.ConfigOption.Tail.enc_inform_line.GetUIOptionKey");
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
					case Options.item:
					case Options.delimiter:
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
				Log.PrintError(e.Message + " (\"" + option.ToString() + "\" : \"" + jprop + "\")", "UserControls.ConfigOption.Tail.enc_inform_line.GetUIOptionValue");
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
