using System;
using System.Numerics;

namespace Whim;

/// <inheritdoc />
public class Location<T> : ILocation<T> where T : INumber<T>
{
	/// <inheritdoc />
	public T X { get; set; }

	/// <inheritdoc />
	public T Y { get; set; }

	/// <inheritdoc />
	public T Width { get; set; }

	/// <inheritdoc />
	public T Height { get; set; }

	/// <inheritdoc />
	public Location()
	{
		X = T.Zero;
		Y = T.Zero;
		Width = T.Zero;
		Height = T.Zero;
	}

	/// <inheritdoc />
	public Location(T x, T y, T width, T height)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
	}

	/// <inheritdoc />
	public bool IsPointInside(IPoint<T> point) =>
		X <= point.X && Y <= point.Y && X + Width > point.X && Y + Height > point.Y;

	/// <inheritdoc />
	public override string ToString() => $"(X: {X}, Y: {Y}, Width: {Width}, Height: {Height})";

	/// <inheritdoc />
	public static ILocation<T> Add(ILocation<T> a, ILocation<T> b) =>
		new Location<T>(a.X + b.X, a.Y + b.Y, a.Width + b.Width, a.Height + b.Height);

	/// <inheritdoc />
	public override bool Equals(object? obj) =>
		obj is Location<T> location
		&& location.X == X
		&& location.Y == Y
		&& location.Width == Width
		&& location.Height == Height;

	/// <inheritdoc />
	public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
}
