using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Whim.SliceLayout;

internal record SliceRectangleItem(int Index, Rectangle<int> Rectangle);

/// <summary>
/// A layout engine that divides the screen into "areas", which correspond to "slices" of a list
/// of windows. This can be used to accomplish a variety of layouts, including master-stack layouts.
/// </summary>
/// <remarks>
/// The layout engine is a tree of <see cref="IArea"/>s. Each <see cref="IArea"/> is either a
/// <see cref="ParentArea"/>, <see cref="SliceArea"/>, or <see cref="OverflowArea"/>.
///
/// Windows are assigned to <see cref="SliceArea"/>s according to the <see cref="SliceArea.Order"/>.
/// </remarks>
public partial record SliceLayoutEngine : ILayoutEngine
{
	private readonly IContext _context;
	private readonly ImmutableList<IWindow> _windows;
	private readonly ParentArea _rootArea;
	private readonly ISliceLayoutPlugin _plugin;

	public string Name { get; init; } = "Leader Stack";

	public int Count => _windows.Count;

	public LayoutEngineIdentity Identity { get; }

	private const int _cachedWindowStatesScale = 10000;

	/// <summary>
	/// The areas in the tree which contain windows, in order of the <see cref="SliceArea.Order"/>.
	/// The last area is an <see cref="OverflowArea"/>.
	/// </summary>
	private readonly BaseSliceArea[] _windowAreas;

	/// <summary>
	/// Cheekily cache the window states with fake coordinates, to facilitate linear searching.
	///
	/// NOTE: Do not access this directly - instead use <see cref="GetLazyWindowStates"/>
	/// </summary>
	private IWindowState[]? _cachedWindowStates;

	/// <summary>
	/// The root area, with any empty areas pruned.
	/// </summary>
	private readonly ParentArea _prunedRootArea;

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

		(_rootArea, _windowAreas) = rootArea.SetStartIndexes();
		_prunedRootArea = _rootArea.Prune(_windows.Count);
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

	public IEnumerable<IWindowState> DoLayout(IRectangle<int> rectangle, IMonitor monitor)
	{
		Logger.Debug($"Doing layout on {rectangle} on {monitor}");

		if (_windows.Count == 0)
		{
			return Enumerable.Empty<IWindowState>();
		}

		// Get the rectangles for each window
		SliceRectangleItem[] items = new SliceRectangleItem[_windows.Count];
		items.DoParentLayout(0, rectangle, _prunedRootArea);

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
		Logger.Debug($"Focusing window in direction {direction} from {window}");
		GetWindowInDirection(direction, window)?.Focus();
	}

	public IWindow? GetFirstWindow()
	{
		Logger.Debug($"Getting first window");
		return _windows.Count > 0 ? _windows[0] : null;
	}

	public ILayoutEngine MoveWindowEdgesInDirection(Direction edges, IPoint<double> deltas, IWindow window)
	{
		// TODO: Put issue reference here
		return this;
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
		Logger.Debug($"Swapping {window} in direction {direction}");

		IWindow? windowInDirection = GetWindowInDirection(direction, window);
		if (windowInDirection == null)
		{
			return this;
		}

		return MoveWindowToIndex(_windows.IndexOf(window), _windows.IndexOf(windowInDirection));
	}

	private ILayoutEngine PromoteWindowInStack(IWindow window)
	{
		Logger.Debug($"Promoting {window} in stack");

		int windowIndex = _windows.IndexOf(window);
		if (windowIndex == 0)
		{
			return this;
		}

		// Find the area which contains the window.
		int areaIndex = GetAreaStackForWindowIndex(windowIndex);
		if (areaIndex <= 0)
		{
			return this;
		}

		SliceArea targetArea = (SliceArea)_windowAreas[areaIndex - 1];
		int targetIndex = (int)(targetArea.StartIndex + targetArea.MaxChildren - 1);

		return MoveWindowToIndex(windowIndex, targetIndex);
	}

	private ILayoutEngine DemoteWindowInStack(IWindow window)
	{
		Logger.Debug($"Demoting {window} in stack");

		int windowIndex = _windows.IndexOf(window);
		if (windowIndex == _windows.Count - 1)
		{
			return this;
		}

		// Find the area which contains the window.
		int areaIndex = GetAreaStackForWindowIndex(windowIndex);
		if (areaIndex > _windowAreas.Length - 2 || areaIndex == -1)
		{
			return this;
		}

		BaseSliceArea targetArea = _windowAreas[areaIndex + 1];
		int targetIndex = (int)targetArea.StartIndex;

		return MoveWindowToIndex(windowIndex, targetIndex);
	}

	private int GetAreaStackForWindowIndex(int windowIndex)
	{
		for (int idx = 0; idx < _windowAreas.Length; idx++)
		{
			IArea area = _windowAreas[idx];
			if (area is SliceArea sliceArea)
			{
				int areaEndIndex = (int)(sliceArea.StartIndex + sliceArea.MaxChildren);
				if (windowIndex >= sliceArea.StartIndex && windowIndex < areaEndIndex)
				{
					return idx;
				}
			}

			if (area is OverflowArea)
			{
				return idx;
			}
		}

		return -1;
	}

	public ILayoutEngine PerformCustomAction<T>(LayoutEngineCustomAction<T> action) =>
		action switch
		{
			LayoutEngineCustomAction<IWindow> promoteAction when promoteAction.Name == _plugin.PromoteActionName
				=> PromoteWindowInStack(promoteAction.Payload),
			LayoutEngineCustomAction<IWindow> demoteAction when demoteAction.Name == _plugin.DemoteActionName
				=> DemoteWindowInStack(demoteAction.Payload),
			_ => this
		};
}
