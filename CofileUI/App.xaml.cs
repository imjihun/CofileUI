using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace CofileUI
{
	/// <summary>
	/// App.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class App : Application
	{
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
}
