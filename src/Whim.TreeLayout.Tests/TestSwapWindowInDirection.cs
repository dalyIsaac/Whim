using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestSwapWindowInDirection
{
	private readonly TestTreeEngineMocks _testEngine = new();

	[Fact]
	public void RightTopLeftBottomLeft_Left()
	{
		_testEngine.Engine.SwapWindowInDirection(_testEngine.RightTopLeftBottomLeftWindow.Object, Direction.Left);

		WindowNode rightTopLeftBottomLeftNode = (WindowNode)_testEngine.RightTopLeftBottomNode[0].node;
		Assert.Equal(rightTopLeftBottomLeftNode.Window, _testEngine.LeftWindow.Object);

		WindowNode leftNode = (WindowNode)_testEngine.RootNode[0].node;
		Assert.Equal(leftNode.Window, _testEngine.RightTopLeftBottomLeftWindow.Object);
	}

	[Fact]
	public void RightTopLeftBottomLeft_Right()
	{
		_testEngine.Engine.SwapWindowInDirection(_testEngine.RightTopLeftBottomLeftWindow.Object, Direction.Right);

		WindowNode rightTopLeftBottomLeftNode = (WindowNode)_testEngine.RightTopLeftBottomNode[0].node;
		Assert.Equal(rightTopLeftBottomLeftNode.Window, _testEngine.RightTopLeftBottomRightTopWindow.Object);

		WindowNode rightTopLeftBottomRightTopNode = (WindowNode)_testEngine.RightTopLeftBottomRightNode[0].node;
		Assert.Equal(rightTopLeftBottomRightTopNode.Window, _testEngine.RightTopLeftBottomLeftWindow.Object);
	}

	[Fact]
	public void RightTopLeftBottomLeft_Up()
	{
		_testEngine.Engine.SwapWindowInDirection(_testEngine.RightTopLeftBottomLeftWindow.Object, Direction.Up);

		WindowNode rightTopLeftBottomLeftNode = (WindowNode)_testEngine.RightTopLeftBottomNode[0].node;
		Assert.Equal(rightTopLeftBottomLeftNode.Window, _testEngine.RightTopLeftTopWindow.Object);

		WindowNode rightTopLeftTopNode = (WindowNode)_testEngine.RightTopLeftNode[0].node;
		Assert.Equal(rightTopLeftTopNode.Window, _testEngine.RightTopLeftBottomLeftWindow.Object);
	}

	[Fact]
	public void RightTopLeftBottomLeft_Down()
	{
		_testEngine.Engine.SwapWindowInDirection(_testEngine.RightTopLeftBottomLeftWindow.Object, Direction.Down);

		WindowNode rightTopLeftBottomLeftNode = (WindowNode)_testEngine.RightTopLeftBottomNode[0].node;
		Assert.Equal(rightTopLeftBottomLeftNode.Window, _testEngine.RightBottomWindow.Object);

		WindowNode rightBottomNode = (WindowNode)_testEngine.RightNode[1].node;
		Assert.Equal(rightBottomNode.Window, _testEngine.RightTopLeftBottomLeftWindow.Object);
	}

	[Fact]
	public void RightTopLeftTop_Twice()
	{
		_testEngine.Engine.SwapWindowInDirection(_testEngine.RightTopLeftTopWindow.Object, Direction.Left);
		_testEngine.Engine.SwapWindowInDirection(_testEngine.RightTopLeftTopWindow.Object, Direction.Right);

		WindowNode leftNode = (WindowNode)_testEngine.RootNode[0].node;
		Assert.Equal(leftNode.Window, _testEngine.LeftWindow.Object);

		WindowNode rightTopLeftTopNode = (WindowNode)_testEngine.RightTopLeftNode[0].node;
		Assert.Equal(rightTopLeftTopNode.Window, _testEngine.RightTopLeftTopWindow.Object);
	}

	[Fact]
	public void Left_Left()
	{
		_testEngine.Engine.SwapWindowInDirection(_testEngine.LeftWindow.Object, Direction.Left);

		WindowNode leftNode = (WindowNode)_testEngine.RootNode[0].node;
		Assert.Equal(leftNode.Window, _testEngine.LeftWindow.Object);
	}

	[Fact]
	public void RightTopRight1_RightTopRight2()
	{
		_testEngine.Engine.SwapWindowInDirection(_testEngine.RightTopRight1Window.Object, Direction.Down);

		WindowNode rightTopRight1Node = (WindowNode)_testEngine.RightTopRightNode[0].node;
		Assert.Equal(rightTopRight1Node.Window, _testEngine.RightTopRight2Window.Object);

		WindowNode rightTopRight2Node = (WindowNode)_testEngine.RightTopRightNode[1].node;
		Assert.Equal(rightTopRight2Node.Window, _testEngine.RightTopRight1Window.Object);
	}
}
