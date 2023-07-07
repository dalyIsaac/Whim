using System.Collections.Generic;
using System.Collections.Immutable;

namespace Whim.ImmutableTreeLayout;

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
	public static (SplitNode[] SplitNodes, LeafNode LeafNode)? GetNodeAtPath(this Node root, ImmutableArray<int> path)
	{
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

				return (splitNodes, leafNode);
			}

			if (currentNode is not SplitNode splitNode)
			{
				Logger.Error($"Expected split node at index {idx} of path {path}");
				return null;
			}

			splitNodes[idx] = splitNode;
			currentNode = splitNode.Children[index];
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
}
