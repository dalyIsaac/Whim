using Moq;

namespace Whim.TreeLayout.Tests;

/// <summary>
/// Returns all the nodes of the following tree, for tests. The tree exists within the coordinates (0,0) to (1,1).
/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------
/// |                                                                               |                                     |                                          |
/// |                                                                               |                                     |                                          |
/// |                                                                               |                                     |               RightTopRight1             |
/// |                                                                               |                                     |                                          |
/// |                                                                               |            RightTopLeftTop          |                                          |
/// |                                                                               |                                     |                                          |
/// |                                                                               |                                     R------------------------------------------|
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
internal class TestTree
{
	public SplitNode Root;
	public LeafNode Left;
	public SplitNode Right;
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
	public LeafNode RightBottom;

	public TestTree(
		Mock<IWindow>? leftWindow = null,
		Mock<IWindow>? rightTopLeftTopWindow = null,
		Mock<IWindow>? rightTopRight1Window = null,
		Mock<IWindow>? rightTopRight2Window = null,
		Mock<IWindow>? rightTopRight3Window = null,
		Mock<IWindow>? rightTopLeftBottomLeftWindow = null,
		Mock<IWindow>? rightTopLeftBottomRightTopWindow = null,
		Mock<IWindow>? rightTopLeftBottomRightBottomWindow = null,
		Mock<IWindow>? rightBottomWindow = null
	)
	{
		leftWindow ??= new Mock<IWindow>();
		rightTopLeftTopWindow ??= new Mock<IWindow>();
		rightTopLeftBottomLeftWindow ??= new Mock<IWindow>();
		rightTopLeftBottomRightTopWindow ??= new Mock<IWindow>();
		rightTopLeftBottomRightBottomWindow ??= new Mock<IWindow>();
		rightTopRight1Window ??= new Mock<IWindow>();
		rightTopRight2Window ??= new Mock<IWindow>();
		rightTopRight3Window ??= new Mock<IWindow>();
		rightBottomWindow ??= new Mock<IWindow>();

		Root = new SplitNode(NodeDirection.Right);

		// left
		Left = new LeafNode(leftWindow.Object, Root) { Weight = 0.5 };
		Root.Children.Add(Left);

		// Right
		Right = new SplitNode(NodeDirection.Down, Root) { Weight = 0.5 };
		Root.Children.Add(Right);

		// RightTop
		RightTop = new SplitNode(NodeDirection.Right, Right) { Weight = 0.5 };
		Right.Children.Add(RightTop);

		// RightTopLeft
		RightTopLeft = new SplitNode(NodeDirection.Down, RightTop) { Weight = 0.5 };
		RightTop.Children.Add(RightTopLeft);

		// RightBottom
		RightBottom = new LeafNode(rightBottomWindow.Object, Right) { Weight = 0.5 };
		Right.Children.Add(RightBottom);

		// RightTopLeftTop
		RightTopLeftTop = new LeafNode(rightTopLeftTopWindow.Object, RightTopLeft) { Weight = 0.5 };
		RightTopLeft.Children.Add(RightTopLeftTop);

		// RightTopLeftBottom
		RightTopLeftBottom = new SplitNode(NodeDirection.Right, RightTopLeft) { Weight = 0.5 };
		RightTopLeft.Children.Add(RightTopLeftBottom);

		// RightTopLeftBottomLeft
		RightTopLeftBottomLeft = new LeafNode(rightTopLeftBottomLeftWindow.Object, RightTopLeftBottom) { Weight = 0.5 };
		RightTopLeftBottom.Children.Add(RightTopLeftBottomLeft);

		// RightTopLeftBottomRight
		RightTopLeftBottomRight = new SplitNode(NodeDirection.Down, RightTopLeftBottom) { Weight = 0.5, EqualWeight = false };
		RightTopLeftBottom.Children.Add(RightTopLeftBottomRight);

		// RightTopLeftBottomRightTop
		RightTopLeftBottomRightTop = new LeafNode(rightTopLeftBottomRightTopWindow.Object, RightTopLeftBottomRight) { Weight = 0.7 };
		RightTopLeftBottomRight.Children.Add(RightTopLeftBottomRightTop);

		// RightTopLeftBottomRightBottom
		RightTopLeftBottomRightBottom = new LeafNode(rightTopLeftBottomRightBottomWindow.Object, RightTopLeftBottomRight) { Weight = 0.3 };
		RightTopLeftBottomRight.Children.Add(RightTopLeftBottomRightBottom);

		// RightTopRight
		RightTopRight = new SplitNode(NodeDirection.Down, RightTop) { Weight = 0.5 };
		RightTop.Children.Add(RightTopRight);

		// RightTopRight1
		RightTopRight1 = new LeafNode(rightTopRight1Window.Object, RightTopRight) { Weight = 1d / 3 };
		RightTopRight.Children.Add(RightTopRight1);

		// RightTopRight2
		RightTopRight2 = new LeafNode(rightTopRight2Window.Object, RightTopRight) { Weight = 1d / 3 };
		RightTopRight.Children.Add(RightTopRight2);

		// RightTopRight3
		RightTopRight3 = new LeafNode(rightTopRight3Window.Object, RightTopRight) { Weight = 1d / 3 };
		RightTopRight.Children.Add(RightTopRight3);
	}
}