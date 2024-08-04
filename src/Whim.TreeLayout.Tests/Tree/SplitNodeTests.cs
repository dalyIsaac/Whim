using System.Collections;
using System.Collections.Immutable;
using NSubstitute;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class SplitNodeTests
{
	private static WindowNode CreateWindowNode()
	{
		return new(Substitute.For<IWindow>());
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
		(double, INode)[] children = [.. splitNode];

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
		(double, INode)[] children = [.. splitNode];

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
		(double, INode)[] children = [.. splitNode];

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
		(double, INode)[] children = [.. splitNode];

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
		ISplitNode result = splitNode.Add(focusedNode, newNode, insertAfter: true);

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
		ISplitNode result = splitNode.Add(focusedNode, newNode, insertAfter: false);
		(double, INode)[] children = [.. result];

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
		ISplitNode result = splitNode.Add(focusedNode, newNode, insertAfter: true);
		(double, INode)[] children = [.. result];

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
		ISplitNode result = splitNode.ToggleEqualWeight();
		ISplitNode result2 = result.Add(focusedNode, newNode, insertAfter: true);
		(double, INode)[] children = [.. result2];

		// Then
		Assert.NotSame(splitNode, result);
		Assert.NotSame(result, result2);
		Assert.Equal(3, result2.Count);
		Assert.Equal((0.5, focusedNode), children[0]);
		Assert.Equal((0.25, newNode), children[1]);
		Assert.Equal((0.25, otherNode), children[2]);
	}

	#endregion

	#region Remove
	[InlineData(-1)]
	[InlineData(2)]
	[Theory]
	public void Remove_CouldNotFindWindow(int index)
	{
		// Given
		SplitNode splitNode = new(CreateWindowNode(), CreateWindowNode(), Direction.Right);

		// When
		ISplitNode result = splitNode.Remove(index);

		// Then
		Assert.Same(splitNode, result);
		Assert.Equal(2, splitNode.Count);
	}

	[Fact]
	public void Remove_FirstWindow_OneWindowRemaining()
	{
		// Given
		INode node1 = CreateWindowNode();
		INode node2 = CreateWindowNode();

		ImmutableList<INode> nodes = ImmutableList.Create(node1, node2);
		ImmutableList<double> weights = ImmutableList.Create(0.75, 0.25);
		SplitNode splitNode = new(equalWeight: false, isHorizontal: true, nodes, weights);

		// When
		ISplitNode result = splitNode.Remove(0);

		// Then
		Assert.NotSame(splitNode, result);
		Assert.Single(result);
		Assert.Single(result.Children);
		Assert.Single(result.Weights);
		Assert.Equal(node2, result.Children[0]);
		Assert.True(result.EqualWeight);
	}

	[Fact]
	public void Remove_LastWindow_OneWindowRemaining()
	{
		// Given
		INode node1 = CreateWindowNode();
		INode node2 = CreateWindowNode();

		ImmutableList<INode> nodes = ImmutableList.Create(node1, node2);
		ImmutableList<double> weights = ImmutableList.Create(0.75, 0.25);
		SplitNode splitNode = new(equalWeight: false, isHorizontal: true, nodes, weights);

		// When
		ISplitNode result = splitNode.Remove(1);

		// Then
		Assert.NotSame(splitNode, result);
		Assert.Single(result);
		Assert.Single(result.Children);
		Assert.Single(result.Weights);
		Assert.Equal(node1, result.Children[0]);
		Assert.Equal(1, result.Weights[0]);
	}

	[Fact]
	public void Remove_FirstWindow_TwoWindowsRemaining()
	{
		// Given
		INode node1 = CreateWindowNode();
		INode node2 = CreateWindowNode();
		INode node3 = CreateWindowNode();

		ImmutableList<INode> nodes = ImmutableList.Create(node1, node2, node3);
		ImmutableList<double> weights = ImmutableList.Create(0.5, 0.25, 0.25);
		SplitNode splitNode = new(equalWeight: false, isHorizontal: true, nodes, weights);

		// When
		ISplitNode result = splitNode.Remove(0);

		// Then
		Assert.NotSame(splitNode, result);
		Assert.Equal(2, result.Count);
		Assert.Equal(2, result.Children.Count);
		Assert.Equal(2, result.Weights.Count);
		Assert.Equal(node2, result.Children[0]);
		Assert.Equal(node3, result.Children[1]);
		Assert.Equal(0.75, result.Weights[0]);
		Assert.Equal(0.25, result.Weights[1]);
	}

	[Fact]
	public void Remove_LastWindow_TwoWindowsRemaining()
	{
		// Given
		INode node1 = CreateWindowNode();
		INode node2 = CreateWindowNode();
		INode node3 = CreateWindowNode();

		ImmutableList<INode> nodes = ImmutableList.Create(node1, node2, node3);
		ImmutableList<double> weights = ImmutableList.Create(0.25, 0.25, 0.5);
		SplitNode splitNode = new(equalWeight: false, isHorizontal: true, nodes, weights);

		// When
		ISplitNode result = splitNode.Remove(2);

		// Then
		Assert.NotSame(splitNode, result);
		Assert.Equal(2, result.Count);
		Assert.Equal(2, result.Children.Count);
		Assert.Equal(2, result.Weights.Count);
		Assert.Equal(node1, result.Children[0]);
		Assert.Equal(node2, result.Children[1]);
		Assert.Equal(0.25, result.Weights[0]);
		Assert.Equal(0.75, result.Weights[1]);
	}

	[Fact]
	public void Remove_MiddleWindow()
	{
		// Given
		INode node1 = CreateWindowNode();
		INode node2 = CreateWindowNode();
		INode node3 = CreateWindowNode();

		ImmutableList<INode> nodes = ImmutableList.Create(node1, node2, node3);
		ImmutableList<double> weights = ImmutableList.Create(0.5, 0.25, 0.25);
		SplitNode splitNode = new(equalWeight: false, isHorizontal: true, nodes, weights);

		// When
		ISplitNode result = splitNode.Remove(1);

		// Then
		Assert.NotSame(splitNode, result);
		Assert.Equal(2, result.Count);
		Assert.Equal(2, result.Children.Count);
		Assert.Equal(2, result.Weights.Count);
		Assert.Equal(node1, result.Children[0]);
		Assert.Equal(node3, result.Children[1]);
		Assert.Equal(0.625, result.Weights[0]);
		Assert.Equal(0.375, result.Weights[1]);
	}

	[Fact]
	public void Remove_EqualWeight()
	{
		// Given
		INode node1 = CreateWindowNode();
		INode node2 = CreateWindowNode();
		INode node3 = CreateWindowNode();

		ISplitNode splitNode = new SplitNode(node1, node2, Direction.Right).Add(node2, node3, insertAfter: true);

		// When
		ISplitNode result = splitNode.Remove(2);

		// Then
		Assert.NotSame(splitNode, result);
		Assert.Equal(2, result.Count);
		Assert.Equal(2, result.Children.Count);
		Assert.Equal(2, result.Weights.Count);
		Assert.Equal(node1, result.Children[0]);
		Assert.Equal(node2, result.Children[1]);
		Assert.Equal(0.5, result.Weights[0]);
		Assert.Equal(0.5, result.Weights[1]);
		Assert.True(result.EqualWeight);
	}
	#endregion

	#region Replace
	[InlineData(-1)]
	[InlineData(2)]
	[Theory]
	public void Replace_NodeOutOfRange(int index)
	{
		// Given
		WindowNode focusedNode = CreateWindowNode();
		WindowNode newNode = CreateWindowNode();

		SplitNode splitNode = new(focusedNode, CreateWindowNode(), Direction.Right);

		// When
		ISplitNode result = splitNode.Replace(index, newNode);

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

		int oldNodeIndex = 1;

		// When
		ISplitNode result = splitNode.Replace(oldNodeIndex, newNode);
		(double, INode)[] children = [.. result];

		// Then
		Assert.NotSame(splitNode, result);
		Assert.Equal(2, result.Count);
		Assert.Equal((0.5, focusedNode), children[0]);
		Assert.Equal((0.5, newNode), children[1]);
	}
	#endregion

	#region Swap
	[InlineData(-1, 1)]
	[InlineData(2, 1)]
	[InlineData(0, -1)]
	[InlineData(0, 2)]
	[Theory]
	public void Swap_CouldNotFindIndex(int aIndex, int bIndex)
	{
		// Given
		WindowNode focusedNode = CreateWindowNode();
		WindowNode otherNode = CreateWindowNode();

		SplitNode splitNode = new(focusedNode, otherNode, Direction.Right);

		// When
		ISplitNode result = splitNode.Swap(aIndex, bIndex);

		// Then
		Assert.Same(splitNode, result);
	}

	[Fact]
	public void Swap_Success()
	{
		// Given
		int aIndex = 0;
		int bIndex = 1;
		WindowNode focusedNode = CreateWindowNode();
		WindowNode otherNode = CreateWindowNode();

		SplitNode splitNode = new(focusedNode, otherNode, Direction.Right);

		// When
		ISplitNode result = splitNode.Swap(aIndex, bIndex);
		(double, INode)[] children = [.. result];

		// Then
		Assert.NotSame(splitNode, result);
		Assert.Equal(2, result.Count);
		Assert.Equal((0.5, otherNode), children[0]);
		Assert.Equal((0.5, focusedNode), children[1]);
	}
	#endregion

	#region AdjustChildWeight
	[InlineData(-1)]
	[InlineData(2)]
	[Theory]
	public void AdjustChildWeight_CouldNotFindNode(int index)
	{
		// Given
		WindowNode focusedNode = CreateWindowNode();
		WindowNode otherNode = CreateWindowNode();

		SplitNode splitNode = new(focusedNode, otherNode, Direction.Right);

		// When
		ISplitNode result = splitNode.AdjustChildWeight(index, 0.5);

		// Then
		Assert.Same(splitNode, result);
	}

	[Fact]
	public void AdjustChildWeight_StartWithEqualWeights()
	{
		// Given
		int index = 0;
		WindowNode focusedNode = CreateWindowNode();
		WindowNode otherNode = CreateWindowNode();

		SplitNode splitNode = new(focusedNode, otherNode, Direction.Right);

		// When
		ISplitNode result = splitNode.AdjustChildWeight(index, 0.1);
		(double, INode)[] children = [.. result];

		// Then
		Assert.NotSame(splitNode, result);
		Assert.False(result.EqualWeight);
		Assert.Equal(2, result.Count);
		Assert.Equal((0.6, focusedNode), children[0]);
		Assert.Equal((0.5, otherNode), children[1]);
	}

	[Fact]
	public void AdjustChildWeight_StartWithUnequalWeights()
	{
		// Given
		int index = 2;
		WindowNode focusedNode = CreateWindowNode();
		WindowNode otherNode = CreateWindowNode();
		WindowNode newNode = CreateWindowNode();

		ISplitNode splitNode = new SplitNode(focusedNode, otherNode, Direction.Right)
			.ToggleEqualWeight()
			.Add(otherNode, newNode, insertAfter: true);

		// When
		ISplitNode result = splitNode.AdjustChildWeight(index, 0.1);
		(double, INode)[] children = [.. result];

		// Then
		Assert.NotSame(splitNode, result);
		Assert.False(result.EqualWeight);
		Assert.Equal(3, result.Count);
		Assert.Equal((0.5, focusedNode), children[0]);
		Assert.Equal((0.25, otherNode), children[1]);
		Assert.Equal((0.35, newNode), children[2]);
	}

	[Fact]
	public void AdjustChildWeight_PreventNegativeWeights()
	{
		// Given
		int index = 0;
		WindowNode focusedNode = CreateWindowNode();
		WindowNode otherNode = CreateWindowNode();

		SplitNode splitNode = new(focusedNode, otherNode, Direction.Right);

		// When
		ISplitNode result = splitNode.AdjustChildWeight(index, -1.0);

		// Then
		Assert.Same(splitNode, result);
	}
	#endregion

	[Fact]
	public void GetEnumerator()
	{
		// Given
		WindowNode node1 = CreateWindowNode();
		WindowNode node2 = CreateWindowNode();

		SplitNode splitNode = new(node1, node2, Direction.Right);

		// When
		IEnumerator enumerator = (splitNode as IEnumerable).GetEnumerator();
		List<(double, INode)> items = [];
		while (enumerator.MoveNext())
		{
			if (enumerator.Current is (double weight, INode node))
			{
				items.Add((weight, node));
			}
		}

		// Then
		Assert.Equal(2, items.Count);
		Assert.Equal((0.5, node1), items[0]);
		Assert.Equal((0.5, node2), items[1]);
	}
}
