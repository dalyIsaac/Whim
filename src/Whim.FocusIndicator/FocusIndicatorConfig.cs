using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System;
using System.ComponentModel;
using Windows.UI;

namespace Whim.FocusIndicator;

public class FocusIndicatorConfig : INotifyPropertyChanged
{
	internal const string Title = "Whim Focus Indicator";

	/// <inheritdoc/>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <inheritdoc/>
	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private Color _color = Colors.Red;

	/// <summary>
	/// The color of the focus indicator.
	/// </summary>
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

	/// <summary>
	/// The size of the focus indicator border, in pixels.
	/// </summary>
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

	/// <summary>
	/// When <see langword="true"/>, the focus indicator is visible.
	/// </summary>
	public bool IsVisible
	{
		get => _isVisible;
		set
		{
			_isVisible = value;
			OnPropertyChanged(nameof(IsVisible));
		}
	}

	/// <summary>
	/// When <see langword="true"/>, the focus indicator will be visible for
	/// <see cref="FadeTimeout"/>.
	/// </summary>
	public bool FadeEnabled { get; set; }

	/// <summary>
	/// The amount of time that the focus indicator will be visible, when
	/// <see langword="FadeEnabled"/>.
	/// </summary>
	public TimeSpan FadeTimeout { get; set; } = TimeSpan.FromSeconds(10);
}
