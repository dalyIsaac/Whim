using System;
using System.ComponentModel;
using Windows.UI;

namespace Whim.FocusIndicator;

public class FocusIndicatorConfig : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private Color _borderColor;
	public Color BorderColor
	{
		get => _borderColor;
		set
		{
			_borderColor = value;
			OnPropertyChanged(nameof(BorderColor));
		}
	}

	private int _borderSize;
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

	public bool FadeEnabled { get; set; }
	public TimeSpan FadeTimeout { get; set; }
}
