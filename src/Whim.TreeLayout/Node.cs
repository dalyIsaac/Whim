using System.Collections.Generic;

namespace Whim.TreeLayout;

public abstract class Node
{
	public SplitNode? Parent { get; set; }

	private double _weight = 1;
	public double Weight
	{
		get => Parent?.EqualWeight == true ? 1d / Parent.Children.Count : _weight;
		set
		{
			_weight = value;
			if (Parent != null)
			{
				Parent.EqualWeight = false;
			}
		}
	}

	/// <summary>
	/// The lineage of this node (i.e., the path from this node to the root).
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

	public LeafNode? GetLeftMostLeaf()
	{
		Node node = this;

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

	public LeafNode? GetRightMostLeaf()
	{
		Node node = this;

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

	public static Node? GetCommonParent(Node[] aParents, Node[] bParents)
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

		return aParents[aIdx + 1];
	}
}
