namespace Whim.ImmutableTreeLayout;

/// <summary>
/// Base class for a node in a tree.
/// </summary>
internal abstract class Node
{
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
}
