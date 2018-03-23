using CofileUI.Classes;
using CofileUI.Windows;
using MahApps.Metro.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace CofileUI.UserControls.ConfigOptions
{
	enum ConfigType
	{
		file = 0,
		sam,
		tail
	}

	public enum GroupBodyStyle
	{
		Basic = 0,
		Radio
	}
	static class ConfigOptionSize
	{
		public static int WIDTH_VALUE = 150;
	}
	static class ConfigOptionManager
	{
		public static char StartDisableProperty = '#';
		public static JToken Root;
		private static JToken CurRoot;
		private static string work_name;
		private static string index;

		private static UserControl current;
		private static string path;
		public static string Path
		{
			get { return path; }
			set
			{
				path = value;

				string path_title = "";
				if(path != null)
					path_title = " [ " + path + " ]";
				
				if(current as ConfigOptions.File.FileOptions != null)
					((ConfigOptions.File.FileOptions)current).textBlock.Text = "File" + path_title;
				if(current as ConfigOptions.Sam.SamOptions != null)
					((ConfigOptions.Sam.SamOptions)current).textBlock.Text = "Sam" + path_title;
				if(current as ConfigOptions.Tail.TailOptions != null)
					((ConfigOptions.Tail.TailOptions)current).textBlock.Text = "Tail" + path_title;

				Console.WriteLine("JHLIM_DEBUG : path = " + path + " current = " + current?.GetType());
			}
		}
		private static string FileName
		{
			get
			{
				string[] split = path.Split('\\');
				return split[split.Length - 1];
			}
		}
		private static bool _bChanged = false;
		public static bool bChanged
		{
			get { return _bChanged; }
			set
			{
				_bChanged = value;
				//if(value)
				//	WindowMain.current.tabItem_Config.Header = "*" + Application.Current.FindResource("MainTab.Config") as string;
				//else
				//	WindowMain.current.tabItem_Config.Header = Application.Current.FindResource("MainTab.Config") as string;
			}
		}
		static void Clear()
		{
			current = null;

			Path = null;
			bChanged = false;
		}
		private static UserControl CreateOption(string type, JToken token)
		{
			if(type == null)
			{
				Log.PrintError("type = " + "empty", "UserControls.ConfigOptions.ConfigOptionManager.CreateOption");
				WindowMain.current.ShowMessageDialog("Config", "type을 확인 할 수 없습니다.\ntype = " + "empty", MahApps.Metro.Controls.Dialogs.MessageDialogStyle.Affirmative);
				return null;
			}
			else if(type == "file")
				current = new ConfigOptions.File.FileOptions() { DataContext = token };
			else if(type == "sam")
				current = new ConfigOptions.Sam.SamOptions() { DataContext = token };
			else if(type == "tail")
				current = new ConfigOptions.Tail.TailOptions() { DataContext = token };
			else
			{
				Log.PrintError("type = " + type, "UserControls.ConfigOptions.ConfigOptionManager.CreateOption");
				WindowMain.current.ShowMessageDialog("Config", "type을 확인 할 수 없습니다.\ntype = " + type, MahApps.Metro.Controls.Dialogs.MessageDialogStyle.Affirmative);
				return null;
			}

			CurRoot = token;
			CurRoot["work_group"]?.Parent.Remove();
			CurRoot["processes"]?.Parent.Remove();
			CurRoot["type"]?.Parent.Remove();
			Log.PrintLog("type = " + type, "UserControls.ConfigOptions.ConfigOptionManager.CreateOption");
			return current;
		}
		public static UserControl CreateOption(JToken token, string _work_name = null, string _index = null)
		{
			ConfigOptionManager.Clear();

			Root = token;
			work_name = _work_name;
			index = _index;

			JObject new_root = token.DeepClone() as JObject;
			if(_work_name != null)
			{
				JObject jobj_work = ((token as JObject)?.GetValue("work_group") as JObject)?.GetValue(_work_name) as JObject;
				foreach(var v in jobj_work.Properties())
				{
					if(v.Value as JArray == null)
					{
						JObject jobj_cur = new_root.GetValue(v.Name) as JObject;
						if(jobj_cur == null)
							continue;
						foreach(var prop in (v.Value as JObject)?.Properties())
						{
							jobj_cur[prop.Name] = prop.Value;
							if(prop.Name[0] == '#')
								jobj_cur[prop.Name.Substring(1)]?.Parent?.Remove();
							else
								jobj_cur['#' + prop.Name]?.Parent?.Remove();
						}
					}
					else
					{
						JArray jarr_cur = new_root.GetValue(v.Name) as JArray;
						if(jarr_cur == null)
							continue;
						int idx = 0;
						foreach(var jobj in (v.Value as JArray))
						{
							foreach(var prop in (jobj as JObject)?.Properties())
							{
								jarr_cur[idx][prop.Name] = prop.Value;
								if(prop.Name[0] == '#')
									jarr_cur[idx][prop.Name.Substring(1)]?.Parent?.Remove();
								else
									jarr_cur[idx]['#' + prop.Name]?.Parent?.Remove();
							}
							idx++;
						}
					}
				}
			}
			if(_work_name != null && _index != null)
			{
				JObject jobj_process = ((((token as JObject)?.GetValue("work_group") as JObject)?.GetValue(_work_name) as JObject)?.GetValue("processes") as JArray)?[Int32.Parse(_index)] as JObject;
				foreach(var v in jobj_process?.Properties())
				{
					if(v.Value as JArray == null)
					{
						JObject jobj_cur = new_root.GetValue(v.Name) as JObject;
						if(jobj_cur == null)
							continue;
						foreach(var prop in (v.Value as JObject)?.Properties())
						{
							jobj_cur[prop.Name] = prop.Value;
							if(prop.Name[0] == '#')
								jobj_cur[prop.Name.Substring(1)]?.Parent?.Remove();
							else
								jobj_cur['#' + prop.Name]?.Parent?.Remove();
						}
					}
					else
					{
						JArray jarr_cur = new_root.GetValue(v.Name) as JArray;
						if(jarr_cur == null)
							continue;
						int idx = 0;
						foreach(var jobj in (v.Value as JArray))
						{
							foreach(var prop in (jobj as JObject)?.Properties())
							{
								jarr_cur[idx][prop.Name] = prop.Value;
								if(prop.Name[0] == '#')
									jarr_cur[idx][prop.Name.Substring(1)]?.Parent?.Remove();
								else
									jarr_cur[idx]['#' + prop.Name]?.Parent?.Remove();
							}
							idx++;
						}
					}
				}
			}

			current = CreateOption(token["type"]?.ToString(), new_root);
			return current;
		}
		private static string _GetOption(JObject root, string type, string option_name)
		{
			if(root == null || type == null || option_name == null)
				return null;

			string ret_val = null;

			if(option_name == "output_dir")
			{
				if(root["type"]?.ToString() == "file")
					ret_val = root["enc_option"]?[option_name]?.ToString();
				else
					ret_val = root["comm_option"]?[option_name]?.ToString();
			}

			return ret_val;
		}
		public static string GetOption(JObject root, string _work_name, string _process_index, string option_name)
		{
			string ret_val = null;

			if(_process_index != null)
				ret_val = _GetOption(root?["work_group"]?[_work_name]?["processes"]?[Int32.Parse(_process_index)] as JObject, root["type"]?.ToString(), option_name);
			if(_work_name != null && ret_val == null)
				ret_val = _GetOption(root?["work_group"]?[_work_name] as JObject, root["type"]?.ToString(), option_name);
			if(ret_val == null)
				ret_val = _GetOption(root as JObject, root["type"]?.ToString(), option_name);

			return ret_val;
		}

		static string[] arr_json_keyword = { "comm_option", "enc_option", "dec_option", "col_var", "col_fix", "enc_inform", "work_group", "processes" };
		public static int SaveOption(JObject root = null)
		{
			if(root == null)
			{
				if(Root == null
					|| CurRoot == null)
					return -1;
				JObject jobj_cur_data = null;
				JObject jobj_par_data = null;

				if(work_name != null && index == null)
				{
					jobj_cur_data = ((Root as JObject)?.GetValue("work_group") as JObject)?.GetValue(work_name) as JObject;
					jobj_par_data = Root.DeepClone() as JObject;
				}
				else if(work_name != null && index != null)
				{
					jobj_cur_data = ((((Root as JObject)?.GetValue("work_group") as JObject)?.GetValue(work_name) as JObject)?.GetValue("processes") as JArray)?[Int32.Parse(index)] as JObject;
					jobj_par_data = Root.DeepClone() as JObject;
					JObject jobj_work = ((Root as JObject)?.GetValue("work_group") as JObject)?.GetValue(work_name) as JObject;
					foreach(var v in jobj_work.Properties())
					{
						if(v.Value as JArray == null)
						{
							JObject jobj_cur = jobj_par_data.GetValue(v.Name) as JObject;
							if(jobj_cur == null)
								continue;
							foreach(var prop in (v.Value as JObject)?.Properties())
							{
								jobj_cur[prop.Name] = prop.Value;
							}
						}
						else
						{
							JArray jarr_cur = jobj_par_data.GetValue(v.Name) as JArray;
							if(jarr_cur == null)
							{
								jobj_par_data.Add(v.Name, new JArray());
								int i = 0;
								foreach(var jobj in (v.Value as JArray))
								{
									(jobj_par_data[v.Name] as JArray)?.Add(new JObject());
									foreach(var prop in (jobj as JObject)?.Properties())
									{
										(jobj_par_data[v.Name]?[i] as JObject)?.Add(prop.Name, prop.Value);
									}
									i++;
								}
								continue;
							}
							int idx = 0;
							foreach(var jobj in (v.Value as JArray))
							{
								foreach(var prop in (jobj as JObject)?.Properties())
								{
									jarr_cur[idx][prop.Name] = prop.Value;
								}
								idx++;
							}
						}
					}
				}
				else if(work_name == null && index == null)
				{
					jobj_cur_data = Root as JObject;
				}

				if(jobj_cur_data == null)
					return -1;

				foreach(var jprop_section in (CurRoot as JObject)?.Properties())
				{
					int idx_keyword;
					for(idx_keyword = 0; idx_keyword < arr_json_keyword.Length; idx_keyword++)
					{
						if(arr_json_keyword[idx_keyword] == jprop_section.Name)
							break;
					}
					if(idx_keyword == arr_json_keyword.Length)
						continue;

					if(jprop_section.Value as JArray == null)
					{
						foreach(var jprop_changed in (jprop_section.Value as JObject)?.Properties())
						{
							if(jobj_par_data != null
								&& (jobj_par_data[jprop_section.Name]?[jprop_changed.Name] as JValue)?.Value != null
								&& (jobj_par_data[jprop_section.Name]?[jprop_changed.Name] as JValue)?.Value.Equals((jprop_changed.Value as JValue)?.Value) == true)
							{
								jobj_cur_data[jprop_section.Name]?[jprop_changed.Name]?.Parent.Remove();
								if(jprop_changed.Name[0] == '#')
									jobj_cur_data[jprop_section.Name]?[jprop_changed.Name.Substring(1)]?.Parent?.Remove();
								else
									jobj_cur_data[jprop_section.Name]?['#' + jprop_changed.Name]?.Parent?.Remove();

								if(jobj_cur_data[jprop_section.Name]?.LongCount() == 0)
									jobj_cur_data[jprop_section.Name]?.Parent.Remove();
							}
							else if(jobj_cur_data[jprop_section.Name] == null)
							{
								if(jobj_cur_data["processes"] != null)
									jobj_cur_data["processes"]?.Parent?.AddBeforeSelf(new JProperty(jprop_section.Name, new JObject()));
								else
									(jobj_cur_data as JObject)?.Add(jprop_section.Name, new JObject());
								(jobj_cur_data[jprop_section.Name] as JObject)?.Add(jprop_changed.Name, jprop_changed.Value);
							}
							else if(jobj_cur_data[jprop_section.Name][jprop_changed.Name] == null)
							{
								(jobj_cur_data[jprop_section.Name] as JObject)?.Add(jprop_changed.Name, jprop_changed.Value);
								if(jprop_changed.Name[0] == '#')
									jobj_cur_data[jprop_section.Name][jprop_changed.Name.Substring(1)]?.Parent?.Remove();
								else
									jobj_cur_data[jprop_section.Name]['#' + jprop_changed.Name]?.Parent?.Remove();
							}
							else
								jobj_cur_data[jprop_section.Name][jprop_changed.Name] = jprop_changed.Value;
						}
					}
					else
					{
						int idx = 0;
						foreach(var jarr_changed in jprop_section.Value as JArray)
						{
							foreach(var jprop_changed in (jarr_changed as JObject)?.Properties())
							{
								if(jobj_par_data != null
									&& (jobj_par_data[jprop_section.Name]?[idx]?[jprop_changed.Name] as JValue)?.Value != null
									&& (jobj_par_data[jprop_section.Name]?[idx]?[jprop_changed.Name] as JValue)?.Value.Equals((jprop_changed.Value as JValue)?.Value) == true)
								{
									jobj_cur_data[jprop_section.Name]?[idx]?[jprop_changed.Name]?.Parent.Remove();
									if(jprop_changed.Name[0] == '#')
										jobj_cur_data[jprop_section.Name]?[idx]?[jprop_changed.Name.Substring(1)]?.Parent?.Remove();
									else
										jobj_cur_data[jprop_section.Name]?[idx]?['#' + jprop_changed.Name]?.Parent?.Remove();
									int i;
									if(jobj_cur_data[jprop_section.Name]?.LongCount() != null)
									{
										for(i = 0; i < jobj_cur_data[jprop_section.Name].LongCount(); i++)
										{
											if(jobj_cur_data[jprop_section.Name]?[idx]?.LongCount() > 0)
												break;
										}
										if(jobj_cur_data[jprop_section.Name].LongCount() == i)
											jobj_cur_data[jprop_section.Name]?.Parent.Remove();
									}
								}
								else if(jobj_cur_data[jprop_section.Name] == null)
								{
									if(jobj_cur_data["processes"] != null)
										jobj_cur_data["processes"]?.Parent?.AddBeforeSelf(new JProperty(jprop_section.Name, new JArray()));
									else
										(jobj_cur_data as JObject)?.Add(jprop_section.Name, new JArray());
									while((jobj_cur_data[jprop_section.Name] as JArray)?.Count < idx + 1)
										(jobj_cur_data[jprop_section.Name] as JArray)?.Add(new JObject());
									(jobj_cur_data[jprop_section.Name]?[idx] as JObject)?.Add(jprop_changed.Name, jprop_changed.Value);
								}
								else if((jobj_cur_data[jprop_section.Name] as JArray)?.Count < idx + 1)
								{
									while((jobj_cur_data[jprop_section.Name] as JArray)?.Count < idx + 1)
										(jobj_cur_data[jprop_section.Name] as JArray)?.Add(new JObject());
									(jobj_cur_data[jprop_section.Name]?[idx] as JObject)?.Add(jprop_changed.Name, jprop_changed.Value);
								}
								else if(jobj_cur_data[jprop_section.Name][idx][jprop_changed.Name] == null)
								{
									(jobj_cur_data[jprop_section.Name]?[idx] as JObject)?.Add(jprop_changed.Name, jprop_changed.Value);
									if(jprop_changed.Name[0] == '#')
										jobj_cur_data[jprop_section.Name][idx][jprop_changed.Name.Substring(1)]?.Parent?.Remove();
									else
										jobj_cur_data[jprop_section.Name][idx]['#' + jprop_changed.Name]?.Parent?.Remove();
								}
								else
									jobj_cur_data[jprop_section.Name][idx][jprop_changed.Name] = jprop_changed.Value;
							}
							idx++;
						}
					}
				}
				//WindowMain.current?.enableConnect?.sshManager?.UploadString(Root.ToString(), WindowMain.current.enableConnect.sshManager.EnvCoHome + "/var/conf/" + WindowMain.current.enableConnect.sshManager.id + "/" + Root["type"].ToString() + ".json");
				if(WindowMain.current?.EnableConnect?.Name == null
					|| WindowMain.current?.EnableConnect?.Id == null)
					return -1;

				string local_dir = MainSettings.Path.PathDirConfigFile + WindowMain.current.EnableConnect.Name  + "\\" + WindowMain.current.EnableConnect.Id;

				FileContoller.Write(local_dir + "\\" + Root["type"].ToString() + ".json", Root.ToString());
				if(WindowMain.current?.EnableConnect?.SshManager?.SetConfig(local_dir) != 0)
					return -5;
			}
			else
			{
				if(root == null
					|| root["type"] == null
					|| WindowMain.current?.EnableConnect?.Id == null
					|| WindowMain.current?.EnableConnect?.SshManager?.EnvCoHome == null)
					return -2;

				//WindowMain.current?.enableConnect?.sshManager?.UploadString(root.ToString(), WindowMain.current.enableConnect.sshManager.EnvCoHome + "/var/conf/" + WindowMain.current.enableConnect.sshManager.id + "/" + root["type"].ToString() + ".json");
				if(WindowMain.current?.EnableConnect?.Name == null
					|| WindowMain.current?.EnableConnect?.Id == null)
					return -1;

				string local_dir = MainSettings.Path.PathDirConfigFile + WindowMain.current.EnableConnect.Name + "\\" + WindowMain.current.EnableConnect.Id;

				FileContoller.Write(local_dir + "\\" + root["type"].ToString() + ".json", root.ToString());
				if(WindowMain.current?.EnableConnect?.SshManager?.SetConfig(local_dir) != 0)
					return -5;
			}
			return 0;
		}

		public class Group
		{
			public FrameworkElement Header { get; set; }
			public int[] Arr { get; set; }

			// Key UI 의 CheckBox 보다 우선순위가 높다.
			private GroupBodyStyle bodyStyle = GroupBodyStyle.Basic;
			public GroupBodyStyle BodyStyle { get { return bodyStyle; } set { bodyStyle = value; } }
			public string RadioButtonGroupName { get; set; }
		}

		private static T Find<T>(FrameworkElement ui) where T : FrameworkElement
		{
			T retval = ui as T;
			if(retval == null)
			{
				var list = ui.FindChildren<T>();
				foreach(var v in list)
				{
					retval = v;
					break;
				}
			}
			return retval;
		}
		public static void CheckedKey(ref JProperty jprop)
		{
			try
			{
				if(jprop.Parent[jprop.Name.TrimStart(ConfigOptionManager.StartDisableProperty)] != null)
				{
					if(jprop != jprop.Parent[jprop.Name.TrimStart(ConfigOptionManager.StartDisableProperty)].Parent)
						jprop.Parent[jprop.Name.TrimStart(ConfigOptionManager.StartDisableProperty)].Parent.Remove();
				}
				JProperty newJprop = new JProperty(jprop.Name.TrimStart(ConfigOptionManager.StartDisableProperty), jprop.Value);
				jprop.Replace(newJprop);
				Log.PrintLog(jprop + " -> " + newJprop, "UserControls.ConfigOptions.ConfigOptionManager.CheckedKey");
				// delegate 에 지역변수를 사용하면 지역변수를 메모리에서 계속 잡고있는다. (전역변수 화 (어디 소속으로 전역변수 인지 모르겠다.))
				jprop = newJprop;

			}
			catch(Exception ex)
			{
				Log.PrintError(ex.Message, "UserControls.ConfigOptions.ConfigOptionManager.CheckedKey");
			}
			ConfigOptionManager.bChanged = true;
		}
		public static void UncheckedKey(ref JProperty jprop)
		{
			try
			{
				if(jprop.Parent[ConfigOptionManager.StartDisableProperty + jprop.Name] != null)
				{
					if(jprop != jprop.Parent[ConfigOptionManager.StartDisableProperty + jprop.Name].Parent)
						jprop.Parent[ConfigOptionManager.StartDisableProperty + jprop.Name].Parent.Remove();
				}
				JProperty newJprop = new JProperty(ConfigOptionManager.StartDisableProperty + jprop.Name, jprop.Value);
				jprop.Replace(newJprop);
				Log.PrintLog(jprop + " -> " + newJprop, "UserControls.ConfigOptions.ConfigOptionManager.UncheckedKey");
				// delegate 에 지역변수를 사용하면 지역변수를 메모리에서 계속 잡고있는다. (전역변수 화 (어디 소속으로 전역변수 인지 모르겠다.))
				jprop = newJprop;
			}
			catch(Exception ex)
			{
				Log.PrintError(ex.Message, "UserControls.ConfigOptions.ConfigOptionManager.UncheckedKey");
			}
			ConfigOptionManager.bChanged = true;
		}

		public delegate FrameworkElement GetUI(int opt, JObject root);
		const int WIDTH_KEY = 450;
		public static void MakeUI(Grid grid, JObject root, string[] detailOptions, Group[] groups,
			GetUI GetUIOptionKey, GetUI GetUIOptionValue)
		{
			for(int i = 0; i < groups.Length; i++)
			{
				Border bd = new Border() {BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(1) };
				Grid grid_group = new Grid();
				grid_group.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
				grid_group.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
				bd.Child = grid_group;
				grid.Children.Add(bd);

				grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
				Grid.SetRow(bd, grid.RowDefinitions.Count - 1);

				Grid grid_group_body = new Grid();
				grid_group_body.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(WIDTH_KEY) });
				grid_group_body.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
				grid_group_body.Margin = new Thickness(15, 0, 0, 0);
				if(groups[i].Header != null)
				{
					grid_group.Children.Add(groups[i].Header);
					Grid.SetRow(groups[i].Header, 0);
				}

				grid_group.Children.Add(grid_group_body);
				Grid.SetRow(grid_group_body, 1);

				for(int j = 0; j < groups[i].Arr.Length; j++)
				{
					try
					{
						Grid grid_key = new Grid();
						Grid grid_value = new Grid();

						FrameworkElement ui_key = GetUIOptionKey(groups[i].Arr[j], root);
						if(ui_key == null)
							break;
						StackPanel sp = ui_key as StackPanel;
						if(sp != null && sp.Children.Count > 0)
						{
							CheckBox cb = sp.Children[0] as CheckBox;
							if(cb != null)
							{
								Binding _bd = new Binding("IsChecked") { Source = cb, Mode = BindingMode.OneWay, Converter = new OnlyBooleanConverter() };
								grid_value.SetBinding(Grid.IsEnabledProperty, _bd);
								//grid_value.IsEnabled = cb.IsChecked.Value;
								//cb.Checked += delegate { grid_value.IsEnabled = cb.IsChecked.Value; };
								//cb.Unchecked += delegate { grid_value.IsEnabled = cb.IsChecked.Value; };
							}

							RadioButton rb = sp.Children[0] as RadioButton;
							if(rb != null)
							{
								rb.GroupName = groups[i].RadioButtonGroupName;
								Binding _bd = new Binding("IsChecked") { Source = rb, Mode = BindingMode.OneWay, Converter = new OnlyBooleanConverter() };
								grid_value.SetBinding(Grid.IsEnabledProperty, _bd);

								var rg = grid_group_body.FindChildren<RadioButton>();
								foreach(var v in rg)
								{
									if(v.IsChecked == true)
									{
										rb.IsChecked = false;
										break;
									}
								}
							}
						}

						grid_key.Children.Add(ui_key);

						FrameworkElement ui_value = GetUIOptionValue(groups[i].Arr[j], root);
						if(ui_value == null)
							break;

						grid_value.Children.Add(ui_value);

						if(groups[i].Header == null)
						{
							Grid grid_group_header = new Grid();
							grid_group_header.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(WIDTH_KEY) });
							grid_group_header.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
							grid_group.Children.Add(grid_group_header);
							Grid.SetRow(grid_group_header, 0);

							Grid.SetColumn(grid_key, 0);
							grid_group_header.Children.Add(grid_key);

							Grid.SetColumn(grid_value, 1);
							grid_group_header.Children.Add(grid_value);

							groups[i].Header = grid_group_header;

							ToggleSwitch ts = ui_value as ToggleSwitch;
							if(ts != null)
							{
								Binding _bd = new Binding("IsChecked") { Source = ts, Mode = BindingMode.OneWay, Converter = new OnlyBooleanConverter() };
								grid_group_body.SetBinding(Grid.IsEnabledProperty, _bd);
								//ts.Checked += delegate { grid_group_body.IsEnabled = true; };
								//ts.Unchecked += delegate { grid_group_body.IsEnabled = false; };
							}

							ComboBox cb = ui_value as ComboBox;
							if(cb != null)
							{
								Binding _bd = new Binding("SelectedIndex")
								{
									Source = cb,
									Mode = BindingMode.OneWay,
									Converter = new Int32ToBooleanConverter(),
									ConverterParameter = Root
								};
								grid_group_body.SetBinding(Grid.IsEnabledProperty, _bd);
							}
						}
						else
						{
							grid_group_body.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
							int idxRow = grid_group_body.RowDefinitions.Count - 1;

							Grid.SetRow(grid_key, idxRow);
							Grid.SetColumn(grid_key, 0);
							grid_group_body.Children.Add(grid_key);

							Grid.SetRow(grid_value, idxRow);
							Grid.SetColumn(grid_value, 1);
							grid_group_body.Children.Add(grid_value);
						}
					}
					catch(Exception e)
					{
						Console.WriteLine(e.Message);
						continue;
					}
				}
			}
		}

	}

	#region Converter
	public sealed class MultiBinderConverter : IMultiValueConverter
	{
		public object Convert(object[] values, System.Type targetType, object parameter, CultureInfo culture)
		{
			if(values == null)
				return null;

			int i;
			for(i = 0; i < values.Length; i++)
			{
				if(values[i] != null)
					break;
			}

			return System.Convert.ToInt64(values[i]);
		}

		public object[] ConvertBack(object value, System.Type[] targetTypes, object parameter, CultureInfo culture)
		{
			return new object[] { value, value };
		}
	}
	public sealed class OnlyStringConverter : IValueConverter
	{
		public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
		{
			return System.Convert.ToString(value);
		}

		public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
		{
			return System.Convert.ToString(value);
		}
	}
	public sealed class OnlyBooleanConverter : IValueConverter
	{
		public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
		{
			return System.Convert.ToBoolean(value);
		}

		public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
		{
			return System.Convert.ToBoolean(value);
		}
	}
	public sealed class OnlyInt64Converter : IValueConverter
	{
		public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
		{
			return System.Convert.ToInt64(value);
		}

		public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
		{
			return System.Convert.ToInt64(value);
		}
	}
	public sealed class OnlyInt64TailTypeConverter : IValueConverter
	{
		public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
		{
			return System.Convert.ToInt64(value) - 1;
		}

		public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
		{
			return System.Convert.ToInt64(value) + 1;
		}
	}

	public sealed class Int64ToStringConverter : IValueConverter
	{
		public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
		{
			return System.Convert.ToString(value);
		}

		public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
		{
			return System.Convert.ToInt64(value);
		}
	}
	public sealed class StringToInt64Converter : IValueConverter
	{
		public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
		{
			return System.Convert.ToInt64(value);
		}

		public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
		{
			return System.Convert.ToString(value);
		}
	}
	public sealed class Int32ToBooleanConverter : IValueConverter
	{
		public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
		{
			JToken jtok = parameter as JToken;
			if(jtok == null || jtok["type"] == null)
				return false;

			if(jtok["type"].ToString() == "sam")
			{
				if(System.Convert.ToInt32(value) == 0)
					return true;
				else
					return false;
			}
			else if(jtok["type"].ToString() == "tail")
			{
				if(System.Convert.ToInt32(value) == 1)
					return true;
				else
					return false;
			}

			return false;
		}

		public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
		{
			return 0;
		}
	}

	public sealed class MethodToValueConverter : IValueConverter
	{
		public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
		{
			var methodName = parameter as string;
			if(value == null || methodName == null)
				return null;
			var methodInfo = value.GetType().GetMethod(methodName, new System.Type[0]);
			if(methodInfo == null)
				return null;
			return methodInfo.Invoke(value, new object[0]);
		}

		public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException(GetType().Name + " can only be used for one way conversion.");
		}
	}
	#endregion

}
