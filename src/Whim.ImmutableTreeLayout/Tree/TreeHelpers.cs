using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Whim.ImmutableTreeLayout;

/// <summary>
/// The state of a node.
/// </summary>
/// <param name="Node">The node.</param>
/// <param name="Ancestors">The ancestors of the node.</param>
/// <param name="Location">The location of the node.</param>
internal record NodeState(INode Node, IReadOnlyList<ISplitNode> Ancestors, ILocation<double> Location);

/// <summary>
/// The state of a leaf node.
/// </summary>
/// <param name="LeafNode">The leaf node.</param>
/// <param name="Ancestors">The ancestors of the node.</param>
/// <param name="Path">The path to the node in the tree/ancestors.</param>
internal record LeafNodeState(LeafNode LeafNode, IReadOnlyList<ISplitNode> Ancestors, ImmutableArray<int> Path);

/// <summary>
/// The state of a node at a point.
/// </summary>
/// <param name="LeafNode">The leaf node.</param>
/// <param name="Ancestors">The ancestors of the node.</param>
/// <param name="Path">The path to the node in the tree/ancestors.</param>
/// <param name="Direction">The direction of the node from the point of interest.</param>
internal record LeafNodeStateAtPoint(
	LeafNode LeafNode,
	ImmutableArray<ISplitNode> Ancestors,
	ImmutableArray<int> Path,
	Direction Direction
);

