using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Whim;

/// <summary>
/// Column layout engine with a stack data structure.
/// </summary>
public record ColumnLayoutEngine : ILayoutEngine
{
	/// <summary>
	/// The stack of windows in the engine.
	/// </summary>
	private readonly ImmutableList<IWindow> _stack;

	private readonly ImmutableList<IWindow> _minimizedStack;

	/// <inheritdoc/>
	public string Name { get; init; } = "Column";

	/// <inheritdoc/>
	public LayoutEngineIdentity Identity { get; }

	/// <summary>
	/// Indicates the direction of the layout. Defaults to <see langword="false"/>.
	/// </summary>
	public bool LeftToRight { get; init; } = true;

	/// <inheritdoc/>
	public int Count => _stack.Count;

	/// <summary>
	/// Creates a new instance of the <see cref="ColumnLayoutEngine"/> class.
	/// </summary>
	/// <param name="identity">The identity of the layout engine.</param>
	public ColumnLayoutEngine(LayoutEngineIdentity identity)
	{
		Identity = identity;
		_stack = ImmutableList<IWindow>.Empty;
		_minimizedStack = ImmutableList<IWindow>.Empty;
	}

	private ColumnLayoutEngine(
		ColumnLayoutEngine layoutEngine,
		ImmutableList<IWindow> stack,
		ImmutableList<IWindow> minimizedStack
	)
	{
		Name = layoutEngine.Name;
		Identity = layoutEngine.Identity;
		LeftToRight = layoutEngine.LeftToRight;
		_stack = stack;
		_minimizedStack = minimizedStack;
	}

	/// <inheritdoc/>
	public ILayoutEngine AddWindow(IWindow window)
	{
		Logger.Debug($"Adding window {window} to layout engine {Name}");

		if (_stack.Contains(window))
		{
			Logger.Error($"Window {window} already exists in layout engine {Name}");
			return this;
		}

		return new ColumnLayoutEngine(this, _stack.Add(window), _minimizedStack);
	}

	/// <inheritdoc/>
	public ILayoutEngine RemoveWindow(IWindow window)
	{
		Logger.Debug($"Removing window {window} from layout engine {Name}");

		ImmutableList<IWindow> newStack = _stack.Remove(window);
		return newStack == _stack ? this : new ColumnLayoutEngine(this, newStack, _minimizedStack);
	}

	/// <inheritdoc/>
	public bool ContainsWindow(IWindow window)
	{
		Logger.Debug($"Checking if layout engine {Name} contains window {window}");
		return _stack.Any(x => x.Equals(window));
	}

	/// <inheritdoc/>
	public IEnumerable<IWindowState> DoLayout(IRectangle<int> rectangle, IMonitor _)
	{
		string direction = LeftToRight ? "left to right" : "right to left";
		Logger.Debug($"Performing a column layout {direction}");

		if (_stack.Count == 0)
		{
			yield break;
		}

		int x;
		int y;
		int width;
		int height;

		if (LeftToRight)
		{
			width = rectangle.Width / _stack.Count;
			height = rectangle.Height;
			x = rectangle.X;
			y = rectangle.Y;
		}
		else
		{
			width = rectangle.Width / _stack.Count;
			height = rectangle.Height;
			x = rectangle.X + rectangle.Width - width;
			y = rectangle.Y;
		}

		foreach (IWindow window in _stack)
		{
			yield return new WindowState()
			{
				Window = window,
				Rectangle = new Rectangle<int>()
				{
					X = x,
					Y = y,
					Width = width,
					Height = height
				},
				WindowSize = WindowSize.Normal
			};

			if (LeftToRight)
			{
				x += width;
			}
			else
			{
				x -= width;
			}
		}
	}

	/// <inheritdoc/>
	public IWindow? GetFirstWindow() => _stack.FirstOrDefault();

