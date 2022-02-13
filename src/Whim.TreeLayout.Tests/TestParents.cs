using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestParents
{
	[Fact]
	public void GetParents_RootNode()
	{
		TestTree tree = new();

		Assert.Equal(new Node[] { tree.Root }, tree.Root.GetLineage());
	}

	[Fact]
	public void GetParents_RightTopLeftBottomRightTop()
	{
		TestTree tree = new();

		Node[] expected = new Node[] { tree.RightTopLeftBottomRightTop, tree.RightTopLeftBottomRight, tree.RightTopLeftBottom, tree.RightTopLeft, tree.RightTop, tree.Right, tree.Root };
		Node[] actual = tree.RightTopLeftBottomRightTop.GetLineage().ToArray();
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void GetCommonParent_Left_RightBottom()
	{
		TestTree engine = new();

		Node[] leftAncestors = engine.Left.GetLineage().ToArray();
		Node[] rightAncestors = engine.RightBottom.GetLineage().ToArray();

		Assert.Equal(engine.Root, Node.GetCommonParent(leftAncestors, rightAncestors));
	}

	[Fact]
	public void GetCommonParent_RightTopLeftBottomRightTop_RightTopRight3()
	{
		TestTree engine = new();

		Node[] rightTopLeftBottomRightTopAncestors = engine.RightTopLeftBottomRightTop.GetLineage().ToArray();
		Node[] rightTopRight3Ancestors = engine.RightTopRight3.GetLineage().ToArray();

		Assert.Equal(engine.RightTop, Node.GetCommonParent(rightTopLeftBottomRightTopAncestors, rightTopRight3Ancestors));
	}

	[Fact]
	public void GetCommonParent_Left_IllegalNode()
	{
		TestTree engine = new();

		Node[] leftAncestors = engine.Left.GetLineage().ToArray();
		Node[] illegalAncestors = new[] { new SplitNode(NodeDirection.Right) };

		Assert.Null(Node.GetCommonParent(leftAncestors, illegalAncestors));
	}
}
