using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Whim.SliceLayout;

internal record SliceRectangleItem(int Index, Rectangle<int> Rectangle);

public record SliceLayoutEngine : ILayoutEngine
{
	private readonly IContext _context;
	private readonly ImmutableList<IWindow> _windows;
	private readonly ParentArea _rootArea;
	private readonly ISliceLayoutPlugin _plugin;

	public string Name { get; init; } = "Leader Stack";

	public int Count => _windows.Count;

	public LayoutEngineIdentity Identity { get; }

	private SliceLayoutEngine(
		IContext context,
		ISliceLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		ImmutableList<IWindow> windows,
		ParentArea rootArea
	)
	{
		_context = context;
		_plugin = plugin;
		Identity = identity;
		_windows = windows;
		_rootArea = rootArea;
	}

	public SliceLayoutEngine(
		IContext context,
		ISliceLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		ParentArea rootArea
	)
		: this(context, plugin, identity, ImmutableList<IWindow>.Empty, rootArea) { }

	public ILayoutEngine AddWindow(IWindow window)
	{
		Logger.Debug($"Adding {window}");
		return new SliceLayoutEngine(_context, _plugin, Identity, _windows.Add(window), _rootArea);
	}

	public ILayoutEngine RemoveWindow(IWindow window)
	{
		Logger.Debug($"Removing {window}");
		return new SliceLayoutEngine(_context, _plugin, Identity, _windows.Remove(window), _rootArea);
	}

	public bool ContainsWindow(IWindow window)
	{
		Logger.Debug($"Checking if {window} is contained");
		return _windows.Contains(window);
	}

	// TODO: Handle when areas are partially or completely empty.
	public IEnumerable<IWindowState> DoLayout(IRectangle<int> rectangle, IMonitor monitor)
	{
		Logger.Debug($"Doing layout on {rectangle} on {monitor}");

		if (_windows.Count == 0)
		{
			return Enumerable.Empty<IWindowState>();
		}

		// Prune the empty areas from the tree
		ParentArea prunedRootArea = _rootArea.Prune(_windows.Count);

		// Get the rectangles for each window
		SliceRectangleItem[] items = new SliceRectangleItem[_windows.Count];
		items.DoParentLayout(0, rectangle, prunedRootArea);

		// Get the window states
		IWindowState[] windowStates = new IWindowState[_windows.Count];
		for (int idx = 0; idx < _windows.Count; idx++)
		{
			windowStates[idx] = new WindowState()
			{
				Rectangle = items[idx].Rectangle,
				Window = _windows[idx],
				WindowSize = WindowSize.Normal
			};
		}

		return windowStates;
	}

	public void FocusWindowInDirection(Direction direction, IWindow window)
	{
		// TODO
		throw new System.NotImplementedException();
	}

	public IWindow? GetFirstWindow()
	{
		Logger.Debug($"Getting first window");
		return _windows.Count > 0 ? _windows[0] : null;
	}

	public ILayoutEngine MoveWindowEdgesInDirection(Direction edges, IPoint<double> deltas, IWindow window)
	{
		// TODO
		throw new System.NotImplementedException();
	}

	public ILayoutEngine MoveWindowToPoint(IWindow window, IPoint<double> point)
	{
		Logger.Debug($"Moving {window} to {point}");

		if (GetWindowAtPoint(point) is not (int, IWindow) windowAtPoint)
		{
			return this;
		}

		return MoveWindowToIndex(_windows.IndexOf(window), windowAtPoint.Index);
	}

	public ILayoutEngine SwapWindowInDirection(Direction direction, IWindow window)
	{
		// TODO
		throw new System.NotImplementedException();
	}

	private ILayoutEngine MoveWindowToIndex(int currentIndex, int targetIndex)
	{
		if (_plugin.WindowInsertionType == WindowInsertionType.Swap)
		{
			return SwapWindowIndices(currentIndex, targetIndex);
		}

		return RotateWindowIndices(currentIndex, targetIndex);
	}

	private ILayoutEngine SwapWindowIndices(int currentIndex, int targetIndex)
	{
		Logger.Debug($"Swapping {currentIndex} and {targetIndex}");

		if (currentIndex == targetIndex)
		{
			return this;
		}

		IWindow currentWindow = _windows[currentIndex];
		IWindow targetWindow = _windows[targetIndex];

		ImmutableList<IWindow> newWindows = _windows
			.SetItem(currentIndex, targetWindow)
			.SetItem(targetIndex, currentWindow);

		return new SliceLayoutEngine(_context, _plugin, Identity, newWindows, _rootArea);
	}

	private ILayoutEngine RotateWindowIndices(int currentIndex, int targetIndex)
	{
		Logger.Debug($"Rotating {currentIndex} and {targetIndex}");

		if (currentIndex == targetIndex)
		{
			return this;
		}

		IWindow currentWindow = _windows[currentIndex];
		ImmutableList<IWindow> newWindows = _windows.Insert(targetIndex, currentWindow).RemoveAt(currentIndex);
		return new SliceLayoutEngine(_context, _plugin, Identity, newWindows, _rootArea);
	}

	private (int Index, IWindow Window)? GetWindowAtPoint(IPoint<double> point)
	{
		Logger.Debug($"Getting window at {point}");

		// This is the easy way - we DoLayout with fake coordinates, then linearly search for the
		// window at the point, then swap or rotate.

		Point<int> scaledPoint = new((int)(point.X * 10000), (int)(point.Y * 10000));
		Rectangle<int> rectangle = new(0, 0, 10000, 10000);

		int idx = 0;
		foreach (IWindowState windowState in DoLayout(rectangle, _context.MonitorManager.PrimaryMonitor))
		{
			if (windowState.Rectangle.ContainsPoint(scaledPoint))
			{
				return (idx, windowState.Window);
			}

			idx++;
		}

		return null;
	}
}
