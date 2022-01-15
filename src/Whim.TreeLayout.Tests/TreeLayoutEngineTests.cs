using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class Tests
{
	/// <summary>
	/// Returns all the nodes of the following tree, for tests. The tree exists within the coordinates (0,0) to (1,1).
	/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------
	/// |                                                                               |                                     |                                          |
	/// |                                                                               |                                     |                                          |
	/// |                                                                               |                                     |               RightTopRight1             |
	/// |                                                                               |                                     |                                          |
	/// |                                                                               |            RightTopLeftTop          |                                          |
	/// |                                                                               |                                     |                                          |
	/// |                                                                               |                                     r------------------------------------------|
	/// |                                                                               |                                     i                                          |
	/// |                                                                               |                                     g                                          |
	/// |                                                                               |------------RightTopLeft-------------h                                          |
	/// |                                                                               |                  |                  t               RightTopRight2             |
	/// |                                                                               |                  |                  T                                          |
	/// |                                                                               |                  b   RightTopLeft   o                                          |
	/// |                                                                               |                  o      Bottom      p------------------------------------------|
	/// |                                                                               |   RightTopLeft   t     RightTop     |                                          |
	/// |                                                                               |       Bottom     t                  |                                          |
	/// |                                                                               |        Left      o------Right-------|               RightTopRight3             |
	/// |                                                                               R                  m                  |                                          |
	/// |                                   Left                                        o                  |                  |                                          |
	/// |                                                                               o-----------------------------------Right----------------------------------------|
	/// |                                                                               t                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                 RightBottom                                    |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------
	/// </summary>
	private class TestTree
	{
		public SplitNode Root;
		public LeafNode Left;
		public SplitNode Right;
		public LeafNode RightBottom;
		public SplitNode RightTop;
		public SplitNode RightTopLeft;
		public LeafNode RightTopLeftTop;
		public SplitNode RightTopLeftBottom;
		public LeafNode RightTopLeftBottomLeft;
		public SplitNode RightTopLeftBottomRight;
		public LeafNode RightTopLeftBottomRightTop;
		public LeafNode RightTopLeftBottomRightBottom;
		public SplitNode RightTopRight;
		public LeafNode RightTopRight1;
		public LeafNode RightTopRight2;
		public LeafNode RightTopRight3;

		public TestTree()
		{
			Root = new SplitNode(NodeDirection.Right);

			// left
			Left = new LeafNode(new Mock<IWindow>().Object, Root);
			Root.Children.Add(Left);

			// Right
			Right = new SplitNode(NodeDirection.Bottom, Root);
			Root.Children.Add(Right);

			// RightTop
			RightTop = new SplitNode(NodeDirection.Right, Right);
			Right.Children.Add(RightTop);

			// RightTopLeft
			RightTopLeft = new SplitNode(NodeDirection.Bottom, RightTop);
			RightTop.Children.Add(RightTopLeft);

			// RightTopLeftTop
			RightTopLeftTop = new LeafNode(new Mock<IWindow>().Object, RightTopLeft);
			RightTopLeft.Children.Add(RightTopLeftTop);

			// RightTopLeftBottom
			RightTopLeftBottom = new SplitNode(NodeDirection.Right, RightTopLeft);
			RightTopLeft.Children.Add(RightTopLeftBottom);

			// RightTopLeftBottomLeft
			RightTopLeftBottomLeft = new LeafNode(new Mock<IWindow>().Object, RightTopLeftBottom);
			RightTopLeftBottom.Children.Add(RightTopLeftBottomLeft);

			// RightTopLeftBottomRight
			RightTopLeftBottomRight = new SplitNode(NodeDirection.Bottom, RightTopLeftBottom) { EqualWeight = false };
			RightTopLeftBottom.Children.Add(RightTopLeftBottomRight);

			// RightTopLeftBottomRightTop
			RightTopLeftBottomRightTop = new LeafNode(new Mock<IWindow>().Object, RightTopLeftBottomRight) { Weight = 0.7 };
			RightTopLeftBottomRight.Children.Add(RightTopLeftBottomRightTop);

			// RightTopLeftBottomRightBottom
			RightTopLeftBottomRightBottom = new LeafNode(new Mock<IWindow>().Object, RightTopLeftBottomRight) { Weight = 0.3 };
			RightTopLeftBottomRight.Children.Add(RightTopLeftBottomRightBottom);

			// RightTopRight
			RightTopRight = new SplitNode(NodeDirection.Bottom, RightTop) { EqualWeight = true };
			RightTop.Children.Add(RightTopRight);

			// RightTopRight1
			RightTopRight1 = new LeafNode(new Mock<IWindow>().Object, RightTopRight);
			RightTopRight.Children.Add(RightTopRight1);

			// RightTopRight2
			RightTopRight2 = new LeafNode(new Mock<IWindow>().Object, RightTopRight);
			RightTopRight.Children.Add(RightTopRight2);

			// RightTopRight3
			RightTopRight3 = new LeafNode(new Mock<IWindow>().Object, RightTopRight);
			RightTopRight.Children.Add(RightTopRight3);

			// RightBottom
			RightBottom = new LeafNode(new Mock<IWindow>().Object, Right);
			Right.Children.Add(RightBottom);
		}
	}

	[Fact]
	public void GetWeightAndIndex_Left()
	{
		TestTree tree = new();

		(double weight, double precedingWeight) = TreeLayoutEngine.GetWeightAndIndex(tree.Root, tree.Left);
		Assert.Equal(0.5, weight);
		Assert.Equal(0, precedingWeight);
	}

	[Fact]
	public void GetWeightAndIndex_RightTopLeftBottomRightTop()
	{
		TestTree tree = new();

		(double weight, double precedingWeight) = TreeLayoutEngine.GetWeightAndIndex(tree.RightTopLeftBottomRight, tree.RightTopLeftBottomRightTop);
		Assert.Equal(0.7, weight);
		Assert.Equal(0, precedingWeight);
	}

	[Fact]
	public void GetWeightAndIndex_RightTopLeftBottomRightBottom()
	{
		TestTree tree = new();

		(double weight, double precedingWeight) = TreeLayoutEngine.GetWeightAndIndex(tree.RightTopLeftBottomRight, tree.RightTopLeftBottomRightBottom);
		Assert.Equal(0.3, weight);
		Assert.Equal(0.7, precedingWeight);
	}

	[Fact]
	public void GetWeightAndIndex_RightTopRight1()
	{
		TestTree tree = new();

		(double weight, double precedingWeight) = TreeLayoutEngine.GetWeightAndIndex(tree.RightTopRight, tree.RightTopRight1);
		Assert.Equal(1d / 3, weight);
		Assert.Equal(0, precedingWeight);
	}

	[Fact]
	public void GetWeightAndIndex_RightTopRight2()
	{
		TestTree tree = new();

		(double weight, double precedingWeight) = TreeLayoutEngine.GetWeightAndIndex(tree.RightTopRight, tree.RightTopRight2);
		Assert.Equal(1d / 3, weight);
		Assert.Equal(1d / 3, precedingWeight);
	}

	[Fact]
	public void GetWeightAndIndex_RightTopRight3()
	{
		TestTree tree = new();

		(double weight, double precedingWeight) = TreeLayoutEngine.GetWeightAndIndex(tree.RightTopRight, tree.RightTopRight3);
		Assert.Equal(1d / 3, weight);
		Assert.Equal(2d / 3, precedingWeight);
	}

	[Fact]
	public void GetNodeLocation_Left()
	{
		TestTree tree = new();

		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(tree.Left);

		Assert.NotNull(location);
		Assert.Equal(0, location?.X);
		Assert.Equal(0, location?.Y);
		Assert.Equal(0.5, location?.Width);
		Assert.Equal(1.0, location?.Height);
	}

	[Fact]
	public void GetNodeLocation_RightBottom()
	{
		TestTree tree = new();

		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(tree.RightBottom);

		Assert.NotNull(location);
		Assert.Equal(0.5, location?.X);
		Assert.Equal(0.5, location?.Y);
		Assert.Equal(0.5, location?.Width);
		Assert.Equal(0.5, location?.Height);
	}

	[Fact]
	public void GetNodeLocation_RightTopLeftTop()
	{
		TestTree tree = new();

		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(tree.RightTopLeftTop);

		Assert.NotNull(location);
		Assert.Equal(0.5, location?.X);
		Assert.Equal(0, location?.Y);
		Assert.Equal(0.25, location?.Width);
		Assert.Equal(0.25, location?.Height);
	}

	[Fact]
	public void GetNodeLocation_RightTopLeftBottomLeft()
	{
		TestTree tree = new();

		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(tree.RightTopLeftBottomLeft);

		Assert.NotNull(location);
		Assert.Equal(0.5, location?.X);
		Assert.Equal(0.25, location?.Y);
		Assert.Equal(0.125, location?.Width);
		Assert.Equal(0.25, location?.Height);
	}

	[Fact]
	public void GetNodeLocation_RightTopLeftBottomRightTop()
	{
		TestTree tree = new();

		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(tree.RightTopLeftBottomRightTop);

		Assert.NotNull(location);
		Assert.Equal(0.625, location?.X);
		Assert.Equal(0.25, location?.Y);
		Assert.Equal(0.125, location?.Width);
		Assert.Equal(0.175, location?.Height);
	}

	[Fact]
	public void GetNodeLocation_RightTopLeftBottomRightBottom()
	{
		TestTree tree = new();

		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(tree.RightTopLeftBottomRightBottom);

		Assert.NotNull(location);
		Assert.Equal(0.625, location?.X);
		Assert.Equal(0.425, location?.Y);
		Assert.Equal(0.125, location?.Width);
		Assert.Equal(0.075, location?.Height);
	}

	[Fact]
	public void GetNodeLocation_RightTopRight1()
	{
		TestTree tree = new();

		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(tree.RightTopRight1);

		Assert.NotNull(location);
		Assert.Equal(0.75, location?.X);
		Assert.Equal(0, location?.Y);
		Assert.Equal(0.25, location?.Width);
		Assert.Equal(0.5 * 1d / 3, location?.Height);
	}

	[Fact]
	public void GetNodeLocation_RightTopRight2()
	{
		TestTree tree = new();

		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(tree.RightTopRight2);

		Assert.NotNull(location);
		Assert.Equal(0.75, location?.X);
		Assert.Equal(0.5 * 1d / 3, location?.Y);
		Assert.Equal(0.25, location?.Width);
		Assert.Equal(0.5 * 1d / 3, location?.Height);
	}

	[Fact]
	public void GetNodeLocation_RightTopRight3()
	{
		TestTree tree = new();

		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(tree.RightTopRight3);

		Assert.NotNull(location);
		Assert.Equal(0.75, location?.X);
		Assert.Equal(1d / 3, location?.Y);
		Assert.Equal(0.25, location?.Width);
		Assert.Equal(0.5 * 1d / 3, location?.Height);
	}

	[Fact]
	public void GetLeftMostLeaf()
	{
		TestTree tree = new();

		LeafNode? node = TreeLayoutEngine.GetLeftMostLeaf(tree.Left);

		Assert.Equal(tree.Left, node);
	}

	[Fact]
	public void GetRightMostLeaf()
	{
		TestTree tree = new();

		LeafNode? node = TreeLayoutEngine.GetRightMostLeaf(tree.RightBottom);

		Assert.Equal(tree.RightBottom, node);
	}
}

