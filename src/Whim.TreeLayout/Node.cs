using System.Collections.Generic;

namespace Whim.TreeLayout;

/// <summary>
/// Base class for a node in a tree.
/// </summary>
internal abstract class Node
{
	/// <summary>
	/// The parent of this <see cref="Node"/>.
	/// This is only <see langword="null"/> for the root node.
	/// </summary>
	public SplitNode? Parent { get; internal set; }

	/// <summary>
	/// The lineage of this node (i.e., the path from this node to the root).
	/// This node is returned first, and the root is returned last.
	/// I.e., if as an array: <code>new[] { this, ..., Parent, ..., Root }</code>.
	/// </summary>
	public IEnumerable<Node> Lineage
	{
		get
		{
			Node? node = this;
			while (node != null)
			{
				yield return node;
				node = node.Parent;
			}
		}
	}

	/// <summary>
	/// Gets the left-most node in the tree. Left-most refers to the node's
	/// position in the tree, not necessarily the position in the screen.
	/// </summary>
	/// <returns>The left-most node in the tree.</returns>
	public LeafNode? LeftMostLeaf
	{
		get
		{
			Node node = this;

			while (node is SplitNode splitNode)
			{
				if (splitNode.Count == 0)
				{
					return null;
				}

				(double _, node) = splitNode[0];
			}

			return (LeafNode)node;
		}
	}

	/// <summary>
	/// Gets the right-most node in the tree. Right-most refers to the node's
	/// position in the tree, not necessarily the position in the screen.
	/// </summary>
	/// <returns>The right-most node in the tree.</returns>
	public LeafNode? RightMostLeaf
	{
		get
		{
			Node node = this;

			while (node is SplitNode splitNode)
			{
				if (splitNode.Count == 0)
				{
					return null;
				}

				(double _, node) = splitNode[^1];
			}

			return (LeafNode)node;
		}
	}

	/// <summary>
	/// Get the depth of this node in the tree, where the root has depth of 0.
	/// </summary>
	/// <returns>The depth of this node.</returns>
	public int Depth
	{
		get
		{
			int depth = 0;
			Node? node = Parent;
			while (node != null)
			{
				depth++;
				node = node.Parent;
			}

			return depth;
		}
	}

	/// <summary>
	/// Gets the deepest common parent of the lineage of <paramref name="aParents"/> and <paramref name="bParents"/>.
	/// Deepest, where you get deeper the further away from the root.
	///
	/// The lineages should match <see cref="Lineage"/>, where the root is the last node in the array.
	/// I.e., <code>new[] { aNode, ..., Parent, ..., Root }</code>.
	/// </summary>
	/// <param name="aParents">The lineage of the first node.</param>
	/// <param name="bParents">The lineage of the second node.</param>
	/// <returns>The deepest common parent of the two node lineages.</returns>
	public static SplitNode? GetCommonParent(Node[] aParents, Node[] bParents)
	{
		if (aParents.Length == 0 || bParents.Length == 0)
		{
			return null;
		}

		// Start at the root, and work our way down.
		int aIdx = aParents.Length - 1;
		int bIdx = bParents.Length - 1;

		while (aIdx >= 0 && bIdx >= 0)
		{
			if (aParents[aIdx] != bParents[bIdx])
			{
				break;
			}

			aIdx--;
			bIdx--;
		}

		// If either of the indices is negative, then there is no common parent, as we've gone past the root.
		if (aIdx < -1 || bIdx < -1 || aIdx == aParents.Length - 1 || bIdx == bParents.Length - 1)
		{
			return null;
		}

		// Otherwise, check that the parent is a split node. Since we want the parent (<see cref="SplitNode"/>),
		// we don't consider a leaf node to be its own parent.
		if (aParents[aIdx + 1] is SplitNode aSplitNode)
		{
			return aSplitNode;
		}
		return null;
	}
}
