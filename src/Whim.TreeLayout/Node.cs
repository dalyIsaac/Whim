using System.Collections.Generic;

namespace Whim.TreeLayout;

public abstract class Node
{
	public SplitNode? Parent { get; internal set; }

	/// <summary>
	/// The lineage of this node (i.e., the path from this node to the root).
	/// This node is returned first, and the root is returned last.
	/// </summary>
	public IEnumerable<Node> GetLineage()
	{
		Node? node = this;
		while (node != null)
		{
			yield return node;
			node = node.Parent;
		}
	}

	/// <summary>
	/// Gets the left-most node in the tree. Left-most refers to the node's
	/// position in the tree, not the position in the screen.
	/// </summary>
	public LeafNode? GetLeftMostLeaf()
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

	/// <summary>
	/// Gets the right-most node in the tree. Right-most refers to the node's
	/// position in the tree, not the position in the screen.
	/// </summary>
	public LeafNode? GetRightMostLeaf()
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

	public int GetDepth()
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

	public static SplitNode? GetCommonParent(Node[] aParents, Node[] bParents)
	{
		if (aParents.Length == 0 || bParents.Length == 0)
		{
			return null;
		}

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

		if (aIdx < -1 || bIdx < -1 || aIdx == aParents.Length - 1 || bIdx == bParents.Length - 1)
		{
			return null;
		}

		if (aParents[aIdx + 1] is SplitNode aSplitNode)
		{
			return aSplitNode;
		}
		return null;
	}
}
