using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestSwapWindowInDirection
{
	private readonly TestTreeEngine _testEngine = new();

	[Fact]
	public void RightTopLeftBottomLeft_Left()
	{
		_testEngine.Engine.SwapWindowInDirection(WindowDirection.Left, _testEngine.RightTopLeftBottomLeftWindow.Object);

		Assert.Equal(_testEngine.RightTopLeftBottomLeftNode.Window, _testEngine.LeftWindow.Object);
		Assert.Equal(_testEngine.LeftNode.Window, _testEngine.RightTopLeftBottomLeftWindow.Object);
	}

	[Fact]
	public void RightTopLeftBottomLeft_Right()
	{
		_testEngine.Engine.SwapWindowInDirection(WindowDirection.Right, _testEngine.RightTopLeftBottomLeftWindow.Object);

		Assert.Equal(_testEngine.RightTopLeftBottomLeftNode.Window, _testEngine.RightTopLeftBottomRightTopWindow.Object);
		Assert.Equal(_testEngine.RightTopLeftBottomRightTopNode.Window, _testEngine.RightTopLeftBottomLeftWindow.Object);
	}

	[Fact]
	public void RightTopLeftBottomLeft_Up()
	{
		_testEngine.Engine.SwapWindowInDirection(WindowDirection.Up, _testEngine.RightTopLeftBottomLeftWindow.Object);

		Assert.Equal(_testEngine.RightTopLeftBottomLeftNode.Window, _testEngine.RightTopLeftTopWindow.Object);
		Assert.Equal(_testEngine.RightTopLeftTopNode.Window, _testEngine.RightTopLeftBottomLeftWindow.Object);
	}

	[Fact]
	public void RightTopLeftBottomLeft_Down()
	{
		_testEngine.Engine.SwapWindowInDirection(WindowDirection.Down, _testEngine.RightTopLeftBottomLeftWindow.Object);

		Assert.Equal(_testEngine.RightTopLeftBottomLeftNode.Window, _testEngine.RightBottomWindow.Object);
		Assert.Equal(_testEngine.RightBottomNode.Window, _testEngine.RightTopLeftBottomLeftWindow.Object);
	}

	/// <summary>
	/// Swapping Left left should do nothing.
	/// </summary>
	[Fact]
	public void Left_Left()
	{
		_testEngine.Engine.SwapWindowInDirection(WindowDirection.Left, _testEngine.LeftWindow.Object);

		Assert.Equal(_testEngine.LeftNode.Window, _testEngine.LeftWindow.Object);
	}
}
