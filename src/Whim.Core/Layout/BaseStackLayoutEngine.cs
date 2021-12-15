using System.Collections.Generic;

namespace Whim.Core.Layout;

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
		Logger.Debug("Adding window {title} to layout engine {engine}", window.Title, Name);
		_stack.Add(window);
	}

	public void RemoveWindow(IWindow window)
	{
		Logger.Debug("Removing window {title} from layout engine {engine}", window.Title, Name);
		_stack.RemoveAll(x => x.Handle == window.Handle);
	}

	public abstract IEnumerable<IWindowLocation> DoLayout(IArea area);
}
