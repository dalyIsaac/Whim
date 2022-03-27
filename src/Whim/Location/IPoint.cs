using Windows.Win32.Foundation;

namespace Whim;

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

public static class PointHelpers
{
	public static POINT ToSystemPoint(this IPoint<int> point)
	{
		return new POINT() { x = point.X, y = point.Y };
	}
}
