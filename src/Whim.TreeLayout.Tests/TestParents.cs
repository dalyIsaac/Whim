using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestParents
{
	[Fact]
	public void GetParents_RootNode()
	{
		TestTree tree = new();

		Assert.Equal(new Node[] { tree.Root }, tree.Root.Lineage);
	}

	[Fact]
	public void GetParents_RightTopLeftBottomRightTop()
	{
		TestTree tree = new();

		Node[] expected = new Node[]
		{
			tree.RightTopLeftBottomRightTop,
			tree.RightTopLeftBottomRight,
			tree.RightTopLeftBottom,
			tree.RightTopLeft,
			tree.RightTop,
			tree.Right,
			tree.Root
		};
		Node[] actual = tree.RightTopLeftBottomRightTop.Lineage.ToArray();
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void GetCommonParent_Left_RightBottom()
	{
		TestTree engine = new();

		Node[] leftAncestors = engine.Left.Lineage.ToArray();
		Node[] rightAncestors = engine.RightBottom.Lineage.ToArray();

		Assert.Equal(engine.Root, Node.GetCommonParent(leftAncestors, rightAncestors));
	}

	[Fact]
	public void GetCommonParent_RightTopLeftBottomRightTop_RightTopRight3()
	{
		TestTree engine = new();

		Node[] rightTopLeftBottomRightTopAncestors = engine.RightTopLeftBottomRightTop.Lineage.ToArray();
		Node[] rightTopRight3Ancestors = engine.RightTopRight3.Lineage.ToArray();

		Assert.Equal(
			engine.RightTop,
			Node.GetCommonParent(rightTopLeftBottomRightTopAncestors, rightTopRight3Ancestors)
		);
	}

	[Fact]
	public void GetCommonParent_Left_IllegalNode()
	{
		TestTree engine = new();

		Node[] leftAncestors = engine.Left.Lineage.ToArray();
		Node[] illegalAncestors = new[] { new SplitNode(isHorizontal: true) };

		Assert.Null(Node.GetCommonParent(leftAncestors, illegalAncestors));
	}

	[Fact]
	public void GetCommonParent_Left_Empty()
	{
		TestTree engine = new();

		Node[] leftAncestors = engine.Left.Lineage.ToArray();
		Node[] emptyAncestors = Array.Empty<Node>();

		Assert.Null(Node.GetCommonParent(leftAncestors, emptyAncestors));
	}

	[Fact]
	public void GetCommonParent_Empty_Right()
	{
		TestTree engine = new();

		Node[] emptyAncestors = Array.Empty<Node>();
		Node[] rightAncestors = engine.Right.Lineage.ToArray();

		Assert.Null(Node.GetCommonParent(emptyAncestors, rightAncestors));
	}
}
