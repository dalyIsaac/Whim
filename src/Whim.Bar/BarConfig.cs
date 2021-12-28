using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace Whim.Bar;

using BarComponent = Func<IConfigContext, UserControl>;

public class BarConfig : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	private List<BarComponent> _leftComponents;
	public IEnumerable<BarComponent> LeftComponents
	{
		get => _leftComponents;
		set
		{
			_leftComponents = value.ToList();
			OnPropertyChanged(nameof(LeftComponents));
		}
	}

	private List<BarComponent> _middleComponents;
	public IEnumerable<BarComponent> CenterComponents
	{
		get => _middleComponents;
		set
		{
			_middleComponents = value.ToList();
			OnPropertyChanged(nameof(CenterComponents));
		}
	}

	private List<BarComponent> _rightComponents;
	public IEnumerable<BarComponent> RightComponents
	{
		get => _rightComponents;
		set
		{
			_rightComponents = value.ToList();
			OnPropertyChanged(nameof(RightComponents));
		}
	}

	private int _height;
	public int Height
	{
		get => _height;
		set
		{
			_height = value;
			OnPropertyChanged(nameof(Height));
		}
	}

	private int _padding;
	public int Padding
	{
		get => _padding;
		set
		{
			_padding = value;
			OnPropertyChanged(nameof(Padding));
		}
	}

	private int _spacing;
	public int Spacing
	{
		get => _spacing;
		set
		{
			_spacing = value;
			OnPropertyChanged(nameof(Spacing));
		}
	}

	private int _borderRadius;
	public int BorderRadius
	{
		get => _borderRadius;
		set
		{
			_borderRadius = value;
			OnPropertyChanged(nameof(BorderRadius));
		}
	}

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public BarConfig(IEnumerable<BarComponent>? leftComponents = null, IEnumerable<BarComponent>? middleComponents = null, IEnumerable<BarComponent>? rightComponents = null)
	{
		_leftComponents = (leftComponents ?? Enumerable.Empty<BarComponent>()).ToList();
		_middleComponents = (middleComponents ?? Enumerable.Empty<BarComponent>()).ToList();
		_rightComponents = (rightComponents ?? Enumerable.Empty<BarComponent>()).ToList();
	}
}
