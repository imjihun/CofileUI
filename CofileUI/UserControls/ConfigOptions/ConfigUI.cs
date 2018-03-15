using MahApps.Metro;
using MahApps.Metro.IconPacks;
using CofileUI.Classes;
using CofileUI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using CofileUI.Windows;
using System.Windows.Data;

namespace CofileUI.UserControls.ConfigOptions
{
	#region Server Panel
	/// <summary>
	/// ServerPanel	-> ServerMenuButton
	///				-> SubPanel -> ServerList -> ServerInfoTextBlock(ServerInfo)
	/// </summary>
	
	public class RelayCommand : ICommand
	{
		#region Fields

		readonly Action<object> _execute;
		readonly Predicate<object> _canExecute;

		#endregion // Fields

		#region Constructors

		public RelayCommand(Action<object> execute)
			: this(execute, null)
		{
		}

		public RelayCommand(Action<object> execute, Predicate<object> canExecute)
		{
			if(execute == null)
				throw new ArgumentNullException("execute");

			_execute = execute;
			_canExecute = canExecute;
		}
		#endregion // Constructors

		#region ICommand Members

		public bool CanExecute(object parameter)
		{
			return _canExecute == null ? true : _canExecute(parameter);
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void Execute(object parameter)
		{
			_execute(parameter);
		}

		#endregion // ICommand Members
	}
	
	
	public class ConfigInfoPanel : TreeViewItem
	{
		public JObject root;
		public string work_name;
		public string index;
		public string path;
		
		public StackPanel sp;
		public TextBlock tb;
		public PackIconModern icon;

		public ConfigMenuButton bnt_parent;

