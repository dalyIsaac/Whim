using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestGetNodes
{
	private readonly TestTree _tree = new();

	public TestGetNodes()
	{
		Logger.Initialize();
	}

	[Fact]
	public void GetLeftMostLeaf()
	{
		LeafNode? node = TreeLayoutEngine.GetLeftMostLeaf(_tree.Left);

		Assert.Equal(_tree.Left, node);
	}

	[Fact]
	public void GetRightMostLeaf()
	{
		LeafNode? node = TreeLayoutEngine.GetRightMostLeaf(_tree.RightBottom);

		Assert.Equal(_tree.RightBottom, node);
	}
}
