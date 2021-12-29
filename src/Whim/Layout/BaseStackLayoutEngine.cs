using System.Collections.Generic;

namespace Whim;

/// <summary>
/// Abstract layout engine with a stack data structure.
/// </summary>
public abstract class BaseStackLayoutEngine : ILayoutEngine
{
	protected readonly List<IWindow> _stack = new();

	public Commander Commander { get; } = new();

	public abstract string Name { get; }

	public void AddWindow(IWindow window)
	{
		Logger.Debug($"Adding window {window.Title} to layout engine {Name}");
		_stack.Add(window);
	}

	public bool RemoveWindow(IWindow window)
	{
		Logger.Debug($"Removing window {window.Title} from layout engine {Name}");
		return _stack.RemoveAll(x => x.Handle == window.Handle) > 0;
	}

	public abstract IEnumerable<IWindowLocation> DoLayout(ILocation location);
}
