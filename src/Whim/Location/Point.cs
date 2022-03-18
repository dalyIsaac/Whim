namespace Whim;

public class Point<T> : IPoint<T>
{
	public T X { get; }
	public T Y { get; }

	public Point(T x, T y)
	{
		X = x;
		Y = y;
	}

	public override string ToString()
	{
		return $"({X}, {Y})";
	}
}
