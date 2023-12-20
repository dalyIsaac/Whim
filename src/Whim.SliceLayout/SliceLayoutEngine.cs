using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Whim.SliceLayout;

internal record SliceRectangleItem(int Index, Rectangle<int> Rectangle);

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

	public SliceLayoutEngine(LayoutEngineIdentity identity, IArea rootArea)
		: this(identity, ImmutableList<IWindow>.Empty, rootArea) { }

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

		if (_windows.Count == 0)
		{
			return Enumerable.Empty<IWindowState>();
		}

		// Construct an ordered list of the rectangles to be laid out
		SliceRectangleItem[] items;
		if (_rootArea is ParentArea parentArea)
		{
			items = DoLayoutParent(rectangle, parentArea, 0).ToArray();
		}
		else if (_rootArea is SliceArea sliceArea)
		{
			items = DoLayoutSlice(rectangle, sliceArea, 0).ToArray();
		}
		else if (_rootArea is BaseArea baseArea)
		{
			items = DoLayoutBase(rectangle, baseArea, 0).ToArray();
		}
		else
		{
			Logger.Error($"Unknown area type: {_rootArea.GetType()}");
			return Array.Empty<IWindowState>();
		}

		// Assign the windows, in order
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

	private IEnumerable<SliceRectangleItem> DoLayoutParent(IRectangle<int> rectangle, ParentArea area, int startIdx)
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

			Rectangle<int> childRectangle = new(x, y, width, height);

			int currentStartIdx = startIdx + idx;
			if (childArea is BaseArea baseArea)
			{
				foreach (SliceRectangleItem sliceItem in DoLayoutBase(childRectangle, baseArea, currentStartIdx))
				{
					yield return sliceItem;
				}
			}
			else if (childArea is SliceArea sliceArea)
			{
				foreach (SliceRectangleItem sliceItem in DoLayoutSlice(childRectangle, sliceArea, currentStartIdx))
				{
					yield return sliceItem;
				}
			}
			else if (childArea is ParentArea parentArea)
			{
				foreach (SliceRectangleItem sliceItem in DoLayoutParent(childRectangle, parentArea, currentStartIdx))
				{
					yield return sliceItem;
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

	private IEnumerable<SliceRectangleItem> DoLayoutSlice(IRectangle<int> rectangle, SliceArea area, int startIdx)
	{
		if (area.MaxChildren == 0 || startIdx >= _windows.Count)
		{
			yield break;
		}

		int x = rectangle.X;
		int y = rectangle.Y;
		int width = rectangle.Width;
		int height = rectangle.Height;

		int deltaX = 0;
		int deltaY = 0;

		int sliceItemsCount = _windows.Count - startIdx;

		if (area.IsHorizontal)
		{
			deltaX = rectangle.Width / sliceItemsCount;
			width = deltaX;
		}
		else
		{
			deltaY = rectangle.Height / sliceItemsCount;
			height = deltaY;
		}

		for (int idx = startIdx; idx < area.MaxChildren; idx++)
		{
			if (idx >= _windows.Count)
			{
				break;
			}

			yield return new SliceRectangleItem(idx, new Rectangle<int>(x, y, width, height));

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

	private IEnumerable<SliceRectangleItem> DoLayoutBase(IRectangle<int> rectangle, BaseArea area, int startIdx)
	{
		if (startIdx >= _windows.Count)
		{
			yield break;
		}

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

		for (int idx = startIdx; idx < _windows.Count; idx++)
		{
			yield return new SliceRectangleItem(idx, new Rectangle<int>(x, y, width, height));

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
