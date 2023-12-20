using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Whim.SliceLayout;

public record SliceLayoutEngine : ILayoutEngine
{
	private readonly ImmutableList<IWindow> _windows;
	private readonly IArea _rootArea;

	public string Name { get; init; } = "Leader Stack";

	public int Count => _windows.Count;

	public LayoutEngineIdentity Identity { get; }

	private SliceLayoutEngine(LayoutEngineIdentity identity, ImmutableList<IWindow> windows, IArea rootArea)
	{
		Identity = identity;
		_windows = windows;
		_rootArea = rootArea;
	}

	public ILayoutEngine AddWindow(IWindow window)
	{
		Logger.Debug($"Adding {window}");
		return new SliceLayoutEngine(Identity, _windows.Add(window), _rootArea);
	}

	public ILayoutEngine RemoveWindow(IWindow window)
	{
		Logger.Debug($"Removing {window}");
		return new SliceLayoutEngine(Identity, _windows.Remove(window), _rootArea);
	}

	public bool ContainsWindow(IWindow window)
	{
		Logger.Debug($"Checking if {window} is contained");
		return _windows.Contains(window);
	}

	public IEnumerable<IWindowState> DoLayout(IRectangle<int> rectangle, IMonitor monitor)
	{
		Logger.Debug($"Doing layout on {rectangle} on {monitor}");

		if (_rootArea is SliceArea sliceArea)
		{
			return DoLayoutSlice(rectangle, sliceArea, 0);
		}
		else if (_rootArea is BaseArea baseArea)
		{
			return DoLayoutBase(rectangle, baseArea, 0);
		}
		else
		{
			Logger.Error($"Unknown area type: {_rootArea.GetType()}");
			return Array.Empty<IWindowState>();
		}
	}

	private IEnumerable<IWindowState> DoLayoutSlice(IRectangle<int> rectangle, SliceArea area, int startIdx)
	{
		int x = rectangle.X;
		int y = rectangle.Y;
		int width = rectangle.Width;
		int height = rectangle.Height;

		for (int idx = 0; idx < area.Children.Count; idx++)
		{
			IArea childArea = area.Children[idx];

			if (area.IsHorizontal)
			{
				width = Convert.ToInt32(rectangle.Width * area.Weights[idx]);
			}
			else
			{
				height = Convert.ToInt32(rectangle.Height * area.Weights[idx]);
			}

			IRectangle<int> childRectangle = new Rectangle<int>(x, y, width, height);

			if (childArea is BaseArea baseArea)
			{
				foreach (IWindowState windowState in DoLayoutBase(childRectangle, baseArea, startIdx))
				{
					yield return windowState;
				}
			}
			else if (childArea is SliceArea sliceArea)
			{
				foreach (IWindowState windowState in DoLayoutSlice(childRectangle, sliceArea, startIdx))
				{
					yield return windowState;
				}
			}

			if (area.IsHorizontal)
			{
				x += width;
			}
			else
			{
				y += height;
			}
		}
	}

	private IEnumerable<IWindowState> DoLayoutBase(IRectangle<int> rectangle, BaseArea area, int startIdx)
	{
		int x = rectangle.X;
		int y = rectangle.Y;
		int width = rectangle.Width;
		int height = rectangle.Height;

		int deltaX = 0;
		int deltaY = 0;

		int baseItemsCount = _windows.Count - startIdx;

		if (area.IsHorizontal)
		{
			deltaX = rectangle.Width / baseItemsCount;
			width = deltaX;
		}
		else
		{
			deltaY = rectangle.Height / baseItemsCount;
			height = deltaY;
		}

		for (int i = startIdx; i < _windows.Count; i++)
		{
			IWindow window = _windows[i];

			yield return new WindowState()
			{
				Window = window,
				Rectangle = new Rectangle<int>(x, y, width, height),
				WindowSize = WindowSize.Normal
			};

			if (area.IsHorizontal)
			{
				x += deltaX;
			}
			else
			{
				y += deltaY;
			}
		}
	}

	public void FocusWindowInDirection(Direction direction, IWindow window) =>
		throw new System.NotImplementedException();

	public IWindow? GetFirstWindow()
	{
		Logger.Debug($"Getting first window");
		return _windows.Count > 0 ? _windows[0] : null;
	}

	public ILayoutEngine MoveWindowEdgesInDirection(Direction edges, IPoint<double> deltas, IWindow window) =>
		throw new System.NotImplementedException();

	public ILayoutEngine MoveWindowToPoint(IWindow window, IPoint<double> point) =>
		throw new System.NotImplementedException();

	public ILayoutEngine SwapWindowInDirection(Direction direction, IWindow window) =>
		throw new System.NotImplementedException();
}
