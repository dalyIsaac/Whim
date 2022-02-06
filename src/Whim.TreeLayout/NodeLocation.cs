using System;

namespace Whim.TreeLayout;

public class NodeLocation : ILocation<double>
{
	public double X { get; set; }
	public double Y { get; set; }
	public double Width { get; set; }
	public double Height { get; set; }

	public NodeLocation() {}

	/// <summary>
	/// Constructor which creates a copy of the given location.
	/// </summary>
	/// <param name="location">The location to copy.</param>
	public NodeLocation(ILocation<double> location)
	{
		X = location.X;
		Y = location.Y;
		Width = location.Width;
		Height = location.Height;
	}

	public bool IsPointInside(double x, double y) => ILocation<double>.IsPointInside(this, x, y);

	// override object.Equals
	public override bool Equals(object? obj)
	{
		//
		// See the full list of guidelines at
		//   http://go.microsoft.com/fwlink/?LinkID=85237
		// and also the guidance for operator== at
		//   http://go.microsoft.com/fwlink/?LinkId=85238
		//

		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}

		return obj is NodeLocation node &&
			node.X == X &&
			node.Y == Y &&
			node.Width == Width &&
			node.Height == Height;
	}

	// override object.GetHashCode
	public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
}
