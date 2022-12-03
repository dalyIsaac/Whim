using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// Extension methods for <see cref="RECT"/>.
/// </summary>
public static class RectHelpers
{
	/// <summary>
	/// Converts a <see cref="RECT"/> to a <see cref="ILocation{T}"/>.
	/// </summary>
	/// <param name="rect">The <see cref="RECT"/> to convert.</param>
	/// <returns>The converted <see cref="ILocation{T}"/>.</returns>
	public static ILocation<int> ToLocation(this RECT rect) =>
		new Location(x: rect.left, y: rect.top, width: rect.right - rect.left, height: rect.bottom - rect.top);
}
