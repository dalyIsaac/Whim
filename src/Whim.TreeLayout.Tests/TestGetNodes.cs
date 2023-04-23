using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestGetNodes
{
	private readonly TestTree _tree = new();

	[Fact]
	public void GetLeftMostLeaf_Left()
	{
		LeafNode? node = _tree.Root.LeftMostLeaf;

		Assert.Equal(_tree.Left, node);
	}

	[Fact]
	public void GetLeftMostLeaf_Right()
	{
		LeafNode? node = _tree.Right.LeftMostLeaf;

		Assert.Equal(_tree.RightTopLeftTop, node);
	}

	[Fact]
	public void GetLeftMostLeaf_EmptySplitNode()
	{
		SplitNode node = new();
		Assert.Null(node.LeftMostLeaf);
	}

	[Fact]
	public void GetFirstWindow()
	{
		TestTreeEngineMocks engine = new();
		Assert.NotNull(engine.Engine.GetFirstWindow());
	}

	[Fact]
	public void GetFirstWindow_Null()
	{
		TestTreeEngineEmptyMocks emptyEngine = new();
		Assert.Null(emptyEngine.Engine.GetFirstWindow());
	}

	[Fact]
	public void GetRightMostLeaf()
	{
		LeafNode? node = _tree.Right.RightMostLeaf;

		Assert.Equal(_tree.RightBottom, node);
	}

	[Fact]
	public void GetRightMostLeaf_EmptySplitNode()
	{
		SplitNode node = new();
		Assert.Null(node.RightMostLeaf);
	}
}
