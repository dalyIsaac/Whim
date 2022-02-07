using System.Collections.Generic;

namespace Whim;

public class ColumnLayoutEngine : BaseStackLayoutEngine
{
	public override string Name { get; }
	public bool LeftToRight { get; }

	public ColumnLayoutEngine(string name = "Column", bool leftToRight = true)
	{
		Name = name;
		LeftToRight = leftToRight;
	}

	public override IEnumerable<IWindowLocation> DoLayout(ILocation<int> location)
	{
		string direction = LeftToRight ? "left to right" : "right to left";
		Logger.Debug($"Performing a column layout {direction}");

		if (_stack.Count == 0)
		{
			yield break;
		}

		int x, y, width, height;

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
			yield return new WindowLocation(window,
											new Location(x, y, width, height),
											WindowState.Normal);

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
			Logger.Error($"Window {window.Title} not found in layout engine {Name}");
			return;
		}

		int delta = GetDelta(LeftToRight, direction);
		int adjIndex = (windowIndex + delta).Mod(_stack.Count);

		IWindow adjWindow = _stack[adjIndex];
		adjWindow.Focus();
	}

	public override void SwapWindowInDirection(Direction direction, IWindow window)
	{
		Logger.Debug($"Swapping window {window} in layout engine {Name}");

		if (direction != Direction.Left && direction != Direction.Right)
		{
			return;
		}

		// Find the index of the window in the stack
		int windowIndex = _stack.FindIndex(x => x.Handle == window?.Handle);
		if (windowIndex == -1)
		{
			Logger.Error($"Window {window?.Title} not found in layout engine {Name}");
			return;
		}

		int delta = GetDelta(LeftToRight, direction);
		int adjIndex = (windowIndex + delta).Mod(_stack.Count);

		// Swap window
		IWindow adjWindow = _stack[adjIndex];
		_stack[windowIndex] = adjWindow;
		_stack[adjIndex] = window;
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
