using System.Numerics;

namespace Whim;

/// <summary>
/// The location of an item, where the origin is in the top-left of the primary monitor.
/// </summary>
public interface ILocation<T> : IPoint<T> where T : INumber<T>
{
	/// <summary>
	/// The width of the item, in pixels.
	/// </summary>
	public T Width { get; }

	/// <summary>
	/// The height of the item, in pixels.
	/// </summary>
	public T Height { get; }

	/// <summary>
	/// Indicates whether the specified point is inside this item's bounding box.
	/// </summary>

	/// <summary>
	/// Checks if the given <paramref name="point"/> is inside the bounding box of this <see cref="ILocation{T}"/>.
	/// </summary>
	/// <param name="point">The point to check.</param>
	/// <returns>
	/// <see langword="true"/> if the location given by <paramref name="point"/> is inside this
	/// <see cref="ILocation{T}"/>'s bounding box; otherwise, <see langword="false"/>.
	/// </returns>
	public bool IsPointInside(IPoint<T> point);
}
