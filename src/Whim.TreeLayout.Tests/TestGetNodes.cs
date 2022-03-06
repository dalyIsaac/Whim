using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestGetNodes
{
	private readonly TestTree _tree = new();

	[Fact]
	public void GetLeftMostLeaf_Left()
	{
		WindowNode? node = _tree.Root.GetLeftMostLeaf();

		Assert.Equal(_tree.Left, node);
	}

	[Fact]
	public void GetLeftMostLeaf_Right()
	{
		WindowNode? node = _tree.Right.GetLeftMostLeaf();

		Assert.Equal(_tree.RightTopLeftTop, node);
	}

	[Fact]
	public void GetRightMostLeaf()
	{
		WindowNode? node = _tree.Right.GetRightMostLeaf();

		Assert.Equal(_tree.RightBottom, node);
	}
}
