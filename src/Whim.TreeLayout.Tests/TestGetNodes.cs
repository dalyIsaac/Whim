using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestGetNodes
{
	private readonly TestTree _tree = new();

	[Fact]
	public void GetLeftMostLeaf_Left()
	{
		LeafNode? node = _tree.Root.GetLeftMostLeaf();

		Assert.Equal(_tree.Left, node);
	}

	[Fact]
	public void GetLeftMostLeaf_Right()
	{
		LeafNode? node = _tree.Right.GetLeftMostLeaf();

		Assert.Equal(_tree.RightTopLeftTop, node);
	}

	[Fact]
	public void GetLeftMostLeaf_EmptySplitNode()
	{
		SplitNode node = new();
		Assert.Null(node.GetLeftMostLeaf());
	}

	[Fact]
	public void GetRightMostLeaf()
	{
		LeafNode? node = _tree.Right.GetRightMostLeaf();

		Assert.Equal(_tree.RightBottom, node);
	}

	[Fact]
	public void GetRightMostLeaf_EmptySplitNode()
	{
		SplitNode node = new();
		Assert.Null(node.GetRightMostLeaf());
	}
}