	/// <inheritdoc/>
	public ILayoutEngine FocusWindowInDirection(Direction direction, IWindow window)
	{
		Logger.Debug($"Focusing window {window} in layout engine {Name}");

		if (direction != Direction.Left && direction != Direction.Right)
		{
			return this;
		}

		// Find the index of the window in the stack
		int windowIndex = _stack.FindIndex(x => x.Equals(window));
		if (windowIndex == -1)
		{
			Logger.Error($"Window {window} not found in layout engine {Name}");
			return this;
		}

		int delta = GetDelta(LeftToRight, direction);
		int adjIndex = (windowIndex + delta).Mod(_stack.Count);

		IWindow adjWindow = _stack[adjIndex];
		adjWindow.Focus();
		return this;
	}

	/// <inheritdoc/>
	public ILayoutEngine SwapWindowInDirection(Direction direction, IWindow window)
	{
		Logger.Debug($"Swapping window {window} in layout engine {Name} in direction {direction}");

		if (direction != Direction.Left && direction != Direction.Right)
		{
			return this;
		}

		// Find the index of the window in the stack.
		int windowIndex = _stack.FindIndex(x => x.Equals(window));
		if (windowIndex == -1)
		{
			Logger.Error($"Window {window} not found in layout engine {Name}");
			return this;
		}

		int delta = GetDelta(LeftToRight, direction);
		int adjIndex = (windowIndex + delta).Mod(_stack.Count);

		// Swap window
		IWindow adjWindow = _stack[adjIndex];
		ImmutableList<IWindow> newStack = _stack.SetItem(windowIndex, adjWindow).SetItem(adjIndex, window);
		return new ColumnLayoutEngine(this, newStack, _minimizedStack);
	}

	/// <inheritdoc/>
	public ILayoutEngine MoveWindowEdgesInDirection(Direction edge, IPoint<double> deltas, IWindow window) => this;

	/// <inheritdoc/>
	public ILayoutEngine MoveWindowToPoint(IWindow window, IPoint<double> point)
	{
		Logger.Debug($"Adding window {window} to layout engine {Name} at point {point}");

		ImmutableList<IWindow> newStack = _stack.Remove(window);
		// Calculate the index of the window in the stack.
		int idx = (int)Math.Round(point.X * newStack.Count, MidpointRounding.AwayFromZero);

		// Bound idx.
		if (idx < 0)
		{
			idx = 0;
		}
		else if (idx > newStack.Count)
		{
			idx = newStack.Count;
		}

		if (!LeftToRight)
		{
			idx = newStack.Count - idx;
		}

		return new ColumnLayoutEngine(this, newStack.Insert(idx, window), _minimizedStack);
	}

	/// <summary>
	/// Gets the delta to determine whether we want to move towards 0 or not.
	/// </summary>
	/// <param name="leftToRight">Whether we are moving left to right or right to left.</param>
	/// <param name="direction">The window direction to move.</param>
	private static int GetDelta(bool leftToRight, Direction direction)
	{
		if (leftToRight)
		{
			return direction == Direction.Left ? -1 : 1;
		}
		else
		{
			return direction == Direction.Left ? 1 : -1;
		}
	}

	/// <inheritdoc/>
	public ILayoutEngine MinimizeWindowStart(IWindow window)
	{
		Logger.Debug($"Minimizing window {window} in layout engine {Name}");

		ImmutableList<IWindow> newStack = _stack.Remove(window);
		ImmutableList<IWindow> newMinimizedStack = _minimizedStack.Add(window);

		return new ColumnLayoutEngine(this, newStack, newMinimizedStack);
	}

	/// <inheritdoc/>
	public ILayoutEngine MinimizeWindowEnd(IWindow window)
	{
		Logger.Debug($"Minimizing window {window} in layout engine {Name}");

		ImmutableList<IWindow> newStack = _stack.Remove(window);
		ImmutableList<IWindow> newMinimizedStack = _minimizedStack.Insert(0, window);

		return new ColumnLayoutEngine(this, newStack, newMinimizedStack);
	}

	/// <inheritdoc/>
	public ILayoutEngine PerformCustomAction<T>(LayoutEngineCustomAction<T> action) => this;
}
