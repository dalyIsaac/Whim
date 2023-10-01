using AutoFixture;
using FluentAssertions;
using NSubstitute;
using System.Collections.Immutable;
using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TreeHelpersCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IMonitor monitor = fixture.Freeze<IMonitor>();
		monitor.WorkingArea.Returns(new Location<int>() { Width = 1920, Height = 1080 });
	}
}

public class TreeHelpersTests
{
	[InlineAutoSubstituteData<TreeHelpersCustomization>(Direction.Right, true)]
	[InlineAutoSubstituteData<TreeHelpersCustomization>(Direction.Down, true)]
	[InlineAutoSubstituteData<TreeHelpersCustomization>(Direction.Up, false)]
	[InlineAutoSubstituteData<TreeHelpersCustomization>(Direction.Left, false)]
	[Theory]
	internal void InsertAfter_ReturnsExpected(Direction direction, bool expected)
	{
		// Given
		// When
		bool result = direction.InsertAfter();

		// Then
		Assert.Equal(expected, result);
	}

	#region GetNodeAtPath
	[Theory, AutoSubstituteData<TreeHelpersCustomization>]
	internal void GetNodeAtPath_WithEmptyPath_ReturnsRoot(INode root)
	{
		// When
		var result = root.GetNodeAtPath(Array.Empty<int>());

		// Then
		Assert.NotNull(result);
		Assert.Empty(result.Ancestors);
		Assert.Equal(root, result.Node);
		Assert.Equal(Location.UnitSquare<double>(), result.Location);
	}

	[Theory, AutoSubstituteData<TreeHelpersCustomization>]
	internal void GetNodeAtPath_CurrentNodeIsNotSplitNode(INode root)
	{
		// Given
		int[] path = { 0 };

		// When
		var result = root.GetNodeAtPath(path);

		// Then
		Assert.Null(result);
	}

	[Fact]
	internal void GetNodeAtPath_EqualWeight_Horizontal()
	{
		// Given
		TestTree tree = new();
		int[] path = { 1 };

		// When
		var result = tree.Root.GetNodeAtPath(path);

		// Then
		Assert.NotNull(result);
		Assert.Equal(tree.Right, result.Node);
		Assert.Equal(
			new Location<double>()
			{
				X = 0.5,
				Width = 0.5,
				Height = 1
			},
			result.Location
		);
	}

	[Fact]
	internal void GetNodeAtPath_EqualWeight_Vertical()
	{
		// Given
		TestTree tree = new();
		int[] path = { 1, 0 };

		// When
		var result = tree.Root.GetNodeAtPath(path);

		// Then
		Assert.NotNull(result);
		Assert.Equal(tree.RightTop, result.Node);
		Assert.Equal(
			new Location<double>()
			{
				X = 0.5,
				Y = 0,
				Width = 0.5,
				Height = 0.5
			},
			result.Location
		);
	}

	[Fact]
	internal void GetNodeAtPath_Unequal()
	{
		// Given
		TestTree tree = new();
		int[] path = { 1, 0, 0, 1, 1, 1 };

		// When
		var result = tree.Root.GetNodeAtPath(path);

		// Then
		Assert.NotNull(result);
		Assert.Equal(tree.RightTopLeftBottomRightBottom, result.Node);
		Assert.Equal(TestTreeWindowStates.RightTopLeftBottomRightBottom, result.Location);
	}
	#endregion

	[Fact]
	internal void GetRightMostWindow()
	{
		// Given
		TestTree tree = new();

		// When
		var result = tree.Root.GetRightMostWindow();

		// Then
		new SplitNode[] { tree.Root, tree.Right }
			.Should()
			.Equal(result.Ancestors, (a, b) => a.Equals(b));

		new int[] { 1, 1 }
			.Should()
			.Equal(result.Path);
		Assert.Equal(tree.RightBottom, result.WindowNode);
	}

