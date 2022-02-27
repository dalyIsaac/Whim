using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestRemove
{
	private readonly Mock<IConfigContext> _configContext = new();

	[Fact]
	public void Remove_IllegalWindow()
	{
		TreeLayoutEngine engine = new(_configContext.Object);
		engine.Add(new Mock<IWindow>().Object);

		Assert.False(engine.Remove(new Mock<IWindow>().Object));
		Assert.NotNull(engine.Root);
	}

	[Fact]
	public void Remove_Root_SingleNodeTree()
	{
		Mock<IConfigContext> configContext = new();
		Mock<IWindow> window = new();

		TreeLayoutEngine engine = new(_configContext.Object);
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
		_configContext.Setup(x => x.WorkspaceManager).Returns(workspaceManager.Object);

		// Set up the windows
		Mock<IWindow> leftWindow = new();
		Mock<IWindow> rightWindow = new();

		// Set up the engine
		TreeLayoutEngine engine = new(_configContext.Object);
		engine.Add(leftWindow.Object);
		activeWorkspace.Setup(x => x.LastFocusedWindow).Returns(leftWindow.Object);
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
		_configContext.Setup(x => x.WorkspaceManager).Returns(workspaceManager.Object);

		// Set up the windows
		Mock<IWindow> leftWindow = new();
		Mock<IWindow> rightWindow1 = new();
		Mock<IWindow> rightWindow2 = new();

		// Set up the engine
		TreeLayoutEngine engine = new(_configContext.Object);

		engine.Add(leftWindow.Object);
		activeWorkspace.Setup(x => x.LastFocusedWindow).Returns(leftWindow.Object);

		engine.Add(rightWindow1.Object);
		activeWorkspace.Setup(x => x.LastFocusedWindow).Returns(rightWindow1.Object);

		engine.AddNodeDirection = Direction.Down;
		engine.Add(rightWindow2.Object);

		// The root should be a split node, with two children.
		Assert.True(engine.Remove(rightWindow1.Object));
		Assert.True(engine.Root is SplitNode);

		SplitNode root = (engine.Root as SplitNode)!;
		Assert.Equal(2, root.Count);
		Assert.Equal(leftWindow.Object, (root[0].node as LeafNode)?.Window);
		Assert.Equal(rightWindow2.Object, (root[1].node as LeafNode)?.Window);
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
		_configContext.Setup(x => x.WorkspaceManager).Returns(workspaceManager.Object);

		// Set up the windows
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();

		// Set up the engine
		TreeLayoutEngine engine = new(_configContext.Object);

		engine.Add(window1.Object);
		engine.Add(window2.Object);
		engine.Add(window3.Object);

		// The root should be a split node, with three children.
		Assert.True(engine.Remove(window2.Object));
		Assert.True(engine.Root is SplitNode);

		SplitNode root = (engine.Root as SplitNode)!;
		Assert.Equal(2, root.Count);
		Assert.Equal(window1.Object, (root[0].node as LeafNode)?.Window);
		Assert.Equal(window3.Object, (root[1].node as LeafNode)?.Window);
	}

	/// <summary>
	/// Removes a child window, so that a sibling window becomes the new root.
	/// The new root should have no parent.
	/// </summary>
	[Fact]
	public void Remove_Child_RootBecomesSibling()
	{
		// Set up the active workspace
		Mock<IWorkspace> activeWorkspace = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		workspaceManager.Setup(x => x.ActiveWorkspace).Returns(activeWorkspace.Object);

		// Set up the config context
		Mock<IConfigContext> configContext = new();
		_configContext.Setup(x => x.WorkspaceManager).Returns(workspaceManager.Object);

		// Set up the windows
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();

		// Set up the engine
		TreeLayoutEngine engine = new(_configContext.Object);

		engine.Add(window1.Object);
		engine.Add(window2.Object);

		// The root should be a split node, with three children.
		Assert.True(engine.Remove(window2.Object));
		Assert.True(engine.Root is LeafNode);
		Assert.Equal(window1.Object, (engine.Root as LeafNode)?.Window);
		Assert.Null((engine.Root as LeafNode)?.Parent);
	}
}
