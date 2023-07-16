using System.Numerics;

namespace Whim;

/// <inheritdoc />
public record Point<T> : IPoint<T>
	where T : INumber<T>
{
	/// <inheritdoc />
	public T X { get; set; } = T.Zero;

	/// <inheritdoc />
	public T Y { get; set; } = T.Zero;

	/// <summary>
	/// Creates a point with the <see cref="X"/> and <see cref="Y"/> values set to 0.
	/// </summary>
	public Point() { }

	/// <summary>
	/// Creates a new copy of <paramref name="point"/>.
	/// </summary>
	/// <param name="point"></param>
	public Point(IPoint<T> point)
	{
		X = point.X;
		Y = point.Y;
	}

	/// <inheritdoc />
	public override string ToString() => $"({X}, {Y})";
}
