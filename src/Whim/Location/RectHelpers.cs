using Windows.Win32.Foundation;

namespace Whim;

public static class RectHelpers
{
	public static ILocation<int> ToLocation(this RECT rect) => new Location(
		x: rect.left,
		y: rect.top,
		width: rect.right - rect.left,
		height: rect.bottom - rect.top
	);
}
