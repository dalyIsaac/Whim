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
	public TestSplitNode Root;
	public WindowNode Left;
	public TestSplitNode Right;
	public TestSplitNode RightTop;
	public TestSplitNode RightTopLeft;
	public WindowNode RightTopLeftTop;
	public TestSplitNode RightTopLeftBottom;
	public WindowNode RightTopLeftBottomLeft;
	public TestSplitNode RightTopLeftBottomRight;
	public WindowNode RightTopLeftBottomRightTop;
	public WindowNode RightTopLeftBottomRightBottom;
	public TestSplitNode RightTopRight;
	public WindowNode RightTopRight1;
	public WindowNode RightTopRight2;
	public WindowNode RightTopRight3;
	public WindowNode RightBottom;

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

		Root = new TestSplitNode(isHorizontal: true);

		// left
		Left = new WindowNode(leftWindow.Object, Root);

		// Right
		Right = new TestSplitNode(isHorizontal: false, Root);

		// RightTop
		RightTop = new TestSplitNode(isHorizontal: true, Right);

		// RightTopLeft
		RightTopLeft = new TestSplitNode(isHorizontal: false, RightTop);

		// RightBottom
		RightBottom = new WindowNode(rightBottomWindow.Object, Right);

		// RightTopLeftTop
		RightTopLeftTop = new WindowNode(rightTopLeftTopWindow.Object, RightTopLeft);

		// RightTopLeftBottom
		RightTopLeftBottom = new TestSplitNode(isHorizontal: true, RightTopLeft);

		// RightTopLeftBottomLeft
		RightTopLeftBottomLeft = new WindowNode(rightTopLeftBottomLeftWindow.Object, RightTopLeftBottom);

		// RightTopLeftBottomRight
		RightTopLeftBottomRight = new TestSplitNode(isHorizontal: false, RightTopLeftBottom);

		// RightTopLeftBottomRightTop
		RightTopLeftBottomRightTop = new WindowNode(rightTopLeftBottomRightTopWindow.Object, RightTopLeftBottomRight);

		// RightTopLeftBottomRightBottom
		RightTopLeftBottomRightBottom = new WindowNode(rightTopLeftBottomRightBottomWindow.Object, RightTopLeftBottomRight);

		// RightTopRight
		RightTopRight = new TestSplitNode(isHorizontal: false, RightTop);

		// RightTopRight1
		RightTopRight1 = new WindowNode(rightTopRight1Window.Object, RightTopRight);

		// RightTopRight2
		RightTopRight2 = new WindowNode(rightTopRight2Window.Object, RightTopRight);

		// RightTopRight3
		RightTopRight3 = new WindowNode(rightTopRight3Window.Object, RightTopRight);

		// Initialize the split nodes.
		Root.Initialize(new List<Node> { Left, Right }, new List<double> { 0.5, 0.5 });
		Right.Initialize(new List<Node> { RightTop, RightBottom }, new List<double> { 0.5, 0.5 });
		RightTop.Initialize(new List<Node> { RightTopLeft, RightTopRight }, new List<double> { 0.5, 0.5 });
		RightTopLeft.Initialize(new List<Node> { RightTopLeftTop, RightTopLeftBottom }, new List<double> { 0.5, 0.5 });
		RightTopLeftBottom.Initialize(new List<Node> { RightTopLeftBottomLeft, RightTopLeftBottomRight }, new List<double> { 0.5, 0.5 });
		RightTopLeftBottomRight.Initialize(new List<Node> { RightTopLeftBottomRightTop, RightTopLeftBottomRightBottom }, new List<double> { 0.7, 0.3 });
		RightTopRight.Initialize(new List<Node> { RightTopRight1, RightTopRight2, RightTopRight3 }, new List<double> { 1d / 3, 1d / 3, 1d / 3 });
	}
}