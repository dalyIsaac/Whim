using System.Collections.Generic;

namespace Whim;

/// <summary>
/// Layout engines dictate how windows are laid out.
/// </summary>
public interface ILayoutEngine : ICollection<IWindow>, ICommandable
{
	/// <summary>
	/// The name of the layout engine.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Performs a layout inside the available <see cref="ILocation"/> region.
	/// </summary>
	/// <param name="location">The available area to do a layout inside.</param>
	/// <returns></returns>
	public IEnumerable<IWindowLocation> DoLayout(ILocation<int> location);

	/// <summary>
	/// Retrieves the first window in the layout engine.
	/// </summary>
	public IWindow? GetFirstWindow();

	/// <summary>
	/// Focuses the <paramref name="window"/> in the <paramref name="direction"/>.
	/// It's recommended that this method is not called directly, but rather
	/// through the <see cref="IWorkspace.FocusWindowInDirection"/> method.
	/// </summary>
	/// <param name="direction">The direction to focus in.</param>
	/// <param name="window">The origin window</param>
	public void FocusWindowInDirection(Direction direction, IWindow window);

	/// <summary>
	/// Swaps the <paramref name="window"/> in the <paramref name="direction"/>.
	/// It's recommended that this method is not called directly, but rather
	/// through <see cref="IWorkspace.SwapWindowInDirection"/>.
	/// </summary>
	/// <param name="direction">The direction to swap the window in.</param>
	/// <param name="window">The window to swap.</param>
	public void SwapWindowInDirection(Direction direction, IWindow window);

	/// <summary>
	/// Change the focused window's edge by the specified <paramref name="fractionDelta"/>.
	/// </summary>
	/// <param name="edge">The edge to change.</param>
	/// <param name="fractionDelta">The percentage to change the edge by.</param>
	/// <param name="window">The window to change the edge of.</param>
	public void MoveWindowEdgeInDirection(Direction edge, double fractionDelta, IWindow window);

	/// <summary>
	/// Hides all phantom windows belonging to the layout engine.
	/// </summary>
	public void HidePhantomWindows();

	/// <summary>
	/// Move the <paramref name="window"/> to the <paramref name="point"/>.
	/// The point has a coordinate space of [0, 1] for both x and y.
	/// </summary>
	/// <param name="window">The window to move.</param>
	/// <param name="point">The point to move the window to.</param>
	/// <param name="isPhantom">Whether the window is a phantom window.</param>
	public void AddWindowAtPoint(IWindow window, IPoint<double> point, bool isPhantom = false);

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
		if (root is T layoutEngine)
		{
			return layoutEngine;
		}

		if (root is BaseProxyLayoutEngine proxy)
		{
			return BaseProxyLayoutEngine.GetLayoutEngine<T>(proxy);
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

		if (root is BaseProxyLayoutEngine proxy)
		{
			return BaseProxyLayoutEngine.ContainsEqual(proxy, layoutEngine);
		}

		return false;
	}
}
