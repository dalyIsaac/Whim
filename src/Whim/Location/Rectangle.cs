using System;
using System.Numerics;

namespace Whim;

/// <summary>
/// Helpers for <see cref="IRectangle{T}"/>.
/// </summary>
public static class Rectangle
{
	/// <summary>
	/// Creates a new <see cref="Rectangle{T}"/> of the unit square.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public static Rectangle<T> UnitSquare<T>()
		where T : INumber<T> => new() { Width = T.One, Height = T.One };
}

/// <inheritdoc />
public record Rectangle<T> : IRectangle<T>, IEquatable<Rectangle<T>>
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
	/// Creates a new <see cref="Rectangle{T}"/> with zero values.
	/// </summary>
	public Rectangle() { }

	/// <summary>
	/// Creates a new <see cref="Rectangle{T}"/> with the given values.
	/// </summary>
	/// <param name="rectangle"></param>
	public Rectangle(IRectangle<T> rectangle)
	{
		X = rectangle.X;
		Y = rectangle.Y;
		Width = rectangle.Width;
		Height = rectangle.Height;
	}

	/// <inheritdoc />
	public bool ContainsPoint(IPoint<T> point) =>
		point.X >= X && point.X < X + Width && point.Y >= Y && point.Y < Y + Height;

	/// <inheritdoc />
	public override string ToString() => $"(X: {X}, Y: {Y}, Width: {Width}, Height: {Height})";

	/// <inheritdoc />
	public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
}
