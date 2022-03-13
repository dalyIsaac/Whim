namespace Whim;

/// <summary>
/// The location of an item, where the origin is in the top-left of the primary monitor.
/// </summary>
public interface ILocation<T>
{
	/// <summary>
	/// The x-coordinate of the left of the item.
	/// </summary>
	public T X { get; }

	/// <summary>
	/// The y-coordinate of the top of the item.
	/// </summary>
	public T Y { get; }

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
	public bool IsPointInside(T x, T y);

	/// <param name="x">The x-coordinate of the point to check.</param>
	/// <param name="y">The y-coordinate of the point to check.</param>
	/// <returns>
	/// <see langword="true"/> if the location given by <paramref name="x"/> and <paramref name="y"/>
	/// is inside <paramref name="location"/>.
	/// </returns>
	public static bool IsPointInside(ILocation<int> location, int x, int y) => location.X <= x
	&& location.Y <= y
	&& location.X + location.Width >= x
	&& location.Y + location.Height >= y;

	/// <param name="x">The x-coordinate of the point to check.</param>
	/// <param name="y">The y-coordinate of the point to check.</param>
	/// <returns>
	/// <see langword="true"/> if the location given by <paramref name="x"/> and <paramref name="y"/>
	/// is inside <paramref name="location"/>.
	/// </returns>
	public static bool IsPointInside(ILocation<double> location, double x, double y) => location.X <= x
	&& location.Y <= y
	&& location.X + location.Width >= x
	&& location.Y + location.Height >= y;
}
