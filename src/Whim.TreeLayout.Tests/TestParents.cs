using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestParents
{
	[Fact]
	public void GetParents_NullNode()
	{
		Assert.Empty(TreeLayoutEngine.GetParents(null));
	}

	[Fact]
	public void GetParents_RootNode()
	{
		TestTree tree = new();

		Assert.Empty(TreeLayoutEngine.GetParents(tree.Root));
	}

	[Fact]
	public void GetParents_RightTopLeftBottomRightTop()
	{
		TestTree tree = new();

		Node[] expected = new[] { tree.RightTopLeftBottomRight, tree.RightTopLeftBottom, tree.RightTopLeft, tree.RightTop, tree.Right, tree.Root };
		Node[] actual = TreeLayoutEngine.GetParents(tree.RightTopLeftBottomRightTop).ToArray();
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void GetCommonParent_Left_RightBottom()
	{
		TestTree engine = new();

		Assert.Equal(engine.Root, TreeLayoutEngine.GetCommonParent(engine.Left, engine.RightBottom));
	}

	[Fact]
	public void GetCommonParent_RightTopLeftBottomRightTop_RightTopRight3()
	{
		TestTree engine = new();

		Assert.Equal(engine.RightTop, TreeLayoutEngine.GetCommonParent(engine.RightTopLeftBottomRightTop, engine.RightTopRight3));
	}

	[Fact]
	public void GetCommonParent_Left_IllegalNode()
	{
		TestTree engine = new();

		Assert.Null(TreeLayoutEngine.GetCommonParent(engine.Left, new LeafNode(new Mock<IWindow>().Object)));
	}
}
