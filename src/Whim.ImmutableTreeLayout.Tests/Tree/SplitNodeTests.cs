using FluentAssertions;
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

	[Fact]
	public void Add_InvalidDirection()
	{
		// Given
		WindowNode focusedNode = CreateWindowNode();
		SplitNode splitNode = new(focusedNode, CreateWindowNode(), Direction.Right);
		WindowNode newNode = CreateWindowNode();

		// When
		SplitNode result = splitNode.Add(focusedNode, newNode, Direction.Up);
		(double, Node)[] children = result.ToArray();

		// Then
		double aThird = 1d / 3;
		Assert.NotSame(splitNode, result);
		Assert.Equal(3, result.Count);
		Assert.Equal((aThird, newNode), children[0]);
		Assert.Equal((aThird, focusedNode), children[1]);
	}
	#endregion

	#region Remove
	[Fact]
	public void Remove_CouldNotFindWindow()
	{
		// Given
		WindowNode focusedNode = CreateWindowNode();
		SplitNode splitNode = new(focusedNode, CreateWindowNode(), Direction.Right);
		WindowNode otherNode = CreateWindowNode();

		// When
		SplitNode result = splitNode.Remove(otherNode);

		// Then
		Assert.Same(splitNode, result);
		Assert.Equal(2, splitNode.Count);
	}

	[Fact]
	public void Remove_EqualWeight()
	{
		// Given
		WindowNode focusedNode = CreateWindowNode();
		WindowNode otherNode = CreateWindowNode();
		SplitNode splitNode = new(focusedNode, otherNode, Direction.Right);

		// When
		SplitNode result = splitNode.Remove(otherNode);
		(double, Node)[] children = result.ToArray();

		// Then
		Assert.NotSame(splitNode, result);
		Assert.Equal(1, result.Count);
		Assert.Equal((1d, focusedNode), children[0]);
	}

	[Fact]
	public void Remove_NotEqualWeight()
	{
		// Given
		WindowNode focusedNode = CreateWindowNode();
		WindowNode otherNode = CreateWindowNode();
		WindowNode newNode = CreateWindowNode();

		SplitNode splitNode = new SplitNode(focusedNode, otherNode, Direction.Right)
			.ToggleEqualWeight()
			.Add(otherNode, newNode, Direction.Right);

		// When
		SplitNode result = splitNode.Remove(focusedNode);
		(double, Node)[] children = result.ToArray();

		// Then
		Assert.NotSame(splitNode, result);
		Assert.Equal(2, result.Count);
		Assert.Equal((0.25, otherNode), children[0]);
		Assert.Equal((0.75, newNode), children[1]);
	}
	#endregion

	#region Replace
	[Fact]
	public void Replace_CouldNotFindOldNode()
	{
		// Given
		WindowNode focusedNode = CreateWindowNode();
		WindowNode newNode = CreateWindowNode();
		WindowNode oldNode = CreateWindowNode();

		SplitNode splitNode = new(focusedNode, CreateWindowNode(), Direction.Right);

		// When
		SplitNode result = splitNode.Replace(oldNode, newNode);

		// Then
		Assert.Same(splitNode, result);
	}

	[Fact]
	public void Replace_Success()
	{
		// Given
		WindowNode focusedNode = CreateWindowNode();
		WindowNode newNode = CreateWindowNode();
		WindowNode oldNode = CreateWindowNode();

		SplitNode splitNode = new(focusedNode, oldNode, Direction.Right);

		// When
		SplitNode result = splitNode.Replace(oldNode, newNode);
		(double, Node)[] children = result.ToArray();

		// Then
		Assert.NotSame(splitNode, result);
		Assert.Equal(2, result.Count);
		Assert.Equal((0.5, focusedNode), children[0]);
		Assert.Equal((0.5, newNode), children[1]);
	}
	#endregion
}
