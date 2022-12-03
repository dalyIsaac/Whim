using Moq;
using Xunit;

namespace Whim.Tests;

public class WorkspacerTests
{
	[Fact]
	public void Rename()
	{
		// Given
		Mock<ILayoutEngine> layoutEngine = new();
		layoutEngine.Setup(e => e.Name).Returns("Layout");

		Mock<IConfigContext> configContext = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		configContext.Setup(c => c.WorkspaceManager).Returns(workspaceManager.Object);

#pragma warning disable IDE0017 // Simplify object initialization
		Workspace workspace = new(configContext.Object, "Workspace", layoutEngine.Object);
#pragma warning restore IDE0017 // Simplify object initialization

		// When
#pragma warning disable IDE0017 // Simplify object initialization
		workspace.Name = "Workspace2";
#pragma warning restore IDE0017 // Simplify object initialization

		// Then
		Assert.Equal("Workspace2", workspace.Name);
		workspace.Dispose();
	}

	[Fact]
	public void TrySetLayoutEngine_CannotFindEngine()
	{
		// Given
		Mock<ILayoutEngine> layoutEngine = new();
		layoutEngine.Setup(e => e.Name).Returns("Layout");

		Mock<IConfigContext> configContext = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		configContext.Setup(c => c.WorkspaceManager).Returns(workspaceManager.Object);

		Workspace workspace = new(configContext.Object, "Workspace", layoutEngine.Object);

		// When
		bool result = workspace.TrySetLayoutEngine("Layout2");

		// Then
		Assert.False(result);
		workspace.Dispose();
	}

	[Fact]
	public void TrySetLayoutEngine_AlreadyActive()
	{
		// Given
		Mock<ILayoutEngine> layoutEngine = new();
		layoutEngine.Setup(e => e.Name).Returns("Layout");

		Mock<IConfigContext> configContext = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		workspaceManager.Setup(m => m.GetMonitorForWorkspace(It.IsAny<IWorkspace>())).Returns(null as IMonitor);
		configContext.Setup(c => c.WorkspaceManager).Returns(workspaceManager.Object);

		Workspace workspace = new(configContext.Object, "Workspace", layoutEngine.Object);

		// When
		bool result = workspace.TrySetLayoutEngine("Layout");

		// Then
		Assert.True(result);
		workspace.Dispose();
	}

	[Fact]
	public void TrySetLayoutEngine_Success()
	{
		// Given
		Mock<ILayoutEngine> layoutEngine = new();
		layoutEngine.Setup(e => e.Name).Returns("Layout");

		Mock<ILayoutEngine> layoutEngine2 = new();
		layoutEngine2.Setup(e => e.Name).Returns("Layout2");

		Mock<IConfigContext> configContext = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		configContext.Setup(c => c.WorkspaceManager).Returns(workspaceManager.Object);

		Workspace workspace = new(configContext.Object, "Workspace", layoutEngine.Object, layoutEngine2.Object);

		// When
		bool result = workspace.TrySetLayoutEngine("Layout2");

		// Then
		Assert.True(result);
		workspace.Dispose();
	}
}
