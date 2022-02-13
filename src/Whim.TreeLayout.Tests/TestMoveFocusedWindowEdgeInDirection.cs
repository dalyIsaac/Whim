using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestMoveFocusedWindowEdgeInDirection
{
	/// <summary>
	/// You can't move the left node further left.
	/// </summary>
	[Fact]
	public void MoveFocusedWindowEdgeInDirection_Left_Left()
	{
		TestTree tree = new();
		TestTreeEngine testEngine = new();
		testEngine.ActiveWorkspace.Setup(w => w.FocusedWindow).Returns(testEngine.LeftWindow.Object);

		testEngine.Engine.MoveFocusedWindowEdgeInDirection(Direction.Left, 0.1);
		Assert.Equal(tree.Left.Weight, testEngine.LeftNode.Weight);
		Assert.Equal(tree.Root.Weight, testEngine.Engine.Root?.Weight);
	}

	/// <summary>
	/// Move the left node further right.
	/// </summary>
	[Fact]
	public void MoveFocusedWindowEdgeInDirection_Left_Right()
	{
		TestTreeEngine testEngine = new();
		testEngine.ActiveWorkspace.Setup(w => w.FocusedWindow).Returns(testEngine.LeftWindow.Object);

		testEngine.Engine.MoveFocusedWindowEdgeInDirection(Direction.Right, 0.1);
		Assert.Equal(0.5 + 0.1, testEngine.LeftNode.Weight);
		Assert.Equal(0.5 - 0.1, testEngine.RightBottomNode.Parent?.Weight);
	}

	/// <summary>
	/// Move RightTopLeftBottomLeft to the left.
	/// </summary>
	[Fact]
	public void MoveFocusedWindowEdgeInDirection_RightTopLeftBottomLeft_Left()
	{
		TestTreeEngine testEngine = new();
		testEngine.ActiveWorkspace.Setup(w => w.FocusedWindow).Returns(testEngine.RightTopLeftBottomLeftWindow.Object);

		testEngine.Engine.MoveFocusedWindowEdgeInDirection(Direction.Left, 0.1);
		Assert.Equal(0.5 + 0.1, testEngine.RightBottomNode.Parent?.Weight);
		Assert.Equal(0.5 - 0.1, testEngine.LeftNode.Weight);
	}

	/// <summary>
	/// Move RightBottom up.
	/// </summary>
	[Fact]
	public void MoveFocusedWindowEdgeInDirection_RightBottom_Up()
	{
		TestTreeEngine testEngine = new();
		testEngine.ActiveWorkspace.Setup(w => w.FocusedWindow).Returns(testEngine.RightBottomWindow.Object);

		testEngine.Engine.MoveFocusedWindowEdgeInDirection(Direction.Up, 0.1);
		Assert.Equal(0.5 + 0.1, testEngine.RightBottomNode.Weight);
		Assert.Equal(0.5 - 0.1, testEngine.RightTopLeftTopNode.Parent?.Parent?.Weight);
	}

	/// <summary>
	/// Move RightTopRight3 down.
	/// </summary>
	[Fact]
	public void MoveFocusedWindowEdgeInDirection_RightTopRight3_Down()
	{
		TestTreeEngine testEngine = new();
		testEngine.ActiveWorkspace.Setup(w => w.FocusedWindow).Returns(testEngine.RightTopRight3Window.Object);

		testEngine.Engine.MoveFocusedWindowEdgeInDirection(Direction.Down, 0.1);
		Assert.Equal(0.5 + 0.1, testEngine.RightTopRight3Node.Parent?.Parent?.Weight);
		Assert.Equal(0.5 - 0.1, testEngine.RightBottomNode.Weight);
	}
}
