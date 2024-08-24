using System.Numerics;

namespace Whim;

/// <summary>
/// Extension methods for <see cref="IRectangle{T}"/>.
/// </summary>
public static class RectangleExtensions
{
	/// <summary>
	/// Adds the given <paramref name="other"/> to this <see cref="IRectangle{T}"/>.
	/// </summary>
	/// <param name="rectangle">The rectangle to add to.</param>
	/// <param name="other">The rectangle to add.</param>
	/// <returns>A new <see cref="IRectangle{T}"/> with the given <paramref name="other"/> added to this one.</returns>
	public static IRectangle<T> Add<T>(this IRectangle<T> rectangle, IRectangle<T> other)
		where T : INumber<T> =>
		new Rectangle<T>()
		{
			X = rectangle.X + other.X,
			Y = rectangle.Y + other.Y,
			Width = rectangle.Width + other.Width,
			Height = rectangle.Height + other.Height,
		};
}
