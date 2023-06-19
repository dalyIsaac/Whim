using System;
using System.Collections.Generic;

namespace Whim;

/// <summary>
/// Arranges windows in columns.
/// </summary>
public class ColumnLayoutEngine : BaseStackLayoutEngine
{
	/// <inheritdoc />
	public override string Name { get; }

	/// <summary>
	/// Indicates the direction of the layout.
	/// </summary>
	public bool LeftToRight { get; }

	/// <summary>
	/// Creates a new instance of the <see cref="ColumnLayoutEngine"/> class.
	/// </summary>
	/// <param name="name">The name of the layout engine.</param>
	/// <param name="leftToRight">Indicates the direction of the layout.</param>
	public ColumnLayoutEngine(string name = "Column", bool leftToRight = true)
	{
		Name = name;
		LeftToRight = leftToRight;
	}

	/// <inheritdoc />
	public override IEnumerable<IWindowState> DoLayout(ILocation<int> location, IMonitor monitor)
	{
		string direction = LeftToRight ? "left to right" : "right to left";
		Logger.Debug($"Performing a column layout {direction}");

		if (_stack.Count == 0)
		{
			yield break;
		}

		int x,
			y,
			width,
			height;

		if (LeftToRight)
		{
			width = location.Width / _stack.Count;
			height = location.Height;
			x = location.X;
			y = location.Y;
		}
		else
		{
			width = location.Width / _stack.Count;
			height = location.Height;
			x = location.X + location.Width - width;
			y = location.Y;
		}

		foreach (IWindow window in _stack)
		{
			yield return new WindowState()
			{
				Window = window,
				Location = new Location<int>()
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

	/// <inheritdoc />
	public override void FocusWindowInDirection(Direction direction, IWindow window)
	{
		Logger.Debug($"Focusing window {window} in layout engine {Name}");

		if (direction != Direction.Left && direction != Direction.Right)
		{
			return;
		}

		// Find the index of the window in the stack
		int windowIndex = _stack.FindIndex(x => x.Handle == window.Handle);
		if (windowIndex == -1)
		{
			Logger.Error($"Window {window} not found in layout engine {Name}");
			return;
		}

		int delta = GetDelta(LeftToRight, direction);
		int adjIndex = (windowIndex + delta).Mod(_stack.Count);

		IWindow adjWindow = _stack[adjIndex];
		adjWindow.Focus();
	}

	/// <inheritdoc />
	public override void SwapWindowInDirection(Direction direction, IWindow window)
	{
		Logger.Debug($"Swapping window {window} in layout engine {Name} in direction {direction}");

		if (direction != Direction.Left && direction != Direction.Right)
		{
			return;
		}

		// Find the index of the window in the stack.
		int windowIndex = _stack.FindIndex(x => x.Handle == window?.Handle);
		if (windowIndex == -1)
		{
			Logger.Error($"Window {window} not found in layout engine {Name}");
			return;
		}

		int delta = GetDelta(LeftToRight, direction);
		int adjIndex = (windowIndex + delta).Mod(_stack.Count);

		// Swap window
		IWindow adjWindow = _stack[adjIndex];
		_stack[windowIndex] = adjWindow;
		_stack[adjIndex] = window;
	}

	/// <inheritdoc />
	public override void MoveWindowEdgeInDirectionFraction(Direction edge, double delta, IWindow window)
	{
		// Not implemented.
	}

	/// <inheritdoc />
	public override void AddWindowAtPoint(IWindow window, IPoint<double> point)
	{
		Logger.Debug($"Adding window {window} to layout engine {Name} at point {point}");

		// Calculate the index of the window in the stack.
		int idx = (int)Math.Round(point.X / _stack.Count, MidpointRounding.AwayFromZero);

		// Bound idx.
		if (idx < 0)
		{
			idx = 0;
		}
		else if (idx > _stack.Count)
		{
			idx = _stack.Count;
		}

		if (!LeftToRight)
		{
			idx = _stack.Count - idx;
		}

		_stack.Insert(idx, window);
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
}
