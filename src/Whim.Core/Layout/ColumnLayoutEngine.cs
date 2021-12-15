using System.Collections.Generic;

namespace Whim.Core.Layout;

public class ColumnLayoutEngine : BaseStackLayoutEngine
{
	public override string Name => "Column";

	public override IEnumerable<IWindowLocation> DoLayout(IArea area)
	{
		int x = 0;
		int y = 0;
		int width = area.Width / _stack.Count;
		int height = area.Height;

		foreach (IWindow window in _stack)
		{
			yield return new WindowLocation(window,
											new Location.Location(x, y, width, height),
											WindowState.Normal);
			x += width;
		}
	}
}
