using Moq;
using System.Collections.Immutable;

namespace Whim.TreeLayout.Tests;

/// <summary>
/// -----------------------------------------------
/// |                      |                      |
/// |                      |                      |
/// |                      |                      |
/// |                      T                      |
/// |        TopLeft       o       TopRight       |
/// |                      p                      |
/// |                      |                      |
/// |                      |                      |
/// |                      |                      |
/// ---------------------Root----------------------
/// |                      |                      |
/// |                      |                      |
/// |                      B                      |
/// |                      o                      |
/// |      BottomLeft      t      BottomRight     |
/// |                      t                      |
/// |                      o                      |
/// |                      m                      |
/// |                      |                      |
/// -----------------------------------------------
/// </summary>
internal class SimpleTestTree
{
	public SplitNode Root;
	public SplitNode Top;
	public SplitNode Bottom;
	public WindowNode TopLeft;
	public WindowNode TopRight;
	public WindowNode BottomLeft;
	public WindowNode BottomRight;

	public SimpleTestTree()
	{
		BottomRight = new WindowNode(new Mock<IWindow>().Object);
		BottomLeft = new WindowNode(new Mock<IWindow>().Object);

		TopRight = new WindowNode(new Mock<IWindow>().Object);
		TopLeft = new WindowNode(new Mock<IWindow>().Object);

		Bottom = new SplitNode(
			equalWeight: true,
			isHorizontal: true,
			children: ImmutableList.Create<INode>(BottomLeft, BottomRight),
			weights: ImmutableList.Create(0.5, 0.5)
		);

		Top = new SplitNode(
			equalWeight: true,
			isHorizontal: true,
			children: ImmutableList.Create<INode>(TopLeft, TopRight),
			weights: ImmutableList.Create(0.5, 0.5)
		);

		Root = new SplitNode(
			equalWeight: true,
			isHorizontal: false,
			children: ImmutableList.Create<INode>(Top, Bottom),
			weights: ImmutableList.Create(0.5, 0.5)
		);
	}
}
