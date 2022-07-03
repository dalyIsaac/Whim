using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Whim;

/// <summary>
/// Abstract layout engine with a stack data structure.
/// </summary>
public abstract class BaseStackLayoutEngine : ILayoutEngine
{
	/// <summary>
	/// The stack of windows in the engine.
	/// </summary>
	protected readonly List<IWindow> _stack = new();

	/// <summary>
	/// The weights of each window in the stack.
	/// </summary>
	protected readonly List<double> _weights = new();

	/// <inheritdoc/>
	public abstract string Name { get; }

	/// <inheritdoc/>
	public int Count => _stack.Count;

	/// <inheritdoc/>
	public bool IsReadOnly => false;

	/// <inheritdoc/>
	public void Add(IWindow window)
	{
		Logger.Debug($"Adding window {window} to layout engine {Name}");
		_stack.Add(window);
	}

	/// <inheritdoc/>
	public bool Remove(IWindow window)
	{
		Logger.Debug($"Removing window {window} from layout engine {Name}");
		return _stack.RemoveAll(x => x.Handle == window.Handle) > 0;
	}

	/// <inheritdoc/>
	public void Clear()
	{
		Logger.Debug($"Clearing layout engine {Name}");
		_stack.Clear();
	}

	/// <inheritdoc/>
	public bool Contains(IWindow window)
	{
		Logger.Debug($"Checking if layout engine {Name} contains window {window}");
		return _stack.Any(x => x.Handle == window.Handle);
	}

	/// <inheritdoc/>
	public void CopyTo(IWindow[] array, int arrayIndex)
	{
		Logger.Debug($"Copying layout engine {Name} to array");
		_stack.CopyTo(array, arrayIndex);
	}

	/// <inheritdoc/>
	public IEnumerator<IWindow> GetEnumerator() => _stack.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <inheritdoc/>
	public abstract IEnumerable<IWindowState> DoLayout(ILocation<int> location);

	/// <inheritdoc/>
	public IWindow? GetFirstWindow() => _stack.FirstOrDefault();

	/// <inheritdoc/>
	public abstract void FocusWindowInDirection(Direction direction, IWindow window);

	/// <inheritdoc/>
	public abstract void SwapWindowInDirection(Direction direction, IWindow window);

	/// <inheritdoc/>
	public abstract void MoveWindowEdgeInDirection(Direction edge, double delta, IWindow window);

	/// <inheritdoc/>
	public abstract void AddWindowAtPoint(IWindow window, IPoint<double> point, bool isPhantom);

	/// <inheritdoc/>
	public void HidePhantomWindows() { }
}
