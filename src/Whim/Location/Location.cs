using System;

namespace Whim;

public class Location : ILocation<int>
{
	public int X { get; set; }

	public int Y { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public Location(int x, int y, int width, int height)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
	}

	public bool IsPointInside(IPoint<int> point) => ILocation<int>.IsPointInside(this, point);

	public override string ToString() => $"(X: {X}, Y: {Y}, Width: {Width}, Height: {Height})";

	public static ILocation<int> Add(ILocation<int> a, ILocation<int> b) => new Location(
	a.X + b.X,
	a.Y + b.Y,
	a.Width + b.Width,
	a.Height + b.Height);

	public override bool Equals(object? obj) => obj is Location location
											 && location.X == X
											 && location.Y == Y
											 && location.Width == Width
											 && location.Height == Height;

	public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
}
