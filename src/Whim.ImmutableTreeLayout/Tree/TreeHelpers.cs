using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Whim.ImmutableTreeLayout;

internal record NodeState(INode Node, ILocation<int> Location, WindowSize WindowSize);

internal record NodeAtPointData(
	IReadOnlyList<ISplitNode> Ancestors,
	ImmutableArray<int> Path,
	LeafNode LeafNode,
	Direction Direction
);

internal static class TreeHelpers
{
	/// <summary>
	/// Returns <see langword="true"/> if the direction indicates that a
	/// newly added node would be placed after the currently focused node.
	/// </summary>
	public static bool InsertAfter(this Direction direction) => direction is Direction.Right or Direction.Down;

	/// <summary>
	/// Gets the node at the given path in the tree, and its parent nodes.
	/// </summary>
	/// <param name="root">The root node of the tree.</param>
	/// <param name="path">The path to the node.</param>
	/// <returns></returns>
	public static (ISplitNode[] Ancestors, INode Node, ILocation<double> Location)? GetNodeAtPath(
		this INode root,
		IReadOnlyList<int> path
	)
	{
		Location<double> location = Location.UnitSquare<double>();
		if (path.Count == 0)
		{
			return (Array.Empty<ISplitNode>(), root, location);
		}

		ISplitNode[] ancestors = new ISplitNode[path.Count];

		INode currentNode = root;
		for (int idx = 0; idx < path.Count; idx++)
		{
			int index = path[idx];

			if (currentNode is not ISplitNode splitNode)
			{
				Logger.Error($"Expected split node at index {idx} of path {path}");
				return null;
			}

			ancestors[idx] = splitNode;
			currentNode = splitNode.Children[index];

			// Update the weight.
			double precedingWeight;
			double weight;

			if (splitNode.EqualWeight)
			{
				weight = 1.0 / splitNode.Children.Count;
				precedingWeight = weight * index;
			}
			else
			{
				weight = splitNode.Weights[index];
				precedingWeight = splitNode.Weights.Take(index).Sum();
			}

			if (splitNode.IsHorizontal)
			{
				location.X += precedingWeight * location.Width;
				location.Width = weight * location.Width;
			}
			else
			{
				location.Y += precedingWeight * location.Height;
				location.Height = weight * location.Height;
			}
		}

		return (ancestors, currentNode, location);
	}

	/// <summary>
	/// Gets the right-most child <see cref="LeafNode"/> of the given <see cref="ISplitNode"/>.
	/// </summary>
	/// <param name="ISplitNode"></param>
	/// <returns></returns>
	public static (ISplitNode[] Ancestors, ImmutableArray<int> Path, LeafNode LeafNode) GetRightMostLeaf(
		this ISplitNode ISplitNode
	)
	{
		List<ISplitNode> splitNodes = new();
		ImmutableArray<int>.Builder pathBuilder = ImmutableArray.CreateBuilder<int>();

		INode currentNode = ISplitNode;
		while (currentNode is ISplitNode split)
		{
			splitNodes.Add(split);
			pathBuilder.Add(split.Children.Count - 1);
			currentNode = split.Children[^1];
		}

		// NOTE: This assumes that leaf nodes are always at the end of the path.
		return (splitNodes.ToArray(), pathBuilder.ToImmutable(), (LeafNode)currentNode);
	}

	/// <summary>
	/// Gets the left-most child <see cref="LeafNode"/> of the given <see cref="ISplitNode"/>.
	/// </summary>
	/// <param name="rootSplitNode"></param>
	/// <returns></returns>
	public static (ISplitNode[] Ancestors, ImmutableArray<int> Path, LeafNode LeafNode) GetLeftMostLeaf(
		this ISplitNode rootSplitNode
	)
	{
		List<ISplitNode> splitNodes = new();
		ImmutableArray<int>.Builder pathBuilder = ImmutableArray.CreateBuilder<int>();

		INode currentNode = rootSplitNode;
		while (currentNode is ISplitNode split)
		{
			splitNodes.Add(split);
			pathBuilder.Add(0);
			currentNode = split.Children[0];
		}

		// NOTE: This assumes that leaf nodes are always at the end of the path.
		return (splitNodes.ToArray(), pathBuilder.ToImmutable(), (LeafNode)currentNode);
	}

