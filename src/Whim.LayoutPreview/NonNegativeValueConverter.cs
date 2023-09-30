using Microsoft.UI.Xaml.Data;
using System;

namespace Whim.LayoutPreview;

public class NonNegativeValueConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		bool isNegative = value switch {
			int i => i < 0,
			double d => d < 0,
			_ => throw new ArgumentException($"Unexpected type {value.GetType().Name}", nameof(value))
		};
		return System.Convert.ChangeType(isNegative ? 0 : value, targetType);
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotImplementedException();
	}
}
