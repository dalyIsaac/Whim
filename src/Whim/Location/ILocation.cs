namespace Whim;

/// <summary>
/// The location of an item, where the origin is in the top-left of the primary monitor.
/// </summary>
public interface ILocation<T> : IPoint<T>
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
	public bool IsPointInside(IPoint<T> point);

	/// <summary>
	/// Checks if the given <paramref name="point"/> is inside the bounding box of the given
	/// <paramref name="location"/>.
	/// </summary>
	/// <param name="location"></param>
	/// <param name="point">The point to check.</param>
	/// <returns>
	/// <see langword="true"/> if the location given by <paramref name="point"/> is inside the
	/// <paramref name="location"/>'s bounding box; otherwise, <see langword="false"/>.
	/// </returns>
	public static bool IsPointInside(ILocation<int> location, IPoint<int> point) =>
		location.X <= point.X
		&& location.Y <= point.Y
		&& location.X + location.Width > point.X
		&& location.Y + location.Height > point.Y;

	/// <summary>
	/// Checks if the given <paramref name="point"/> is inside the bounding box of the given
	/// <paramref name="location"/>.
	/// </summary>
	/// <param name="location"></param>
	/// <param name="point">The point to check.</param>
	/// <returns>
	/// <see langword="true"/> if the location given by <paramref name="point"/> is inside the
	/// <paramref name="location"/>'s bounding box; otherwise, <see langword="false"/>.
	/// </returns>
	public static bool IsPointInside(ILocation<double> location, IPoint<double> point) =>
		location.X <= point.X
		&& location.Y <= point.Y
		&& location.X + location.Width > point.X
		&& location.Y + location.Height > point.Y;
}
