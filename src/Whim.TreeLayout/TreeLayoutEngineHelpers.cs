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

		foreach (Node child in parent.Children)
		{
			// Set up the width/height of the child.
			if (parent.Direction == NodeDirection.Right)
			{
				childLocation.Width = child.Weight * rootLocation.Width;
			}
			else if (parent.Direction == NodeDirection.Down)
			{
				childLocation.Height = child.Weight * rootLocation.Height;
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
		(double weight, double precedingWeight) = GetWeightAndIndex(parent, node);

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
	/// Gets the weight and index of a node within its parent.
	/// </summary>
	/// <param name="parent">The parent node.</param>
	/// <param name="node">The node to get the weight and index for.</param>
	public static (double weight, double precedingWeight) GetWeightAndIndex(SplitNode parent, Node node)
	{
		int idx = parent.Children.IndexOf(node);
		double weight, precedingWeight;

		if (parent.EqualWeight)
		{
			weight = 1d / parent.Children.Count;
			precedingWeight = idx * weight;
		}
		else
		{
			weight = node.Weight;
			precedingWeight = parent.Children.Take(idx).Sum(child => child.Weight);
		}

		return (weight, precedingWeight);
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
		foreach (Node child in parent.Children)
		{
			double weight = parent.EqualWeight ? 1d / parent.Children.Count : child.Weight;

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

	public static LeafNode? GetLeftMostLeaf(Node? root)
	{
		if (root == null)
		{
			return null;
		}

		Node node = root;

		while (node is SplitNode splitNode)
		{
			if (splitNode.Children.Count == 0)
			{
				return null;
			}

			node = splitNode.Children[0];
		}

		return (LeafNode)node;
	}

	public static LeafNode? GetRightMostLeaf(Node? root)
	{
		if (root == null)
		{
			return null;
		}

		Node node = root;

		while (node is SplitNode splitNode)
		{
			if (splitNode.Children.Count == 0)
			{
				return null;
			}

			node = splitNode.Children[^1];
		}

		return (LeafNode)node;
	}

	public static SplitNode? GetCommonParent(Node a, Node b)
	{
		HashSet<SplitNode> aParents = new(GetParents(a));

		SplitNode? bParent = b.Parent;
		while (bParent != null && !aParents.Contains(bParent))
		{
			bParent = bParent.Parent;
		}

		return bParent as SplitNode;
	}

	public static SplitNode[] GetParents(Node? node)
	{
		List<SplitNode> parents = new();

		node = node?.Parent;
		while (node != null)
		{
			if (node is SplitNode splitNode)
			{
				parents.Add(splitNode);
				node = splitNode.Parent;
			}
			else
			{
				break;
			}
		}

		return parents.ToArray();
	}
}
