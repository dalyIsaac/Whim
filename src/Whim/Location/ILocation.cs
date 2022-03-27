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

	/// <param name="point">The point to check.</param>
	/// <returns>
	/// <see langword="true"/> if the location given by <paramref name="x"/> and <paramref name="y"/>
	/// is inside <paramref name="location"/>.
	/// </returns>
	public static bool IsPointInside(ILocation<int> location, IPoint<int> point) => location.X <= point.X
	&& location.Y <= point.Y
	&& location.X + location.Width > point.X
	&& location.Y + location.Height > point.Y;

	/// <param name="point">The point to check.</param>
	/// <returns>
	/// <see langword="true"/> if the location given by <paramref name="x"/> and <paramref name="y"/>
	/// is inside <paramref name="location"/>.
	/// </returns>
	public static bool IsPointInside(ILocation<double> location, IPoint<double> point) => location.X <= point.X
	&& location.Y <= point.Y
	&& location.X + location.Width > point.X
	&& location.Y + location.Height > point.Y;
}

