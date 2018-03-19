using MahApps.Metro.IconPacks;
using CofileUI.Classes;
using Newtonsoft.Json.Linq;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using CofileUI.Windows;
using CofileUI.UserControls.ConfigOptions;

namespace CofileUI.UserControls
{

	public class ConfigMenuButton : ToggleButton
	{
		private JObject root = null;
		public JObject Root
		{
			get { return root; }
			set
			{
				root = value;
				RefreshChild();
			}
		}

		const double HEIGHT = 30;
		const double FONTSIZE = 13;
		public ConfigMenuTreeView child;
		public ConfigMenuRootPanel pan_parent = null;
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
					|| this.pan_parent?.btn_selected.Root?["work_group"] as JObject == null)
						return;
					(this.pan_parent?.btn_selected.Root?["work_group"] as JObject).Add(new JProperty(work_group_name, new JObject(new JProperty("processes", new JArray()))));
					ConfigOptionManager.SaveOption(this.Root);

					ConfigMenuTreeViewItem ui_config_group = new ConfigMenuTreeViewItem(this, this.pan_parent?.btn_selected.Root, work_group_name);
					ui_config_group.IsExpanded = true;
					this.pan_parent?.btn_selected.child.Items.Add(ui_config_group);
				}
				catch(Exception ex)
				{
					Log.ErrorIntoUI("config 그룹명이 중복됩니다.\r", "Add Config Group Name", Status.current.richTextBox_status);
					Log.PrintError(ex.Message, "UserControls.ConfigOptions.ConfigPanel.ConfigInfoPanel");
					Console.WriteLine("JHLIM_DEBUG : " + ex.Message);
				}
			}
		}
		public ConfigMenuButton(ConfigMenuRootPanel _pan_parent, JObject _Root, string header)
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

			this.child = new ConfigMenuTreeView();
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

		void RefreshChild()
		{
			this.child?.Items.Clear();

			JObject jobj_work_group_root = root?["work_group"] as JObject;
			if(jobj_work_group_root == null)
				return;

			foreach(var work in jobj_work_group_root.Properties())
			{
				JObject jobj_config_menu = work.Value as JObject;
				if(jobj_config_menu == null)
					continue;

				ConfigMenuTreeViewItem ui_config_group = new ConfigMenuTreeViewItem(this, root, work.Name);
				ui_config_group.IsExpanded = true;
				this.child.Items.Add(ui_config_group);

				JArray jarr_processes = jobj_config_menu?["processes"] as JArray;
				if(jarr_processes == null)
					continue;
				int i = 0;
				foreach(var jprop_config_info in jarr_processes)
				{
					JObject jobj_process_info = jprop_config_info as JObject;
					if(jobj_process_info == null)
						continue;
					string dir = null;
					if(root["type"].ToString() == "file")
						dir = (jobj_process_info["enc_option"] as JObject)?["input_dir"]?.ToString();
					else
						dir = (jobj_process_info["comm_option"] as JObject)?["input_dir"]?.ToString();
					ui_config_group.Items.Add(new ConfigMenuTreeViewItem(this, root, work.Name, i.ToString(), dir));

					i++;
				}
			}
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
			if(Root != null && e.ChangedButton == MouseButton.Left && this.IsChecked == true)
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

}
