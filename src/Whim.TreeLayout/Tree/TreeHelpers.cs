using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Whim.TreeLayout;

/// <summary>
/// The state of a node.
/// </summary>
/// <param name="Node">The node.</param>
/// <param name="Ancestors">The ancestors of the node.</param>
/// <param name="Rectangle">The rectangle of the node.</param>
internal record NodeState(INode Node, IReadOnlyList<ISplitNode> Ancestors, IRectangle<double> Rectangle);

/// <summary>
/// The state of a window node.
/// </summary>
/// <param name="WindowNode">The window node.</param>
/// <param name="Ancestors">The ancestors of the node.</param>
/// <param name="Path">The path to the node in the tree/ancestors.</param>
internal record WindowNodeState(WindowNode WindowNode, IReadOnlyList<ISplitNode> Ancestors, ImmutableArray<int> Path);

/// <summary>
/// The state of a node at a point.
/// </summary>
/// <param name="WindowNode">The window node.</param>
/// <param name="Ancestors">The ancestors of the node.</param>
/// <param name="Path">The path to the node in the tree/ancestors.</param>
/// <param name="Direction">The direction of the node from the point of interest.</param>
internal record WindowNodeStateAtPoint(
	WindowNode WindowNode,
	ImmutableArray<ISplitNode> Ancestors,
	ImmutableArray<int> Path,
	Direction Direction
);

