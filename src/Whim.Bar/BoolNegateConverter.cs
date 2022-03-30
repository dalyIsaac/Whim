using Microsoft.UI.Xaml.Data;
using System;

namespace Whim.Bar;

internal class BoolNegateConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		return !(value is bool v && v);
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		return !(value is bool v && v);
	}
}
