using System.Collections.Generic;
using System.ComponentModel;
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
public class BarConfig : INotifyPropertyChanged
{
	/// <inheritdoc/>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>
	/// The components to display on the left side of the bar.
	/// </summary>
	internal IList<BarComponent> LeftComponents;

	/// <summary>
	/// The components to display in the center of the bar.
	/// </summary>
	internal IList<BarComponent> CenterComponents;

	/// <summary>
	/// The components to display on the right side of the bar.
	/// </summary>
	internal IList<BarComponent> RightComponents;

	private int _height = 48;

	/// <summary>
	/// The height of the bar, in <see href="https://learn.microsoft.com/en-us/windows/win32/learnwin32/dpi-and-device-independent-pixels">device-independent pixels</see>.
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
	/// Creates a new bar configuration.
	/// </summary>
	/// <param name="leftComponents">The components to display on the left side of the bar.</param>
	/// <param name="centerComponents">The components to display in the center of the bar.</param>
	/// <param name="rightComponents">The components to display on the right side of the bar.</param>
	public BarConfig(
		IList<BarComponent> leftComponents,
		IList<BarComponent> centerComponents,
		IList<BarComponent> rightComponents
	)
	{
		LeftComponents = leftComponents;
		CenterComponents = centerComponents;
		RightComponents = rightComponents;
	}
}
