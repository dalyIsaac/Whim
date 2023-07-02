using Moq;
using Xunit;

namespace Whim.ImmutableTreeLayout.Tests;

public class SplitNodeTests
{
	private static WindowNode CreateWindowNode()
	{
		return new(new Mock<IWindow>().Object);
	}

	#region Ctor
	[Fact]
	public void Ctor_Right()
	{
		// Given
		WindowNode focusedNode = CreateWindowNode();
		WindowNode newNode = CreateWindowNode();

		// When
		SplitNode splitNode = new(focusedNode, newNode, Direction.Right);
		(double, Node)[] children = splitNode.ToArray();

		// Then
		Assert.Equal(2, children.Length);
		Assert.True(splitNode.IsHorizontal);
		Assert.Equal((0.5, focusedNode), children[0]);
		Assert.Equal((0.5, newNode), children[1]);
	}

	[Fact]
	public void Ctor_Left()
	{
		// Given
		WindowNode focusedNode = CreateWindowNode();
		WindowNode newNode = CreateWindowNode();

		// When
		SplitNode splitNode = new(focusedNode, newNode, Direction.Left);
		(double, Node)[] children = splitNode.ToArray();

		// Then
		Assert.Equal(2, children.Length);
		Assert.True(splitNode.IsHorizontal);
		Assert.Equal((0.5, newNode), children[0]);
		Assert.Equal((0.5, focusedNode), children[1]);
	}

	[Fact]
	public void Ctor_Up()
	{
		// Given
		WindowNode focusedNode = CreateWindowNode();
		WindowNode newNode = CreateWindowNode();

		// When
		SplitNode splitNode = new(focusedNode, newNode, Direction.Up);
		(double, Node)[] children = splitNode.ToArray();

		// Then
		Assert.Equal(2, children.Length);
		Assert.False(splitNode.IsHorizontal);
		Assert.Equal((0.5, newNode), children[0]);
		Assert.Equal((0.5, focusedNode), children[1]);
	}

	[Fact]
	public void Ctor_Down()
	{
		// Given
		WindowNode focusedNode = CreateWindowNode();
		WindowNode newNode = CreateWindowNode();

		// When
		SplitNode splitNode = new(focusedNode, newNode, Direction.Down);
		(double, Node)[] children = splitNode.ToArray();

		// Then
		Assert.Equal(2, children.Length);
		Assert.False(splitNode.IsHorizontal);
		Assert.Equal((0.5, focusedNode), children[0]);
		Assert.Equal((0.5, newNode), children[1]);
	}
	#endregion

	#region Add
	[Fact]
	public void Add_CouldNotFindFocusedNode()
	{
		// Given
		SplitNode splitNode = new(CreateWindowNode(), CreateWindowNode(), Direction.Right);
		WindowNode focusedNode = CreateWindowNode();
		WindowNode newNode = CreateWindowNode();

		// When
		SplitNode result = splitNode.Add(focusedNode, newNode, Direction.Right);

		// Then
		Assert.Same(splitNode, result);
		Assert.Equal(2, splitNode.Count);
	}

	[Fact]
	public void Add_InsertAtStart()
	{
		// Given
		WindowNode focusedNode = CreateWindowNode();
		SplitNode splitNode = new(focusedNode, CreateWindowNode(), Direction.Right);
		WindowNode newNode = CreateWindowNode();

		// When
		SplitNode result = splitNode.Add(focusedNode, newNode, Direction.Left);
		(double, Node)[] children = result.ToArray();

		// Then
		double aThird = 1d / 3;
		Assert.NotSame(splitNode, result);
		Assert.Equal(3, result.Count);
		Assert.Equal((aThird, newNode), children[0]);
		Assert.Equal((aThird, focusedNode), children[1]);
	}

	[Fact]
	public void Add_InsertAtEnd()
	{
		// Given
		WindowNode focusedNode = CreateWindowNode();
		SplitNode splitNode = new(focusedNode, CreateWindowNode(), Direction.Right);
		WindowNode newNode = CreateWindowNode();

		// When
		SplitNode result = splitNode.Add(focusedNode, newNode, Direction.Right);
		(double, Node)[] children = result.ToArray();

		// Then
		double aThird = 1d / 3;
		Assert.NotSame(splitNode, result);
		Assert.Equal(3, result.Count);
		Assert.Equal((aThird, focusedNode), children[0]);
		Assert.Equal((aThird, newNode), children[1]);
	}

	[Fact]
	public void Add_NotEqualWeight()
	{
		// Given
		WindowNode focusedNode = CreateWindowNode();
		WindowNode otherNode = CreateWindowNode();
		SplitNode splitNode = new(focusedNode, otherNode, Direction.Right);
		WindowNode newNode = CreateWindowNode();

		// When
		SplitNode result = splitNode.ToggleEqualWeight();
		SplitNode result2 = result.Add(focusedNode, newNode, Direction.Right);
		(double, Node)[] children = result2.ToArray();

		// Then
		Assert.NotSame(splitNode, result);
		Assert.NotSame(result, result2);
		Assert.Equal(3, result2.Count);
		Assert.Equal((0.5, focusedNode), children[0]);
		Assert.Equal((0.25, newNode), children[1]);
		Assert.Equal((0.25, otherNode), children[2]);
	}
	#endregion
}
