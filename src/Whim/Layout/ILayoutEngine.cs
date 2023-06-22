using System.Collections.Generic;

namespace Whim;

/// <summary>
/// Layout engines dictate how windows are laid out.
/// </summary>
public interface ILayoutEngine : ICollection<IWindow>
{
	/// <summary>
	/// The name of the layout engine.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Performs a layout inside the available <paramref name="location"/>.
	/// </summary>
	/// <param name="location">The available area to do a layout inside.</param>
	/// <param name="monitor">The monitor which the layout is being done for.</param>
	/// <returns></returns>
	IEnumerable<IWindowState> DoLayout(ILocation<int> location, IMonitor monitor);

	/// <summary>
	/// Retrieves the first window in the layout engine.
	/// </summary>
	IWindow? GetFirstWindow();

	/// <summary>
	/// Focuses the <paramref name="window"/> in the <paramref name="direction"/>.
	/// It's recommended that this method is not called directly, but rather
	/// through the <see cref="IWorkspace.FocusWindowInDirection"/> method.
	/// </summary>
	/// <param name="direction">The direction to focus in.</param>
	/// <param name="window">The origin window</param>
	void FocusWindowInDirection(Direction direction, IWindow window);

	/// <summary>
	/// Swaps the <paramref name="window"/> in the <paramref name="direction"/>.
	/// It's recommended that this method is not called directly, but rather
	/// through <see cref="IWorkspace.SwapWindowInDirection"/>.
	/// </summary>
	/// <param name="direction">The direction to swap the window in.</param>
	/// <param name="window">The window to swap.</param>
	void SwapWindowInDirection(Direction direction, IWindow window);

	/// <summary>
	/// Changes the focused window's edge by the specified <paramref name="pixelDelta"/>.
	/// </summary>
	/// <param name="edge">The edge to change.</param>
	/// <param name="pixelDelta">
	/// The number of pixels to change the edge by. When positive, the edge will move in the
	/// <paramref name="edge"/> direction. When negative, the edge will move in the opposite
	/// direction.
	/// </param>
	/// <param name="window">The window to change the edge of.</param>
	void MoveWindowEdgeInDirection(Direction edge, double pixelDelta, IWindow window);

	/// <summary>
	/// Hides all phantom windows belonging to the layout engine.
	/// </summary>
	void HidePhantomWindows();

	/// <summary>
	/// Move the <paramref name="window"/> to the <paramref name="point"/>.
	/// The point has a coordinate space of [0, 1] for both x and y.
	/// </summary>
	/// <param name="window">The window to move.</param>
	/// <param name="point">The point to move the window to.</param>
	void AddWindowAtPoint(IWindow window, IPoint<double> point);

	/// <summary>
	/// Checks to see if this <see cref="ILayoutEngine"/> or a child layout engine is type
	/// <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">The type of layout engine to check for.</typeparam>
	/// <returns>
	/// The layout engine with type <typeparamref name="T"/>, or null if none is found.
	/// </returns>
	T? GetLayoutEngine<T>()
		where T : ILayoutEngine
	{
		if (this is T layoutEngine)
		{
			return layoutEngine;
		}

		if (this is BaseProxyLayoutEngine proxy)
		{
			return proxy.GetLayoutEngine<T>();
		}

		return default;
	}

	/// <summary>
	/// Checks to see if this <see cref="ILayoutEngine"/> or a child layout engine is
	/// <paramref name="layoutEngine"/>.
	/// </summary>
	/// <param name="layoutEngine">The layout engine to check for.</param>
	/// <returns>
	/// <cref name="true"/> if the layout engine is found, <cref name="false"/> otherwise.
	/// </returns>
	bool ContainsEqual(ILayoutEngine layoutEngine)
	{
		if (this == layoutEngine)
		{
			return true;
		}

		if (this is BaseProxyLayoutEngine proxy)
		{
			return proxy.ContainsEqual(layoutEngine);
		}

		return false;
	}
}
