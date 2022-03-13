using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestSwapWindowInDirection
{
	private readonly TestTreeEngine _testEngine = new();

	[Fact]
	public void RightTopLeftBottomLeft_Left()
	{
		_testEngine.Engine.SwapWindowInDirection(Direction.Left, _testEngine.RightTopLeftBottomLeftWindow.Object);

		WindowNode rightTopLeftBottomLeftNode = (WindowNode)_testEngine.RightTopLeftBottomNode[0].node;
		Assert.Equal(rightTopLeftBottomLeftNode.Window, _testEngine.LeftWindow.Object);

		WindowNode leftNode = (WindowNode)_testEngine.RootNode[0].node;
		Assert.Equal(leftNode.Window, _testEngine.RightTopLeftBottomLeftWindow.Object);
	}

	[Fact]
	public void RightTopLeftBottomLeft_Right()
	{
		_testEngine.Engine.SwapWindowInDirection(Direction.Right, _testEngine.RightTopLeftBottomLeftWindow.Object);

		WindowNode rightTopLeftBottomLeftNode = (WindowNode)_testEngine.RightTopLeftBottomNode[0].node;
		Assert.Equal(rightTopLeftBottomLeftNode.Window, _testEngine.RightTopLeftBottomRightTopWindow.Object);

		WindowNode rightTopLeftBottomRightTopNode = (WindowNode)_testEngine.RightTopLeftBottomRightNode[0].node;
		Assert.Equal(rightTopLeftBottomRightTopNode.Window, _testEngine.RightTopLeftBottomLeftWindow.Object);
	}

	[Fact]
	public void RightTopLeftBottomLeft_Up()
	{
		_testEngine.Engine.SwapWindowInDirection(Direction.Up, _testEngine.RightTopLeftBottomLeftWindow.Object);

		WindowNode rightTopLeftBottomLeftNode = (WindowNode)_testEngine.RightTopLeftBottomNode[0].node;
		Assert.Equal(rightTopLeftBottomLeftNode.Window, _testEngine.RightTopLeftTopWindow.Object);

		WindowNode rightTopLeftTopNode = (WindowNode)_testEngine.RightTopLeftNode[0].node;
		Assert.Equal(rightTopLeftTopNode.Window, _testEngine.RightTopLeftBottomLeftWindow.Object);
	}

	[Fact]
	public void RightTopLeftBottomLeft_Down()
	{
		_testEngine.Engine.SwapWindowInDirection(Direction.Down, _testEngine.RightTopLeftBottomLeftWindow.Object);

		WindowNode rightTopLeftBottomLeftNode = (WindowNode)_testEngine.RightTopLeftBottomNode[0].node;
		Assert.Equal(rightTopLeftBottomLeftNode.Window, _testEngine.RightBottomWindow.Object);

		WindowNode rightBottomNode = (WindowNode)_testEngine.RightNode[1].node;
		Assert.Equal(rightBottomNode.Window, _testEngine.RightTopLeftBottomLeftWindow.Object);
	}

	[Fact]
	public void RightTopLeftTop_Twice()
	{
		_testEngine.Engine.SwapWindowInDirection(Direction.Left, _testEngine.RightTopLeftTopWindow.Object);
		_testEngine.Engine.SwapWindowInDirection(Direction.Right, _testEngine.RightTopLeftTopWindow.Object);

		WindowNode leftNode = (WindowNode)_testEngine.RootNode[0].node;
		Assert.Equal(leftNode.Window, _testEngine.LeftWindow.Object);

		WindowNode rightTopLeftTopNode = (WindowNode)_testEngine.RightTopLeftNode[0].node;
		Assert.Equal(rightTopLeftTopNode.Window, _testEngine.RightTopLeftTopWindow.Object);
	}

	[Fact]
	public void Left_Left()
	{
		_testEngine.Engine.SwapWindowInDirection(Direction.Left, _testEngine.LeftWindow.Object);

		WindowNode leftNode = (WindowNode)_testEngine.RootNode[0].node;
		Assert.Equal(leftNode.Window, _testEngine.LeftWindow.Object);
	}
}
