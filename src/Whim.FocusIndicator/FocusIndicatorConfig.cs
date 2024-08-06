using System;
using System.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Whim.FocusIndicator;

/// <summary>
/// Configuration for the focus indicator plugin.
/// </summary>
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

	/// <summary>
	/// The backdrop/material to use for the focus indicator. Changes to this property during runtime will
	/// not be reflected in the UI - any changes must be done in the csx config <i>prior</i> to initializing Whim.
	/// Switching between different backdrops can influence the transparency - for more see <see cref="BackdropType"/>.
	///
	/// Defaults to <see cref="BackdropType.Mica"/>, and to always show the backdrop.
	/// </summary>
	public WindowBackdropConfig Backdrop { get; set; } = new(BackdropType.Mica, AlwaysShowBackdrop: true);

	private Color _color = Colors.Transparent;

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

	private int _borderSize = 4;

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
