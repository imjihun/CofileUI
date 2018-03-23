using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace CofileUI
{
	/// <summary>
	/// App.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class App : Application
	{
		public App() : base()
		{
			//Application.Current.DispatcherUnhandledException += App_DispatcherUnhandledException;
		}
	}
	public sealed class ReverseBooleanToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var flag = false;
			if(value is bool)
			{
				flag = (bool)value;
			}
			else if(value is bool?)
			{
				var nullable = (bool?)value;
				flag = nullable.GetValueOrDefault();
			}
			if(parameter != null)
			{
				if(bool.Parse((string)parameter))
				{
					flag = !flag;
				}
			}
			if(flag)
			{
				return Visibility.Visible;
			}
			else
			{
				return Visibility.Collapsed;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var back = ((value is Visibility) && (((Visibility)value) == Visibility.Visible));
			if(parameter != null)
			{
				if((bool)parameter)
				{
					back = !back;
				}
			}
			return back;
		}
	}

	public sealed class BooleanToVisibilityConverter2 : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool? flag = false;
			if(value is bool?)
			{
				flag = (bool?)value;
			}

			if(flag == true)
				return Visibility.Visible;
			else if(flag == false)
				return Visibility.Hidden;
			else
				return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value is Visibility)
				return false;

			Visibility v = (Visibility)value;
			if(v == Visibility.Visible)
				return true;
			else
				return false;
		}
	}

	public sealed class CountToThicknessConverter : IValueConverter
	{
		/*
		 * value = count; 
		 * parameter[0] = index;
		 * parameter[1] = HEIGHT
		 */
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Thickness thickness;
			if(value is int
				&& parameter as object[] != null
				&& ((object[])parameter).Length == 2
				&& ((object[])parameter)[0] is UserControls.ServerGroupPanel
				&& ((object[])parameter)[1] is double)
			{
				int count = (int)value;
				UserControls.ServerGroupPanel sgp = ((object[])parameter)[0] as UserControls.ServerGroupPanel;
				System.Windows.Controls.UIElementCollection children = (sgp.Parent as UserControls.ServerGroupRootPanel)?.Children;
				if(children != null)
				{
					int i = children.IndexOf(sgp);
					double HEIGHT = (double)((object[])parameter)[1];
					thickness = new Thickness(0, i * HEIGHT, 0, (count - (i + 1)) * HEIGHT);
				}
				else
					thickness = new Thickness(0);
			}
			else
			{
				thickness = new Thickness(0);
			}

			return thickness;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return 0;
		}
	}
}
