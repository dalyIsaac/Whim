using System;
using System.Collections.Generic;
using System.Linq;

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
			if (parent.Direction == NodeDirection.Right)
			{
				childLocation.Width = weight * rootLocation.Width;
			}
			else if (parent.Direction == NodeDirection.Down)
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
			if (parent.Direction == NodeDirection.Right)
			{
				childLocation.X += childLocation.Width;
			}
			else if (parent.Direction == NodeDirection.Down)
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
		if (parent.Direction == NodeDirection.Right)
		{
			location.X *= weight;
			location.X += precedingWeight;
			location.Width *= weight;
		}
		else if (parent.Direction == NodeDirection.Down)
		{
			location.Y *= weight;
			location.Y += precedingWeight;
			location.Height *= weight;
		}

		return GetNodeLocation(parent, location);
	}

	/// <summary>
	/// Gets the <see cref="WindowLocation"/> for all windows, within the unit square.
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<IWindowLocation> DoLayout(Node node, ILocation<int> location)
	{
		// If the node is a leaf node, then we can return the location, and break.
		if (node is LeafNode leafNode)
		{
			yield return new WindowLocation(leafNode.Window, location, WindowState.Normal);
			yield break;
		}

		// If the node is not a leaf node, it's a split node.
		SplitNode parent = (SplitNode)node;

		// Perform an in-order traversal of the tree.
		double precedingWeight = 0;
		foreach ((double weight, Node child) in parent)
		{
			Location childLocation = new(
				x: location.X,
				y: location.Y,
				width: location.Width,
				height: location.Height
			);

			// NOTE: We assume that NodeDirection is always either Right or Bottom.
			if (parent.Direction == NodeDirection.Right)
			{
				childLocation.X += Convert.ToInt32(precedingWeight * location.Width);
				childLocation.Width = Convert.ToInt32(weight * location.Width);
			}
			else
			{
				childLocation.Y += Convert.ToInt32(precedingWeight * location.Height);
				childLocation.Height = Convert.ToInt32(weight * location.Height);
			}

			foreach (IWindowLocation childLocationResult in DoLayout(child, childLocation))
			{
				yield return childLocationResult;
			}

			precedingWeight += weight;
		}
	}
}
