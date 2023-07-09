using Moq;
using System.Collections;
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
		(double, INode)[] children = splitNode.ToArray();

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
		(double, INode)[] children = splitNode.ToArray();

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
		(double, INode)[] children = splitNode.ToArray();

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
		(double, INode)[] children = splitNode.ToArray();

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
		(double, INode)[] children = result.ToArray();

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
		(double, INode)[] children = result.ToArray();

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
		(double, INode)[] children = result2.ToArray();

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
	public void Remove_EqualWeight()
	{
		// Given
		int index = 1;
		WindowNode focusedNode = CreateWindowNode();
		WindowNode otherNode = CreateWindowNode();
		SplitNode splitNode = new(focusedNode, otherNode, Direction.Right);

		// When
		ISplitNode result = splitNode.Remove(index);
		(double, INode)[] children = result.ToArray();

		// Then
		Assert.NotSame(splitNode, result);
		Assert.Equal(1, result.Count);
		Assert.Equal((1d, focusedNode), children[0]);
	}

	[Fact]
	public void Remove_NotEqualWeight()
	{
		// Given
		int index = 0;
		WindowNode focusedNode = CreateWindowNode();
		WindowNode otherNode = CreateWindowNode();
		WindowNode newNode = CreateWindowNode();

		ISplitNode splitNode = new SplitNode(focusedNode, otherNode, Direction.Right)
			.ToggleEqualWeight()
			.Add(otherNode, newNode, insertAfter: true);

		// When
		ISplitNode result = splitNode.Remove(index);
		(double, INode)[] children = result.ToArray();

		// Then
		Assert.NotSame(splitNode, result);
		Assert.Equal(2, result.Count);
		Assert.Equal((0.25, otherNode), children[0]);
		Assert.Equal((0.75, newNode), children[1]);
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
		(double, INode)[] children = result.ToArray();

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
		(double, INode)[] children = result.ToArray();

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
		(double, INode)[] children = result.ToArray();

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
		(double, INode)[] children = result.ToArray();

		// Then
		Assert.NotSame(splitNode, result);
		Assert.False(result.EqualWeight);
		Assert.Equal(3, result.Count);
		Assert.Equal((0.5, focusedNode), children[0]);
		Assert.Equal((0.25, otherNode), children[1]);
		Assert.Equal((0.35, newNode), children[2]);
	}
	#endregion

	#region
	[Fact]
	public void GetChildWeight_CouldNotFindNode()
	{
		// Given
		WindowNode focusedNode = CreateWindowNode();
		WindowNode otherNode = CreateWindowNode();

		SplitNode splitNode = new(focusedNode, otherNode, Direction.Right);

		// When
		double? result = splitNode.GetChildWeight(CreateWindowNode());

		// Then
		Assert.Null(result);
	}

	[Fact]
	public void GetChildWeight_Success()
	{
		// Given
		WindowNode focusedNode = CreateWindowNode();
		WindowNode otherNode = CreateWindowNode();

		ISplitNode splitNode = new SplitNode(focusedNode, otherNode, Direction.Right)
			.ToggleEqualWeight()
			.Add(otherNode, CreateWindowNode(), insertAfter: true);

		// When
		double? result = splitNode.GetChildWeight(otherNode);

		// Then
		Assert.Equal(0.25, result);
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
		List<(double, INode)> items = new();
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
