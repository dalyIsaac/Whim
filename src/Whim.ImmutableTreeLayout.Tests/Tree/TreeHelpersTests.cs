using FluentAssertions;
using Moq;
using System.Collections.Immutable;
using Windows.UI.Input.Inking;
using Xunit;

namespace Whim.ImmutableTreeLayout.Tests;

public class TreeHelpersTests
{
	[InlineData(Direction.Right, true)]
	[InlineData(Direction.Down, true)]
	[InlineData(Direction.Up, false)]
	[InlineData(Direction.Left, false)]
	[Theory]
	public void InsertAfter_ReturnsExpected(Direction direction, bool expected)
	{
		// Given
		// When
		bool result = direction.InsertAfter();

		// Then
		Assert.Equal(expected, result);
	}

	#region GetNodeAtPath
	[Fact]
	public void GetNodeAtPath_WithEmptyPath_ReturnsRoot()
	{
		// Given
		Mock<INode> root = new();

		// When
		var result = root.Object.GetNodeAtPath(Array.Empty<int>());

		// Then
		Assert.NotNull(result);
		Assert.Empty(result.Value.Ancestors);
		Assert.Equal(root.Object, result.Value.Node);
		Assert.Equal(Location.UnitSquare<double>(), result.Value.Location);
	}

	[Fact]
	public void GetNodeAtPath_CurrentNodeIsNotSplitNode()
	{
		// Given
		Mock<INode> root = new();
		int[] path = { 0 };

		// When
		var result = root.Object.GetNodeAtPath(path);

		// Then
		Assert.Null(result);
	}

	[Fact]
	public void GetNodeAtPath_EqualWeight_Horizontal()
	{
		// Given
		TestTree tree = new();
		int[] path = { 1 };

		// When
		var result = tree.Root.GetNodeAtPath(path);

		// Then
		Assert.NotNull(result);
		Assert.Equal(tree.Right, result.Value.Node);
		Assert.Equal(
			new Location<double>()
			{
				X = 0.5,
				Width = 0.5,
				Height = 1
			},
			result.Value.Location
		);
	}

	[Fact]
	public void GetNodeAtPath_EqualWeight_Vertical()
	{
		// Given
		TestTree tree = new();
		int[] path = { 1, 0 };

		// When
		var result = tree.Root.GetNodeAtPath(path);

		// Then
		Assert.NotNull(result);
		Assert.Equal(tree.RightTop, result.Value.Node);
		Assert.Equal(
			new Location<double>()
			{
				X = 0.5,
				Y = 0,
				Width = 0.5,
				Height = 0.5
			},
			result.Value.Location
		);
	}

	[Fact]
	public void GetNodeAtPath_Unequal()
	{
		// Given
		TestTree tree = new();
		int[] path = { 1, 0, 0, 1, 1, 1 };

		// When
		var result = tree.Root.GetNodeAtPath(path);

		// Then
		Assert.NotNull(result);
		Assert.Equal(tree.RightTopLeftBottomRightBottom, result.Value.Node);
		Assert.Equal(TestTreeWindowStates.RightTopLeftBottomRightBottom, result.Value.Location);
	}
	#endregion


	[Fact]
	public void GetRightMostLeaf()
	{
		// Given
		TestTree tree = new();

		// When
		var result = tree.Root.GetRightMostLeaf();

		// Then
		var (ancestors, path, leafNode) = result;

		new SplitNode[] { tree.Root, tree.Right }
			.Should()
			.BeEquivalentTo(ancestors);
		new int[] { 1, 1 }
			.Should()
			.BeEquivalentTo(path);
		Assert.Equal(tree.RightBottom, leafNode);
	}

	[Fact]
	public void GetLeftMostLeaf()
	{
		// Given
		TestTree tree = new();

		// When
		var result = tree.Root.GetLeftMostLeaf();

		// Then
		var (ancestors, path, leafNode) = result;

		new SplitNode[] { tree.Root }
			.Should()
			.BeEquivalentTo(ancestors);
		new int[] { 0 }
			.Should()
			.BeEquivalentTo(path);
		Assert.Equal(tree.Left, leafNode);
	}

	#region GetNodeContainingPoint
	[Fact]
	public void GetNodeContainingPoint_DoesNotContainPoint()
	{
		// Given
		TestTree tree = new();
		Point<double> point = new() { X = -0.5, Y = 0.5 };

		// When
		var result = tree.Root.GetNodeContainingPoint(point);

		// Then
		Assert.Null(result);
	}

