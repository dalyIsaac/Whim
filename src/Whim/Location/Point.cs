namespace Whim;

/// <inheritdoc />
public class Point<T> : IPoint<T>
{
	/// <inheritdoc />
	public T X { get; }

	/// <inheritdoc />
	public T Y { get; }

	/// <summary>
	/// Constructs a new point at the given coordinates.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public Point(T x, T y)
	{
		X = x;
		Y = y;
	}

	/// <inheritdoc />
	public override string ToString() => $"({X}, {Y})";
}