	[Fact]
	internal void GetLeftMostWindow()
	{
		// Given
		TestTree tree = new();

		// When
		var result = tree.Root.GetLeftMostWindow();

		// Then
		new SplitNode[] { tree.Root }
			.Should()
			.Equal(result.Ancestors, (a, b) => a.Equals(b));

		new int[] { 0 }
			.Should()
			.Equal(result.Path);
		Assert.Equal(tree.Left, result.WindowNode);
	}

	#region GetNodeContainingPoint
	[Fact]
	internal void GetNodeContainingPoint_DoesNotContainPoint()
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
	internal void GetNodeContainingPoint_RootIsWindow()
	{
		// Given
		WindowNode root = new(Substitute.For<IWindow>());
		Point<double> point = new() { X = 0.5, Y = 0.5 };

		// When
		var result = root.GetNodeContainingPoint(point);

		// Then
		Assert.NotNull(result);
		Assert.Equal(root, result.WindowNode);
		Assert.Empty(result.Ancestors);
		Assert.Empty(result.Path);
	}

	[Fact]
	internal void GetNodeContainingPoint_UnknownNodeType()
	{
		// Given
		SplitNode root = new(Substitute.For<INode>(), Substitute.For<INode>(), Direction.Right);
		Point<double> point = new() { X = 0.5, Y = 0.5 };

		// When
		var result = root.GetNodeContainingPoint(point);

		// Then
		Assert.Null(result);
	}

	[Fact]
	internal void GetNodeContainingPoint_Origin()
	{
		// Given
		TestTree tree = new();
		Point<double> point = new() { X = 0, Y = 0 };

		// When
		var result = tree.Root.GetNodeContainingPoint(point);

		// Then
		Assert.NotNull(result);
		Assert.Equal(tree.Left, result.WindowNode);
		new SplitNode[] { tree.Root }
			.Should()
			.Equal(result.Ancestors, (a, b) => a.Equals(b));

		new int[] { 0 }
			.Should()
			.Equal(result.Path);
	}

	[Fact]
	internal void GetNodeContainingPoint_Middle()
	{
		// Given
		TestTree tree = new();
		Point<double> point = new() { X = 0.5, Y = 0.5 };

		// When
		var result = tree.Root.GetNodeContainingPoint(point);

		// Then
		Assert.NotNull(result);
		Assert.Equal(tree.RightBottom, result.WindowNode);
		new SplitNode[] { tree.Root, tree.Right }
			.Should()
			.Equal(result.Ancestors, (a, b) => a.Equals(b));

		new int[] { 1, 1 }
			.Should()
			.Equal(result.Path);
	}

	[Fact]
	internal void GetNodeContainingPoint_RightTopRight3()
	{
		// Given
		TestTree tree = new();
		Point<double> point = new() { X = 0.8, Y = 0.4 };

		// When
		var result = tree.Root.GetNodeContainingPoint(point);

		// Then
		Assert.NotNull(result);
		Assert.Equal(tree.RightTopRight3, result.WindowNode);
		new SplitNode[] { tree.Root, tree.Right, tree.RightTop, tree.RightTopRight }
			.Should()
			.Equal(result.Ancestors, (a, b) => a.Equals(b));

		new int[] { 1, 0, 1, 2 }
			.Should()
			.Equal(result.Path);
	}

	[Theory, AutoSubstituteData<TreeHelpersCustomization>]
	internal void GetNodeContainingPoint_InvalidNode(INode node)
	{
		// Given
		Point<double> point = new() { X = 0.8, Y = 0.4 };

		// When
		var result = node.GetNodeContainingPoint(point);

		// Then
		Assert.Null(result);
	}

