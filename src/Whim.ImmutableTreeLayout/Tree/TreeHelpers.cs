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
}
