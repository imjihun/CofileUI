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
		JObject root;
		public JObject Root { get { return root; } set { root = value; } }

		public const double HEIGHT = 30;
		public const double FONTSIZE = 13;
		public ConfigMenuTreeView child;
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
			if(WindowMain.current?.EnableConnect?.SshManager?.IsConnected != true)
				return;

			Window_AddConfigWorkGroup wms = new Window_AddConfigWorkGroup();
			Point pt = this.PointToScreen(new Point(0, 0));
			wms.Left = pt.X;
			wms.Top = pt.Y;
			if(wms.ShowDialog() == true)
			{
				try
				{
					string work_group_name = wms.textBox_name.Text;
					JObject cloneRoot = this.Root?.DeepClone() as JObject;
					JProperty jprop_new = new JProperty(work_group_name, new JObject(new JProperty("processes", new JArray())));
					if(cloneRoot?["work_group"] as JObject == null)
						return;
					(cloneRoot?["work_group"] as JObject).Add(jprop_new);
					if(ConfigOptionManager.SaveOption(cloneRoot) != 0)
						return;

					(this.Root?["work_group"] as JObject)?.Add(jprop_new);

					ConfigMenuTreeViewItem ui_config_group = new ConfigMenuTreeViewItem() {
						TreeRoot = (this.Parent as ConfigMenuPanel)?.DetailView,
						ConfigIdx = new ConfigMenuModel(this.Root) { WorkName = work_group_name }
					};
					ui_config_group.IsExpanded = true;
					(this.Parent as ConfigMenuPanel)?.DetailView.Items.Add(ui_config_group);
				}
				catch(Exception ex)
				{
					Log.ErrorIntoUI("config 그룹명이 중복됩니다.\r", "Add Config Group Name", Status.current.richTextBox_status);
					Log.PrintError(ex.Message, "UserControls.ConfigOptions.ConfigPanel.ConfigInfoPanel");
					Console.WriteLine("JHLIM_DEBUG : " + ex.Message);
					WindowMain.current?.ShowMessageDialog("Add Config Fail", "그룹명이 중복됩니다.\n" + ex.Message, MahApps.Metro.Controls.Dialogs.MessageDialogStyle.Affirmative);
				}
			}
		}
		public ConfigMenuButton()
		{
			this.InitStyle();
			
			this.Height = HEIGHT;
			this.HorizontalAlignment = HorizontalAlignment.Stretch;
			this.VerticalAlignment = VerticalAlignment.Bottom;
			this.FontSize = FONTSIZE;

			this.child = new ConfigMenuTreeView();
			this.child.Visibility = Visibility.Collapsed;
			this.child.VerticalAlignment = VerticalAlignment.Top;
			this.child.parent = this;
			
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
		public ConfigMenuButton(ConfigMenuRootPanel _pan_parent, JObject _Root, string header)
		{
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
			_pan_parent.Children.Add(this.child);

			//this.ParentPanel?.btn_group.Add(this);
			if(_pan_parent != null)
			{
				for(int i = 0; i < _pan_parent.Children.Count; i++)
				{
					ConfigMenuButton btn = _pan_parent.Children[i] as ConfigMenuButton;
					if(btn != null)
						btn.Margin = new Thickness(0, i * HEIGHT, 0, (_pan_parent.Children.Count - (i + 1)) * HEIGHT);
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
		
		protected override void OnUnchecked(RoutedEventArgs e)
		{
			base.OnUnchecked(e);
			ConfigMenuTreeView detailView = (this.Parent as ConfigMenuPanel).DetailView;
			if(detailView != null)
				detailView.Visibility = Visibility.Collapsed;

			UIElementCollection group = ((this.Parent as ConfigMenuPanel)?.Parent as ConfigMenuRootPanel)?.Children;
			if(group == null)
				return;

			int count = 0;
			for(int i = 0; i < group.Count; i++)
			{
				if((group[i] as ConfigMenuPanel).IsChecked == true)
					count++;
			}
			if(count < 1)
				this.IsChecked = true;
		}
		protected override void OnChecked(RoutedEventArgs e)
		{
			base.OnChecked(e);
			ConfigMenuRootPanel root_pan = (this.Parent as ConfigMenuPanel)?.Parent as ConfigMenuRootPanel;
			if(root_pan == null)
				return;

			int idx = root_pan.Children.IndexOf(this.Parent as ConfigMenuPanel);

			int i;
			for(i = 0; i < idx; i++)
			{
				ConfigMenuPanel pan = root_pan.Children[i] as ConfigMenuPanel;
				if(pan != null)
					pan.VerticalAlignment = VerticalAlignment.Top;
				pan.IsChecked = false;
			}
			{
				ConfigMenuPanel pan = root_pan.Children[i++] as ConfigMenuPanel;
				if(pan != null)
					pan.VerticalAlignment = VerticalAlignment.Stretch;
			}
			for(; i < root_pan.Children.Count; i++)
			{
				ConfigMenuPanel pan = root_pan.Children[i] as ConfigMenuPanel;
				if(pan != null)
					pan.VerticalAlignment = VerticalAlignment.Bottom;
				pan.IsChecked = false;
			}

			ConfigMenuTreeView detailView = (this.Parent as ConfigMenuPanel)?.DetailView;
			if(detailView != null)
			{
				//if(root_pan != null)
				//{
				//	detailView.Margin = new Thickness(0, HEIGHT * (idx + 1), 0, HEIGHT * (root_pan.Children.Count - (idx + 1)));
				//}
				detailView.Visibility = Visibility.Visible;
			}
		}
		protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseRightButtonDown(e);
			this.Focus();
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