/// <summary>
/// The rectangle and size of the window in a monitor, using the monitor coordinate space.
/// </summary>
/// <param name="WindowNode">The window node.</param>
/// <param name="Rectangle">The rectangle of the window.</param>
/// <param name="WindowSize">The <see cref="WindowSize"/>.</param>
internal record WindowNodeRectangleState(WindowNode WindowNode, IRectangle<int> Rectangle, WindowSize WindowSize);

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
		Rectangle<double> rect = Rectangle.UnitSquare<double>();
		if (path.Count == 0)
		{
			return new NodeState(root, [], rect);
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
				rect.X += precedingWeight * rect.Width;
				rect.Width = weight * rect.Width;
			}
			else
			{
				rect.Y += precedingWeight * rect.Height;
				rect.Height = weight * rect.Height;
			}
		}

		return new NodeState(currentNode, ancestors, rect);
	}

	/// <summary>
	/// Gets the right-most child <see cref="WindowNode"/> of the given <see cref="ISplitNode"/>.
	/// </summary>
	/// <param name="ISplitNode"></param>
	/// <returns></returns>
	public static WindowNodeState GetRightMostWindow(this ISplitNode ISplitNode)
	{
		List<ISplitNode> splitNodes = [];
		ImmutableArray<int>.Builder pathBuilder = ImmutableArray.CreateBuilder<int>();

		INode currentNode = ISplitNode;
		while (currentNode is ISplitNode split)
		{
			splitNodes.Add(split);
			pathBuilder.Add(split.Children.Count - 1);
			currentNode = split.Children[^1];
		}

		// NOTE: This assumes that window nodes are always at the end of the path.
		return new WindowNodeState((WindowNode)currentNode, splitNodes, pathBuilder.ToImmutable());
	}

	/// <summary>
	/// Gets the left-most child <see cref="WindowNode"/> of the given <see cref="ISplitNode"/>.
	/// </summary>
	/// <param name="rootSplitNode"></param>
	/// <returns></returns>
	public static WindowNodeState GetLeftMostWindow(this ISplitNode rootSplitNode)
	{
		List<ISplitNode> splitNodes = [];
		List<int> pathBuilder = [];

		INode currentNode = rootSplitNode;
		while (currentNode is ISplitNode split)
		{
			splitNodes.Add(split);
			pathBuilder.Add(0);
			currentNode = split.Children[0];
		}

		// NOTE: This assumes that window nodes are always at the end of the path.
		return new WindowNodeState((WindowNode)currentNode, splitNodes, [.. pathBuilder]);
	}

	/// <summary>
	/// Gets the window node containing the given point, or <see langword="null"/> if the point is not
	/// inside the root rectangle.
	/// </summary>
	/// <param name="rootNode">The root node of the tree.</param>
	/// <param name="searchPoint">The point to search for.</param>
	/// <returns></returns>
	public static WindowNodeStateAtPoint? GetNodeContainingPoint(this INode rootNode, IPoint<double> searchPoint)
	{
		Rectangle<double> parentRect = Rectangle.UnitSquare<double>();
		if (!parentRect.ContainsPoint(searchPoint))
		{
			return null;
		}

		if (rootNode is WindowNode rootWindowNode)
		{
			return new WindowNodeStateAtPoint(rootWindowNode, [], [], parentRect.GetDirectionToPoint(searchPoint));
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
		ISplitNode parent = rootSplitNode;
		while (true)
		{
			bool foundChild = false;

			double deltaX = 0;
			double deltaY = 0;
			for (int idx = 0; idx < parent.Children.Count; idx++)
			{
				double weight = parent.Weights[idx];
				INode child = parent.Children[idx];
				Rectangle<double> childRect = new(parentRect);
				childRect.X += deltaX;
				childRect.Y += deltaY;

				// Scale the width/height of the child.
				if (parent.IsHorizontal)
				{
					childRect.Width = weight * parentRect.Width;
				}
				else
				{
					childRect.Height = weight * parentRect.Height;
				}

				if (!childRect.ContainsPoint(searchPoint))
				{
					// Since it wasn't a match, update the position of the child.
					if (parent.IsHorizontal)
					{
						deltaX += childRect.Width;
					}
					else
					{
						deltaY += childRect.Height;
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
					parentRect = childRect;
					break;
				}

				if (child is WindowNode WindowNode)
				{
					return new WindowNodeStateAtPoint(
						WindowNode,
						ancestorsBuilder.ToImmutable(),
						pathBuilder.ToImmutable(),
						childRect.GetDirectionToPoint(searchPoint)
					);
				}

				Logger.Error($"Unknown node type {child.GetType()}");
				return null;
			}

			if (!foundChild)
			{
				Logger.Error(
					$"The root node contains the point, but could not find the window node containing the point."
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
	public static Direction GetDirectionToPoint(this IRectangle<double> rectangle, IPoint<double> searchPoint)
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
			Y = searchPoint.Y - rectangle.Y,
		};

		// Equations for the diagonals.
		double grad = rectangle.Height / rectangle.Width;

		double y1 = normPoint.X * grad;
		double y2 = rectangle.Height - y1;

		// Check if the point is above the diagonals.
		bool isAboveDiagonal1 = y1 >= normPoint.Y;
		bool isAboveDiagonal2 = y2 >= normPoint.Y;

		// Depending on which diagonal the point is above, we can determine the direction.
		if (isAboveDiagonal1)
		{
			if (isAboveDiagonal2)
			{
				return Direction.Up;
			}

			return Direction.Right;
		}

		// Implicitly below diagonal 1.
		if (isAboveDiagonal2)
		{
			return Direction.Left;
		}

		return Direction.Down;
	}

	/// <summary>
	/// Gets the <see cref="WindowState"/> for all windows, within the unit square.
	/// </summary>
	/// <param name="node">The root node of the tree.</param>
	/// <param name="rectangle">The rectangle of the root node, in monitor coordinates.</param>
	/// <returns></returns>
	public static IEnumerable<WindowNodeRectangleState> GetWindowRectangles(this INode node, IRectangle<int> rectangle)
	{
		// If the node is a window node, then we can return the rectangle, and break.
		if (node is WindowNode WindowNode)
		{
			yield return new WindowNodeRectangleState(WindowNode, rectangle, WindowSize.Normal);
			yield break;
		}

		// If the node is not a window node, it's a split node.
		ISplitNode parent = (ISplitNode)node;

		// Perform an in-order traversal of the tree.
		double precedingWeight = 0;
		foreach ((double weight, INode child) in parent)
		{
			Rectangle<int> childRectangle = new()
			{
				X = rectangle.X,
				Y = rectangle.Y,
				Width = rectangle.Width,
				Height = rectangle.Height,
			};

			if (parent.IsHorizontal)
			{
				childRectangle.X += Convert.ToInt32(precedingWeight * rectangle.Width);
				childRectangle.Width = Convert.ToInt32(weight * rectangle.Width);
			}
			else
			{
				childRectangle.Y += Convert.ToInt32(precedingWeight * rectangle.Height);
				childRectangle.Height = Convert.ToInt32(weight * rectangle.Height);
			}

			foreach (WindowNodeRectangleState childRectangleResult in GetWindowRectangles(child, childRectangle))
			{
				yield return childRectangleResult;
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
	public static WindowNodeStateAtPoint? GetAdjacentWindowNode(
		INode rootNode,
		IReadOnlyList<int> pathToNode,
		Direction direction,
		IMonitor monitor
	)
	{
		// If the root node is a window node, then we can't find an adjacent node.
		if (rootNode is WindowNode)
		{
			return null;
		}

		if (rootNode is not ISplitNode rootSplitNode)
		{
			Logger.Error($"Unknown node type {rootNode.GetType()}");
			return null;
		}

		// Get the coordinates of the node given by the path.
		NodeState? result = rootNode.GetNodeAtPath(pathToNode);
		if (result is null)
		{
			Logger.Error($"Failed to find node at path {pathToNode}");
			return null;
		}

		return GetAdjacentWindowNode(rootSplitNode, direction, monitor, result.Rectangle);
	}

	/// <summary>
	/// Gets the adjacent node to the node at the <paramref name="nodeRectangle"/>, in the given
	/// <paramref name="direction"/>.
	/// </summary>
	/// <param name="rootSplitNode">The root split node of the tree.</param>
	/// <param name="direction">The direction to search in.</param>
	/// <param name="monitor">The monitor that the root node is currently displayed in.</param>
	/// <param name="nodeRectangle">The rectangle of the node, in monitor coordinates.</param>
	/// <returns></returns>
	public static WindowNodeStateAtPoint? GetAdjacentWindowNode(
		ISplitNode rootSplitNode,
		Direction direction,
		IMonitor monitor,
		IRectangle<double> nodeRectangle
	)
	{
		// Next, we figure out the adjacent point of the nodeRectangle.
		double x = nodeRectangle.X;
		double y = nodeRectangle.Y;

		if (direction.HasFlag(Direction.Left))
		{
			x -= 1d / monitor.WorkingArea.Width;
		}
		else if (direction.HasFlag(Direction.Right))
		{
			x += nodeRectangle.Width + (1d / monitor.WorkingArea.Width);
		}

		if (direction.HasFlag(Direction.Up))
		{
			y -= 1d / monitor.WorkingArea.Height;
		}
		else if (direction.HasFlag(Direction.Down))
		{
			y += nodeRectangle.Height + (1d / monitor.WorkingArea.Height);
		}

		// Get the adjacent node (the node containing the point (x, y)).
		if (rootSplitNode.GetNodeContainingPoint(new Point<double>() { X = x, Y = y }) is WindowNodeStateAtPoint state)
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
		List<(INode, ImmutableArray<int>)> stack = [];
		if (windowPaths.IsEmpty)
		{
			stack.Add((root, ImmutableArray<int>.Empty));
		}
		else
		{
			// Skip to the node that changed when the given windowPaths is empty.
			// We only want to update the paths for the subtree that changed.
			INode node = root;
			for (int level = 0; level < pathToChangedNode.Length; level++)
			{
				if (node is not ISplitNode splitNode)
				{
					Logger.Error($"Expected split node at level {level} of path {pathToChangedNode}");
					return windowPaths;
				}

				int index = pathToChangedNode[level];
				node = splitNode.Children[index];
			}
			stack.Add((node, pathToChangedNode));
		}

		// Iterate over the tree in-order, and create the updated paths.
		List<KeyValuePair<IWindow, ImmutableArray<int>>> updatedPaths = [];
		while (stack.Count > 0)
		{
			(INode current, ImmutableArray<int> path) = stack[^1];
			stack.RemoveAt(stack.Count - 1);

			if (current is SplitNode splitNode)
			{
				// Add the children in reverse order, so that they are processed from the stack in the
				// correct order (left to right).
				for (int idx = splitNode.Children.Count - 1; idx >= 0; idx--)
				{
					stack.Add((splitNode.Children[idx], path.Add(idx)));
				}
			}
			else if (current is WindowNode WindowNode)
			{
				updatedPaths.Add(new KeyValuePair<IWindow, ImmutableArray<int>>(WindowNode.Window, path));
			}
		}

		// Return the updated paths.
		return windowPaths.SetItems(updatedPaths);
	}
}
