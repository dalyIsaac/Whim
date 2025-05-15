using System.Numerics;

namespace Whim;

/// <summary>
/// A rectangle, where the origin is in the top-left of the primary monitor.
/// </summary>
public interface IRectangle<T> : IPoint<T>
	where T : INumber<T>
{
	/// <summary>
	/// The width, in pixels.
	/// </summary>
	T Width { get; }

	/// <summary>
	/// The height, in pixels.
	/// </summary>
	T Height { get; }

	/// <summary>
	/// The center of the rectangle.
	/// </summary>
	IPoint<T> Center { get; }

	/// <summary>
	/// Indicates whether the specified point is inside this item's bounding box.
	/// </summary>
	/// <summary>
	/// Checks if the given <paramref name="point"/> is inside the bounding box of this <see cref="IRectangle{T}"/>.
	/// </summary>
	/// <param name="point">The point to check.</param>
	/// <returns>
	/// <see langword="true"/> if the location given by <paramref name="point"/> is inside this
	/// <see cref="IRectangle{T}"/>'s bounding box; otherwise, <see langword="false"/>.
	/// </returns>
	bool ContainsPoint(IPoint<T> point);
}