/// <summary>
/// The location and size of the window in a monitor, using the monitor coordinate space.
/// </summary>
/// <param name="LeafNode">The leaf node.</param>
/// <param name="Location">The location of the window.</param>
/// <param name="WindowSize">The <see cref="WindowSize"/>.</param>
internal record LeafNodeWindowLocationState(LeafNode LeafNode, ILocation<int> Location, WindowSize WindowSize);

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
	public static NodeState? GetNodeAtPath(this INode root, IReadOnlyList<int> path)
	{
		Location<double> location = Location.UnitSquare<double>();
		if (path.Count == 0)
		{
			return new NodeState(root, Array.Empty<ISplitNode>(), location);
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

		return new NodeState(currentNode, ancestors, location);
	}

	/// <summary>
	/// Gets the right-most child <see cref="LeafNode"/> of the given <see cref="ISplitNode"/>.
	/// </summary>
	/// <param name="ISplitNode"></param>
	/// <returns></returns>
	public static LeafNodeState GetRightMostLeaf(this ISplitNode ISplitNode)
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
		return new LeafNodeState((LeafNode)currentNode, splitNodes, pathBuilder.ToImmutable());
	}

	/// <summary>
	/// Gets the left-most child <see cref="LeafNode"/> of the given <see cref="ISplitNode"/>.
	/// </summary>
	/// <param name="rootSplitNode"></param>
	/// <returns></returns>
	public static LeafNodeState GetLeftMostLeaf(this ISplitNode rootSplitNode)
	{
		List<ISplitNode> splitNodes = new();
		List<int> pathBuilder = new();

		INode currentNode = rootSplitNode;
		while (currentNode is ISplitNode split)
		{
			splitNodes.Add(split);
			pathBuilder.Add(0);
			currentNode = split.Children[0];
		}

		// NOTE: This assumes that leaf nodes are always at the end of the path.
		return new LeafNodeState((LeafNode)currentNode, splitNodes, pathBuilder.ToImmutableArray());
	}

	/// <summary>
	/// Gets the leaf node containing the given point, or <see langword="null"/> if the point is not
	/// inside the root location.
	/// </summary>
	/// <param name="rootNode">The root node of the tree.</param>
	/// <param name="searchPoint">The point to search for.</param>
	/// <returns></returns>
	public static LeafNodeStateAtPoint? GetNodeContainingPoint(this INode rootNode, IPoint<double> searchPoint)
	{
		ILocation<double> parentLocation = Location.UnitSquare<double>();
		if (!parentLocation.ContainsPoint(searchPoint))
		{
			return null;
		}

		if (rootNode is LeafNode rootLeafNode)
		{
			return new LeafNodeStateAtPoint(
				rootLeafNode,
				ImmutableArray.Create<ISplitNode>(),
				ImmutableArray.Create<int>(),
				parentLocation.GetDirectionToPoint(searchPoint)
			);
		}

		if (rootNode is not SplitNode rootSplitNode)
		{
			Logger.Error($"Unknown node type {rootNode.GetType()}");
			return null;
		}

		ImmutableArray<int>.Builder pathBuilder = ImmutableArray.CreateBuilder<int>();
		ImmutableArray<ISplitNode>.Builder ancestorsBuilder = ImmutableArray.CreateBuilder<ISplitNode>();

		ancestorsBuilder.Add(rootSplitNode);

		// Go to the node containing the searchPoint.
		INode parent = rootSplitNode;
		while (true)
		{
			if (parent is not ISplitNode parentSplitNode)
			{
				Logger.Error(
					$"The root node contains the point, but could not find the leaf node containing the point."
				);
				return null;
			}

			bool foundChild = false;

			double deltaX = 0;
			double deltaY = 0;
			for (int idx = 0; idx < parentSplitNode.Children.Count; idx++)
			{
				double weight = parentSplitNode.Weights[idx];
				INode child = parentSplitNode.Children[idx];
				Location<double> childLocation = new(parentLocation);
				childLocation.X += deltaX;
				childLocation.Y += deltaY;

				// Scale the width/height of the child.
				if (parentSplitNode.IsHorizontal)
				{
					childLocation.Width = weight * parentLocation.Width;
				}
				else
				{
					childLocation.Height = weight * parentLocation.Height;
				}

				if (!childLocation.ContainsPoint(searchPoint))
				{
					// Since it wasn't a match, update the position of the child.
					if (parentSplitNode.IsHorizontal)
					{
						deltaX += childLocation.Width;
					}
					else
					{
						deltaY += childLocation.Height;
					}
					continue;
				}

				pathBuilder.Add(idx);

				// We found a split node containing the searchPoint.
				if (child is ISplitNode splitNode)
				{
					foundChild = true;
					ancestorsBuilder.Add(splitNode);
					parent = splitNode;
					parentLocation = childLocation;
					break;
				}

				if (child is LeafNode leafNode)
				{
					return new LeafNodeStateAtPoint(
						leafNode,
						ancestorsBuilder.ToImmutable(),
						pathBuilder.ToImmutable(),
						childLocation.GetDirectionToPoint(searchPoint)
					);
				}
			}

			if (!foundChild)
			{
				Logger.Error(
					$"The root node contains the point, but could not find the leaf node containing the point."
				);
				return null;
			}
		}
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
		// 1. y = x_p * (width_r / height_r)
		// 2. y = height_r - x_p * (width_r / height_r)
		// where p is the point and r is the rectangle.

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

		// Adjust the point to be relative to the origin.
		IPoint<double> normPoint = new Point<double>
		{
			X = searchPoint.X - rectangle.X,
			Y = searchPoint.Y - rectangle.Y
		};

		// Equations for the diagonals.
		double grad = rectangle.Width / rectangle.Height;

		double y1 = normPoint.X * grad;
		double y2 = rectangle.Height - y1;

		// Check if the point is above the diagonals.
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
	public static IEnumerable<LeafNodeWindowLocationState> GetWindowLocations(this INode node, ILocation<int> location)
	{
		// If the node is a leaf node, then we can return the location, and break.
		if (node is LeafNode leafNode)
		{
			yield return new LeafNodeWindowLocationState(leafNode, location, WindowSize.Normal);
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

			foreach (LeafNodeWindowLocationState childLocationResult in GetWindowLocations(child, childLocation))
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
	public static LeafNodeStateAtPoint? GetAdjacentNode(
		INode rootNode,
		IReadOnlyList<int> pathToNode,
		Direction direction,
		IMonitor monitor
	)
	{
		// If the root node is a leaf node, then we can't find an adjacent node.
		if (rootNode is LeafNode)
		{
			return null;
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

		return GetAdjacentNode(rootSplitNode, direction, monitor, result.Location);
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
	public static LeafNodeStateAtPoint? GetAdjacentNode(
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
		if (rootSplitNode.GetNodeContainingPoint(new Point<double>() { X = x, Y = y }) is LeafNodeStateAtPoint state)
		{
			return state;
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
	/// <returns>-1 if the lists are empty, or if they have no common ancestor.</returns>
	public static int GetLastCommonAncestorIndex<T>(IReadOnlyList<T> list1, IReadOnlyList<T> list2)
	{
		if (list1.Count == 0 || list2.Count == 0)
		{
			return -1;
		}

		// Start at the root node, and work our way down.
		int max = Math.Min(list1.Count, list2.Count);
		int idx = 0;

		for (; idx < max; idx++)
		{
			if (!list1[idx]!.Equals(list2[idx]))
			{
				return idx - 1;
			}
		}

		return idx - 1;
	}

	/// <summary>
	/// Returns a new <see cref="ImmutableDictionary{TKey, TValue}"/> with the paths updated to reflect
	/// the new tree.
	/// </summary>
	/// <param name="windowPaths">The old window paths.</param>
	/// <param name="pathToChangedNode">The path to the node that changed.</param>
	/// <param name="root">The root node of the tree.</param>
	/// <returns>A new <see cref="ImmutableDictionary{TKey, TValue}"/> with the paths updated.</returns>
	public static ImmutableDictionary<IWindow, ImmutableArray<int>> CreateUpdatedPaths(
		ImmutableDictionary<IWindow, ImmutableArray<int>> windowPaths,
		ImmutableArray<int> pathToChangedNode,
		ISplitNode root
	)
	{
		List<(INode, ImmutableArray<int>)> stack = new();
		INode node = root;
		int level = 0;

		// Skip to the node that changed.
		while (level < pathToChangedNode.Length)
		{
			int index = pathToChangedNode[level];

			if (node is not ISplitNode splitNode)
			{
				Logger.Error($"Expected split node at level {level} of path {pathToChangedNode}");
				return windowPaths;
			}

			stack.Add((splitNode, pathToChangedNode.Take(level).ToImmutableArray()));

			node = splitNode.Children[index];
			level++;
		}

		// Add the root node.
		stack.Add((node, pathToChangedNode));

		// Iterate over the tree in-order, and create the updated paths.
		List<KeyValuePair<IWindow, ImmutableArray<int>>> updatedPaths = new();
		while (stack.Count > 0)
		{
			(INode current, ImmutableArray<int> path) = stack[^1];
			stack.RemoveAt(stack.Count - 1);

			if (current is SplitNode splitNode)
			{
				for (int idx = splitNode.Children.Count - 1; idx >= 0; idx--)
				{
					stack.Add((splitNode.Children[idx], path.Add(idx)));
				}
			}
			else if (current is LeafNode leafNode)
			{
				updatedPaths.Add(new KeyValuePair<IWindow, ImmutableArray<int>>(leafNode.Window, path));
			}
		}

		// Return the updated paths.
		return windowPaths.SetItems(updatedPaths);
	}
}
