﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Whim.Bar;

/// <summary>
/// Delegate for creating a component.
/// A component will subscribe to <see cref="Microsoft.UI.Xaml.Window.Closed"/> if it has resources to dispose.
/// </summary>
public delegate UserControl BarComponent(IContext context, IMonitor monitor, Microsoft.UI.Xaml.Window window);

/// <summary>
/// Configuration for the bar plugin.
/// </summary>
/// <remarks>
/// The components lists can be changed up until when Whim is initialized.
/// </remarks>
/// <remarks>
/// Creates a new bar configuration.
/// </remarks>
/// <param name="leftComponents">The components to display on the left side of the bar.</param>
/// <param name="centerComponents">The components to display in the center of the bar.</param>
/// <param name="rightComponents">The components to display on the right side of the bar.</param>
public class BarConfig(
	IList<BarComponent> leftComponents,
	IList<BarComponent> centerComponents,
	IList<BarComponent> rightComponents
) : INotifyPropertyChanged
{
	/// <inheritdoc/>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>
	/// The components to display on the left side of the bar.
	/// </summary>
	internal IList<BarComponent> LeftComponents = leftComponents;

	/// <summary>
	/// The components to display in the center of the bar.
	/// </summary>
	internal IList<BarComponent> CenterComponents = centerComponents;

	/// <summary>
	/// The components to display on the right side of the bar.
	/// </summary>
	internal IList<BarComponent> RightComponents = rightComponents;

	private int _height = GetHeightFromResourceDictionary() ?? 30;

	/// <summary>
	/// The height of the bar, in <see href="https://learn.microsoft.com/en-us/windows/win32/learnwin32/dpi-and-device-independent-pixels">device-independent pixels</see>.
	/// Setting this explicitly takes precedence over the ResourceDictionary.
	/// </summary>
	public int Height
	{
		get => _height;
		set
		{
			_height = value;
			OnPropertyChanged(nameof(Height));
		}
	}

	/// <summary>
	/// Handler to call when a property changes.
	/// </summary>
	/// <param name="propertyName">The name of the property that changed.</param>
	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	/// <summary>
	/// The backdrop/material to use for the bar. Changes to this property during runtime will
	/// not be reflected in the UI - any changes must be done in the csx config <i>prior</i> to initializing Whim.
	/// Switching between different backdrops can influence the transparency - for more see <see cref="BackdropType"/>.
	///
	/// Defaults to <see cref="BackdropType.Mica"/>, and to always show the backdrop.
	/// </summary>
	/// <remarks>
	/// To change the opacity for the bar's background color, make sure the hex color includes the alpha values.
	/// </remarks>
	public WindowBackdropConfig Backdrop { get; set; } = new(BackdropType.Mica, AlwaysShowBackdrop: true);

	private static int? GetHeightFromResourceDictionary()
	{
		try
		{
			Style style = (Style)Application.Current.Resources["bar:height"];
			double height = (double)((Setter)style.Setters[0]).Value;
			return Convert.ToInt32(height);
		}
		catch
		{
			Logger.Debug("Could not find valid <bar:height> in ResourceDictionary");
			return null;
		}
	}
}
