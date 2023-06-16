using System.Collections;
using System.Collections.Generic;

namespace Whim;

/// <summary>
/// This delegate takes an in a layout engine and returns it with a wrapper layout engine.
/// The wrapper layout engine provides additional functionality, but still utilises the underlying
/// layout engine.
/// </summary>
public delegate ILayoutEngine ProxyLayoutEngine(ILayoutEngine engine);

/// <summary>
/// Abstract layout engine, which proxy layout engines should inherit from.
/// </summary>
public abstract class BaseProxyLayoutEngine : ILayoutEngine
{
	/// <summary>
	/// The proxied layout engine.
	/// </summary>
	protected ILayoutEngine InnerLayoutEngine { get; }

	/// <summary>
	/// Constructs a new proxy layout engine.
	/// </summary>
	/// <param name="innerLayoutEngine">The proxied layout engine.</param>
	protected BaseProxyLayoutEngine(ILayoutEngine innerLayoutEngine)
	{
		InnerLayoutEngine = innerLayoutEngine;
	}

	/// <summary>
	/// The name is only really important for the user, so we can use the name of the proxied layout engine.
	/// </summary>
	/// <inheritdoc/>
	public virtual string Name => InnerLayoutEngine.Name;

	/// <inheritdoc/>
	public virtual int Count => InnerLayoutEngine.Count;

	/// <inheritdoc/>
	public virtual bool IsReadOnly => InnerLayoutEngine.IsReadOnly;

	/// <inheritdoc/>
	public virtual void Add(IWindow window) => InnerLayoutEngine.Add(window);

	/// <inheritdoc/>
	public virtual bool Remove(IWindow window) => InnerLayoutEngine.Remove(window);

	/// <inheritdoc/>
	public virtual IWindow? GetFirstWindow() => InnerLayoutEngine.GetFirstWindow();

	/// <inheritdoc/>
	public virtual void FocusWindowInDirection(IWindow window, Direction direction) =>
		InnerLayoutEngine.FocusWindowInDirection(window, direction);

	/// <inheritdoc/>
	public virtual void SwapWindowInDirection(IWindow window, Direction direction) =>
		InnerLayoutEngine.SwapWindowInDirection(window, direction);

	/// <inheritdoc/>
	public virtual void Clear() => InnerLayoutEngine.Clear();

	/// <inheritdoc/>
	public virtual bool Contains(IWindow window) => InnerLayoutEngine.Contains(window);

	/// <inheritdoc/>
	public virtual void CopyTo(IWindow[] array, int arrayIndex) => InnerLayoutEngine.CopyTo(array, arrayIndex);

	/// <inheritdoc/>
	public virtual IEnumerator<IWindow> GetEnumerator() => InnerLayoutEngine.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <inheritdoc/>
	public virtual void MoveWindowEdgeInDirection(IWindow window, Direction edge, double fractionDelta) =>
		InnerLayoutEngine.MoveWindowEdgeInDirection(window, edge, fractionDelta);

	/// <inheritdoc/>
	public abstract IEnumerable<IWindowState> DoLayout(ILocation<int> location, IMonitor monitor);

	/// <inheritdoc/>
	public virtual void HidePhantomWindows() => InnerLayoutEngine.HidePhantomWindows();

	/// <inheritdoc/>
	public virtual void AddWindowAtPoint(IWindow window, IPoint<double> point) =>
		InnerLayoutEngine.AddWindowAtPoint(window, point);

	/// <summary>
	/// Checks to see if this <cref name="ILayoutEngine"/>
	/// or a child layout engine is type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">The type of layout engine to check for.</typeparam>
	/// <returns>
	/// The layout engine with type <typeparamref name="T"/>, or null if none is found.
	/// </returns>
	public T? GetLayoutEngine<T>()
		where T : ILayoutEngine
	{
		if (this is T t)
		{
			return t;
		}

		if (InnerLayoutEngine != null)
		{
			return InnerLayoutEngine.GetLayoutEngine<T>();
		}

		return default;
	}

	/// <summary>
	/// Checks to see if this <cref name="ILayoutEngine"/>
	/// or a child layout engine is equal to <paramref name="layoutEngine"/>.
	/// </summary>
	/// <param name="layoutEngine">The layout engine to check for.</param>
	/// <returns>
	/// <cref name="true"/> if the layout engine is found, <cref name="false"/> otherwise.
	/// </returns>
	public bool ContainsEqual(ILayoutEngine layoutEngine)
	{
		if (this == layoutEngine)
		{
			return true;
		}

		if (InnerLayoutEngine != null)
		{
			return InnerLayoutEngine.ContainsEqual(layoutEngine);
		}

		return false;
	}
}
