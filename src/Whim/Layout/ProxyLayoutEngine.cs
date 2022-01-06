using System.Collections;
using System.Collections.Generic;

namespace Whim;

/// <summary>
/// Abstract layout engine, which proxy layout engines should inherit from.
/// </summary>
public abstract class BaseProxyLayoutEngine : ILayoutEngine
{
	protected readonly ILayoutEngine _innerLayoutEngine;
	public Commander Commander { get; } = new();

	public BaseProxyLayoutEngine(ILayoutEngine innerLayoutEngine)
	{
		_innerLayoutEngine = innerLayoutEngine;
	}

	/// <summary>
	/// The name is only really important for the user, so we can use the name of the proxied layout engine.
	/// </summary>
	public string Name => _innerLayoutEngine.Name;

	public int Count => _innerLayoutEngine.Count;

	public bool IsReadOnly => _innerLayoutEngine.IsReadOnly;

	public void Add(IWindow window) => _innerLayoutEngine.Add(window);

	public bool Remove(IWindow window) => _innerLayoutEngine.Remove(window);

	public IWindow? GetFirstWindow() => _innerLayoutEngine.GetFirstWindow();

	public IWindow? GetPreviousWindow(IWindow window) => _innerLayoutEngine.GetPreviousWindow(window);

	public IWindow? GetNextWindow(IWindow window) => _innerLayoutEngine.GetNextWindow(window);

	public void Clear() => _innerLayoutEngine.Clear();

	public bool Contains(IWindow window) => _innerLayoutEngine.Contains(window);

	public void CopyTo(IWindow[] array, int arrayIndex) => _innerLayoutEngine.CopyTo(array, arrayIndex);

	public IEnumerator<IWindow> GetEnumerator() => _innerLayoutEngine.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public abstract IEnumerable<IWindowLocation> DoLayout(ILocation location);
}
