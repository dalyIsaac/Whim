using System.Collections.Immutable;

namespace Whim.SliceLayout;

public partial record SliceLayoutEngine
{
	private IWindow? GetWindowInDirection(Direction direction, IWindow window)
	{
		int index = _windows.IndexOf(window);
		if (index == -1)
		{
			return null;
		}

		// Figure out the adjacent point of the window
		IRectangle<int> rect = _cachedWindowStates[index].Rectangle;
		double x = rect.X;
		double y = rect.Y;

		if (direction.HasFlag(Direction.Left))
		{
			x -= 1d / _cachedWindowStatesScale;
		}
		else if (direction.HasFlag(Direction.Right))
		{
			x += rect.Width + (1d / _cachedWindowStatesScale);
		}

		if (direction.HasFlag(Direction.Up))
		{
			y -= 1d / _cachedWindowStatesScale;
		}
		else if (direction.HasFlag(Direction.Down))
		{
			y += rect.Height + (1d / _cachedWindowStatesScale);
		}

		// Get the window at that point
		foreach (IWindowState windowState in _cachedWindowStates)
		{
			if (
				windowState.Rectangle.ContainsPoint(
					new Point<int>((int)(x * _cachedWindowStatesScale), (int)(y * _cachedWindowStatesScale))
				)
			)
			{
				return windowState.Window;
			}
		}

		Logger.Debug($"No window found at {x}, {y}");
		return null;
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

		Point<int> scaledPoint =
			new((int)(point.X * _cachedWindowStatesScale), (int)(point.Y * _cachedWindowStatesScale));

		for (int idx = 0; idx < _cachedWindowStates.Length; idx++)
		{
			if (_cachedWindowStates[idx].Rectangle.ContainsPoint(scaledPoint))
			{
				return (idx, _cachedWindowStates[idx].Window);
			}
		}

		return null;
	}
}
