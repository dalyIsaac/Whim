using System;
using System.Collections.Generic;

namespace Whim;

public class ColumnLayoutEngine : BaseStackLayoutEngine
{

	public ColumnLayoutEngine(IConfigContext configContext, string name = "Column", bool leftToRight = true) : base(configContext, name, leftToRight, 40)
	{
	}

	public override IEnumerable<IWindowLocation> DoLayout(ILocation location)
	{
		string directionStr = LeftToRight ? "left to right" : "right to left";
		Logger.Debug($"Performing a column layout {directionStr}");

		if (_stack.Count == 0)
		{
			yield break;
		}

		int numInPrimary = Math.Min(_primaryAreaTotal, _stack.Count);
		int numInSecondary = _stack.Count - numInPrimary;

		int primaryAreaWidth = location.Width * (_primaryAreaSizePercent + _primaryAreaSizePercentOffset) / 100;
		int secondaryAreaWidth = (location.Width - primaryAreaWidth);

		int primaryWindowWidth = primaryAreaWidth / numInPrimary;
		int secondaryWindowWidth = secondaryAreaWidth / numInSecondary;

		int x, y, direction;
		int height = location.Height;

		if (LeftToRight)
		{
			x = location.X;
			y = location.Y;
			direction = 1;
		}
		else
		{
			x = location.X + location.Width - primaryWindowWidth;
			y = location.Y;
			direction = -1;
		}

		// Layout the primary windows
		int stackIdx = 0;
		for (; stackIdx < numInPrimary; stackIdx++)
		{
			IWindow window = _stack[stackIdx];
			yield return new WindowLocation(window,
											new Location(x, y, primaryWindowWidth, height),
											WindowState.Normal);

			x += direction * primaryWindowWidth;
		}

		// Layout the secondary windows
		for (; stackIdx < _stack.Count; stackIdx++)
		{
			IWindow window = _stack[stackIdx];
			yield return new WindowLocation(window,
											new Location(x, y, secondaryWindowWidth, height),
											WindowState.Normal);

			x += direction * secondaryWindowWidth;
		}
	}
}
