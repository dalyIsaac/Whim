using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Whim;

/// <summary>
/// Abstract layout engine with a stack data structure.
/// </summary>
public abstract class BaseStackLayoutEngine : ILayoutEngine
{
	protected readonly List<IWindow> _stack = new();

	public Commander Commander { get; } = new();

	public abstract string Name { get; }

	public int Count => _stack.Count;

	public bool IsReadOnly => false;

	public void Add(IWindow window)
	{
		Logger.Debug($"Adding window {window.Title} to layout engine {Name}");
		_stack.Add(window);
	}

	public bool Remove(IWindow window)
	{
		Logger.Debug($"Removing window {window.Title} from layout engine {Name}");
		return _stack.RemoveAll(x => x.Handle == window.Handle) > 0;
	}

	public void Clear()
	{
		Logger.Debug($"Clearing layout engine {Name}");
		_stack.Clear();
	}

	public bool Contains(IWindow window)
	{
		Logger.Debug($"Checking if layout engine {Name} contains window {window.Title}");
		return _stack.Any(x => x.Handle == window.Handle);
	}

	public void CopyTo(IWindow[] array, int arrayIndex)
	{
		Logger.Debug($"Copying layout engine {Name} to array");
		_stack.CopyTo(array, arrayIndex);
	}

	public IEnumerator<IWindow> GetEnumerator() => _stack.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public abstract IEnumerable<IWindowLocation> DoLayout(ILocation location);

	public IWindow? GetFirstWindow()
	{
		return _stack.FirstOrDefault();
	}

	public abstract void FocusWindowInDirection(WindowDirection direction, IWindow window);

	public abstract void SwapWindowInDirection(WindowDirection direction, IWindow window);
}
