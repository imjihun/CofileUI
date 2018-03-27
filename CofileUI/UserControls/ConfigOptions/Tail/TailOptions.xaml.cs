using CofileUI.Classes;
using CofileUI.Windows;
using MahApps.Metro.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace CofileUI.UserControls.ConfigOptions.Tail
{
	/// <summary>
	/// TailOptions.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class TailOptions : UserControl
	{
		bool bInit = false;
		JObject Root { get; set; }
		public static TailOptions current;
		public TailOptions()
		{
			current = this;
			InitializeComponent();
			ConfigOptionManager.IsChanged = false;
			this.Loaded += delegate
			{
				if(!bInit)
				{
					Root = DataContext as JObject;
					if(Root == null)
						return;

					grid1.Children.Add(new comm_option(Root["comm_option"] as JObject));
					ChangeSecondGrid();
					bInit = true;
				}
			};
		}

		public void ChangeSecondGrid()
		{
			JObject root = DataContext as JObject;
			if(root == null)
				return;
			JObject jobj = root["comm_option"] as JObject;
			if(jobj == null)
			{
				Log.PrintLog("NotFound Tail.comm_option", "UserControls.ConfigOptions.Tail.TailOptions.ChangeSecondGrid");
				return;
			}
			JValue jval_tail_type = jobj["tail_type"] as JValue;
			if(jval_tail_type == null)
			{
				Log.PrintLog("NotFound Tail.comm_option.tail_type", "UserControls.ConfigOptions.Tail.TailOptions.ChangeSecondGrid");
				return;
			}

			grid2.Children.Clear();
			if(root["enc_inform"] == null)
			{
				Log.PrintLog("NotFound Tail.enc_inform", "UserControls.ConfigOptions.Tail.TailOptions.ChangeSecondGrid");
				return;
			}

			if(Convert.ToInt64(jval_tail_type.Value) == 1)
			{
				//ChangeBySamType(root, "col_var", "col_fix");
				if(root["enc_inform"].Count() <= 0)
				{
					JArray _jarr = root["enc_inform"] as JArray;
					JObject _jobj = new JObject();
					_jobj.Add(new JProperty("item", "ARIA256CBC"));
					_jarr.Add(_jobj);
				}
				grid2.Children.Add(new enc_inform_line(root["enc_inform"][0] as JObject));
			}
			else if(Convert.ToInt64(jval_tail_type.Value) == 2)
			{
				//JValue jval_reg_yn = jobj["reg_yn"] as JValue;
				//if(jval_reg_yn == null)
				//{
				//	Log.PrintLog("NotFound Tail.comm_option.jval_reg_yn", "UserControls.ConfigOptions.Tail.TailOptions.ChangeSecondGrid");
				//	return;
				//}

				//if(Convert.ToBoolean(jval_reg_yn.Value) == true)
				//{
				//	ChangeEncOptionProperties(root, false);
				//	grid2.Children.Add(new enc_inform_reg() { DataContext = root["enc_inform"].Parent });
				//}
				//else if(Convert.ToBoolean(jval_reg_yn.Value) == false)
				//{
				//	ChangeEncOptionProperties(root, true);
				//	grid2.Children.Add(new enc_inform() { DataContext = root["enc_inform"].Parent });
				//}
				ChangeEncOptionProperties(root, true);
				grid2.Children.Add(new enc_inform() { DataContext = root["enc_inform"].Parent });
			}
		}
		static void ChangeEncOptionProperties(JObject root, bool benc_inform)
		{
			if(root == null)
				return;
			JArray jarr = root["enc_inform"] as JArray;
			if(jarr == null)
				return;

			try
			{
				if(benc_inform)
				{
					for(int k = 0; k < jarr.Children().Count(); k++)
					{
						JObject jobj = jarr[k] as JObject;
						if(jobj == null)
							continue;
						
						for(int i = 0; i < (int)enc_inform.OriginalOption.Length; i++)
						{
							bool bHave = false;
							for(int j = 0; j < (int)enc_inform.Option.Length; j++)
							{
								if(((enc_inform.OriginalOption)i).ToString() == ((enc_inform.Option)j).ToString())
								{
									bHave = true;
									break;
								}
							}

							if(jobj[((enc_inform.OriginalOption)i).ToString()] != null)
							{
								if(bHave)
								{
								}
								else
								{
									jobj[((enc_inform.OriginalOption)i).ToString()].Parent.Replace(
										new JProperty(ConfigOptionManager.StartDisableProperty + ((enc_inform.OriginalOption)i).ToString(), jobj[((enc_inform.OriginalOption)i).ToString()])
										);
									ConfigOptionManager.IsChanged = true;
								}
							}
							else if(jobj[ConfigOptionManager.StartDisableProperty + ((enc_inform.OriginalOption)i).ToString()] != null)
							{
								if(bHave)
								{
									jobj[ConfigOptionManager.StartDisableProperty + ((enc_inform.OriginalOption)i).ToString()].Parent.Replace(
										new JProperty(((enc_inform.OriginalOption)i).ToString(), jobj[ConfigOptionManager.StartDisableProperty + ((enc_inform.OriginalOption)i).ToString()])
										);
									ConfigOptionManager.IsChanged = true;
								}
								else
								{
								}
							}
						}
					}
				}
				else
				{
					for(int k = 0; k < jarr.Children().Count(); k++)
					{
						JObject jobj = jarr[k] as JObject;
						if(jobj == null)
							continue;

						for(int i = 0; i < (int)enc_inform_reg.OriginalOption.Length; i++)
						{
							bool bHave = false;
							for(int j = 0; j < (int)enc_inform_reg.Option.Length; j++)
							{
								if(((enc_inform_reg.OriginalOption)i).ToString() == ((enc_inform_reg.Option)j).ToString())
								{
									bHave = true;
									break;
								}
							}

							if(jobj[((enc_inform_reg.OriginalOption)i).ToString()] != null)
							{
								if(bHave)
								{
								}
								else
								{
									jobj[((enc_inform_reg.OriginalOption)i).ToString()].Parent.Replace(
										new JProperty(ConfigOptionManager.StartDisableProperty + ((enc_inform_reg.OriginalOption)i).ToString(), jobj[((enc_inform_reg.OriginalOption)i).ToString()])
										);
									ConfigOptionManager.IsChanged = true;
								}
							}
							else if(jobj[ConfigOptionManager.StartDisableProperty + ((enc_inform_reg.OriginalOption)i).ToString()] != null)
							{
								if(bHave)
								{
									jobj[ConfigOptionManager.StartDisableProperty + ((enc_inform_reg.OriginalOption)i).ToString()].Parent.Replace(
										new JProperty(((enc_inform_reg.OriginalOption)i).ToString(), jobj[ConfigOptionManager.StartDisableProperty + ((enc_inform_reg.OriginalOption)i).ToString()])
										);
									ConfigOptionManager.IsChanged = true;
								}
								else
								{
								}
							}
						}
					}
				}
			}
			catch(Exception ex)
			{
				Log.PrintError(ex.Message, "UserControls.ConfigOptions.Tail.TailOptions.ChangeEncOptionProperties");
			}
		}
		private void TitleMouseDown(object sender, MouseButtonEventArgs e)
		{
			if(e.ChangedButton == MouseButton.Left)
			{
				Window parentWindow = Window.GetWindow(this);
				parentWindow.DragMove();
			}
		}
	}
}