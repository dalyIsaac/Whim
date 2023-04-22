using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestMoveWindowEdgeInDirection
{
	/// <summary>
	/// You can't move the left node further left.
	/// </summary>
	[Fact]
	public void MoveWindowEdgeInDirection_Left_Left()
	{
		TestTree tree = new();
		TestTreeEngineMocks testEngine = new();

		testEngine.Engine.MoveWindowEdgeInDirection(Direction.Left, 0.1, testEngine.LeftWindow.Object);
		Assert.Equal(tree.Left.GetWeight(), testEngine.LeftNode.GetWeight());
		Assert.Equal(tree.Root.GetWeight(), testEngine.Engine.Root?.GetWeight());
	}

	/// <summary>
	/// Move the left node further right.
	/// </summary>
	[Fact]
	public void MoveWindowEdgeInDirection_Left_Right()
	{
		TestTreeEngineMocks testEngine = new();

		testEngine.Engine.MoveWindowEdgeInDirection(Direction.Right, 0.1, testEngine.LeftWindow.Object);
		Assert.Equal(0.5 + 0.1, testEngine.LeftNode.GetWeight());
		Assert.Equal(0.5 - 0.1, testEngine.RightBottomNode.Parent?.GetWeight());
	}

	/// <summary>
	/// Move RightTopLeftBottomLeft to the left.
	/// </summary>
	[Fact]
	public void MoveWindowEdgeInDirection_RightTopLeftBottomLeft_Left()
	{
		TestTreeEngineMocks testEngine = new();

		testEngine.Engine.MoveWindowEdgeInDirection(
			Direction.Left,
			0.1,
			testEngine.RightTopLeftBottomLeftWindow.Object
		);
		Assert.Equal(0.5 + 0.1, testEngine.RightBottomNode.Parent?.GetWeight());
		Assert.Equal(0.5 - 0.1, testEngine.LeftNode.GetWeight());
	}

	/// <summary>
	/// Move RightBottom up.
	/// </summary>
	[Fact]
	public void MoveWindowEdgeInDirection_RightBottom_Up()
	{
		TestTreeEngineMocks testEngine = new();

		testEngine.Engine.MoveWindowEdgeInDirection(Direction.Up, 0.1, testEngine.RightBottomWindow.Object);
		Assert.Equal(0.5 + 0.1, testEngine.RightBottomNode.GetWeight());
		Assert.Equal(0.5 - 0.1, testEngine.RightTopLeftTopNode.Parent?.Parent?.GetWeight());
	}

	/// <summary>
	/// Move RightTopRight3 down.
	/// </summary>
	[Fact]
	public void MoveWindowEdgeInDirection_RightTopRight3_Down()
	{
		TestTreeEngineMocks testEngine = new();

		testEngine.Engine.MoveWindowEdgeInDirection(Direction.Down, 0.1, testEngine.RightTopRight3Window.Object);
		Assert.Equal(0.5 + 0.1, testEngine.RightTopRight3Node.Parent?.Parent?.GetWeight());
		Assert.Equal(0.5 - 0.1, testEngine.RightBottomNode.GetWeight());
	}
}
