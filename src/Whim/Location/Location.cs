using System;
using System.Numerics;

namespace Whim;

/// <inheritdoc />
public record Location<T> : ILocation<T>, IEquatable<Location<T>>
	where T : INumber<T>
{
	/// <inheritdoc />
	public T X { get; set; } = T.Zero;

	/// <inheritdoc />
	public T Y { get; set; } = T.Zero;

	/// <inheritdoc />
	public T Width { get; set; } = T.Zero;

	/// <inheritdoc />
	public T Height { get; set; } = T.Zero;

	/// <inheritdoc />
	public bool IsPointInside(IPoint<T> point) =>
		X <= point.X && Y <= point.Y && X + Width > point.X && Y + Height > point.Y;

	/// <inheritdoc />
	public override string ToString() => $"(X: {X}, Y: {Y}, Width: {Width}, Height: {Height})";

	/// <inheritdoc />
	public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
}
