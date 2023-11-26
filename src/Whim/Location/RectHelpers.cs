using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// Extension methods for <see cref="RECT"/>.
/// </summary>
internal static class RectHelpers
{
	/// <summary>
	/// Converts a <see cref="RECT"/> to a <see cref="IRectangle{T}"/>.
	/// </summary>
	/// <param name="rect">The <see cref="RECT"/> to convert.</param>
	/// <returns>The converted <see cref="IRectangle{T}"/>.</returns>
	public static IRectangle<int> ToRectangle(this RECT rect) =>
		new Rectangle<int>()
		{
			X = rect.left,
			Y = rect.top,
			Width = rect.right - rect.left,
			Height = rect.bottom - rect.top
		};
}
