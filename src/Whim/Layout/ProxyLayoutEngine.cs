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

	public void FocusWindowInDirection(Direction direction, IWindow window) => _innerLayoutEngine.FocusWindowInDirection(direction, window);

	public void SwapWindowInDirection(Direction direction, IWindow window) => _innerLayoutEngine.SwapWindowInDirection(direction, window);

	public void Clear() => _innerLayoutEngine.Clear();

	public bool Contains(IWindow window) => _innerLayoutEngine.Contains(window);

	public void CopyTo(IWindow[] array, int arrayIndex) => _innerLayoutEngine.CopyTo(array, arrayIndex);

	public IEnumerator<IWindow> GetEnumerator() => _innerLayoutEngine.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public void MoveWindowEdgeInDirection(Direction edge, double delta, IWindow window) => _innerLayoutEngine.MoveWindowEdgeInDirection(edge, delta, window);

	public abstract IEnumerable<IWindowLocation> DoLayout(ILocation<int> location);

	public void HidePhantomWindows() => _innerLayoutEngine.HidePhantomWindows();

	public void MoveWindowToPoint(IWindow window, IPoint<double> point, bool isPhantom) => _innerLayoutEngine.MoveWindowToPoint(window, point, isPhantom);

	/// <summary>
	/// Checks to see if the <paramref name="root"/> <cref name="ILayoutEngine"/>
	/// or a child layout engine is type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">The type of layout engine to check for.</typeparam>
	/// <param name="root">
	/// The root layout engine to check. If this is a proxy layout engine, it'll check its child
	/// proxy layout engines.
	/// </param>
	/// <returns>
	// The layout engine with type <typeparamref name="T"/>, or null if none is found.
	/// </returns>
	public static T? GetLayoutEngine<T>(ILayoutEngine root) where T : ILayoutEngine
	{
		if (root is T t)
		{
			return t;
		}

		if (root is BaseProxyLayoutEngine proxy && proxy._innerLayoutEngine != null)
		{
			return BaseProxyLayoutEngine.GetLayoutEngine<T>(proxy._innerLayoutEngine);
		}

		return default;
	}

	/// <summary>
	/// Checks to see if the <paramref name="root"/> <cref name="ILayoutEngine"/>
	/// or a child layout engine is equal to <paramref name="layoutEngine"/>.
	/// </summary>
	/// <param name="root">
	/// The root layout engine to check. If this is a proxy layout engine, it'll check its child
	/// proxy layout engines.
	/// </param>
	/// <param name="layoutEngine">The layout engine to check for.</param>
	/// <returns>
	/// <cref name="true"/> if the layout engine is found, <cref name="false"/> otherwise.
	/// </returns>
	public static bool ContainsEqual(ILayoutEngine root, ILayoutEngine layoutEngine)
	{
		if (root == layoutEngine)
		{
			return true;
		}

		if (root is BaseProxyLayoutEngine proxy && proxy._innerLayoutEngine != null)
		{
			return BaseProxyLayoutEngine.ContainsEqual(proxy._innerLayoutEngine, layoutEngine);
		}

		return false;
	}
}
