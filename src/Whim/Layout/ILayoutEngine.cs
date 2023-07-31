using System.Collections.Generic;

namespace Whim;

/// <summary>
/// Creates a non-proxy <see cref="ILayoutEngine"/> with the given <paramref name="identity"/>.
/// </summary>
/// <param name="identity"></param>
/// <returns></returns>
public delegate ILayoutEngine CreateLeafLayoutEngine(LayoutEngineIdentity identity);

/// <summary>
/// Layout engines dictate how windows are laid out.
/// </summary>
public interface ILayoutEngine
{
	/// <summary>
	/// The name of the layout engine.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// The number of windows in the layout engine.
	/// </summary>
	int Count { get; }

	/// <summary>
	/// The identity of the layout engine.
	/// </summary>
	LayoutEngineIdentity Identity { get; }

	/// <summary>
	/// Adds a <paramref name="window"/> to the layout engine.
	/// </summary>
	/// <param name="window"></param>
	/// <returns>The new <see cref="ILayoutEngine"/> after the add.</returns>
	ILayoutEngine AddWindow(IWindow window);

	/// <summary>
	/// Move the <paramref name="window"/> to the <paramref name="point"/>.
	/// The point has a coordinate space of [0, 1] for both x and y.
	/// </summary>
	/// <param name="window">The window to move.</param>
	/// <param name="point">The point to move the window to.</param>
	/// <returns>The new <see cref="ILayoutEngine"/> after the move.</returns>
	ILayoutEngine MoveWindowToPoint(IWindow window, IPoint<double> point);

	/// <summary>
	/// Removes a <paramref name="window"/> from the layout engine.
	/// </summary>
	/// <param name="window"></param>
	/// <returns>The new <see cref="ILayoutEngine"/> after the remove.</returns>
	ILayoutEngine RemoveWindow(IWindow window);

	/// <summary>
	/// Determines whether the layout engine contains the <paramref name="window"/>.
	/// </summary>
	/// <param name="window"></param>
	/// <returns>
	/// True if the layout engine contains the <paramref name="window"/>, otherwise false.
	/// </returns>
	bool ContainsWindow(IWindow window);

	/// <summary>
	/// Performs a layout inside the available <paramref name="location"/>.
	/// </summary>
	/// <remarks>
	/// For a given <paramref name="location"/>, the layout engine should return the same
	/// result every time.
	/// </remarks>
	/// <param name="location">The available area to do a layout inside.</param>
	/// <param name="monitor">The monitor which the layout is being done for.</param>
	/// <returns>The layout result.</returns>
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
	/// <returns>The new <see cref="ILayoutEngine"/> after the swap.</returns>
	ILayoutEngine SwapWindowInDirection(Direction direction, IWindow window);

	/// <summary>
	/// Moves the focused window's edges by the specified <paramref name="deltas"/>.
	/// </summary>
	/// <param name="edges">The edges to change.</param>
	/// <param name="deltas">
	/// The deltas to change the given <paramref name="edges"/> by.
	/// The <paramref name="deltas"/> are in the range [0, 1] for both x and y (the unit square).
	/// </param>
	/// <param name="window"></param>
	/// <returns>The new <see cref="ILayoutEngine"/> after the move.</returns>
	ILayoutEngine MoveWindowEdgesInDirection(Direction edges, IPoint<double> deltas, IWindow window);

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
