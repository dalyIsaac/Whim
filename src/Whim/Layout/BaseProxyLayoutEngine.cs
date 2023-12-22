using System.Collections.Generic;

namespace Whim;

/// <summary>
/// This delegate takes an in a layout engine and returns it with a wrapper layout engine.
/// The wrapper layout engine provides additional functionality, but still utilises the underlying
/// layout engine.
/// </summary>
public delegate ILayoutEngine CreateProxyLayoutEngine(ILayoutEngine engine);

/// <summary>
/// Abstract layout engine, which proxy layout engines should inherit from.
/// </summary>
/// <remarks>
/// Proxy layout engines are layout engines that wrap another layout engine.
///
/// Proxy layout engines are useful for adding additional functionality to a layout engine.
/// For example, the <c>Whim.Gaps</c> plugin uses a proxy layout engine to add gaps to a layout engine.
///
/// Proxy layout engine tests should extend the <c>Whim.TestUtils.ProxyLayoutEngineBaseTests</c>
/// class, to verify they do not break in common scenarios.
/// </remarks>
public abstract record BaseProxyLayoutEngine : ILayoutEngine
{
	/// <summary>
	/// The proxied layout engine.
	/// </summary>
	protected ILayoutEngine InnerLayoutEngine { get; }

	/// <summary>
	/// Creates a new <cref name="BaseProxyLayoutEngine"/> with the given <paramref name="innerLayoutEngine"/>.
	/// </summary>
	/// <param name="innerLayoutEngine"></param>
	protected BaseProxyLayoutEngine(ILayoutEngine innerLayoutEngine)
	{
		InnerLayoutEngine = innerLayoutEngine;
	}

	/// <inheritdoc/>
	public LayoutEngineIdentity Identity => InnerLayoutEngine.Identity;

	/// <summary>
	/// The name is only really important for the user, so we can use the name of the proxied layout engine.
	/// </summary>
	/// <inheritdoc/>
	public string Name => InnerLayoutEngine.Name;

	/// <inheritdoc/>
	public abstract int Count { get; }

	/// <inheritdoc/>
	public abstract ILayoutEngine AddWindow(IWindow window);

	/// <inheritdoc/>
	public abstract ILayoutEngine MoveWindowToPoint(IWindow window, IPoint<double> point);

	/// <inheritdoc/>
	public abstract ILayoutEngine RemoveWindow(IWindow window);

	/// <inheritdoc/>
	public abstract IWindow? GetFirstWindow();

	/// <inheritdoc/>
	public abstract void FocusWindowInDirection(Direction direction, IWindow window);

	/// <inheritdoc/>
	public abstract ILayoutEngine SwapWindowInDirection(Direction direction, IWindow window);

	/// <inheritdoc/>
	public abstract bool ContainsWindow(IWindow window);

	/// <inheritdoc/>
	public abstract ILayoutEngine MoveWindowEdgesInDirection(Direction edge, IPoint<double> deltas, IWindow window);

	/// <inheritdoc/>
	public abstract IEnumerable<IWindowState> DoLayout(IRectangle<int> rectangle, IMonitor monitor);

	public abstract ILayoutEngine PerformCustomAction<T>(string actionName, T args);

	/// <summary>
	/// Checks to see if this <cref name="IImmutableLayoutEngine"/>
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
	public bool ContainsEqual(ILayoutEngine layoutEngine)
	{
		if (Equals(layoutEngine) || this == (layoutEngine as BaseProxyLayoutEngine))
		{
			return true;
		}

		return InnerLayoutEngine.ContainsEqual(layoutEngine);
	}
}
