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
	public static System.Drawing.Point ToSystemPoint(this IPoint<int> point)
	{
		return new System.Drawing.Point(point.X, point.Y);
	}
}
