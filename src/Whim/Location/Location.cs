using System;
using System.Numerics;

namespace Whim;

/// <summary>
/// Helpers for <see cref="ILocation{T}"/>.
/// </summary>
public static class Location
{
	/// <summary>
	/// Creates a new <see cref="Location{T}"/> of the unit square.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public static Location<T> UnitSquare<T>()
		where T : INumber<T> => new() { Width = T.One, Height = T.One };
}

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
