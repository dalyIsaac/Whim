using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestRemove
{
	private readonly Mock<IWorkspace> _activeWorkspace = new();
	private readonly Mock<IWorkspaceManager> _workspaceManager = new();
	private readonly Mock<IMonitor> _focusedMonitor = new();
	private readonly Mock<IMonitorManager> _monitorManager = new();
	private readonly Mock<IConfigContext> _configContext = new();
	private readonly TreeLayoutEngine _engine;

	public TestRemove()
	{
		_monitorManager.Setup(m => m.FocusedMonitor).Returns(_focusedMonitor.Object);
		_workspaceManager.Setup(x => x.ActiveWorkspace).Returns(_activeWorkspace.Object);

		_configContext.Setup(x => x.MonitorManager).Returns(_monitorManager.Object);
		_configContext.Setup(c => c.WorkspaceManager).Returns(_workspaceManager.Object);

		_engine = new TreeLayoutEngine(_configContext.Object);
	}

	[Fact]
	public void Remove_IllegalWindow()
	{
		_engine.Add(new Mock<IWindow>().Object);

		Assert.False(_engine.Remove(new Mock<IWindow>().Object));
		Assert.NotNull(_engine.Root);
	}

	[Fact]
	public void Remove_Root_SingleNodeTree()
	{
		Mock<IWindow> window = new();

		_engine.Add(window.Object);

		Assert.True(_engine.Remove(window.Object));
		Assert.Null(_engine.Root);
	}

	/// <summary>
	/// Removes the single child window from a split node, where the split node is the root.
	/// This will cause the root to become the child window's leaf node.
	/// </summary>
	[Fact]
	public void Remove_Split_ParentIsRoot()
	{
		// Set up the windows
		Mock<IWindow> leftWindow = new();
		Mock<IWindow> rightWindow = new();

		// Set up the engine
		_engine.Add(leftWindow.Object);
		_activeWorkspace.Setup(x => x.LastFocusedWindow).Returns(leftWindow.Object);
		_engine.Add(rightWindow.Object);

		// The root should be a split node, with two children.
		Assert.True(_engine.Remove(leftWindow.Object));
		Assert.True(_engine.Root is WindowNode);
		Assert.Equal(rightWindow.Object, (_engine.Root as WindowNode)?.Window);
	}

	/// <summary>
	/// Removes the single child window from a split node, where the split node is not the root.
	/// This will cause the split node to be removed.
	/// </summary>
	[Fact]
	public void Remove_Split_ParentIsNotRoot()
	{
		// Set up the windows
		Mock<IWindow> leftWindow = new();
		Mock<IWindow> rightWindow1 = new();
		Mock<IWindow> rightWindow2 = new();

		// Set up the engine
		_engine.Add(leftWindow.Object);
		_activeWorkspace.Setup(x => x.LastFocusedWindow).Returns(leftWindow.Object);

		_engine.Add(rightWindow1.Object);
		_activeWorkspace.Setup(x => x.LastFocusedWindow).Returns(rightWindow1.Object);

		_engine.AddNodeDirection = Direction.Down;
		_engine.Add(rightWindow2.Object);

		// The root should be a split node, with two children.
		Assert.True(_engine.Remove(rightWindow1.Object));
		Assert.True(_engine.Root is SplitNode);

		SplitNode root = (_engine.Root as SplitNode)!;
		Assert.Equal(2, root.Count);
		Assert.Equal(leftWindow.Object, (root[0].node as WindowNode)?.Window);
		Assert.Equal(rightWindow2.Object, (root[1].node as WindowNode)?.Window);
	}

	/// <summary>
	/// Removes a child window from a split node, where the split node has more than 2 children.
	/// This will cause the child window to be removed from the split node.
	/// </summary>
	[Fact]
	public void Remove_Child()
	{
		// Set up the windows
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();

		// Set up the engine
		_engine.Add(window1.Object);
		_engine.Add(window2.Object);
		_engine.Add(window3.Object);

		// The root should be a split node, with three children.
		Assert.True(_engine.Remove(window2.Object));
		Assert.True(_engine.Root is SplitNode);

		SplitNode root = (_engine.Root as SplitNode)!;
		Assert.Equal(2, root.Count);
		Assert.Equal(window1.Object, (root[0].node as WindowNode)?.Window);
		Assert.Equal(window3.Object, (root[1].node as WindowNode)?.Window);
	}

	/// <summary>
	/// Removes a child window, so that a sibling window becomes the new root.
	/// The new root should have no parent.
	/// </summary>
	[Fact]
	public void Remove_Child_RootBecomesSibling()
	{
		// Set up the windows
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();

		// Set up the engine
		_engine.Add(window1.Object);
		_engine.Add(window2.Object);

		// The root should be a split node, with three children.
		Assert.True(_engine.Remove(window2.Object));
		Assert.True(_engine.Root is WindowNode);
		Assert.Equal(window1.Object, (_engine.Root as WindowNode)?.Window);
		Assert.Null((_engine.Root as WindowNode)?.Parent);
	}
}
