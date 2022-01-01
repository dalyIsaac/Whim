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

	public override IEnumerable<IWindowLocation> DoLayout(ILocation location)
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
}
