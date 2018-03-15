using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CofileUI.UserControls
{
	public static class Behaviours
	{
		#region ExpandedBehaviour (Attached DependencyProperty)
		public static readonly DependencyProperty ExpandedBehaviourProperty =
		DependencyProperty.RegisterAttached("ExpandedBehaviour", typeof(ICommand), typeof(Behaviours),
			new PropertyMetadata(OnExpandedBehaviourChanged));
		public static void SetExpandedBehaviour(DependencyObject o, ICommand value)
		{
			o.SetValue(ExpandedBehaviourProperty, value);
		}
		public static ICommand GetExpandedBehaviour(DependencyObject o)
		{
			return (ICommand)o.GetValue(ExpandedBehaviourProperty);
		}
		private static void OnExpandedBehaviourChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			//Console.WriteLine("JHLIM_DEBUG : OnExpandedBehaviourChanged");
			TreeViewItem tvi = d as TreeViewItem;
			if(tvi != null)
			{
				ICommand ic = e.NewValue as ICommand;
				if(ic != null)
				{
					tvi.Expanded += (_sender, _e) =>
					{
						if(ic.CanExecute(_e))
						{
							ic.Execute(_e);
						}
						_e.Handled = true;
					};
				}
			}
		}
		#endregion

		#region MouseMoveBehaviour (Attached DependencyProperty)
		public static readonly DependencyProperty MouseMoveBehaviourProperty =
		DependencyProperty.RegisterAttached("MouseMoveBehaviour", typeof(ICommand), typeof(Behaviours),
			new PropertyMetadata(OnMouseMoveBehaviourChanged));
		public static void SetMouseMoveBehaviour(DependencyObject o, ICommand value)
		{
			o.SetValue(MouseMoveBehaviourProperty, value);
		}
		public static ICommand GetMouseMoveBehaviour(DependencyObject o)
		{
			return (ICommand)o.GetValue(MouseMoveBehaviourProperty);
		}
		private static void OnMouseMoveBehaviourChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			//Console.WriteLine("JHLIM_DEBUG : OnMouseMoveBehaviourChanged");
			UIElement tv = d as UIElement;
			if(tv != null)
			{
				ICommand ic = e.NewValue as ICommand;
				if(ic != null)
				{
					tv.MouseMove += (_sender, _e) =>
					{
						if(ic.CanExecute(_e))
						{
							ic.Execute(_e);
						}
						_e.Handled = true;
					};
				}
			}
		}
		#endregion

		#region MouseLeftButtonUpBehaviour (Attached DependencyProperty)
		public static readonly DependencyProperty MouseLeftButtonUpBehaviourProperty =
		DependencyProperty.RegisterAttached("MouseLeftButtonUpBehaviour", typeof(ICommand), typeof(Behaviours),
			new PropertyMetadata(OnMouseLeftButtonUpBehaviourChanged));
		public static void SetMouseLeftButtonUpBehaviour(DependencyObject o, ICommand value)
		{
			o.SetValue(MouseLeftButtonUpBehaviourProperty, value);
		}
		public static ICommand GetMouseLeftButtonUpBehaviour(DependencyObject o)
		{
			return (ICommand)o.GetValue(MouseLeftButtonUpBehaviourProperty);
		}
		private static void OnMouseLeftButtonUpBehaviourChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			//Console.WriteLine("JHLIM_DEBUG : OnMouseLeftButtonUpBehaviourChanged");
			UIElement tv = d as UIElement;
			if(tv != null)
			{
				ICommand ic = e.NewValue as ICommand;
				if(ic != null)
				{
					tv.MouseLeftButtonUp += (_sender, _e) =>
					{
						if(ic.CanExecute(_e))
						{
							ic.Execute(_e);
						}
						_e.Handled = true;
					};
				}
			}
		}
		#endregion
	}
}