	/// <summary>
	/// Gets the leaf node containing the given point, or <see langword="null"/> if the point is not
	/// inside the root location.
	/// </summary>
	/// <param name="rootNode">The root node of the tree.</param>
	/// <param name="searchPoint">The point to search for.</param>
	/// <returns></returns>
	public static NodeAtPointData? GetNodeContainingPoint(this INode rootNode, IPoint<double> searchPoint)
	{
		Logger.Debug($"Searching for point {searchPoint} in node {rootNode}");

		InternalNodeAtPointData? internalNodeAtPointData = GetNodeContainingPoint(
			rootNode,
			Location.UnitSquare<double>(),
			searchPoint,
			0
		);

		if (internalNodeAtPointData is null)
		{
			return null;
		}

		return new NodeAtPointData(
			internalNodeAtPointData.Ancestors,
			internalNodeAtPointData.Path.ToImmutableArray(),
			internalNodeAtPointData.LeafNode,
			internalNodeAtPointData.Direction
		);
	}

	private record InternalNodeAtPointData(ISplitNode[] Ancestors, int[] Path, LeafNode LeafNode, Direction Direction);

	private static InternalNodeAtPointData? GetNodeContainingPoint(
		INode root,
		ILocation<double> rootLocation,
		IPoint<double> searchPoint,
		int depth
	)
	{
		if (!rootLocation.ContainsPoint(searchPoint))
		{
			return null;
		}

		if (root is LeafNode leaf)
		{
			return new InternalNodeAtPointData(
				new ISplitNode[depth],
				new int[depth],
				leaf,
				rootLocation.GetDirectionToPoint(searchPoint)
			);
		}

		if (root is not SplitNode splitNode)
		{
			return null;
		}

		Location<double> childLocation = new(rootLocation);
		SplitNode parent = splitNode;

		for (int idx = 0; idx < parent.Children.Count; idx++)
		{
			(double weight, INode child) = parent[idx];

			// Scale the width/height of the child.
			if (parent.IsHorizontal)
			{
				childLocation.Width = weight * rootLocation.Width;
			}
			else
			{
				childLocation.Height = weight * rootLocation.Height;
			}

			InternalNodeAtPointData? result = GetNodeContainingPoint(
				root: child,
				rootLocation: childLocation,
				searchPoint,
				depth + 1
			);

			if (result != null)
			{
				result.Ancestors[depth] = parent;
				result.Path[depth] = idx;
				return result;
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
	/// Gets the direction of the point relative to the center of the given <paramref name="rectangle"/>.
	/// </summary>
	/// <param name="rectangle"></param>
	/// <param name="searchPoint"></param>
	/// <returns></returns>
	public static Direction GetDirectionToPoint(this ILocation<double> rectangle, IPoint<double> searchPoint)
	{
		// We can figure out the direction of the point relative to the rectangle by comparing the
		// point's actual y position compared to points given by the two diagonals of the rectangle.
		//
		// If we move the rectangle back to the origin, then the two diagonals are:
		// 1. y = x_p * width_r
		// 2. y = height_r - x_p * width_r
		// where _p is the point and _r is the rectangle.

		//        (0, 0)            (width, 0)
		//             *---------------* eq2
		//             | *     up    *
		//             |   *       *
		//             |     *   *
		//             | left  *   right
		//             |     *   *
		//             |   *       *
		//             | *           *
		// (height, 0) *     bottom    * eq1
		//
		double xw = searchPoint.X * rectangle.Width;

		// equation 1
		double y1 = xw;

		// equation 2
		double y2 = rectangle.Height - xw;

		// Adjust the point to be relative to the origin.
		IPoint<double> normPoint = new Point<double>
		{
			X = searchPoint.X - rectangle.X,
			Y = searchPoint.Y - rectangle.Y
		};

		bool isAboveDiagonal1 = y1 >= normPoint.Y;
		bool isAboveDiagonal2 = y2 >= normPoint.Y;

		// Depending on which diagonal the point is above, we can determine the direction.
		if (isAboveDiagonal1 && isAboveDiagonal2)
		{
			return Direction.Up;
		}

		if (isAboveDiagonal1 && !isAboveDiagonal2)
		{
			return Direction.Right;
		}

		if (!isAboveDiagonal1 && !isAboveDiagonal2)
		{
			return Direction.Down;
		}

		return Direction.Left;
	}

	/// <summary>
	/// Gets the <see cref="WindowState"/> for all windows, within the unit square.
	/// </summary>
	/// <param name="node">The root node of the tree.</param>
	/// <param name="location">The location of the root node, in monitor coordinates.</param>
	/// <returns></returns>
	public static IEnumerable<NodeState> GetWindowLocations(this INode node, ILocation<int> location)
	{
		// If the node is a leaf node, then we can return the location, and break.
		if (node is LeafNode)
		{
			yield return new NodeState(node, location, WindowSize.Normal);

			yield break;
		}

		// If the node is not a leaf node, it's a split node.
		ISplitNode parent = (ISplitNode)node;

		// Perform an in-order traversal of the tree.
		double precedingWeight = 0;
		foreach ((double weight, INode child) in parent)
		{
			Location<int> childLocation =
				new()
				{
					X = location.X,
					Y = location.Y,
					Width = location.Width,
					Height = location.Height
				};

			if (parent.IsHorizontal)
			{
				childLocation.X += Convert.ToInt32(precedingWeight * location.Width);
				childLocation.Width = Convert.ToInt32(weight * location.Width);
			}
			else
			{
				childLocation.Y += Convert.ToInt32(precedingWeight * location.Height);
				childLocation.Height = Convert.ToInt32(weight * location.Height);
			}

			foreach (NodeState childLocationResult in GetWindowLocations(child, childLocation))
			{
				yield return childLocationResult;
			}

			precedingWeight += weight;
		}
	}

	/// <summary>
	/// Gets the adjacent node to the node at the given <paramref name="pathToNode"/>, in the given
	/// <paramref name="direction"/>.
	/// </summary>
	/// <param name="rootNode">The root node of the tree.</param>
	/// <param name="pathToNode">The path to the node, from the root node.</param>
	/// <param name="direction">The direction to search in.</param>
	/// <param name="monitor">The monitor that the root node is currently displayed in.</param>
	/// <returns></returns>
	public static (ISplitNode[] Ancestors, ImmutableArray<int> Path, LeafNode LeafNode)? GetAdjacentNode(
		INode rootNode,
		IReadOnlyList<int> pathToNode,
		Direction direction,
		IMonitor monitor
	)
	{
		// If the root node is a leaf node, then we can't find an adjacent node.
		if (rootNode is LeafNode leafNode)
		{
			return (Array.Empty<ISplitNode>(), ImmutableArray<int>.Empty, leafNode);
		}

		if (rootNode is not ISplitNode rootSplitNode)
		{
			Logger.Error($"Unknown node type {rootNode.GetType()}");
			return null;
		}

		// Get the coordinates of the node given by the path.
		var result = rootNode.GetNodeAtPath(pathToNode);
		if (result is null)
		{
			Logger.Error($"Failed to find node at path {pathToNode}");
			return null;
		}

		(_, _, ILocation<double> nodeLocation) = result.Value;

		return GetAdjacentNode(rootSplitNode, direction, monitor, nodeLocation);
	}

	/// <summary>
	/// Gets the adjacent node to the node at the <paramref name="nodeLocation"/>, in the given
	/// <paramref name="direction"/>.
	/// </summary>
	/// <param name="rootSplitNode">The root split node of the tree.</param>
	/// <param name="direction">The direction to search in.</param>
	/// <param name="monitor">The monitor that the root node is currently displayed in.</param>
	/// <param name="nodeLocation">The location of the node, in monitor coordinates.</param>
	/// <returns></returns>
	public static (ISplitNode[] Ancestors, ImmutableArray<int> Path, LeafNode LeafNode)? GetAdjacentNode(
		ISplitNode rootSplitNode,
		Direction direction,
		IMonitor monitor,
		ILocation<double> nodeLocation
	)
	{
		// Next, we figure out the adjacent point of the nodeLocation.
		double x = nodeLocation.X;
		double y = nodeLocation.Y;

		if (direction.HasFlag(Direction.Left))
		{
			x -= 1d / monitor.WorkingArea.Width;
		}
		else if (direction.HasFlag(Direction.Right))
		{
			x += nodeLocation.Width + (1d / monitor.WorkingArea.Width);
		}

		if (direction.HasFlag(Direction.Up))
		{
			y -= 1d / monitor.WorkingArea.Height;
		}
		else if (direction.HasFlag(Direction.Down))
		{
			y += nodeLocation.Height + (1d / monitor.WorkingArea.Height);
		}

		// Get the adjacent node (the node containing the point (x, y)).
		if (
			rootSplitNode.GetNodeContainingPoint(new Point<double>() { X = x, Y = y }) is

			(
				ISplitNode[] adjacentNodeAncestors,
				ImmutableArray<int> adjacentNodePath,
				LeafNode adjacentLeafNode,
				Direction
			)
		)
		{
			return (adjacentNodeAncestors, adjacentNodePath, adjacentLeafNode);
		}

		Logger.Error($"Failed to find node containing point {x}, {y}");
		return null;
	}

	/// <summary>
	/// Gets the last index for which the two lists have the same value.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="list1"></param>
	/// <param name="list2"></param>
	/// <returns>-1 if the lists have no common parent.</returns>
	public static int GetLastCommonAncestorIndex<T>(IList<T> list1, IList<T> list2)
	{
		int idx = Math.Min(list1.Count, list2.Count) - 1;

		while (idx >= 0 && !list1[idx]!.Equals(list2[idx]))
		{
			idx--;
		}

		return idx;
	}
}
