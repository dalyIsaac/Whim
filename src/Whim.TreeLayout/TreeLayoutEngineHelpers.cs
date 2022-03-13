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
	/// <param name="originalNode">
	/// The leaf node to search for. The returned node cannot be the same as this node.
	/// </param>
	/// <returns>The node which contains the given <paramref name="searchPoint"/>.</returns>
	public static LeafNode? GetNodeContainingPoint(Node root,
												ILocation<double> rootLocation,
												ILocation<double> searchPoint,
												LeafNode originalNode)
	{
		if (root is LeafNode leaf)
		{
			return leaf == originalNode ? null : leaf;
		}

		if (root is not SplitNode splitNode)
		{
			return null;
		}

		SplitNode parent = splitNode;

		NodeLocation childLocation = new(rootLocation);

		foreach ((double weight, Node child) in parent)
		{
			// Set up the width/height of the child.
			if (parent.IsHorizontal)
			{
				childLocation.Width = weight * rootLocation.Width;
			}
			else
			{
				childLocation.Height = weight * rootLocation.Height;
			}

			if (childLocation.IsPointInside(searchPoint.X, searchPoint.Y))
			{
				LeafNode? result = GetNodeContainingPoint(root: child,
											  rootLocation: childLocation,
											  searchPoint: searchPoint,
											  originalNode: originalNode);
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
	/// <returns>Location of the node. Used for recursion.</returns>
	public static ILocation<double> GetNodeLocation(Node node, NodeLocation? location = null)
	{
		if (location == null)
		{
			location = new NodeLocation() { X = 0, Y = 0, Width = 1, Height = 1 };
		}

		if (node.Parent == null)
		{
			return location;
		}

		SplitNode parent = node.Parent;
		var result = parent.GetWeightAndPrecedingWeight(node);

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
