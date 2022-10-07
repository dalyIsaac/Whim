using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Whim.Bar;

/// <summary>
/// Delegate for creating a component.
/// A component will subscribe to <see cref="Microsoft.UI.Xaml.Window.Closed"/> if it has resources to dispose.
/// </summary>
public delegate UserControl BarComponent(IConfigContext configContext, IMonitor monitor, Microsoft.UI.Xaml.Window window);

/// <summary>
/// Configuration for the bar plugin.
/// </summary>
public class BarConfig : INotifyPropertyChanged
{
	/// <inheritdoc/>
	public event PropertyChangedEventHandler? PropertyChanged;

	private List<BarComponent> _leftComponents;

	/// <summary>
	/// The components to display on the left side of the bar.
	/// </summary>
	public IEnumerable<BarComponent> LeftComponents
	{
		get => _leftComponents;
		set
		{
			_leftComponents = value.ToList();
			OnPropertyChanged(nameof(LeftComponents));
		}
	}

	private List<BarComponent> _centerComponents;

	/// <summary>
	/// The components to display in the center of the bar.
	/// </summary>
	public IEnumerable<BarComponent> CenterComponents
	{
		get => _centerComponents;
		set
		{
			_centerComponents = value.ToList();
			OnPropertyChanged(nameof(CenterComponents));
		}
	}

	private List<BarComponent> _rightComponents;

	/// <summary>
	/// The components to display on the right side of the bar.
	/// </summary>
	public IEnumerable<BarComponent> RightComponents
	{
		get => _rightComponents;
		set
		{
			_rightComponents = value.ToList();
			OnPropertyChanged(nameof(RightComponents));
		}
	}

	private int _height = 48;

	/// <summary>
	/// The height of the bar.
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

	private Thickness _margin = new(10.0);

	/// <summary>
	/// The margin of the bar.
	/// </summary>
	public Thickness Margin
	{
		get => _margin;
		set
		{
			_margin = value;
			OnPropertyChanged(nameof(Margin));
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
	public BarConfig(IEnumerable<BarComponent>? leftComponents = null, IEnumerable<BarComponent>? centerComponents = null, IEnumerable<BarComponent>? rightComponents = null)
	{
		_leftComponents = (leftComponents ?? Enumerable.Empty<BarComponent>()).ToList();
		_centerComponents = (centerComponents ?? Enumerable.Empty<BarComponent>()).ToList();
		_rightComponents = (rightComponents ?? Enumerable.Empty<BarComponent>()).ToList();
	}
}
