using System;
using System.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

namespace Whim.FocusIndicator;

public class FocusIndicatorConfig : INotifyPropertyChanged
{
	internal const string Title = "Whim Focus Indicator";

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private Brush? _borderBrush;
	public Brush BorderBrush
	{
		get => _borderBrush ?? new SolidColorBrush(Colors.Red);
		set
		{
			if (_borderBrush != value)
			{
				_borderBrush = value;
				OnPropertyChanged(nameof(BorderBrush));
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
