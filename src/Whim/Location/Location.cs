using System;

namespace Whim;

/// <inheritdoc />
public class Location : ILocation<int>
{
	/// <inheritdoc />
	public int X { get; set; }

	/// <inheritdoc />
	public int Y { get; set; }

	/// <inheritdoc />
	public int Width { get; set; }

	/// <inheritdoc />
	public int Height { get; set; }

	/// <inheritdoc />
	public Location(int x, int y, int width, int height)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
	}

	/// <inheritdoc />
	public bool IsPointInside(IPoint<int> point) => ILocation<int>.IsPointInside(this, point);

	/// <inheritdoc />
	public override string ToString() => $"(X: {X}, Y: {Y}, Width: {Width}, Height: {Height})";

	/// <summary>
	/// Scale the given <paramref name="location"/> by the given <paramref name="scale"/>.
	/// </summary>
	/// <param name="location"></param>
	/// <param name="scale"></param>
	/// <returns></returns>
	public static ILocation<int> Scale(ILocation<int> location, int scale) => new Location(
		x: location.X * scale,
		y: location.Y * scale,
		width: location.Width * scale,
		height: location.Height * scale
	);

	/// <inheritdoc />
	public static ILocation<int> Add(ILocation<int> a, ILocation<int> b) => new Location(
	a.X + b.X,
	a.Y + b.Y,
	a.Width + b.Width,
	a.Height + b.Height);

	/// <inheritdoc />
	public override bool Equals(object? obj) => obj is Location location
											 && location.X == X
											 && location.Y == Y
											 && location.Width == Width
											 && location.Height == Height;

	/// <inheritdoc />
	public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);

	/// <inheritdoc />
	public static ILocation<int> Empty() => new Location(0, 0, 0, 0);
}