	[Fact]
	internal void GetNodeContainingPoint_DoesNotContainPointInChildNode()
	{
		// Given
		SplitNode node =
			new(
				equalWeight: false,
				isHorizontal: true,
				new INode[]
				{
					new WindowNode(Substitute.For<IWindow>()),
					new WindowNode(Substitute.For<IWindow>())
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

	public static IEnumerable<object[]> GetDirectionToPoint_NonSquareData()
	{
		yield return new object[]
		{
			new Location<double>()
			{
				X = 1,
				Y = 1,
				Width = 2,
				Height = 1
			},
			new Point<double>() { X = 2.0, Y = 1.25 },
			Direction.Up
		};

		yield return new object[]
		{
			new Location<double>()
			{
				X = 1,
				Y = 1,
				Width = 2,
				Height = 1
			},
			new Point<double>() { X = 1.5, Y = 1.5 },
			Direction.Left
		};

		yield return new object[]
		{
			new Location<double>()
			{
				X = 1,
				Y = 1,
				Width = 2,
				Height = 1
			},
			new Point<double>() { X = 2.0, Y = 1.75 },
			Direction.Down
		};

		yield return new object[]
		{
			new Location<double>()
			{
				X = 1,
				Y = 1,
				Width = 2,
				Height = 1
			},
			new Point<double>() { X = 2.5, Y = 1.5 },
			Direction.Right
		};

		yield return new object[]
		{
			new Location<double>()
			{
				X = 1,
				Y = 1,
				Width = 1,
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
				Width = 1,
				Height = 2
			},
			new Point<double>() { X = 1.25, Y = 2.5 },
			Direction.Left
		};

		yield return new object[]
		{
			new Location<double>()
			{
				X = 1,
				Y = 1,
				Width = 1,
				Height = 2
			},
			new Point<double>() { X = 1.25, Y = 2.75 },
			Direction.Down
		};

		yield return new object[]
		{
			new Location<double>()
			{
				X = 1,
				Y = 1,
				Width = 1,
				Height = 2
			},
			new Point<double>() { X = 1.75, Y = 2.5 },
			Direction.Right
		};
	}

	[Theory]
	[MemberData(nameof(GetDirectionToPoint_UnitSquareData))]
	[MemberData(nameof(GetDirectionToPoint_NonUnitSquareData))]
	[MemberData(nameof(GetDirectionToPoint_NonSquareData))]
	internal void GetDirectionToPoint(Location<double> location, Point<double> point, Direction expected)
	{
		// Given
		// When
		Direction result = location.GetDirectionToPoint(point);

		// Then
		Assert.Equal(expected, result);
	}
	#endregion

	[Fact]
	internal void GetWindowLocations()
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
		WindowNodeLocationState[] windowLocations = tree.Root.GetWindowLocations(location).ToArray();

		// Then
		windowLocations
			.Select(
				nodeState =>
					new WindowState()
					{
						Window = nodeState.WindowNode.Window,
						Location = nodeState.Location,
						WindowSize = nodeState.WindowSize
					}
			)
			.Should()
			.Equal(expectedStates, (a, b) => a.Equals(b));
	}

	#region GetAdjacentNode
	[Theory, AutoSubstituteData<TreeHelpersCustomization>]
	internal void GetAdjacentNode_RootIsWindowNode(IWindow window, IMonitor monitor)
	{
		// Given
		WindowNode root = new(window);
		IReadOnlyList<int> pathToNode = Array.Empty<int>();

		// When
		WindowNodeStateAtPoint? result = TreeHelpers.GetAdjacentWindowNode(root, pathToNode, Direction.Right, monitor);

		// Then
		Assert.Null(result);
	}

	[Theory, AutoSubstituteData<TreeHelpersCustomization>]
	internal void GetAdjacentNode_RootIsNotISplitNode(INode root, IMonitor monitor)
	{
		// Given
		IReadOnlyList<int> pathToNode = Array.Empty<int>();

		// When
		WindowNodeStateAtPoint? result = TreeHelpers.GetAdjacentWindowNode(root, pathToNode, Direction.Right, monitor);

		// Then
		Assert.Null(result);
	}

	[Theory, AutoSubstituteData<TreeHelpersCustomization>]
	internal void GetAdjacentNode_CannotFindNodeAtPath(IMonitor monitor)
	{
		// Given
		TestTree tree = new();
		IReadOnlyList<int> pathToNode = new[] { 0, 0, 0, 0, 0 };

		// When
		WindowNodeStateAtPoint? result = TreeHelpers.GetAdjacentWindowNode(
			tree.Root,
			pathToNode,
			Direction.Right,
			monitor
		);

		// Then
		Assert.Null(result);
	}

	[Theory, AutoSubstituteData<TreeHelpersCustomization>]
	internal void GetAdjacentNode_CannotFindAdjacentNode(IMonitor monitor)
	{
		// Given
		TestTree tree = new();
		IReadOnlyList<int> pathToNode = new[] { 0 };

		// When
		WindowNodeStateAtPoint? result = TreeHelpers.GetAdjacentWindowNode(
			tree.Root,
			pathToNode,
			Direction.Left,
			monitor
		);

		// Then
		Assert.Null(result);
	}

	[Theory, AutoSubstituteData<TreeHelpersCustomization>]
	internal void GetAdjacentNode_Success_Left(IMonitor monitor)
	{
		// Given
		TestTree tree = new();
		IReadOnlyList<int> pathToNode = new[] { 1, 0, 0, 1, 0 };

		// When
		WindowNodeStateAtPoint? result = TreeHelpers.GetAdjacentWindowNode(
			tree.Root,
			pathToNode,
			Direction.Left,
			monitor
		);

		// Then
		Assert.Equal(tree.Left.Window, result!.WindowNode.Window);
	}

	[Theory, AutoSubstituteData<TreeHelpersCustomization>]
	internal void GetAdjacentNode_Success_Right(IMonitor monitor)
	{
		// Given
		TestTree tree = new();
		IReadOnlyList<int> pathToNode = new[] { 1, 0, 0, 1, 0 };

		// When
		WindowNodeStateAtPoint? result = TreeHelpers.GetAdjacentWindowNode(
			tree.Root,
			pathToNode,
			Direction.Right,
			monitor
		);

		// Then
		Assert.Equal(tree.RightTopLeftBottomRightTop.Window, result!.WindowNode.Window);
	}

	[Theory, AutoSubstituteData<TreeHelpersCustomization>]
	internal void GetAdjacentNode_Success_Up(IMonitor monitor)
	{
		// Given
		TestTree tree = new();
		IReadOnlyList<int> pathToNode = new[] { 1, 0, 0, 1, 0 };

		// When
		WindowNodeStateAtPoint? result = TreeHelpers.GetAdjacentWindowNode(
			tree.Root,
			pathToNode,
			Direction.Up,
			monitor
		);

		// Then
		Assert.Equal(tree.RightTopLeftTop.Window, result!.WindowNode.Window);
	}

	[Theory, AutoSubstituteData<TreeHelpersCustomization>]
	internal void GetAdjacentNode_Success_Down(IMonitor monitor)
	{
		// Given
		TestTree tree = new();
		IReadOnlyList<int> pathToNode = new[] { 1, 0, 0, 1, 0 };

		// When
		WindowNodeStateAtPoint? result = TreeHelpers.GetAdjacentWindowNode(
			tree.Root,
			pathToNode,
			Direction.Down,
			monitor
		);

		// Then
		Assert.Equal(tree.RightBottom.Window, result!.WindowNode.Window);
	}

	[Theory, AutoSubstituteData<TreeHelpersCustomization>]
	internal void GetAdjacentNode_Success_RightUp(IMonitor monitor)
	{
		// Given
		SimpleTestTree tree = new();
		IReadOnlyList<int> pathToNode = new[] { 1, 0 };

		// When
		WindowNodeStateAtPoint? result = TreeHelpers.GetAdjacentWindowNode(
			tree.Root,
			pathToNode,
			Direction.RightUp,
			monitor
		);

		// Then
		Assert.Equal(tree.TopRight.Window, result!.WindowNode.Window);
	}

	[Theory, AutoSubstituteData<TreeHelpersCustomization>]
	internal void GetAdjacentNode_Success_RightDown(IMonitor monitor)
	{
		// Given
		SimpleTestTree tree = new();
		IReadOnlyList<int> pathToNode = new[] { 0, 0 };

		// When
		WindowNodeStateAtPoint? result = TreeHelpers.GetAdjacentWindowNode(
			tree.Root,
			pathToNode,
			Direction.RightDown,
			monitor
		);

		// Then
		Assert.Equal(tree.BottomRight.Window, result!.WindowNode.Window);
	}

	[Theory, AutoSubstituteData<TreeHelpersCustomization>]
	internal void GetAdjacentNode_Success_LeftUp(IMonitor monitor)
	{
		// Given
		SimpleTestTree tree = new();
		IReadOnlyList<int> pathToNode = new[] { 1, 1 };

		// When
		WindowNodeStateAtPoint? result = TreeHelpers.GetAdjacentWindowNode(
			tree.Root,
			pathToNode,
			Direction.LeftUp,
			monitor
		);

		// Then
		Assert.Equal(tree.TopLeft.Window, result!.WindowNode.Window);
	}

	[Theory, AutoSubstituteData<TreeHelpersCustomization>]
	internal void GetAdjacentNode_Success_LeftDown(IMonitor monitor)
	{
		// Given
		SimpleTestTree tree = new();
		IReadOnlyList<int> pathToNode = new[] { 0, 1 };

		// When
		WindowNodeStateAtPoint? result = TreeHelpers.GetAdjacentWindowNode(
			tree.Root,
			pathToNode,
			Direction.LeftDown,
			monitor
		);

		// Then
		Assert.Equal(tree.BottomLeft.Window, result!.WindowNode.Window);
	}
	#endregion

	#region GetLastCommonAncestor
	[Fact]
	internal void GetLastCommonAncestor_EmptyList()
	{
		// Given
		IReadOnlyList<int> pathToNode1 = Array.Empty<int>();
		IReadOnlyList<int> pathToNode2 = Array.Empty<int>();

		// When
		int? result = TreeHelpers.GetLastCommonAncestorIndex(pathToNode1, pathToNode2);

		// Then
		Assert.Equal(-1, result);
	}

	[Fact]
	internal void GetLastCommonAncestor_SomeCommonAncestor()
	{
		// Given
		IReadOnlyList<int> pathToNode1 = new[] { 1, 0, 0, 1, 0 };
		IReadOnlyList<int> pathToNode2 = new[] { 1, 0, 0, 1, 1 };

		// When
		int? result = TreeHelpers.GetLastCommonAncestorIndex(pathToNode1, pathToNode2);

		// Then
		Assert.Equal(3, result);
	}

	[Fact]
	internal void GetLastCommonAncestor_NoCommonAncestor()
	{
		// Given
		IReadOnlyList<int> pathToNode1 = new[] { 1, 0, 0, 1, 0 };
		IReadOnlyList<int> pathToNode2 = new[] { 0, 0, 0, 1, 1 };

		// When
		int? result = TreeHelpers.GetLastCommonAncestorIndex(pathToNode1, pathToNode2);

		// Then
		Assert.Equal(-1, result);
	}

	[Fact]
	internal void GetLastCommonAncestor_SamePath()
	{
		// Given
		IReadOnlyList<int> pathToNode1 = new[] { 1, 0, 0, 1, 0 };
		IReadOnlyList<int> pathToNode2 = new[] { 1, 0, 0, 1, 0 };

		// When
		int? result = TreeHelpers.GetLastCommonAncestorIndex(pathToNode1, pathToNode2);

		// Then
		Assert.Equal(4, result);
	}
	#endregion

	#region CreateUpdatedPaths
	[Fact]
	internal void CreateUpdatedPaths_Create()
	{
		// Given
		TestTree tree = new();
		ImmutableDictionary<IWindow, ImmutableArray<int>> windowPaths = ImmutableDictionary<
			IWindow,
			ImmutableArray<int>
		>.Empty;
		ImmutableArray<int> pathToNode = ImmutableArray.Create(1, 0, 0, 1, 0);

		// When
		ImmutableDictionary<IWindow, ImmutableArray<int>> result = TreeHelpers.CreateUpdatedPaths(
			windowPaths,
			pathToNode,
			tree.Root
		);

		// Then
		Assert.NotSame(windowPaths, result);
		Assert.NotEqual(windowPaths, result);
		Assert.Equal(9, result.Count);

		new int[] { 0 }
			.Should()
			.Equal(result[tree.Left.Window]);

		new int[] { 1, 0, 0, 0 }
			.Should()
			.Equal(result[tree.RightTopLeftTop.Window]);

		new int[] { 1, 0, 0, 1, 0 }
			.Should()
			.Equal(result[tree.RightTopLeftBottomLeft.Window]);

		new int[] { 1, 0, 0, 1, 1, 0 }
			.Should()
			.Equal(result[tree.RightTopLeftBottomRightTop.Window]);

		new int[] { 1, 0, 0, 1, 1, 1 }
			.Should()
			.Equal(result[tree.RightTopLeftBottomRightBottom.Window]);

		new int[] { 1, 0, 1, 0 }
			.Should()
			.Equal(result[tree.RightTopRight1.Window]);

		new int[] { 1, 0, 1, 1 }
			.Should()
			.Equal(result[tree.RightTopRight2.Window]);

		new int[] { 1, 0, 1, 2 }
			.Should()
			.Equal(result[tree.RightTopRight3.Window]);

		new int[] { 1, 1 }
			.Should()
			.Equal(result[tree.RightBottom.Window]);
	}

	[Fact]
	internal void CreateUpdatedPaths_AmendRightTopRight()
	{
		// Given
		TestTree tree = new();
		ImmutableArray<int> initialPath = ImmutableArray.Create(0);
		ImmutableArray<int> pathToNode = ImmutableArray.Create(1, 0, 1);

		// When
		ImmutableDictionary<IWindow, ImmutableArray<int>> originalDict = TreeHelpers.CreateUpdatedPaths(
			ImmutableDictionary<IWindow, ImmutableArray<int>>.Empty,
			initialPath,
			tree.Root
		);

		ImmutableDictionary<IWindow, ImmutableArray<int>> result = TreeHelpers.CreateUpdatedPaths(
			originalDict,
			pathToNode,
			tree.Root
		);

		// Then
		Assert.NotSame(originalDict, result);

		// Test reference equality.
		Assert.True(originalDict[tree.Left.Window] == result[tree.Left.Window]);
		Assert.True(originalDict[tree.RightTopLeftTop.Window] == result[tree.RightTopLeftTop.Window]);
		Assert.True(originalDict[tree.RightTopLeftBottomLeft.Window] == result[tree.RightTopLeftBottomLeft.Window]);
		Assert.True(
			originalDict[tree.RightTopLeftBottomRightTop.Window] == result[tree.RightTopLeftBottomRightTop.Window]
		);
		Assert.True(
			originalDict[tree.RightTopLeftBottomRightBottom.Window] == result[tree.RightTopLeftBottomRightBottom.Window]
		);
		Assert.False(originalDict[tree.RightTopRight1.Window] == result[tree.RightTopRight1.Window]);
		Assert.False(originalDict[tree.RightTopRight2.Window] == result[tree.RightTopRight2.Window]);
		Assert.False(originalDict[tree.RightTopRight3.Window] == result[tree.RightTopRight3.Window]);
	}

	[Fact]
	internal void CreateUpdatedPaths_DidNotFindSplitNode()
	{
		// Given
		TestTree tree = new();
		ImmutableArray<int> initialPath = ImmutableArray.Create(0);
		ImmutableArray<int> pathToNode = ImmutableArray.Create(0, 0);

		// When
		ImmutableDictionary<IWindow, ImmutableArray<int>> originalDict = TreeHelpers.CreateUpdatedPaths(
			ImmutableDictionary<IWindow, ImmutableArray<int>>.Empty,
			initialPath,
			tree.Root
		);

		ImmutableDictionary<IWindow, ImmutableArray<int>> result = TreeHelpers.CreateUpdatedPaths(
			originalDict,
			pathToNode,
			tree.Root
		);

		// Then
		Assert.Same(originalDict, result);
	}
	#endregion
}
