using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class Tests
{
	/// <summary>
	/// Returns all the nodes of the following tree, for tests. The tree exists within the coordinates (0,0) to (1,1).
	/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------
	/// |                                                                               |                                     |                                          |
	/// |                                                                               |                                     |                                          |
	/// |                                                                               |                                     |               RightTopRight1             |
	/// |                                                                               |                                     |                                          |
	/// |                                                                               |            RightTopLeftTop          |                                          |
	/// |                                                                               |                                     |                                          |
	/// |                                                                               |                                     r------------------------------------------|
	/// |                                                                               |                                     i                                          |
	/// |                                                                               |                                     g                                          |
	/// |                                                                               |------------RightTopLeft-------------h                                          |
	/// |                                                                               |                  |                  t               RightTopRight2             |
	/// |                                                                               |                  |                  T                                          |
	/// |                                                                               |                  b   RightTopLeft   o                                          |
	/// |                                                                               |                  o      Bottom      p------------------------------------------|
	/// |                                                                               |   RightTopLeft   t     RightTop     |                                          |
	/// |                                                                               |       Bottom     t                  |                                          |
	/// |                                                                               |        Left      o------Right-------|               RightTopRight3             |
	/// |                                                                               R                  m                  |                                          |
	/// |                                   Left                                        o                  |                  |                                          |
	/// |                                                                               o-----------------------------------Right----------------------------------------|
	/// |                                                                               t                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                 RightBottom                                    |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// |                                                                               |                                                                                |
	/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------
	/// </summary>
	private class TestTree
	{
		public SplitNode Root;
		public LeafNode Left;
		public SplitNode Right;
		public SplitNode RightTop;
		public SplitNode RightTopLeft;
		public LeafNode RightTopLeftTop;
		public SplitNode RightTopLeftBottom;
		public LeafNode RightTopLeftBottomLeft;
		public SplitNode RightTopLeftBottomRight;
		public LeafNode RightTopLeftBottomRightTop;
		public LeafNode RightTopLeftBottomRightBottom;
		public SplitNode RightTopRight;
		public LeafNode RightTopRight1;
		public LeafNode RightTopRight2;
		public LeafNode RightTopRight3;
		public LeafNode RightBottom;

		public TestTree(
			Mock<IWindow>? leftWindow = null,
			Mock<IWindow>? rightTopLeftTopWindow = null,
			Mock<IWindow>? rightTopRight1Window = null,
			Mock<IWindow>? rightTopRight2Window = null,
			Mock<IWindow>? rightTopRight3Window = null,
			Mock<IWindow>? rightTopLeftBottomLeftWindow = null,
			Mock<IWindow>? rightTopLeftBottomRightTopWindow = null,
			Mock<IWindow>? rightTopLeftBottomRightBottomWindow = null,
			Mock<IWindow>? rightBottomWindow = null
		)
		{
			leftWindow ??= new Mock<IWindow>();
			rightTopLeftTopWindow ??= new Mock<IWindow>();
			rightTopLeftBottomLeftWindow ??= new Mock<IWindow>();
			rightTopLeftBottomRightTopWindow ??= new Mock<IWindow>();
			rightTopLeftBottomRightBottomWindow ??= new Mock<IWindow>();
			rightTopRight1Window ??= new Mock<IWindow>();
			rightTopRight2Window ??= new Mock<IWindow>();
			rightTopRight3Window ??= new Mock<IWindow>();
			rightBottomWindow ??= new Mock<IWindow>();

			Root = new SplitNode(NodeDirection.Right);

			// left
			Left = new LeafNode(leftWindow.Object, Root) { Weight = 0.5 };
			Root.Children.Add(Left);

			// Right
			Right = new SplitNode(NodeDirection.Bottom, Root) { Weight = 0.5 };
			Root.Children.Add(Right);

			// RightTop
			RightTop = new SplitNode(NodeDirection.Right, Right) { Weight = 0.5 };
			Right.Children.Add(RightTop);

			// RightTopLeft
			RightTopLeft = new SplitNode(NodeDirection.Bottom, RightTop) { Weight = 0.5 };
			RightTop.Children.Add(RightTopLeft);

			// RightBottom
			RightBottom = new LeafNode(rightBottomWindow.Object, Right) { Weight = 0.5 };
			Right.Children.Add(RightBottom);

			// RightTopLeftTop
			RightTopLeftTop = new LeafNode(rightTopLeftTopWindow.Object, RightTopLeft) { Weight = 0.5 };
			RightTopLeft.Children.Add(RightTopLeftTop);

			// RightTopLeftBottom
			RightTopLeftBottom = new SplitNode(NodeDirection.Right, RightTopLeft) { Weight = 0.5 };
			RightTopLeft.Children.Add(RightTopLeftBottom);

			// RightTopLeftBottomLeft
			RightTopLeftBottomLeft = new LeafNode(rightTopLeftBottomLeftWindow.Object, RightTopLeftBottom) { Weight = 0.5 };
			RightTopLeftBottom.Children.Add(RightTopLeftBottomLeft);

			// RightTopLeftBottomRight
			RightTopLeftBottomRight = new SplitNode(NodeDirection.Bottom, RightTopLeftBottom) { Weight = 0.5 };
			RightTopLeftBottom.Children.Add(RightTopLeftBottomRight);

			// RightTopLeftBottomRightTop
			RightTopLeftBottomRightTop = new LeafNode(rightTopLeftBottomRightTopWindow.Object, RightTopLeftBottomRight) { Weight = 0.5 };
			RightTopLeftBottomRight.Children.Add(RightTopLeftBottomRightTop);

			// RightTopLeftBottomRightBottom
			RightTopLeftBottomRightBottom = new LeafNode(rightTopLeftBottomRightBottomWindow.Object, RightTopLeftBottomRight) { Weight = 0.5 };
			RightTopLeftBottomRight.Children.Add(RightTopLeftBottomRightBottom);

			// RightTopRight
			RightTopRight = new SplitNode(NodeDirection.Bottom, RightTop) { Weight = 0.5 };
			RightTop.Children.Add(RightTopRight);

			// RightTopRight1
			RightTopRight1 = new LeafNode(rightTopRight1Window.Object, RightTopRight) { Weight = 1d / 3 };
			RightTopRight.Children.Add(RightTopRight1);

			// RightTopRight2
			RightTopRight2 = new LeafNode(rightTopRight2Window.Object, RightTopRight) { Weight = 1d / 3 };
			RightTopRight.Children.Add(RightTopRight2);

			// RightTopRight3
			RightTopRight3 = new LeafNode(rightTopRight3Window.Object, RightTopRight) { Weight = 1d / 3 };
			RightTopRight.Children.Add(RightTopRight3);
		}
	}

	private static class TestTreeWindowLocations
	{
		public static ILocation<double> Left = new NodeLocation() { X = 0, Y = 0, Width = 0.5, Height = 1 };
		public static ILocation<double> RightBottom = new NodeLocation() { X = 0.5, Y = 0.5, Width = 0.5, Height = 0.5 };
		public static ILocation<double> RightTopLeftTop = new NodeLocation() { X = 0.5, Y = 0, Width = 0.25, Height = 0.25 };
		public static ILocation<double> RightTopLeftBottomLeft = new NodeLocation() { X = 0.5, Y = 0.25, Width = 0.125, Height = 0.25 };
		public static ILocation<double> RightTopLeftBottomRightTop = new NodeLocation() { X = 0.625, Y = 0.25, Width = 0.125, Height = 0.175 };
		public static ILocation<double> RightTopLeftBottomRightBottom = new NodeLocation() { X = 0.625, Y = 0.425, Width = 0.125, Height = 0.075 };
		public static ILocation<double> RightTopRight1 = new NodeLocation() { X = 0.75, Y = 0, Width = 0.25, Height = 0.5 * 1d / 3 };
		public static ILocation<double> RightTopRight2 = new NodeLocation() { X = 0.75, Y = 0.5 * 1d / 3, Width = 0.25, Height = 0.5 * 1d / 3 };
		public static ILocation<double> RightTopRight3 = new NodeLocation() { X = 0.75, Y = 1d / 3, Width = 0.25, Height = 0.5 * 1d / 3 };

		public static ILocation<double>[] All = new ILocation<double>[]
		{
			Left,
			RightTopLeftTop,
			RightBottom,
			RightTopLeftBottomLeft,
			RightTopLeftBottomRightTop,
			RightTopLeftBottomRightBottom,
			RightTopRight1,
			RightTopRight2,
			RightTopRight3
		};
	}

	[Fact]
	public void GetWeightAndIndex_Left()
	{
		TestTree tree = new();

		(double weight, double precedingWeight) = TreeLayoutEngine.GetWeightAndIndex(tree.Root, tree.Left);
		Assert.Equal(0.5, weight);
		Assert.Equal(0, precedingWeight);
	}

	[Fact]
	public void GetWeightAndIndex_RightTopLeftBottomRightTop()
	{
		TestTree tree = new();

		(double weight, double precedingWeight) = TreeLayoutEngine.GetWeightAndIndex(tree.RightTopLeftBottomRight, tree.RightTopLeftBottomRightTop);
		Assert.Equal(0.7, weight);
		Assert.Equal(0, precedingWeight);
	}

	[Fact]
	public void GetWeightAndIndex_RightTopLeftBottomRightBottom()
	{
		TestTree tree = new();

		(double weight, double precedingWeight) = TreeLayoutEngine.GetWeightAndIndex(tree.RightTopLeftBottomRight, tree.RightTopLeftBottomRightBottom);
		Assert.Equal(0.3, weight);
		Assert.Equal(0.7, precedingWeight);
	}

	[Fact]
	public void GetWeightAndIndex_RightTopRight1()
	{
		TestTree tree = new();

		(double weight, double precedingWeight) = TreeLayoutEngine.GetWeightAndIndex(tree.RightTopRight, tree.RightTopRight1);
		Assert.Equal(1d / 3, weight);
		Assert.Equal(0, precedingWeight);
	}

	[Fact]
	public void GetWeightAndIndex_RightTopRight2()
	{
		TestTree tree = new();

		(double weight, double precedingWeight) = TreeLayoutEngine.GetWeightAndIndex(tree.RightTopRight, tree.RightTopRight2);
		Assert.Equal(1d / 3, weight);
		Assert.Equal(1d / 3, precedingWeight);
	}

	[Fact]
	public void GetWeightAndIndex_RightTopRight3()
	{
		TestTree tree = new();

		(double weight, double precedingWeight) = TreeLayoutEngine.GetWeightAndIndex(tree.RightTopRight, tree.RightTopRight3);
		Assert.Equal(1d / 3, weight);
		Assert.Equal(2d / 3, precedingWeight);
	}

	[Fact]
	public void GetNodeLocation_Left()
	{
		TestTree tree = new();

		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(tree.Left);

		Assert.NotNull(location);
		Assert.Equal(TestTreeWindowLocations.Left, location);
	}

	[Fact]
	public void GetNodeLocation_RightBottom()
	{
		TestTree tree = new();

		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(tree.RightBottom);

		Assert.NotNull(location);
		Assert.Equal(TestTreeWindowLocations.RightBottom, location);
	}

	[Fact]
	public void GetNodeLocation_RightTopLeftTop()
	{
		TestTree tree = new();

		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(tree.RightTopLeftTop);

		Assert.NotNull(location);
		Assert.Equal(TestTreeWindowLocations.RightTopLeftTop, location);
	}

	[Fact]
	public void GetNodeLocation_RightTopLeftBottomLeft()
	{
		TestTree tree = new();

		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(tree.RightTopLeftBottomLeft);

		Assert.NotNull(location);
		Assert.Equal(TestTreeWindowLocations.RightTopLeftBottomLeft, location);
	}

	[Fact]
	public void GetNodeLocation_RightTopLeftBottomRightTop()
	{
		TestTree tree = new();

		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(tree.RightTopLeftBottomRightTop);

		Assert.NotNull(location);
		Assert.Equal(TestTreeWindowLocations.RightTopLeftBottomRightTop, location);
	}

	[Fact]
	public void GetNodeLocation_RightTopLeftBottomRightBottom()
	{
		TestTree tree = new();

		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(tree.RightTopLeftBottomRightBottom);

		Assert.NotNull(location);
		Assert.Equal(TestTreeWindowLocations.RightTopLeftBottomRightBottom, location);
	}

	[Fact]
	public void GetNodeLocation_RightTopRight1()
	{
		TestTree tree = new();

		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(tree.RightTopRight1);

		Assert.NotNull(location);
		Assert.Equal(TestTreeWindowLocations.RightTopRight1, location);
	}

	[Fact]
	public void GetNodeLocation_RightTopRight2()
	{
		TestTree tree = new();

		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(tree.RightTopRight2);

		Assert.NotNull(location);
		Assert.Equal(TestTreeWindowLocations.RightTopRight2, location);
	}

	[Fact]
	public void GetNodeLocation_RightTopRight3()
	{
		TestTree tree = new();

		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(tree.RightTopRight3);

		Assert.NotNull(location);
		Assert.Equal(TestTreeWindowLocations.RightTopRight3, location);
	}

	[Fact]
	public void GetLeftMostLeaf()
	{
		TestTree tree = new();

		LeafNode? node = TreeLayoutEngine.GetLeftMostLeaf(tree.Left);

		Assert.Equal(tree.Left, node);
	}

	[Fact]
	public void GetRightMostLeaf()
	{
		TestTree tree = new();

		LeafNode? node = TreeLayoutEngine.GetRightMostLeaf(tree.RightBottom);

		Assert.Equal(tree.RightBottom, node);
	}

	[Fact]
	public void Add_Root()
	{
		Logger.Initialize();

		Mock<IWorkspaceManager> workspaceManager = new();
		Mock<IConfigContext> configContext = new();
		configContext.Setup(x => x.WorkspaceManager).Returns(workspaceManager.Object);

		TreeLayoutEngine engine = new(configContext.Object);

		Mock<IWindow> window = new();
		engine.Add(window.Object);

		Assert.Equal(engine.Root, new LeafNode(window.Object));
	}

	[Fact]
	public void Add_TestTree()
	{
		Logger.Initialize();

		Mock<IWorkspace> activeWorkspace = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		workspaceManager.Setup(x => x.ActiveWorkspace).Returns(activeWorkspace.Object);
		Mock<IConfigContext> configContext = new();
		configContext.Setup(x => x.WorkspaceManager).Returns(workspaceManager.Object);

		TreeLayoutEngine engine = new(configContext.Object);

		Mock<IWindow> leftWindow = new();
		leftWindow.Setup(m => m.ToString()).Returns("leftWindow");
		Mock<IWindow> rightTopLeftTopWindow = new();
		rightTopLeftTopWindow.Setup(m => m.ToString()).Returns("rightTopLeftTopWindow");
		Mock<IWindow> rightBottomWindow = new();
		rightBottomWindow.Setup(m => m.ToString()).Returns("rightBottomWindow");
		Mock<IWindow> rightTopRight1Window = new();
		rightTopRight1Window.Setup(m => m.ToString()).Returns("rightTopRight1Window");
		Mock<IWindow> rightTopRight2Window = new();
		rightTopRight2Window.Setup(m => m.ToString()).Returns("rightTopRight2Window");
		Mock<IWindow> rightTopRight3Window = new();
		rightTopRight3Window.Setup(m => m.ToString()).Returns("rightTopRight3Window");
		Mock<IWindow> rightTopLeftBottomLeftWindow = new();
		rightTopLeftBottomLeftWindow.Setup(m => m.ToString()).Returns("rightTopLeftBottomLeftWindow");
		Mock<IWindow> rightTopLeftBottomRightTopWindow = new();
		rightTopLeftBottomRightTopWindow.Setup(m => m.ToString()).Returns("rightTopLeftBottomRightTopWindow");
		Mock<IWindow> rightTopLeftBottomRightBottomWindow = new();
		rightTopLeftBottomRightBottomWindow.Setup(m => m.ToString()).Returns("rightTopLeftBottomRightBottomWindow");

		engine.Add(leftWindow.Object);
		activeWorkspace.Setup(x => x.FocusedWindow).Returns(leftWindow.Object);

		engine.Add(rightTopLeftTopWindow.Object);
		activeWorkspace.Setup(x => x.FocusedWindow).Returns(rightTopLeftTopWindow.Object);

		engine.Direction = NodeDirection.Bottom;
		engine.Add(rightBottomWindow.Object);

		activeWorkspace.Setup(x => x.FocusedWindow).Returns(rightTopLeftTopWindow.Object);
		engine.Direction = NodeDirection.Right;

		engine.Add(rightTopRight1Window.Object);
		activeWorkspace.Setup(x => x.FocusedWindow).Returns(rightTopRight1Window.Object);
		engine.Direction = NodeDirection.Bottom;

		engine.Add(rightTopRight2Window.Object);
		activeWorkspace.Setup(x => x.FocusedWindow).Returns(rightTopRight2Window.Object);

		engine.Add(rightTopRight3Window.Object);

		activeWorkspace.Setup(x => x.FocusedWindow).Returns(rightTopLeftTopWindow.Object);
		engine.Direction = NodeDirection.Bottom;

		engine.Add(rightTopLeftBottomLeftWindow.Object);
		activeWorkspace.Setup(x => x.FocusedWindow).Returns(rightTopLeftBottomLeftWindow.Object);
		engine.Direction = NodeDirection.Right;

		engine.Add(rightTopLeftBottomRightTopWindow.Object);
		activeWorkspace.Setup(x => x.FocusedWindow).Returns(rightTopLeftBottomRightTopWindow.Object);
		engine.Direction = NodeDirection.Bottom;

		engine.Add(rightTopLeftBottomRightBottomWindow.Object);


		TestTree tree = new(
			leftWindow: leftWindow,
			rightTopLeftTopWindow: rightTopLeftTopWindow,
			rightBottomWindow: rightBottomWindow,
			rightTopRight1Window: rightTopRight1Window,
			rightTopRight2Window: rightTopRight2Window,
			rightTopRight3Window: rightTopRight3Window,
			rightTopLeftBottomLeftWindow: rightTopLeftBottomLeftWindow,
			rightTopLeftBottomRightTopWindow: rightTopLeftBottomRightTopWindow,
			rightTopLeftBottomRightBottomWindow: rightTopLeftBottomRightBottomWindow
		);
		Assert.Equal(engine.Root, tree.Root);
	}



	//[Fact]
	//public void DoLayout_UnitSquare()
	//{
	//	TestTree tree = new();

	//	ILocation<int> screen = new Location(0, 0, 1920, 1080);

	//	IWindowLocation[] locations = TreeLayoutEngine.DoLayout(screen, tree.Root).ToArray();

	//	for (int i = 0; i < locations.Length; i++)
	//	{
	//		Assert.Equal(TestTreeWindowLocations.All[i].ToLocation(screen), locations[i].Location);
	//	}
	//}

	// TODO: Add
	// TODO: Remove
}

