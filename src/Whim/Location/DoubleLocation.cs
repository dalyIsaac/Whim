using System;

namespace Whim;

public class DoubleLocation : ILocation<double>
{
	public double X { get; set; }

	public double Y { get; set; }

	public double Width { get; set; }

	public double Height { get; set; }

	public DoubleLocation() { }

	public DoubleLocation(ILocation<double> location)
	{
		X = location.X;
		Y = location.Y;
		Width = location.Width;
		Height = location.Height;
	}

	public DoubleLocation(double x, double y, double width, double height)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
	}

	public bool IsPointInside(IPoint<double> point) => ILocation<int>.IsPointInside(this, point);

	public override string ToString() => $"(X: {X}, Y: {Y}, Width: {Width}, Height: {Height})";

	public static ILocation<double> Add(ILocation<double> a, ILocation<double> b) => new DoubleLocation(
	a.X + b.X,
	a.Y + b.Y,
	a.Width + b.Width,
	a.Height + b.Height);

	public override bool Equals(object? obj) => obj is DoubleLocation DoubleLocation
											 && DoubleLocation.X == X
											 && DoubleLocation.Y == Y
											 && DoubleLocation.Width == Width
											 && DoubleLocation.Height == Height;

	public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
}