	[Fact]
	public void GetNodeContainingPoint_Origin()
	{
		// Given
		TestTree tree = new();
		Point<double> point = new() { X = 0, Y = 0 };

		// When
		var result = tree.Root.GetNodeContainingPoint(point);

		// Then
		Assert.NotNull(result);
		Assert.Equal(tree.Left, result.LeafNode);
		new SplitNode[] { tree.Root }
			.Should()
			.BeEquivalentTo(result.Ancestors);
		new int[] { 0 }
			.Should()
			.BeEquivalentTo(result.Path);
	}

	[Fact]
	public void GetNodeContainingPoint_Middle()
	{
		// Given
		TestTree tree = new();
		Point<double> point = new() { X = 0.5, Y = 0.5 };

		// When
		var result = tree.Root.GetNodeContainingPoint(point);

		// Then
		Assert.NotNull(result);
		Assert.Equal(tree.RightBottom, result.LeafNode);
		new SplitNode[] { tree.Root, tree.Right }
			.Should()
			.BeEquivalentTo(result.Ancestors);
		new int[] { 1, 1 }
			.Should()
			.BeEquivalentTo(result.Path);
	}

	[Fact]
	public void GetNodeContainingPoint_RightTopRight3()
	{
		// Given
		TestTree tree = new();
		Point<double> point = new() { X = 0.8, Y = 0.4 };

		// When
		var result = tree.Root.GetNodeContainingPoint(point);

		// Then
		Assert.NotNull(result);
		Assert.Equal(tree.RightTopRight3, result.LeafNode);
		new SplitNode[] { tree.Root, tree.Right, tree.RightTop, tree.RightTopRight }
			.Should()
			.BeEquivalentTo(result.Ancestors);
		new int[] { 1, 0, 1, 2 }
			.Should()
			.BeEquivalentTo(result.Path);
	}

	[Fact]
	public void GetNodeContainingPoint_InvalidNode()
	{
		// Given
		Mock<INode> node = new();
		Point<double> point = new() { X = 0.8, Y = 0.4 };

		// When
		var result = node.Object.GetNodeContainingPoint(point);

		// Then
		Assert.Null(result);
	}

	[Fact]
	public void GetNodeContainingPoint_DoesNotContainPointInChildNode()
	{
		// Given
		SplitNode node =
			new(
				equalWeight: false,
				isHorizontal: true,
				new INode[]
				{
					new WindowNode(new Mock<IWindow>().Object),
					new WindowNode(new Mock<IWindow>().Object)
				}.ToImmutableList(),
				new double[] { 0.5, 0.25 }.ToImmutableList()
			);

		Point<double> point = new() { X = 0.8, Y = 0.4 };

		// When
		var result = node.GetNodeContainingPoint(point);

		// Then
		Assert.Null(result);
	}
	#endregion

