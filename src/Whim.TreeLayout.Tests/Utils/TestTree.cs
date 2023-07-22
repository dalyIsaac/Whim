using Moq;
using System.Collections.Immutable;

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
	public WindowNode Left;
	public SplitNode Right;
	public SplitNode RightTop;
	public SplitNode RightTopLeft;
	public WindowNode RightTopLeftTop;
	public SplitNode RightTopLeftBottom;
	public WindowNode RightTopLeftBottomLeft;
	public SplitNode RightTopLeftBottomRight;
	public WindowNode RightTopLeftBottomRightTop;
	public WindowNode RightTopLeftBottomRightBottom;
	public SplitNode RightTopRight;
	public WindowNode RightTopRight1;
	public WindowNode RightTopRight2;
	public WindowNode RightTopRight3;
	public WindowNode RightBottom;

	public TestTree()
	{
		Mock<IWindow> leftWindow = new();
		Mock<IWindow> rightTopLeftTopWindow = new();
		Mock<IWindow> rightTopLeftBottomLeftWindow = new();
		Mock<IWindow> rightTopLeftBottomRightTopWindow = new();
		Mock<IWindow> rightTopLeftBottomRightBottomWindow = new();
		Mock<IWindow> rightTopRight1Window = new();
		Mock<IWindow> rightTopRight2Window = new();
		Mock<IWindow> rightTopRight3Window = new();
		Mock<IWindow> rightBottomWindow = new();

		RightTopRight3 = new WindowNode(rightTopRight3Window.Object);

		RightTopRight2 = new WindowNode(rightTopRight2Window.Object);

		RightTopRight1 = new WindowNode(rightTopRight1Window.Object);

		RightTopRight = new SplitNode(
			equalWeight: true,
			isHorizontal: false,
			new INode[] { RightTopRight1, RightTopRight2, RightTopRight3 }.ToImmutableList(),
			new double[] { 1d / 3, 1d / 3, 1d / 3 }.ToImmutableList()
		);

		RightTopLeftBottomRightBottom = new WindowNode(rightTopLeftBottomRightBottomWindow.Object);

		RightTopLeftBottomRightTop = new WindowNode(rightTopLeftBottomRightTopWindow.Object);

		RightTopLeftBottomRight = new SplitNode(
			equalWeight: false,
			isHorizontal: false,
			new INode[] { RightTopLeftBottomRightTop, RightTopLeftBottomRightBottom }.ToImmutableList(),
			new double[] { 0.7, 0.3 }.ToImmutableList()
		);

		RightTopLeftBottomLeft = new WindowNode(rightTopLeftBottomLeftWindow.Object);

		RightTopLeftBottom = new SplitNode(
			equalWeight: true,
			isHorizontal: true,
			new INode[] { RightTopLeftBottomLeft, RightTopLeftBottomRight }.ToImmutableList(),
			new double[] { 0.5, 0.5 }.ToImmutableList()
		);

		RightTopLeftTop = new WindowNode(rightTopLeftTopWindow.Object);

		RightBottom = new WindowNode(rightBottomWindow.Object);

		RightTopLeft = new SplitNode(
			equalWeight: true,
			isHorizontal: false,
			new INode[] { RightTopLeftTop, RightTopLeftBottom }.ToImmutableList(),
			new double[] { 0.5, 0.5 }.ToImmutableList()
		);

		RightTop = new SplitNode(
			equalWeight: true,
			isHorizontal: true,
			new INode[] { RightTopLeft, RightTopRight }.ToImmutableList(),
			new double[] { 0.5, 0.5 }.ToImmutableList()
		);

		Right = new SplitNode(
			equalWeight: true,
			isHorizontal: false,
			new INode[] { RightTop, RightBottom }.ToImmutableList(),
			new double[] { 0.5, 0.5 }.ToImmutableList()
		);

		Left = new WindowNode(leftWindow.Object);

		Root = new SplitNode(
			equalWeight: true,
			isHorizontal: true,
			new INode[] { Left, Right }.ToImmutableList(),
			new double[] { 0.5, 0.5 }.ToImmutableList()
		);
	}
}
