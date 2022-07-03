using System;

namespace Whim;

/// <inheritdoc/>
public class DoubleLocation : ILocation<double>
{
	/// <inheritdoc />
	public double X { get; set; }

	/// <inheritdoc />
	public double Y { get; set; }

	/// <inheritdoc />
	public double Width { get; set; }

	/// <inheritdoc />
	public double Height { get; set; }

	/// <summary>
	/// Constructs a new double location at the origin, with a width and height of 0.
	/// </summary>
	public DoubleLocation() { }

	/// <summary>
	/// Copies a double location.
	/// </summary>
	/// <param name="location">The location to copy.</param>
	public DoubleLocation(ILocation<double> location)
	{
		X = location.X;
		Y = location.Y;
		Width = location.Width;
		Height = location.Height;
	}

	/// <summary>
	/// Creates a new double location.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="width">The width.</param>
	/// <param name="height">The height.</param>
	public DoubleLocation(double x, double y, double width, double height)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
	}

	/// <inheritdoc />
	public bool IsPointInside(IPoint<double> point) => ILocation<int>.IsPointInside(this, point);

	/// <inheritdoc />
	public override string ToString() => $"(X: {X}, Y: {Y}, Width: {Width}, Height: {Height})";

	/// <inheritdoc />
	public static ILocation<double> Add(ILocation<double> a, ILocation<double> b) => new DoubleLocation(
	a.X + b.X,
	a.Y + b.Y,
	a.Width + b.Width,
	a.Height + b.Height);

	/// <inheritdoc />
	public override bool Equals(object? obj) => obj is DoubleLocation DoubleLocation
											 && DoubleLocation.X == X
											 && DoubleLocation.Y == Y
											 && DoubleLocation.Width == Width
											 && DoubleLocation.Height == Height;

	/// <inheritdoc />
	public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
}
