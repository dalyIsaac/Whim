namespace Whim.TreeLayout;

public partial class TreeLayoutEngine
{
	/// <summary>
	/// Gets the node which contains the given <paramref name="searchPoint"/>.
	/// This works by performing a breadth-first search.
	/// </summary>
	/// <param name="root">The root node to start the search from.</param>
	/// <param name="rootLocation">
	/// The location of the parent node. This is used to calculate the
	/// relative location of the point.
	/// </param>
	/// <param name="searchPoint">The point of the leaf node to search for.</param>
	/// <returns>The node which contains the given <paramref name="searchPoint"/>.</returns>
	internal static LeafNode? GetNodeContainingPoint(
		Node root,
		ILocation<double> rootLocation,
		IPoint<double> searchPoint
	)
	{
		if (root is LeafNode leaf && rootLocation.IsPointInside(searchPoint))
		{
			return leaf;
		}

		if (root is not SplitNode splitNode)
		{
			return null;
		}

		SplitNode parent = splitNode;

		Location<double> childLocation =
			new()
			{
				X = rootLocation.X,
				Y = rootLocation.Y,
				Width = rootLocation.Width,
				Height = rootLocation.Height
			};

		foreach ((double weight, Node child) in parent)
		{
			// Scale the width/height of the child.
			if (parent.IsHorizontal)
			{
				childLocation.Width = weight * rootLocation.Width;
			}
			else
			{
				childLocation.Height = weight * rootLocation.Height;
			}

			if (childLocation.IsPointInside(searchPoint))
			{
				LeafNode? result = GetNodeContainingPoint(
					root: child,
					rootLocation: childLocation,
					searchPoint: searchPoint
				);
				if (result != null)
				{
					return result;
				}
			}

			// Since it wasn't a match, update the position of the child.
			if (parent.IsHorizontal)
			{
				childLocation.X += childLocation.Width;
			}
			else
			{
				childLocation.Y += childLocation.Height;
			}
		}

		return null;
	}

	/// <summary>
	/// Gets a node's location within the unit square. This works by moving up
	/// the tree until the root is reached.
	/// </summary>
	/// <param name="node">The node to get the location for.</param>
	/// <param name="location">A location to scale the node's location to.</param>
	/// <returns>Location of the node. Used for recursion.</returns>
	internal static ILocation<double> GetNodeLocation(Node node, Location<double>? location = null)
	{
		location ??= Location.UnitSquare<double>();

		if (node.Parent == null)
		{
			return location;
		}

		SplitNode parent = node.Parent;
		(double weight, double precedingWeight)? result = parent.GetWeightAndPrecedingWeight(node);

		if (result == null)
		{
			return location;
		}

		double weight = result.Value.weight;
		double precedingWeight = result.Value.precedingWeight;

		// We translate by the preceding weight.
		// We then scale by the weight.
		if (parent.IsHorizontal)
		{
			location.X *= weight;
			location.X += precedingWeight;
			location.Width *= weight;
		}
		else
		{
			location.Y *= weight;
			location.Y += precedingWeight;
			location.Height *= weight;
		}

		return GetNodeLocation(parent, location);
	}
}
