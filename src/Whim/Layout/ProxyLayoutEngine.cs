using System.Collections;
using System.Collections.Generic;

namespace Whim;

/// <summary>
/// Abstract layout engine, which proxy layout engines should inherit from.
/// </summary>
public abstract class BaseProxyLayoutEngine : ILayoutEngine
{
	protected ILayoutEngine InnerLayoutEngine { get; }

	protected BaseProxyLayoutEngine(ILayoutEngine innerLayoutEngine)
	{
		InnerLayoutEngine = innerLayoutEngine;
	}

	/// <summary>
	/// The name is only really important for the user, so we can use the name of the proxied layout engine.
	/// </summary>
	public virtual string Name => InnerLayoutEngine.Name;

	public virtual int Count => InnerLayoutEngine.Count;

	public virtual bool IsReadOnly => InnerLayoutEngine.IsReadOnly;

	public virtual void Add(IWindow window) => InnerLayoutEngine.Add(window);

	public virtual bool Remove(IWindow window) => InnerLayoutEngine.Remove(window);

	public virtual IWindow? GetFirstWindow() => InnerLayoutEngine.GetFirstWindow();

	public virtual void FocusWindowInDirection(Direction direction, IWindow window) => InnerLayoutEngine.FocusWindowInDirection(direction, window);

	public virtual void SwapWindowInDirection(Direction direction, IWindow window) => InnerLayoutEngine.SwapWindowInDirection(direction, window);

	public virtual void Clear() => InnerLayoutEngine.Clear();

	public virtual bool Contains(IWindow window) => InnerLayoutEngine.Contains(window);

	public virtual void CopyTo(IWindow[] array, int arrayIndex) => InnerLayoutEngine.CopyTo(array, arrayIndex);

	public virtual IEnumerator<IWindow> GetEnumerator() => InnerLayoutEngine.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public virtual void MoveWindowEdgeInDirection(Direction edge, double delta, IWindow window) => InnerLayoutEngine.MoveWindowEdgeInDirection(edge, delta, window);

	public abstract IEnumerable<IWindowLocation> DoLayout(ILocation<int> location);

	public virtual void HidePhantomWindows() => InnerLayoutEngine.HidePhantomWindows();

	public virtual void AddWindowAtPoint(IWindow window, IPoint<double> point, bool isPhantom) => InnerLayoutEngine.AddWindowAtPoint(window, point, isPhantom);

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

		if (root is BaseProxyLayoutEngine proxy && proxy.InnerLayoutEngine != null)
		{
			return BaseProxyLayoutEngine.GetLayoutEngine<T>(proxy.InnerLayoutEngine);
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

		if (root is BaseProxyLayoutEngine proxy && proxy.InnerLayoutEngine != null)
		{
			return BaseProxyLayoutEngine.ContainsEqual(proxy.InnerLayoutEngine, layoutEngine);
		}

		return false;
	}
}
