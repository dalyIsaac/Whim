using Moq;
using Xunit;

namespace Whim.ImmutableTreeLayout.Tests;

public class NodeTests
{
	private class Wrapper
	{
		public SplitNode RootNode { get; }
		public LeafNode LeftNode { get; }
		public LeafNode RightNode { get; }

		public Wrapper()
		{
			LeftNode = new WindowNode(new Mock<IWindow>().Object);
			RightNode = new WindowNode(new Mock<IWindow>().Object);
			RootNode = new SplitNode(LeftNode, RightNode, Direction.Right);
		}
	}

	[Fact]
	public void LeftMostLeaf_EmptySplitNode()
	{
		// Given
		Wrapper wrapper = new();
		SplitNode splitNode = wrapper.RootNode;

		// When
		splitNode = splitNode.Remove(wrapper.LeftNode).Remove(wrapper.RightNode);
		LeafNode? leftMostLeaf = splitNode.LeftMostLeaf;

		// Then
		Assert.Null(leftMostLeaf);
	}

	[Fact]
	public void LeftMostLeaf_LeafNode()
	{
		// Given
		Wrapper wrapper = new();

		// When
		LeafNode? leftMostLeaf = wrapper.RootNode.LeftMostLeaf;

		// Then
		Assert.Equal(wrapper.LeftNode, leftMostLeaf);
	}

	[Fact]
	public void RightMostLeaf_EmptySplitNode()
	{
		// Given
		Wrapper wrapper = new();
		SplitNode splitNode = wrapper.RootNode;

		// When
		splitNode = splitNode.Remove(wrapper.LeftNode).Remove(wrapper.RightNode);
		LeafNode? rightMostLeaf = splitNode.RightMostLeaf;

		// Then
		Assert.Null(rightMostLeaf);
	}

	[Fact]
	public void RightMostLeaf_LeafNode()
	{
		// Given
		Wrapper wrapper = new();

		// When
		LeafNode? rightMostLeaf = wrapper.RootNode.RightMostLeaf;

		// Then
		Assert.Equal(wrapper.RightNode, rightMostLeaf);
	}
}