	#region GetDirectionToPoint
	public static IEnumerable<object[]> GetDirectionToPoint_UnitSquareData()
	{
		// Top left corner boundary
		yield return new object[] { Location.UnitSquare<double>(), new Point<double>(), Direction.Up };

		yield return new object[]
		{
			Location.UnitSquare<double>(),
			new Point<double>() { X = 0.1, Y = 0 },
			Direction.Up
		};

		yield return new object[]
		{
			Location.UnitSquare<double>(),
			new Point<double>() { X = 0, Y = 0.1 },
			Direction.Left
		};

		// Top right corner boundary
		yield return new object[]
		{
			Location.UnitSquare<double>(),
			new Point<double>() { X = 1, Y = 0 },
			Direction.Up
		};

		yield return new object[]
		{
			Location.UnitSquare<double>(),
			new Point<double>() { X = 0.9, Y = 0 },
			Direction.Up
		};

		yield return new object[]
		{
			Location.UnitSquare<double>(),
			new Point<double>() { X = 1, Y = 0.1 },
			Direction.Right
		};

		// Middle
		yield return new object[]
		{
			Location.UnitSquare<double>(),
			new Point<double>() { X = 0.5, Y = 0.5 },
			Direction.Up
		};

		yield return new object[]
		{
			Location.UnitSquare<double>(),
			new Point<double>() { X = 0.5, Y = 0.4 },
			Direction.Up
		};

		yield return new object[]
		{
			Location.UnitSquare<double>(),
			new Point<double>() { X = 0.5, Y = 0.6 },
			Direction.Down
		};

		yield return new object[]
		{
			Location.UnitSquare<double>(),
			new Point<double>() { X = 0.4, Y = 0.5 },
			Direction.Left
		};

		yield return new object[]
		{
			Location.UnitSquare<double>(),
			new Point<double>() { X = 0.6, Y = 0.5 },
			Direction.Right
		};

		// Bottom left corner boundary
		yield return new object[]
		{
			Location.UnitSquare<double>(),
			new Point<double>() { X = 0, Y = 1 },
			Direction.Left
		};

		yield return new object[]
		{
			Location.UnitSquare<double>(),
			new Point<double>() { X = 0, Y = 0.9 },
			Direction.Left
		};

		yield return new object[]
		{
			Location.UnitSquare<double>(),
			new Point<double>() { X = 0.1, Y = 1 },
			Direction.Down
		};

		// Bottom right corner boundary
		yield return new object[]
		{
			Location.UnitSquare<double>(),
			new Point<double>() { X = 1, Y = 1 },
			Direction.Right
		};

		yield return new object[]
		{
			Location.UnitSquare<double>(),
			new Point<double>() { X = 1, Y = 0.9 },
			Direction.Right
		};

		yield return new object[]
		{
			Location.UnitSquare<double>(),
			new Point<double>() { X = 0.9, Y = 1 },
			Direction.Down
		};

		// Middle of the top triangle
		yield return new object[]
		{
			Location.UnitSquare<double>(),
			new Point<double>() { X = 0.5, Y = 0.25 },
			Direction.Up
		};

		// Middle of the bottom triangle
		yield return new object[]
		{
			Location.UnitSquare<double>(),
			new Point<double>() { X = 0.5, Y = 0.75 },
			Direction.Down
		};

		// Middle of the left triangle
		yield return new object[]
		{
			Location.UnitSquare<double>(),
			new Point<double>() { X = 0.25, Y = 0.5 },
			Direction.Left
		};

		// Middle of the right triangle
		yield return new object[]
		{
			Location.UnitSquare<double>(),
			new Point<double>() { X = 0.75, Y = 0.5 },
			Direction.Right
		};
	}

	public static IEnumerable<object[]> GetDirectionToPoint_NonUnitSquareData()
	{
		yield return new object[]
		{
			new Location<double>()
			{
				X = 1,
				Y = 1,
				Width = 2,
				Height = 2
			},
			new Point<double>() { X = 1.5, Y = 1.5 },
			Direction.Up
		};

		yield return new object[]
		{
			new Location<double>()
			{
				X = 1,
				Y = 1,
				Width = 2,
				Height = 2
			},
			new Point<double>() { X = 1.5, Y = 2 },
			Direction.Left
		};

		yield return new object[]
		{
			new Location<double>()
			{
				X = 1,
				Y = 1,
				Width = 2,
				Height = 2
			},
			new Point<double>() { X = 2.5, Y = 2.75 },
			Direction.Down
		};

		yield return new object[]
		{
			new Location<double>()
			{
				X = 1,
				Y = 1,
				Width = 2,
				Height = 2
			},
			new Point<double>() { X = 2.5, Y = 2.5 },
			Direction.Right
		};
	}

	[Theory]
	[MemberData(nameof(GetDirectionToPoint_UnitSquareData))]
	[MemberData(nameof(GetDirectionToPoint_NonUnitSquareData))]
	public void GetDirectionToPoint(Location<double> location, Point<double> point, Direction expected)
	{
		// Given
		// When
		Direction result = location.GetDirectionToPoint(point);

		// Then
		Assert.Equal(expected, result);
	}
	#endregion

	[Fact]
	public void GetWindowLocations()
	{
		// Given
		TestTree tree = new();
		ILocation<int> location = new Location<int>() { Width = 1920, Height = 1080 };

		IWindowState[] expectedStates = TestTreeWindowStates
			.GetAllWindowStates(
				location,
				tree.Left.Window,
				tree.RightTopLeftTop.Window,
				tree.RightTopLeftBottomLeft.Window,
				tree.RightTopLeftBottomRightTop.Window,
				tree.RightTopLeftBottomRightBottom.Window,
				tree.RightTopRight1.Window,
				tree.RightTopRight2.Window,
				tree.RightTopRight3.Window,
				tree.RightBottom.Window
			)
			.ToArray();

		// When
		LeafNodeState[] windowLocations = tree.Root.GetWindowLocations(location).ToArray();

		// Then
		windowLocations
			.Select(
				nodeState =>
					new WindowState()
					{
						Window = nodeState.Node.Window,
						Location = nodeState.Location,
						WindowSize = nodeState.WindowSize
					}
			)
			.Should()
			.BeEquivalentTo(expectedStates);
	}
}
