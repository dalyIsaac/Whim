using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestGetNodes
{
	private readonly TestTree _tree = new();

	[Fact]
	public void GetLeftMostLeaf()
	{
		LeafNode? node = _tree.Left.GetLeftMostLeaf();

		Assert.Equal(_tree.Left, node);
	}

	[Fact]
	public void GetRightMostLeaf()
	{
		LeafNode? node = _tree.RightBottom.GetRightMostLeaf();

		Assert.Equal(_tree.RightBottom, node);
	}
}