		private void CreateMember()
		{
			sp = new StackPanel();

			sp.Orientation = Orientation.Horizontal;
			icon = new PackIconModern()
			{
				Kind = PackIconModernKind.Connect,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			icon.Margin = new Thickness(2, 0, 3, 0);
			sp.Children.Add(icon);

			//Binding binding = new Binding("IsConnected");
			//binding.Mode = BindingMode.OneWay;
			//binding.Source = this;
			//binding.Converter = new BooleanToVisibilityConverter();
			//BindingOperations.SetBinding(icon,PackIconModern.VisibilityProperty, binding);
			icon.Visibility = Visibility.Hidden;

			tb = new TextBlock();
			tb.Foreground = Brushes.Black;
			sp.Children.Add(tb);

			this.Header = sp;
		}

		RelayCommand AddConfigWorkGroupCommand;
		void AddConfigWorkGroup(object parameter)
		{
			Window_AddConfigWorkGroup wms = new Window_AddConfigWorkGroup();
			Point pt = this.PointToScreen(new Point(0, 0));
			wms.Left = pt.X;
			wms.Top = pt.Y;
			if(wms.ShowDialog() == true)
			{
				try
				{
					string work_group_name = wms.textBox_name.Text;
					if(this.bnt_parent?.pan_parent?.btn_selected == null
					|| this.root["work_group"] as JObject == null)
						return;
					(this.root["work_group"] as JObject).Add(new JProperty(work_group_name, new JObject(new JProperty("processes", new JArray()))));

					ConfigInfoPanel ui_config_group = new ConfigInfoPanel(this.bnt_parent, this.root, work_group_name);
					ui_config_group.IsExpanded = true;
					this.bnt_parent?.pan_parent?.btn_selected.child.Items.Add(ui_config_group);
				}
				catch(Exception ex)
				{
					Log.ErrorIntoUI("config 그룹명이 중복됩니다.\r", "Add Config Group Name", Status.current.richTextBox_status);
					Log.PrintError(ex.Message, "UserControls.ConfigOptions.ConfigPanel.ServerInfoPanel");
				}
			}
		}
		RelayCommand DelConfigWorkGroupCommand;
		void DelConfigWorkGroup(object parameter)
		{
			if(ServerInfo.jobj_root == null)
				return;

			WindowMain.current.ShowMessageDialog("Delete Config Work Group", "해당 Config Group 을 정말 삭제하시겠습니까? 하위 Config 정보도 모두 삭제됩니다.", MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegative, 
				() =>
				{
					try
					{
						this.root["work_group"]?[work_name]?.Parent?.Remove();
						int? _cnt = this.bnt_parent?.pan_parent?.btn_selected.child.Items.Count;
						int cnt = _cnt == null ? 0 : _cnt.Value;
						for(int i = 0; i < cnt; i++)
						{
							if(work_name != null
							&& (this.bnt_parent?.pan_parent?.btn_selected.child.Items[i] as ConfigInfoPanel)?.tb.Text == work_name)
							{
								this.bnt_parent?.pan_parent?.btn_selected.child.Items.RemoveAt(i);
								break;
							}
						}
					}
					catch(Exception ex)
					{
						Log.ErrorIntoUI(ex.Message, "Del Server Menu", Status.current.richTextBox_status);
						Log.PrintError(ex.Message, "UserControls.ServerMenuButton.DeleteServerMenuUI");
					}
				});
		}

		public ConfigInfoPanel(ConfigMenuButton _bnt_parent, JObject _root, string _work_name, string _index = null, string _path = null)
		{
			CreateMember();
			this.bnt_parent = _bnt_parent;
			this.root = _root;
			this.work_name = _work_name;
			this.index = _index;
			this.HorizontalAlignment = HorizontalAlignment.Stretch;
			if(_index == null)
				this.tb.Text = _work_name;
			else
				this.tb.Text = _index + "\t" + _path;
			this.AllowDrop = true;
			path = _path;

			AddConfigWorkGroupCommand = new RelayCommand(AddConfigWorkGroup);
			this.ContextMenu = new ContextMenu();
			MenuItem item;

			item = new MenuItem();
			item.Command = AddConfigWorkGroupCommand;
			item.Header = "Add Config Work Group";
			item.Icon = new PackIconMaterial()
			{
				Kind = PackIconMaterialKind.FolderPlus,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			this.ContextMenu.Items.Add(item);

			DelConfigWorkGroupCommand = new RelayCommand(DelConfigWorkGroup);
			item = new MenuItem();
			item.Command = DelConfigWorkGroupCommand;
			item.Header = "Del Config Work Group";
			item.Icon = new PackIconMaterial()
			{
				Kind = PackIconMaterialKind.FolderRemove,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			this.ContextMenu.Items.Add(item);
		}

		protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseRightButtonDown(e);
			//e.Handled = true;
			//this.IsSelected = true;
			if(this.Parent as ConfigList != null)
				this.IsSelected = true;
		}

		protected override void OnDragOver(DragEventArgs e)
		{
			base.OnDragOver(e);
			//var par = this.Parent as ConfigList;
			//if(par != null)
			//{
			//	this.Focus();
			//}
			this.Focus();
			e.Handled = true;
		}
		protected override void OnDrop(DragEventArgs e)
		{
			base.OnDrop(e);
			Object data_obj = (Object)e.Data.GetData("Object");
			List<LinuxTreeViewItem> list_ltvi = data_obj as List<LinuxTreeViewItem>;
			if(list_ltvi != null)
			{
				int i;
				for(i = 0; i<list_ltvi.Count;i++)
				{
					LinuxTreeViewItem ltvi = list_ltvi[i];

					string tmp_index = "";
					bool? retval = false;
					if(this.work_name != null && this.index == null && ltvi.IsDirectory)
					{
						JObject jobj;
						if(this.root["type"].ToString() == "file")
							jobj = new JObject(new JProperty("enc_option", new JObject(new JProperty("input_dir", ltvi.Path))));
						else
							jobj = new JObject(new JProperty("comm_option", new JObject(new JProperty("input_dir", ltvi.Path))));
						if(jobj != null)
						{
							JObject tmp_root = this.root.DeepClone() as JObject;
							(tmp_root?["work_group"]?[work_name]?["processes"] as JArray)?.Add(jobj);
							tmp_index = this.Items.Count.ToString();

							Window_Config wc = new Window_Config(tmp_root, this.work_name, tmp_index, true, ltvi.Path );
							Point pt = WindowMain.current.PointToScreen(new Point(WindowMain.current.Width - wc.Width, WindowMain.current.Height - wc.Height));
							wc.Left = pt.X;
							wc.Top = pt.Y;
							retval = wc.ShowDialog();
							if(retval == true)
							{
								(this.root?["work_group"]?[work_name]?["processes"] as JArray)?.Add(jobj);
								this.Items.Add(new ConfigInfoPanel(this.bnt_parent, this.root, this.work_name, tmp_index, ltvi.Path));

								//string cofile_type = this.root["type"].ToString();
								//string config_info = SSHController.EnvCoHome + "/var/conf/test/" + cofile_type + ".json";
								//SSHController.NewSendNRecvCofileCommand(list_ltvi, true, config_info, cofile_type);
								Console.WriteLine("JHLIM_DEBUG : Encrypt {0} {1} {2} [{3}]", this.root["type"], this.work_name, tmp_index, ltvi.Path);
							}
							else
								break;
						}
					}
				}
				if(i == list_ltvi.Count)
				{
					WindowMain.current.ShowMessageDialog(
						this.root["type"] + " 암호화",
						"해당항목들을 암호화 하시겠습니까?",
						MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegative,
						() =>
						{
							string cofile_type = this.root["type"].ToString();
							string config_info = SSHController.EnvCoHome + "/var/conf/test/" + cofile_type + ".json";
							if(SSHController.NewSendNRecvCofileCommand(list_ltvi, true, config_info, cofile_type))
							{
								WindowMain.current.ShowMessageDialog(
									this.root["type"] + " 암호화",
									"암호화 요청을 보냈습니다.",
									MahApps.Metro.Controls.Dialogs.MessageDialogStyle.Affirmative);
							}
							else
							{
								WindowMain.current.ShowMessageDialog(
									this.root["type"] + " 암호화",
									"암호화 요청에 실패하였습니다.",
									MahApps.Metro.Controls.Dialogs.MessageDialogStyle.Affirmative);
							}
						});
				}
			}
			e.Handled = true;
		}
	}

	public class ConfigList : TreeView
	{
		public ConfigMenuButton parent;

		private void OnClickEnvSetting(object sender, RoutedEventArgs e)
		{
			Window_EnvSetting wms = new Window_EnvSetting();
			Point pt = this.PointToScreen(new Point(0, 0));
			wms.Left = pt.X;
			wms.Top = pt.Y;
			wms.textBox_cohome.Text = SSHController.EnvCoHome;
			if(wms.ShowDialog() == true)
			{
				SSHController.EnvCoHome = wms.textBox_cohome.Text;
			}
		}
		
		public ConfigList()
		{
			this.Margin = new Thickness(20, 0, 0, 0);
			this.BorderBrush = null;

			this.HorizontalContentAlignment = HorizontalAlignment.Stretch;
		}

		protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
		{
			base.OnMouseDoubleClick(e);
			if(e.ChangedButton == MouseButton.Left)
			{
				JObject root = null;
				string work_name = null;
				string index = null;

				root = (this.SelectedItem as ConfigInfoPanel)?.root;
				work_name = (this.SelectedItem as ConfigInfoPanel)?.work_name;
				index = (this.SelectedItem as ConfigInfoPanel)?.index;

				Window_Config wc = new Window_Config(root, work_name, index, path: (this.SelectedItem as ConfigInfoPanel)?.path);
				Point pt = WindowMain.current.PointToScreen(new Point(WindowMain.current.Width - wc.Width, WindowMain.current.Height - wc.Height));
				wc.Left = pt.X;
				wc.Top = pt.Y;
				wc.Show();
				e.Handled = true;
			}
		}
	}

	public class ConfigMenuButton : ToggleButton
	{
		public JObject Root = null;
		const double HEIGHT = 30;
		const double FONTSIZE = 13;
		public ConfigList child;
		public ConfigPanel pan_parent = null;
		private void InitStyle()
		{
			Style style = new Style(typeof(ConfigMenuButton), (Style)App.Current.Resources["MetroToggleButton"]);
			Trigger trigger_selected = new Trigger() {Property = ToggleButton.IsCheckedProperty, Value = true };
			trigger_selected.Setters.Add(new Setter(ToggleButton.BackgroundProperty, (SolidColorBrush)App.Current.Resources["AccentColorBrush"]));
			trigger_selected.Setters.Add(new Setter(ToggleButton.ForegroundProperty, Brushes.White));
			style.Triggers.Add(trigger_selected);
			
			Trigger trigger_mouseover = new Trigger() {Property = ToggleButton.IsMouseOverProperty, Value = true };
			SolidColorBrush s = new SolidColorBrush(((SolidColorBrush)App.Current.Resources["AccentColorBrush"]).Color);
			s.Opacity = .5;
			trigger_mouseover.Setters.Add(new Setter(ToggleButton.BackgroundProperty, s));
			style.Triggers.Add(trigger_mouseover);

			this.Style = style;
		}

		RelayCommand AddConfigWorkGroupCommand;
		void AddConfigWorkGroup(object parameter)
		{
			Window_AddConfigWorkGroup wms = new Window_AddConfigWorkGroup();
			Point pt = this.PointToScreen(new Point(0, 0));
			wms.Left = pt.X;
			wms.Top = pt.Y;
			if(wms.ShowDialog() == true)
			{
				try
				{
					string work_group_name = wms.textBox_name.Text;
					if(this.pan_parent?.btn_selected == null
					|| this.pan_parent?.btn_selected.Root["work_group"] as JObject == null)
						return;
					(this.pan_parent?.btn_selected.Root["work_group"] as JObject).Add(new JProperty(work_group_name, new JObject(new JProperty("processes", new JArray()))));

					ConfigInfoPanel ui_config_group = new ConfigInfoPanel(this, this.pan_parent?.btn_selected.Root, work_group_name);
					ui_config_group.IsExpanded = true;
					this.pan_parent?.btn_selected.child.Items.Add(ui_config_group);
				}
				catch(Exception ex)
				{
					Log.ErrorIntoUI("config 그룹명이 중복됩니다.\r", "Add Config Group Name", Status.current.richTextBox_status);
					Log.PrintError(ex.Message, "UserControls.ConfigOptions.ConfigPanel.ServerInfoPanel");
				}
			}
		}
		public ConfigMenuButton(ConfigPanel _pan_parent, JObject _Root, string header)
		{
			this.pan_parent = _pan_parent;
			Root = _Root;
			this.InitStyle();

			this.Content = header;
			//this.Background = Brushes.White;
			this.Height = HEIGHT;
			this.HorizontalAlignment = HorizontalAlignment.Stretch;
			this.VerticalAlignment = VerticalAlignment.Bottom;
			this.FontSize = FONTSIZE;

			this.child = new ConfigList();
			this.child.Visibility = Visibility.Collapsed;
			this.child.VerticalAlignment = VerticalAlignment.Top;
			this.child.parent = this;

			this.pan_parent?.btn_group.Add(this);
			if(this.pan_parent != null)
			{
				for(int i = 0; i < this.pan_parent.btn_group.Count; i++)
				{
					this.pan_parent.btn_group[i].Margin = new Thickness(0, i * HEIGHT, 0, (this.pan_parent.btn_group.Count - (i + 1)) * HEIGHT);
				}
			}

			AddConfigWorkGroupCommand = new RelayCommand(AddConfigWorkGroup);
			this.ContextMenu = new ContextMenu();
			MenuItem item;

			item = new MenuItem();
			item.Command = AddConfigWorkGroupCommand;
			item.Header = "Add Config Work Group";
			item.Icon = new PackIconMaterial()
			{
				Kind = PackIconMaterialKind.FolderPlus,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			this.ContextMenu.Items.Add(item);
		}

		protected override void OnToggle()
		{
			if(this.pan_parent != null)
			{
				for(int i = 0; i < this.pan_parent.btn_group.Count; i++)
				{
					this.pan_parent.btn_group[i].IsChecked = false;
				}
			}
			base.OnToggle();
		}

		protected override void OnUnchecked(RoutedEventArgs e)
		{
			base.OnUnchecked(e);
			this.child.Visibility = Visibility.Collapsed;
		}
		// Brush background_unchecked = null;
		protected override void OnChecked(RoutedEventArgs e)
		{
			base.OnChecked(e);
			if(this.pan_parent == null)
				return;

			int idx = this.pan_parent.btn_group.IndexOf(this);

			int i;
			for(i = 0; i <= idx; i++)
			{
				this.pan_parent.btn_group[i].VerticalAlignment = VerticalAlignment.Top;
			}
			for(; i < this.pan_parent.btn_group.Count; i++)
			{
				this.pan_parent.btn_group[i].VerticalAlignment = VerticalAlignment.Bottom;
			}

			if(this.pan_parent != null)
				this.pan_parent.SubPanel.Margin = new Thickness(0, HEIGHT * (idx + 1), 0, HEIGHT * (this.pan_parent.btn_group.Count - (idx + 1)));
			this.child.Visibility = Visibility.Visible;
			if(this.pan_parent != null)
				this.pan_parent.btn_selected = this;
		}
		protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseRightButtonDown(e);
			this.OnToggle();
		}
		protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
		{
			base.OnMouseDoubleClick(e);
			if(e.ChangedButton == MouseButton.Left && this.IsChecked == true)
			{
				Window_Config wc = new Window_Config(this.Root);
				Point pt = WindowMain.current.PointToScreen(new Point(WindowMain.current.Width - wc.Width, WindowMain.current.Height - wc.Height));
				wc.Left = pt.X;
				wc.Top = pt.Y;
				wc.Show();
			}
			e.Handled = true;
		}
		protected override void OnClick()
		{
			base.OnClick();
		}
	}

	public class ConfigPanel : Grid
	{
		public ConfigMenuButton btn_selected = null;
		public List<ConfigMenuButton> btn_group = new List<ConfigMenuButton>();

		#region ServerInfoPanel
		public class ConfigInfoPanel : Grid
		{
			ConfigPanel pan_parent = null;
			RelayCommand AddConfigWorkGroupCommand;

			void AddConfigWorkGroup(object parameter)
			{
				Window_AddConfigWorkGroup wms = new Window_AddConfigWorkGroup();
				Point pt = this.PointToScreen(new Point(0, 0));
				wms.Left = pt.X;
				wms.Top = pt.Y;
				if(wms.ShowDialog() == true)
				{
					try
					{
						string work_group_name = wms.textBox_name.Text;
						if(this.pan_parent?.btn_selected == null
						|| this.pan_parent?.btn_selected.Root["work_group"] as JObject == null)
							return;
						(this.pan_parent?.btn_selected.Root["work_group"] as JObject).Add(new JProperty(work_group_name, new JObject(new JProperty("processes", new JArray()))));

						ConfigOptions.ConfigInfoPanel ui_config_group = new ConfigOptions.ConfigInfoPanel(this.pan_parent?.btn_selected, this.pan_parent?.btn_selected.Root, work_group_name);
						ui_config_group.IsExpanded = true;
						this.pan_parent?.btn_selected.child.Items.Add(ui_config_group);
					}
					catch(Exception ex)
					{
						Log.ErrorIntoUI("config 그룹명이 중복됩니다.\r", "Add Config Group Name", Status.current.richTextBox_status);
						Log.PrintError(ex.Message, "UserControls.ConfigOptions.ConfigPanel.ServerInfoPanel");
					}
				}
			}
			public ConfigInfoPanel(ConfigPanel _pan_parent)
			{
				this.pan_parent = _pan_parent;
				this.Background = Brushes.White;
				AddConfigWorkGroupCommand = new RelayCommand(AddConfigWorkGroup);
				this.ContextMenu = new ContextMenu();
				MenuItem item;

				item = new MenuItem();
				item.Command = AddConfigWorkGroupCommand;
				item.Header = "Add Config Work Group";
				item.Icon = new PackIconMaterial()
				{
					Kind = PackIconMaterialKind.FolderPlus,
					VerticalAlignment = VerticalAlignment.Center,
					HorizontalAlignment = HorizontalAlignment.Center
				};
				this.ContextMenu.Items.Add(item);
			}
		}
		public ConfigInfoPanel SubPanel;
		#endregion

		public ConfigPanel()
		{
			this.Background = null;

			SubPanel = new ConfigInfoPanel(this);
			this.Children.Add(SubPanel);
		}
	}
	#endregion
}
