using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestSwapWindowInDirection
{
	private readonly TestTreeEngine _testEngine = new();

	[Fact]
	public void RightTopLeftBottomLeft_Left()
	{
		_testEngine.Engine.SwapWindowInDirection(Direction.Left, _testEngine.RightTopLeftBottomLeftWindow.Object);

		Assert.Equal(_testEngine.RightTopLeftBottomLeftNode.Window, _testEngine.LeftWindow.Object);
		Assert.Equal(_testEngine.LeftNode.Window, _testEngine.RightTopLeftBottomLeftWindow.Object);
	}

	[Fact]
	public void RightTopLeftBottomLeft_Right()
	{
		_testEngine.Engine.SwapWindowInDirection(Direction.Right, _testEngine.RightTopLeftBottomLeftWindow.Object);

		Assert.Equal(_testEngine.RightTopLeftBottomLeftNode.Window, _testEngine.RightTopLeftBottomRightTopWindow.Object);
		Assert.Equal(_testEngine.RightTopLeftBottomRightTopNode.Window, _testEngine.RightTopLeftBottomLeftWindow.Object);
	}

	[Fact]
	public void RightTopLeftBottomLeft_Up()
	{
		_testEngine.Engine.SwapWindowInDirection(Direction.Up, _testEngine.RightTopLeftBottomLeftWindow.Object);

		Assert.Equal(_testEngine.RightTopLeftBottomLeftNode.Window, _testEngine.RightTopLeftTopWindow.Object);
		Assert.Equal(_testEngine.RightTopLeftTopNode.Window, _testEngine.RightTopLeftBottomLeftWindow.Object);
	}

	[Fact]
	public void RightTopLeftBottomLeft_Down()
	{
		_testEngine.Engine.SwapWindowInDirection(Direction.Down, _testEngine.RightTopLeftBottomLeftWindow.Object);

		Assert.Equal(_testEngine.RightTopLeftBottomLeftNode.Window, _testEngine.RightBottomWindow.Object);
		Assert.Equal(_testEngine.RightBottomNode.Window, _testEngine.RightTopLeftBottomLeftWindow.Object);
	}

	/// <summary>
	/// Test that moving a window more than once works. This should implicitly check that the
	/// <c>IWindow</c>-<c>LeafNode</c> is updated.
	/// </summary>
	[Fact]
	public void RightTopLeftTop_Twice()
	{
		_testEngine.Engine.SwapWindowInDirection(Direction.Left, _testEngine.RightTopLeftTopWindow.Object);
		_testEngine.Engine.SwapWindowInDirection(Direction.Right, _testEngine.RightTopLeftTopWindow.Object);

		Assert.Equal(_testEngine.LeftNode.Window, _testEngine.LeftWindow.Object);
		Assert.Equal(_testEngine.RightTopLeftTopNode.Window, _testEngine.RightTopLeftTopWindow.Object);
	}

	/// <summary>
	/// Swapping Left left should do nothing.
	/// </summary>
	[Fact]
	public void Left_Left()
	{
		_testEngine.Engine.SwapWindowInDirection(Direction.Left, _testEngine.LeftWindow.Object);

		Assert.Equal(_testEngine.LeftNode.Window, _testEngine.LeftWindow.Object);
	}
}
