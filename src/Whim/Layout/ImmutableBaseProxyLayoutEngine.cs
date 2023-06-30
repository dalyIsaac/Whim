using System.Collections.Generic;

namespace Whim;

/// <summary>
/// This delegate takes an in a layout engine and returns it with a wrapper layout engine.
/// The wrapper layout engine provides additional functionality, but still utilises the underlying
/// layout engine.
/// </summary>
public delegate IImmutableLayoutEngine ImmutableProxyLayoutEngine(IImmutableLayoutEngine engine);

/// <summary>
/// Abstract layout engine, which proxy layout engines should inherit from.
/// </summary>
public abstract class ImmutableBaseProxyLayoutEngine : IImmutableLayoutEngine
{
	/// <summary>
	/// The proxied layout engine.
	/// </summary>
	protected IImmutableLayoutEngine InnerLayoutEngine { get; }

	/// <summary>
	/// Constructs a new proxy layout engine.
	/// </summary>
	/// <param name="innerLayoutEngine">The proxied layout engine.</param>
	protected ImmutableBaseProxyLayoutEngine(IImmutableLayoutEngine innerLayoutEngine)
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
	public virtual IImmutableLayoutEngine Add(IWindow window) => InnerLayoutEngine.Add(window);

	/// <inheritdoc/>
	public virtual IImmutableLayoutEngine AddAtPoint(IWindow window, IPoint<double> point) =>
		InnerLayoutEngine.AddAtPoint(window, point);

	/// <inheritdoc/>
	public virtual IImmutableLayoutEngine Remove(IWindow window) => InnerLayoutEngine.Remove(window);

	/// <inheritdoc/>
	public virtual IWindow? GetFirstWindow() => InnerLayoutEngine.GetFirstWindow();

	/// <inheritdoc/>
	public virtual void FocusWindowInDirection(Direction direction, IWindow window) =>
		InnerLayoutEngine.FocusWindowInDirection(direction, window);

	/// <inheritdoc/>
	public virtual IImmutableLayoutEngine SwapWindowInDirection(Direction direction, IWindow window) =>
		InnerLayoutEngine.SwapWindowInDirection(direction, window);

	/// <inheritdoc/>
	public virtual bool Contains(IWindow window) => InnerLayoutEngine.Contains(window);

	/// <inheritdoc/>
	public virtual IImmutableLayoutEngine MoveWindowEdgesInDirection(
		Direction edge,
		IPoint<double> deltas,
		IWindow window
	) => InnerLayoutEngine.MoveWindowEdgesInDirection(edge, deltas, window);

	/// <inheritdoc/>
	public abstract IEnumerable<IWindowState> DoLayout(ILocation<int> location, IMonitor monitor);

	/// <inheritdoc/>
	public virtual IImmutableLayoutEngine HidePhantomWindows() => InnerLayoutEngine.HidePhantomWindows();

	/// <summary>
	/// Checks to see if this <cref name="IImmutableLayoutEngine"/>
	/// or a child layout engine is type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">The type of layout engine to check for.</typeparam>
	/// <returns>
	/// The layout engine with type <typeparamref name="T"/>, or null if none is found.
	/// </returns>
	public T? GetLayoutEngine<T>()
		where T : IImmutableLayoutEngine
	{
		if (this is T t)
		{
			return t;
		}

		return InnerLayoutEngine.GetLayoutEngine<T>();
	}

	/// <summary>
	/// Checks to see if this <cref name="IImmutableLayoutEngine"/>
	/// or a child layout engine is equal to <paramref name="layoutEngine"/>.
	/// </summary>
	/// <param name="layoutEngine">The layout engine to check for.</param>
	/// <returns>
	/// <cref name="true"/> if the layout engine is found, <cref name="false"/> otherwise.
	/// </returns>
	public bool ContainsEqual(IImmutableLayoutEngine layoutEngine)
	{
		if (this == layoutEngine)
		{
			return true;
		}

		return InnerLayoutEngine.ContainsEqual(layoutEngine);
	}
}
