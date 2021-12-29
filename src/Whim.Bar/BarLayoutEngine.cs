using System.Collections.Generic;

namespace Whim.Bar;

public class BarLayoutEngine : ILayoutEngine
{
	private readonly BarConfig _barConfig;
	private readonly ILayoutEngine _innerLayoutEngine;

	public string Name => "Whim.Bar";

	public Commander Commander { get; } = new();

	public BarLayoutEngine(BarConfig barConfig, ILayoutEngine innerLayoutEngine)
	{
		_barConfig = barConfig;
		_innerLayoutEngine = innerLayoutEngine;
	}

	public void AddWindow(IWindow window)
	{
		_innerLayoutEngine.AddWindow(window);
	}

	public IEnumerable<IWindowLocation> DoLayout(ILocation location)
	{
		Location proxiedLocation = new(location.X, location.Y + _barConfig.Height, location.Width, location.Height - _barConfig.Height);
		return _innerLayoutEngine.DoLayout(proxiedLocation);
	}

	public bool RemoveWindow(IWindow window)
	{
		return _innerLayoutEngine.RemoveWindow(window);
	}
}
