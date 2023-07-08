using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Whim.ImmutableTreeLayout;

internal record NodeState(Node Node, ILocation<int> Location, WindowSize WindowSize);

internal static class TreeHelpers
{
	/// <summary>
	/// Returns <see langword="true"/> if the direction indicates that a
	/// newly added node would be placed after the currently focused node.
	/// </summary>
	public static bool IsPositiveIndex(this Direction direction) => direction is Direction.Right or Direction.Down;

	/// <summary>
	/// Gets the node at the given path in the tree, and its parent nodes.
	/// </summary>
	/// <param name="root">The root node of the tree.</param>
	/// <param name="path">The path to the node.</param>
	/// <returns></returns>
	public static (SplitNode[] SplitNodes, LeafNode LeafNode, ILocation<double> Rectangle)? GetNodeAtPath(
		this Node root,
		ImmutableArray<int> path
	)
	{
		Location<double> location = new() { Height = 1, Width = 1 };
		SplitNode[] splitNodes = new SplitNode[path.Length - 1];

		Node currentNode = root;
		for (int idx = 0; idx < path.Length; idx++)
		{
			int index = path[idx];
			if (idx == path.Length - 1)
			{
				if (currentNode is not LeafNode leafNode)
				{
					Logger.Error($"Expected leaf node at index {idx} of path {path}");
					return null;
				}

				return (splitNodes, leafNode, location);
			}

			if (currentNode is not SplitNode splitNode)
			{
				Logger.Error($"Expected split node at index {idx} of path {path}");
				return null;
			}

			splitNodes[idx] = splitNode;
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

		Logger.Error($"Expected leaf node at end of path {path}");
		return null;
	}

	/// <summary>
	/// Gets the right-most child <see cref="LeafNode"/> of the given <see cref="SplitNode"/>.
	/// </summary>
	/// <param name="splitNode"></param>
	/// <returns></returns>
	public static (SplitNode[] Ancestors, ImmutableArray<int> Path, LeafNode LeafNode) GetRightMostLeaf(
		this SplitNode splitNode
	)
	{
		List<SplitNode> splitNodes = new();
		ImmutableArray<int>.Builder pathBuilder = ImmutableArray.CreateBuilder<int>();

		Node currentNode = splitNode;
		while (currentNode is SplitNode split)
		{
			splitNodes.Add(split);
			pathBuilder.Add(split.Children.Count - 1);
			currentNode = split.Children[^1];
		}

		if (currentNode is not LeafNode leaf)
		{
			Logger.Error($"Expected leaf node at end of path");
			return default;
		}

		return (splitNodes.ToArray(), pathBuilder.ToImmutable(), leaf);
	}

	/// <summary>
	/// Gets the leaf node containing the given point, or <see langword="null"/> if the point is not
	/// inside the root location.
	/// </summary>
	/// <param name="rootNode">The root node of the tree.</param>
	/// <param name="searchPoint">The point to search for.</param>
	/// <returns></returns>
	public static (
		SplitNode[] Ancestors,
		ImmutableArray<int> Path,
		LeafNode LeafNode,
		Direction Direction
	)? GetNodeContainingPoint(this SplitNode rootNode, IPoint<double> searchPoint)
	{
		ILocation<double> rootRectangle = new Location<double>
		{
			X = 0,
			Y = 0,
			Width = 1,
			Height = 1
		};

		if (!rootRectangle.IsPointInside(searchPoint))
		{
			Logger.Error($"Search point {searchPoint} is not inside root location {rootRectangle}");
			return null;
		}

		List<SplitNode> splitNodes = new();
		ImmutableArray<int>.Builder pathBuilder = ImmutableArray.CreateBuilder<int>();

		Node currentNode = rootNode;

		Location<double> childLocation =
			new()
			{
				X = rootRectangle.X,
				Y = rootRectangle.Y,
				Width = rootRectangle.Width,
				Height = rootRectangle.Height
			};

		while (currentNode is SplitNode split)
		{
			foreach ((double weight, Node child) in split)
			{
				// Scale the width/height of the child.
				if (split.IsHorizontal)
				{
					childLocation.Width = rootRectangle.Width * weight;
				}
				else
				{
					childLocation.Height = rootRectangle.Height * weight;
				}

				if (childLocation.IsPointInside(searchPoint))
				{
					splitNodes.Add(split);
					pathBuilder.Add(split.Children.IndexOf(child));
					currentNode = child;
					break;
				}
			}
		}

		if (currentNode is not LeafNode leaf)
		{
			Logger.Error($"Expected leaf node at end of path");
			return default;
		}

		// Get the direction of the search point relative to the leaf node.
		Direction direction = rootRectangle.GetDirectionToPoint(searchPoint);

		return (splitNodes.ToArray(), pathBuilder.ToImmutable(), leaf, direction);
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
		// 1. y = x * width
		// 2. y = height - x * width

		//        (0, 0)            (width, 0)
		//             *---------------* eq2
		//             | *    top    *
		//             |   *       *
		//             |     *   *
		//             | left  *   right
		//             |     *   *
		//             |   *       *
		//             | *           *
		// (height, 0) *     bottom    * eq1
		//
		double xw = rectangle.X * rectangle.Width;

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

		bool isAboveDiagonal1 = normPoint.Y < y1;
		bool isAboveDiagonal2 = normPoint.Y < y2;

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
	public static IEnumerable<NodeState> GetWindowLocations(this Node node, ILocation<int> location)
	{
		// If the node is a leaf node, then we can return the location, and break.
		if (node is LeafNode)
		{
			yield return new NodeState(node, location, WindowSize.Normal);

			yield break;
		}

		// If the node is not a leaf node, it's a split node.
		SplitNode parent = (SplitNode)node;

		// Perform an in-order traversal of the tree.
		double precedingWeight = 0;
		foreach ((double weight, Node child) in parent)
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

	public static (SplitNode[] Ancestors, ImmutableArray<int> Path, LeafNode LeafNode)? GetAdjacentNode(
		Node rootNode,
		ImmutableArray<int> path,
		Direction direction,
		IMonitor monitor
	)
	{
		// Get the coordinates of the node.
		(SplitNode[], LeafNode, ILocation<double>)? result = rootNode.GetNodeAtPath(path);
		if (result is null)
		{
			Logger.Error($"Failed to find node at path {path}");
			return null;
		}

		(_, _, ILocation<double> nodeLocation) = result.Value;

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

		if (rootNode is LeafNode leafNode)
		{
			return (Array.Empty<SplitNode>(), ImmutableArray<int>.Empty, leafNode);
		}

		if (rootNode is not SplitNode splitNode)
		{
			Logger.Error($"Unknown node type {rootNode.GetType()}");
			return null;
		}

		if (
			splitNode.GetNodeContainingPoint(new Point<double>() { X = x, Y = y }) is

			(
				SplitNode[] adjacentNodeAncestors,
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
}
