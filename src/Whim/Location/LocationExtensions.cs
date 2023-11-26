using System.Numerics;

namespace Whim;

/// <summary>
/// Extension methods for <see cref="IRectangle{T}"/>.
/// </summary>
public static class LocationExtensions
{
	/// <summary>
	/// Adds the given <paramref name="other"/> to this <see cref="IRectangle{T}"/>.
	/// </summary>
	/// <param name="location">The location to add to.</param>
	/// <param name="other">The location to add.</param>
	/// <returns>A new <see cref="IRectangle{T}"/> with the given <paramref name="other"/> added to this one.</returns>
	public static IRectangle<T> Add<T>(this IRectangle<T> location, IRectangle<T> other)
		where T : INumber<T> =>
		new Rectangle<T>()
		{
			X = location.X + other.X,
			Y = location.Y + other.Y,
			Width = location.Width + other.Width,
			Height = location.Height + other.Height
		};
}
