using System;
using System.Collections.Generic;

namespace Whim;

public class TallLayoutEngine : BasePrimaryStackLayoutEngine
{
	private IWindowLocation[] _lastLayout = Array.Empty<IWindowLocation>();

	public TallLayoutEngine(string name = "Tall", bool leftToRight = true) : base(name, leftToRight, 50, 5)
	{
	}

	public override IEnumerable<IWindowLocation> DoLayout(ILocation location)
	{
		string direction = LeftToRight ? "left to right" : "right to left";
		Logger.Debug($"Performing a tall layout {direction}");

		if (_stack.Count == 0)
		{
			yield break;
		}

		_lastLayout = new IWindowLocation[_stack.Count];

		int numInPrimary = Math.Min(_primaryAreaTotal, _stack.Count);
		int primaryAreaWidth = location.Width * (_primaryPercentBase + _primaryPercentOffset) / 100;
		int primaryWindowHeight = location.Height / numInPrimary;

		int secondaryAreaWidth = location.Width - primaryAreaWidth;
		int secondaryWindowHeight = 0;

		// If there are more "primary" windows than actual windows, then we want the
		// pane to spread the entire width of the working area.
		if (numInPrimary >= _stack.Count)
		{
			primaryAreaWidth = location.Width;
		}
		else
		{
			secondaryWindowHeight= location.Height / (_stack.Count - numInPrimary);
		}

		int primaryX, secondaryX;
		int y =  location.Y;

		if (LeftToRight)
		{
			primaryX = location.X;
			secondaryX = location.X + primaryAreaWidth;
		}
		else
		{
			primaryX = location.X + location.Width - primaryAreaWidth;
			secondaryX = location.X;
		}

		// Layout the primary windows
		int stackIdx = 0;
		for (; stackIdx < numInPrimary; stackIdx++)
		{
			IWindow window = _stack[stackIdx];
			_lastLayout[stackIdx] = new WindowLocation(window,
											new Location(primaryX, y, primaryAreaWidth, primaryWindowHeight),
											WindowState.Normal);
			y += primaryWindowHeight;
		}

		// Layout the secondary windows
		y = location.Y;
		for (; stackIdx < _stack.Count; stackIdx++)
		{
			IWindow window = _stack[stackIdx];
			_lastLayout[stackIdx] = new WindowLocation(window,
											new Location(secondaryX, y, secondaryAreaWidth, secondaryWindowHeight),
											WindowState.Normal);
			y += secondaryWindowHeight;
		}

		// Yield the layout
		foreach (IWindowLocation windowLocation in _lastLayout)
		{
			yield return windowLocation;
		}
	}
}
