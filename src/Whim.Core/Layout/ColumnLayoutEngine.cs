using System.Collections.Generic;

namespace Whim.Core;

public class ColumnLayoutEngine : BaseStackLayoutEngine
{
	public override string Name => "Column";

	public override IEnumerable<IWindowLocation> DoLayout(IArea area)
	{
		Logger.Debug("Performing a column layout");

		if (_stack.Count == 0)
		{
			yield break;
		}

		int x = 0;
		int y = 0;
		int width = area.Width / _stack.Count;
		int height = area.Height;

		foreach (IWindow window in _stack)
		{
			yield return new WindowLocation(window,
											new Location(x, y, width, height),
											WindowState.Normal);
			x += width;
		}
	}
}
