using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// A point in the coordinate space.
/// </summary>
public interface IPoint<T>
{
	/// <summary>
	/// The x-coordinate of the left of the item.
	/// </summary>
	public T X { get; }

	/// <summary>
	/// The y-coordinate of the top of the item.
	/// </summary>
	public T Y { get; }
}

/// <summary>
/// Extension methods for the <see cref="IPoint{T}"/> interface.
/// </summary>
public static class PointHelpers
{
	/// <summary>
	/// Converts the given <see cref="IPoint{T}"/> to a <see cref="POINT"/>.
	/// </summary>
	/// <param name="point">The point to convert.</param>
	public static POINT ToSystemPoint(this IPoint<int> point) => new() { x = point.X, y = point.Y };
}
