using System;

namespace Whim.TreeLayout;

/// <summary>
/// The location of a <see cref="Node"/> inside a monitor. The <see cref="NodeLocation"/>
/// is calculated by the <see cref="TreeLayoutEngine"/>, as the tree operates inside
/// in the unit square.
/// The relative location of each <see cref="NodeLocation"/> is dependent on its usage.
/// </summary>
public class NodeLocation : ILocation<double>
{
	/// <summary>
	/// The x-coordinate (top-left corner) of the node.
	/// </summary>
	public double X { get; set; }

	/// <summary>
	/// The y-coordinate (top-left corner) of the node.
	/// </summary>
	public double Y { get; set; }

	/// <summary>
	/// The width of the node. The x-coordinate of the bottom-right corner is
	/// given by <see cref="X"/> + <see cref="Width"/>.
	/// </summary>
	public double Width { get; set; }

	/// <summary>
	/// The height of the node. The y-coordinate of the bottom-right corner is
	/// given by <see cref="Y"/> + <see cref="Height"/>.
	/// </summary>
	public double Height { get; set; }

	/// <summary>
	/// Constructor which sets the location of the node to all zeros.
	/// </summary>
	public NodeLocation() { }

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

	/// <param name="point">The point to check.</param>
	/// <returns>
	/// <see langword="true"/> if the location given by <paramref name="point"/> is inside the location.
	/// </returns>
	public bool IsPointInside(IPoint<double> point) => ILocation<double>.IsPointInside(this, point);

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
