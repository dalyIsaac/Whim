using System.Collections.Immutable;

namespace Whim.SliceLayout;

public partial record SliceLayoutEngine
{
	private IWindowState[] GetLazyWindowStates()
	{
		if (_cachedWindowStates is not null)
		{
			return _cachedWindowStates;
		}

		IWindowState[] cachedWindowStates = new IWindowState[_windows.Count + _minimizedWindows.Count];

		Rectangle<int> rectangle = new(0, 0, _cachedWindowStatesScale, _cachedWindowStatesScale);
		int idx = 0;
		foreach (IWindowState windowState in DoLayout(rectangle, _context.MonitorManager.PrimaryMonitor))
		{
			cachedWindowStates[idx] = windowState;
			idx++;
		}

		_cachedWindowStates = cachedWindowStates;
		return cachedWindowStates;
	}

	private IWindow? GetWindowInDirection(Direction direction, IWindow window)
	{
		int index = _windows.IndexOf(window);
		if (index == -1)
		{
			return null;
		}

		// Figure out the adjacent point of the window
		IWindowState[] windowStates = GetLazyWindowStates();
		IRectangle<int> rect = windowStates[index].Rectangle;
		int x = rect.X;
		int y = rect.Y;

		int delta = 1;
		if (direction.HasFlag(Direction.Left))
		{
			x -= delta;
		}
		else if (direction.HasFlag(Direction.Right))
		{
			x += rect.Width + delta;
		}

		if (direction.HasFlag(Direction.Up))
		{
			y -= delta;
		}
		else if (direction.HasFlag(Direction.Down))
		{
			y += rect.Height + delta;
		}

		// Get the window at that point
		Point<int> point = new(x, y);
		foreach (IWindowState windowState in windowStates)
		{
			if (windowState.Rectangle.ContainsPoint(point))
			{
				return windowState.Window;
			}
		}

		Logger.Debug($"No window found at {x}, {y}");
		return null;
	}

	private SliceLayoutEngine MoveWindowToIndex(int currentIndex, int targetIndex)
	{
		if (_plugin.WindowInsertionType == WindowInsertionType.Swap)
		{
			return SwapWindowIndices(currentIndex, targetIndex);
		}

		return RotateWindowIndices(currentIndex, targetIndex);
	}

	private SliceLayoutEngine SwapWindowIndices(int currentIndex, int targetIndex)
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

		return new SliceLayoutEngine(this, newWindows, _minimizedWindows);
	}

	private SliceLayoutEngine RotateWindowIndices(int currentIndex, int targetIndex)
	{
		Logger.Debug($"Rotating {currentIndex} and {targetIndex}");

		if (currentIndex == targetIndex)
		{
			return this;
		}

		IWindow currentWindow = _windows[currentIndex];
		ImmutableList<IWindow> newWindows = _windows.RemoveAt(currentIndex).Insert(targetIndex, currentWindow);
		return new SliceLayoutEngine(this, newWindows, _minimizedWindows);
	}

	/// <summary>
	/// Does a linear search through the window states to find the window at the given point.
	/// If the point is not in any window, then <see langword="null"/> is returned.
	///
	/// We do a linear search here, because we need to get the window index, and the position inside
	/// the tree doesn't correspond to the window index.
	/// </summary>
	/// <param name="point"></param>
	/// <returns></returns>
	private (int Index, IWindow Window)? GetWindowAtPoint(IPoint<double> point)
	{
		Logger.Debug($"Getting window at {point}");

		Point<int> scaledPoint =
			new((int)(point.X * _cachedWindowStatesScale), (int)(point.Y * _cachedWindowStatesScale));
		IWindowState[] windowStates = GetLazyWindowStates();

		for (int idx = 0; idx < windowStates.Length; idx++)
		{
			if (windowStates[idx].Rectangle.ContainsPoint(scaledPoint))
			{
				return (idx, windowStates[idx].Window);
			}
		}

		return null;
	}

	private SliceLayoutEngine PromoteWindowInStack(IWindow window, bool promote)
	{
		Logger.Debug($"Promoting {window} in stack");
		if (GetPromoteTargetIndex(window, promote) is not (int windowIndex, int targetIndex))
		{
			return this;
		}

		return MoveWindowToIndex(windowIndex, targetIndex);
	}

	private SliceLayoutEngine PromoteFocusInStack(IWindow window, bool promote)
	{
		Logger.Debug($"Promoting focus in stack");
		if (GetPromoteTargetIndex(window, promote) is not (int, int targetIndex))
		{
			return this;
		}

		_windows[targetIndex].Focus();
		return this;
	}

	private (int WindowIndex, int TargetIndex)? GetPromoteTargetIndex(IWindow window, bool promote)
	{
		int windowIndex = _windows.IndexOf(window);
		if (windowIndex == -1)
		{
			Logger.Error($"Could not find {window} in stack");
			return null;
		}

		// Find the area which contains the window.
		int areaIndex = GetAreaStackForWindowIndex(windowIndex);

		if (promote)
		{
			if (areaIndex <= 0)
			{
				return (windowIndex, 0);
			}

			SliceArea targetArea = (SliceArea)_windowAreas[areaIndex - 1];
			return (windowIndex, targetArea.StartIndex + targetArea.MaxChildren - 1);
		}
		else
		{
			if (areaIndex >= _windowAreas.Length - 1)
			{
				return (windowIndex, _windows.Count - 1);
			}

			BaseSliceArea targetArea = _windowAreas[areaIndex + 1];
			return (windowIndex, targetArea.StartIndex);
		}
	}

	private int GetAreaStackForWindowIndex(int windowIndex)
	{
		for (int idx = 0; idx < _windowAreas.Length; idx++)
		{
			IArea area = _windowAreas[idx];
			if (area is SliceArea sliceArea)
			{
				int areaEndIndex = sliceArea.StartIndex + sliceArea.MaxChildren;
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
}
