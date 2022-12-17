using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// Extension methods for <see cref="RECT"/>.
/// </summary>
internal static class RectHelpers
{
	/// <summary>
	/// Converts a <see cref="RECT"/> to a <see cref="ILocation{T}"/>.
	/// </summary>
	/// <param name="rect">The <see cref="RECT"/> to convert.</param>
	/// <returns>The converted <see cref="ILocation{T}"/>.</returns>
	public static ILocation<int> ToLocation(this RECT rect) =>
		new Location<int>()
		{
			X = rect.left,
			Y = rect.top,
			Width = rect.right - rect.left,
			Height = rect.bottom - rect.top
		};
}
