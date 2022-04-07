using System;
using System.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Whim.FocusIndicator;

public class FocusIndicatorConfig : INotifyPropertyChanged
{
	internal const string Title = "Whim Focus Indicator";

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private Color _color = Colors.Red;
	public Brush Color
	{
		get => new SolidColorBrush(_color);
		set
		{
			if (value is SolidColorBrush colorBrush)
			{
				_color = colorBrush.Color;
				OnPropertyChanged(nameof(Color));
			}
		}
	}

	private int _borderSize = 10;
	public int BorderSize
	{
		get => _borderSize;
		set
		{
			_borderSize = value;
			OnPropertyChanged(nameof(BorderSize));
		}
	}

	private bool _isVisible;
	public bool IsVisible
	{
		get => _isVisible;
		set
		{
			_isVisible = value;
			OnPropertyChanged(nameof(IsVisible));
		}
	}

	public bool FadeEnabled { get; set; } = false;
	public TimeSpan FadeTimeout { get; set; } = TimeSpan.FromSeconds(10);
}
