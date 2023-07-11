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

	/// <summary>
	/// Creates a new <see cref="Location{T}"/> with zero values.
	/// </summary>
	public Location() { }

	/// <summary>
	/// Creates a new <see cref="Location{T}"/> with the given values.
	/// </summary>
	/// <param name="location"></param>
	public Location(ILocation<T> location)
	{
		X = location.X;
		Y = location.Y;
		Width = location.Width;
		Height = location.Height;
	}

	/// <inheritdoc />
	public bool ContainsPoint(IPoint<T> point) =>
		point.X >= X && point.X < X + Width && point.Y >= Y && point.Y < Y + Height;

	/// <inheritdoc />
	public override string ToString() => $"(X: {X}, Y: {Y}, Width: {Width}, Height: {Height})";

	/// <inheritdoc />
	public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
}
