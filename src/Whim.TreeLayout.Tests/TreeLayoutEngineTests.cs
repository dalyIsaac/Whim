using FluentAssertions;
using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class Tests
{
	public Tests()
	{
		Logger.Initialize();
	}

	#region GetWeightAndIndex
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
	#endregion

	#region GetNodeLocation
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
	#endregion

	#region GetLeftMostLeaf
	[Fact]
	public void GetLeftMostLeaf()
	{
		TestTree tree = new();

		LeafNode? node = TreeLayoutEngine.GetLeftMostLeaf(tree.Left);

		Assert.Equal(tree.Left, node);
	}
	#endregion

	#region GetRightMostLeaf
	[Fact]
	public void GetRightMostLeaf()
	{
		TestTree tree = new();

		LeafNode? node = TreeLayoutEngine.GetRightMostLeaf(tree.RightBottom);

		Assert.Equal(tree.RightBottom, node);
	}
	#endregion

	#region Add
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
		TestTreeEngine testEngine = new();

		TestTree tree = new(
			leftWindow: testEngine.LeftWindow,
			rightTopLeftTopWindow: testEngine.RightTopLeftTopWindow,
			rightBottomWindow: testEngine.RightBottomWindow,
			rightTopRight1Window: testEngine.RightTopRight1Window,
			rightTopRight2Window: testEngine.RightTopRight2Window,
			rightTopRight3Window: testEngine.RightTopRight3Window,
			rightTopLeftBottomLeftWindow: testEngine.RightTopLeftBottomLeftWindow,
			rightTopLeftBottomRightTopWindow: testEngine.RightTopLeftBottomRightTopWindow,
			rightTopLeftBottomRightBottomWindow: testEngine.RightTopLeftBottomRightBottomWindow
		);
		Assert.Equal(testEngine.Engine.Root, tree.Root);
	}
	#endregion

	#region Remove
	[Fact]
	public void Remove_IllegalWindow()
	{
		Mock<IConfigContext> configContext = new();

		TreeLayoutEngine engine = new(configContext.Object);
		engine.Add(new Mock<IWindow>().Object);

		Assert.False(engine.Remove(new Mock<IWindow>().Object));
		Assert.NotNull(engine.Root);
	}

	[Fact]
	public void Remove_Root_SingleNodeTree()
	{
		Mock<IConfigContext> configContext = new();
		Mock<IWindow> window = new();

		TreeLayoutEngine engine = new(configContext.Object);
		engine.Add(window.Object);

		Assert.True(engine.Remove(window.Object));
		Assert.Null(engine.Root);
	}

	/// <summary>
	/// Removes the single child window from a split node, where the split node is the root.
	/// This will cause the root to become the child window's leaf node.
	/// </summary>
	[Fact]
	public void Remove_Split_ParentIsRoot()
	{
		// Set up the active workspace
		Mock<IWorkspace> activeWorkspace = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		workspaceManager.Setup(x => x.ActiveWorkspace).Returns(activeWorkspace.Object);

		// Set up the config context
		Mock<IConfigContext> configContext = new();
		configContext.Setup(x => x.WorkspaceManager).Returns(workspaceManager.Object);

		// Set up the windows
		Mock<IWindow> leftWindow = new();
		Mock<IWindow> rightWindow = new();

		// Set up the engine
		TreeLayoutEngine engine = new(configContext.Object);
		engine.Add(leftWindow.Object);
		activeWorkspace.Setup(x => x.FocusedWindow).Returns(leftWindow.Object);
		engine.Add(rightWindow.Object);

		// The root should be a split node, with two children.
		Assert.True(engine.Remove(leftWindow.Object));
		Assert.True(engine.Root is LeafNode);
		Assert.Equal(rightWindow.Object, (engine.Root as LeafNode)?.Window);
	}

	/// <summary>
	/// Removes the single child window from a split node, where the split node is not the root.
	/// This will cause the split node to be removed.
	/// </summary>
	[Fact]
	public void Remove_Split_ParentIsNotRoot()
	{
		// Set up the active workspace
		Mock<IWorkspace> activeWorkspace = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		workspaceManager.Setup(x => x.ActiveWorkspace).Returns(activeWorkspace.Object);

		// Set up the config context
		Mock<IConfigContext> configContext = new();
		configContext.Setup(x => x.WorkspaceManager).Returns(workspaceManager.Object);

		// Set up the windows
		Mock<IWindow> leftWindow = new();
		Mock<IWindow> rightWindow1 = new();
		Mock<IWindow> rightWindow2 = new();

		// Set up the engine
		TreeLayoutEngine engine = new(configContext.Object);

		engine.Add(leftWindow.Object);
		activeWorkspace.Setup(x => x.FocusedWindow).Returns(leftWindow.Object);

		engine.Add(rightWindow1.Object);
		activeWorkspace.Setup(x => x.FocusedWindow).Returns(rightWindow1.Object);

		engine.Direction = NodeDirection.Down;
		engine.Add(rightWindow2.Object);

		// The root should be a split node, with two children.
		Assert.True(engine.Remove(rightWindow1.Object));
		Assert.True(engine.Root is SplitNode);
		Assert.Equal(2, (engine.Root as SplitNode)?.Children.Count);
		Assert.Equal(leftWindow.Object, ((engine.Root as SplitNode)?.Children[0] as LeafNode)?.Window);
		Assert.Equal(rightWindow2.Object, ((engine.Root as SplitNode)?.Children[1] as LeafNode)?.Window);
	}

	/// <summary>
	/// Removes a child window from a split node, where the split node has more than 2 children.
	/// This will cause the child window to be removed from the split node.
	/// </summary>
	[Fact]
	public void Remove_Child()
	{
		// Set up the active workspace
		Mock<IWorkspace> activeWorkspace = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		workspaceManager.Setup(x => x.ActiveWorkspace).Returns(activeWorkspace.Object);

		// Set up the config context
		Mock<IConfigContext> configContext = new();
		configContext.Setup(x => x.WorkspaceManager).Returns(workspaceManager.Object);

		// Set up the windows
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();

		// Set up the engine
		TreeLayoutEngine engine = new(configContext.Object);

		engine.Add(window1.Object);
		engine.Add(window2.Object);
		engine.Add(window3.Object);

		// The root should be a split node, with three children.
		Assert.True(engine.Remove(window2.Object));
		Assert.True(engine.Root is SplitNode);
		Assert.Equal(2, (engine.Root as SplitNode)?.Children.Count);
		Assert.Equal(window1.Object, ((engine.Root as SplitNode)?.Children[0] as LeafNode)?.Window);
		Assert.Equal(window3.Object, ((engine.Root as SplitNode)?.Children[1] as LeafNode)?.Window);
	}
	#endregion

	#region DoLayout
	[Fact]
	public void DoLayout()
	{
		TestTree tree = new();

		ILocation<int> screen = new Location(0, 0, 1920, 1080);

		IWindowLocation[] locations = TreeLayoutEngine.DoLayout(tree.Root, screen).ToArray();
		ILocation<int>[] actual = locations.Select(x => x.Location).ToArray();

		ILocation<int>[] expected = TestTreeWindowLocations.All.Select(x => x.ToLocation(screen)).ToArray();

		actual.Should().Equal(expected);
	}
	#endregion

	#region GetAdjacentNode
	/// <summary>
	/// There is no node to the left of Left.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_Left()
	{
		// Set up the populated tree layout engine
		TestTreeEngine testEngine = new();

		// Set up the monitor which we are using to calculate the adjacent position.
		Mock<IMonitor> monitor = new();
		monitor.Setup(m => m.Width).Returns(1920);
		monitor.Setup(m => m.Height).Returns(1080);

		// The root should be a split node, with two children.
		Assert.Null(testEngine.Engine.GetAdjacentNode(testEngine.LeftWindow.Object, WindowDirection.Left, monitor.Object));
	}

	/// <summary>
	/// The node to the left of RightBottom is Left.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_RightBottom_Left()
	{
		// Set up the populated tree layout engine
		TestTreeEngine testEngine = new();

		// Set up the monitor which we are using to calculate the adjacent position.
		Mock<IMonitor> monitor = new();
		monitor.Setup(m => m.Width).Returns(1920);
		monitor.Setup(m => m.Height).Returns(1080);

		Assert.Equal(testEngine.LeftNode, testEngine.Engine.GetAdjacentNode(testEngine.RightBottomWindow.Object, WindowDirection.Left, monitor.Object));
	}

	/// <summary>
	/// The node above RightBottom should be RightTopLeftBottomLeft.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_RightBottom_Up()
	{
		// Set up the populated tree layout engine
		TestTreeEngine testEngine = new();

		// Set up the monitor which we are using to calculate the adjacent position.
		Mock<IMonitor> monitor = new();
		monitor.Setup(m => m.Width).Returns(1920);
		monitor.Setup(m => m.Height).Returns(1080);

		Assert.Equal(testEngine.RightTopLeftBottomLeftNode, testEngine.Engine.GetAdjacentNode(testEngine.RightBottomWindow.Object, WindowDirection.Up, monitor.Object));
	}

	/// <summary>
	/// There is no node to the right or below RightBottom.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_RightBottom_Null()
	{
		// Set up the populated tree layout engine
		TestTreeEngine testEngine = new();

		// Set up the monitor which we are using to calculate the adjacent position.
		Mock<IMonitor> monitor = new();
		monitor.Setup(m => m.Width).Returns(1920);
		monitor.Setup(m => m.Height).Returns(1080);

		Assert.Null(testEngine.Engine.GetAdjacentNode(testEngine.RightBottomWindow.Object, WindowDirection.Right, monitor.Object));
		Assert.Null(testEngine.Engine.GetAdjacentNode(testEngine.RightBottomWindow.Object, WindowDirection.Down, monitor.Object));
	}

	/// <summary>
	/// The node to the left of RightTopRight3 is RightTopLeftBottomRightTop.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_RightTopRight3_Left()
	{
		// Set up the populated tree layout engine
		TestTreeEngine testEngine = new();

		// Set up the monitor which we are using to calculate the adjacent position.
		Mock<IMonitor> monitor = new();
		monitor.Setup(m => m.Width).Returns(1920);
		monitor.Setup(m => m.Height).Returns(1080);

		Assert.Equal(testEngine.RightTopLeftBottomRightTopNode, testEngine.Engine.GetAdjacentNode(testEngine.RightTopRight3Window.Object, WindowDirection.Left, monitor.Object));
	}

	/// <summary>
	/// The node to the right of RightTopLeftBottomRightTop is RightTopRight2.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_RightTopLeftBottomRightTop_Right()
	{
		// Set up the populated tree layout engine
		TestTreeEngine testEngine = new();

		// Set up the monitor which we are using to calculate the adjacent position.
		Mock<IMonitor> monitor = new();
		monitor.Setup(m => m.Width).Returns(1920);
		monitor.Setup(m => m.Height).Returns(1080);

		Assert.Equal(testEngine.RightTopRight2Node, testEngine.Engine.GetAdjacentNode(testEngine.RightTopLeftBottomRightTopWindow.Object, WindowDirection.Right, monitor.Object));
	}

	/// <summary>
	/// The node to the right of RightTopLeftBottomRightTop is RightTopLeftBottomRightBottom.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_RightTopLeftBottomRightTop_Down()
	{
		// Set up the populated tree layout engine
		TestTreeEngine testEngine = new();

		// Set up the monitor which we are using to calculate the adjacent position.
		Mock<IMonitor> monitor = new();
		monitor.Setup(m => m.Width).Returns(1920);
		monitor.Setup(m => m.Height).Returns(1080);

		Assert.Equal(testEngine.RightTopLeftBottomRightBottomNode, testEngine.Engine.GetAdjacentNode(testEngine.RightTopLeftBottomRightTopWindow.Object, WindowDirection.Down, monitor.Object));
	}

	/// <summary>
	/// The node to the left of RightTopLeftBottomRightTop is RightTopLeftBottomLeft.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_RightTopLeftBottomRightTop_Left()
	{
		// Set up the populated tree layout engine
		TestTreeEngine testEngine = new();

		// Set up the monitor which we are using to calculate the adjacent position.
		Mock<IMonitor> monitor = new();
		monitor.Setup(m => m.Width).Returns(1920);
		monitor.Setup(m => m.Height).Returns(1080);

		Assert.Equal(testEngine.RightTopLeftBottomLeftNode, testEngine.Engine.GetAdjacentNode(testEngine.RightTopLeftBottomRightTopWindow.Object, WindowDirection.Left, monitor.Object));
	}

	/// <summary>
	/// The node above RightTopLeftBottomRightTop is RightTopLeftTop.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_RightTopLeftBottomRightTop_Up()
	{
		// Set up the populated tree layout engine
		TestTreeEngine testEngine = new();

		// Set up the monitor which we are using to calculate the adjacent position.
		Mock<IMonitor> monitor = new();
		monitor.Setup(m => m.Width).Returns(1920);
		monitor.Setup(m => m.Height).Returns(1080);

		Assert.Equal(testEngine.RightTopLeftTopNode, testEngine.Engine.GetAdjacentNode(testEngine.RightTopLeftBottomRightTopWindow.Object, WindowDirection.Up, monitor.Object));
	}
	#endregion
}

