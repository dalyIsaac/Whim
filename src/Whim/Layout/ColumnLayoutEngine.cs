using System.Collections.Generic;

namespace Whim;

public class ColumnLayoutEngine : BaseStackLayoutEngine
{
	public override string Name => "Column";

	public override IEnumerable<IWindowLocation> DoLayout(ILocation location)
	{
		Logger.Debug("Performing a column layout");

		if (_stack.Count == 0)
		{
			yield break;
		}

		int x = location.X;
		int y = location.Y;
		int width = location.Width / _stack.Count;
		int height = location.Height;

		foreach (IWindow window in _stack)
		{
			yield return new WindowLocation(window,
											new Location(x, y, width, height),
											WindowState.Normal);
			x += width;
		}
	}
}
