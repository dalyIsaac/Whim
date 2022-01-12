using System.Collections.Generic;
using Moq;
using Xunit;

namespace Whim.Tests;

public class WorkspaceManagerTests
{
	private static Mock<IConfigContext> CreateConfigContextMock()
	{
		Mock<IConfigContext> mock = new();
		mock.Setup(x => x.MonitorManager).Returns(new Mock<IMonitorManager>().Object);
		return mock;
	}

	private static (Mock<IMonitorManager>, Mock<IMonitor>, Mock<IMonitor>) CreateMonitorManagerMock()
	{
		Mock<IMonitor> monitorMock = new();
		Mock<IMonitor> secondMonitorMock = new();

		Mock<IMonitorManager> monitorManagerMock = new();
		monitorManagerMock.Setup(m => m.FocusedMonitor).Returns(monitorMock.Object);

		List<IMonitor> monitors = new() { monitorMock.Object, secondMonitorMock.Object };

		monitorManagerMock.Setup(m => m.GetEnumerator()).Returns(monitors.GetEnumerator());
		return (monitorManagerMock, monitorMock, secondMonitorMock);
	}

	public WorkspaceManagerTests()
	{
		Logger.Initialize();
	}

	[Fact]
	public void AddWorkspace()
	{
		Mock<IConfigContext> configContextMock = CreateConfigContextMock();
		WorkspaceManager workspaceManager = new(configContextMock.Object);

		Assert.Raises<WorkspaceEventArgs>(h => workspaceManager.WorkspaceAdded += h,
			h => workspaceManager.WorkspaceAdded -= h,
			() =>
			{
				workspaceManager.Add(new Mock<IWorkspace>().Object);
			}
		);
	}

	#region Remove
	[Fact]
	public void RemoveUnknownWorkspace()
	{
		Mock<IConfigContext> configContextMock = CreateConfigContextMock();
		WorkspaceManager workspaceManager = new(configContextMock.Object);

		Assert.False(workspaceManager.Remove(new Mock<IWorkspace>().Object));
	}

	[Fact]
	public void RemoveTooManyWorkspaces()
	{
		Mock<IConfigContext> configContextMock = CreateConfigContextMock();
		WorkspaceManager workspaceManager = new(configContextMock.Object);

		Assert.False(workspaceManager.Remove(new Mock<IWorkspace>().Object));
	}

	[Fact]
	public void RemoveWorkspaceNotInList()
	{
		Mock<IConfigContext> configContextMock = CreateConfigContextMock();
		WorkspaceManager workspaceManager = new(configContextMock.Object);

		Assert.False(workspaceManager.Remove(new Mock<IWorkspace>().Object));
	}

	[Fact]
	public void RemoveWorkspace()
	{
		Mock<IConfigContext> configContextMock = CreateConfigContextMock();
		WorkspaceManager workspaceManager = new(configContextMock.Object);

		Mock<IWindow> windowMock = new();

		Mock<IWorkspace> workspace = new();
		workspace.Setup(w => w.Name).Returns("Test");
		workspace.Setup(w => w.Windows).Returns(new[] { windowMock.Object });

		Mock<IWorkspace> otherWorkspace = new();

		workspaceManager.Add(workspace.Object);
		workspaceManager.Add(otherWorkspace.Object);

		Assert.RaisedEvent<WorkspaceEventArgs>? result = Assert.Raises<WorkspaceEventArgs>(
			h => workspaceManager.WorkspaceRemoved += h,
			h => workspaceManager.WorkspaceRemoved -= h,
			() =>
			{
				workspaceManager.Remove(workspace.Object);
			}
		);

		Assert.Equal(result.Arguments.Workspace, workspace.Object);
		Assert.Null(workspaceManager.TryGet("Test"));

		otherWorkspace.Verify(w => w.AddWindow(windowMock.Object), Times.Once);
	}

	[Fact]
	public void RemoveWorkspaceStringFail()
	{
		Mock<IConfigContext> configContextMock = CreateConfigContextMock();
		WorkspaceManager workspaceManager = new(configContextMock.Object);

		Assert.False(workspaceManager.Remove("Test"));
	}

	[Fact]
	public void RemoveWorkspaceString()
	{
		Mock<IConfigContext> configContextMock = CreateConfigContextMock();
		WorkspaceManager workspaceManager = new(configContextMock.Object);

		Mock<IWindow> windowMock = new();

		Mock<IWorkspace> workspace = new();
		workspace.Setup(w => w.Name).Returns("Test");
		workspace.Setup(w => w.Windows).Returns(new[] { windowMock.Object });

		Mock<IWorkspace> otherWorkspace = new();

		workspaceManager.Add(workspace.Object);
		workspaceManager.Add(otherWorkspace.Object);

		Assert.True(workspaceManager.Remove("Test"));
		Assert.Null(workspaceManager.TryGet("Test"));

		otherWorkspace.Verify(w => w.AddWindow(windowMock.Object), Times.Once);
	}
	#endregion

	[Fact]
	public void Activate()
	{
		// Setup monitor manager
		var (monitorManagerMock, monitorMock, secondMonitorMock) = CreateMonitorManagerMock();

		// Setup config context
		Mock<IConfigContext> configContextMock = CreateConfigContextMock();
		configContextMock.Setup(x => x.MonitorManager).Returns(monitorManagerMock.Object);

		// Setup workspace manager
		WorkspaceManager workspaceManager = new(configContextMock.Object);

		// Setup workspace manager workspaces
		Mock<IWorkspace> workspace = new();
		workspace.Setup(w => w.Name).Returns("First");

		Mock<IWorkspace> secondWorkspace = new();
		secondWorkspace.Setup(w => w.Name).Returns("Second");

		workspaceManager.Add(workspace.Object);
		workspaceManager.Add(secondWorkspace.Object);

		// Activate the two workspaces and monitors
		workspaceManager.Activate(workspace.Object, monitorMock.Object);
		workspaceManager.Activate(secondWorkspace.Object, secondMonitorMock.Object);

		// Now we test the Activate method
		monitorManagerMock.Setup(m => m.FocusedMonitor).Returns(secondMonitorMock.Object);
		var result = Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => workspaceManager.MonitorWorkspaceChanged += h,
			h => workspaceManager.MonitorWorkspaceChanged -= h,
			() =>
			{
				workspaceManager.Activate(workspace.Object);
			}
		);

		// Loser monitor. I'm not sure how to access this from xunit, since Activate throws two
		// events here.
		// Assert.Equal(monitorMock.Object, result.Arguments.Monitor);
		// Assert.Equal(secondWorkspace.Object, result.Arguments.NewWorkspace);
		// Assert.Equal(workspace.Object, result.Arguments.OldWorkspace);

		// Winner monitor
		Assert.Equal(secondMonitorMock.Object, result.Arguments.Monitor);
		Assert.Equal(workspace.Object, result.Arguments.NewWorkspace);
		Assert.Equal(secondWorkspace.Object, result.Arguments.OldWorkspace);

		// Get the monitor for workspace
		Assert.Equal(secondMonitorMock.Object, workspaceManager.GetMonitorForWorkspace(workspace.Object));
	}

	[Fact]
	public void GetMonitorForWorkspaceNull()
	{
		var (monitorManagerMock, monitorMock, secondMonitorMock) = CreateMonitorManagerMock();

		Mock<IConfigContext> configContextMock = CreateConfigContextMock();
		configContextMock.Setup(x => x.MonitorManager).Returns(monitorManagerMock.Object);

		WorkspaceManager workspaceManager = new(configContextMock.Object);

		// Register the monitor in _monitorWorkspaceMap
		workspaceManager.Activate(new Mock<IWorkspace>().Object, monitorMock.Object);
		workspaceManager.Activate(new Mock<IWorkspace>().Object, secondMonitorMock.Object);

		Assert.Null(workspaceManager.GetMonitorForWorkspace(new Mock<IWorkspace>().Object));
	}

	[Fact]
	public void LayoutAllActiveWorkspaces()
	{
		// Setup monitor manager
		var (monitorManagerMock, monitorMock, secondMonitorMock) = CreateMonitorManagerMock();

		// Setup config context
		Mock<IConfigContext> configContextMock = CreateConfigContextMock();
		configContextMock.Setup(x => x.MonitorManager).Returns(monitorManagerMock.Object);

		// Setup workspace manager
		WorkspaceManager workspaceManager = new(configContextMock.Object);

		// Setup workspace manager workspaces
		Mock<IWorkspace> workspace = new();
		workspace.Setup(w => w.Name).Returns("First");

		Mock<IWorkspace> secondWorkspace = new();
		secondWorkspace.Setup(w => w.Name).Returns("Second");

		workspaceManager.Add(workspace.Object);
		workspaceManager.Add(secondWorkspace.Object);

		// Activate the two workspaces and monitors
		workspaceManager.Activate(workspace.Object, monitorMock.Object);
		workspaceManager.Activate(secondWorkspace.Object, secondMonitorMock.Object);

		// Now we test the LayoutAllActiveWorkspaces method
		workspaceManager.LayoutAllActiveWorkspaces();

		// They activate twice, because once in Activate, and once in LayoutAllActiveWorkspaces
		workspace.Verify(w => w.DoLayout(), Times.Exactly(2));
		secondWorkspace.Verify(w => w.DoLayout(), Times.Exactly(2));
	}

	[Fact]
	public void WindowManager_WindowRegistered_WindowUnregistered()
	{
		Mock<IWindowManager> windowManagerMock = new();
		Mock<IWindow> windowMock = new();
		Mock<IMonitor> monitorMock = new();

		Mock<IMonitorManager> monitorManagerMock = new();
		monitorManagerMock.Setup(m => m.GetEnumerator()).Returns(new List<IMonitor> { monitorMock.Object }.GetEnumerator());
		monitorManagerMock.Setup(m => m.FocusedMonitor).Returns(monitorMock.Object);

		Mock<IWorkspace> workspaceMock = new();

		// Setup config context
		Mock<IConfigContext> configContextMock = new();
		configContextMock.Setup(x => x.WindowManager).Returns(windowManagerMock.Object);
		configContextMock.Setup(x => x.MonitorManager).Returns(monitorManagerMock.Object);

		WorkspaceManager workspaceManager = new(configContextMock.Object);
		workspaceManager.Add(workspaceMock.Object);
		workspaceManager.Initialize();

		// Setup the ActiveWorkspace
		workspaceManager.Activate(workspaceMock.Object, monitorMock.Object);

		var registerResult = Assert.Raises<RouteEventArgs>(
			h => workspaceManager.WindowRouted += h,
			h => workspaceManager.WindowRouted -= h,
			() =>
			{
				windowManagerMock.Raise(w => w.WindowRegistered += null, new WindowEventArgs(windowMock.Object));
			}
		);

		Assert.Equal(workspaceMock.Object, registerResult.Arguments.CurrentWorkspace);

		// Unregister the window
		var unregisterResult = Assert.Raises<RouteEventArgs>(
			h => workspaceManager.WindowRouted += h,
			h => workspaceManager.WindowRouted -= h,
			() =>
			{
				windowManagerMock.Raise(w => w.WindowUnregistered += null, new WindowEventArgs(windowMock.Object));
			}
		);

		Assert.Equal(workspaceMock.Object, unregisterResult.Arguments.PreviousWorkspace);
	}

	[Fact]
	public void MoveWindowToWorkspace()
	{
		Mock<IWindowManager> windowManagerMock = new();
		Mock<IWindow> windowMock = new();
		Mock<IMonitor> monitorMock = new();

		Mock<IMonitorManager> monitorManagerMock = new();
		monitorManagerMock.Setup(m => m.GetEnumerator()).Returns(new List<IMonitor> { monitorMock.Object }.GetEnumerator());
		monitorManagerMock.Setup(m => m.FocusedMonitor).Returns(monitorMock.Object);

		Mock<IWorkspace> workspaceMock = new();
		Mock<IWorkspace> secondWorkspaceMock = new();

		// Setup config context
		Mock<IConfigContext> configContextMock = new();
		configContextMock.Setup(x => x.WindowManager).Returns(windowManagerMock.Object);
		configContextMock.Setup(x => x.MonitorManager).Returns(monitorManagerMock.Object);

		WorkspaceManager workspaceManager = new(configContextMock.Object);
		workspaceManager.Add(workspaceMock.Object);
		workspaceManager.Initialize();

		// Setup the ActiveWorkspace
		workspaceManager.Activate(workspaceMock.Object, monitorMock.Object);

		// Move the window to the workspace
		workspaceManager.MoveWindowToWorkspace(secondWorkspaceMock.Object, windowMock.Object);

		workspaceMock.Verify(w => w.RemoveWindow(windowMock.Object), Times.Once);
		secondWorkspaceMock.Verify(w => w.AddWindow(windowMock.Object), Times.Once);
	}

	[Fact]
	public void MoveWindowToWorkspace_NullWindow()
	{
		Mock<IWindowManager> windowManagerMock = new();
		Mock<IWindow> windowMock = new();
		Mock<IMonitor> monitorMock = new();

		Mock<IMonitorManager> monitorManagerMock = new();
		monitorManagerMock.Setup(m => m.GetEnumerator()).Returns(new List<IMonitor> { monitorMock.Object }.GetEnumerator());
		monitorManagerMock.Setup(m => m.FocusedMonitor).Returns(monitorMock.Object);

		Mock<IWorkspace> workspaceMock = new();
		Mock<IWorkspace> secondWorkspaceMock = new();

		// Setup config context
		Mock<IConfigContext> configContextMock = new();
		configContextMock.Setup(x => x.WindowManager).Returns(windowManagerMock.Object);
		configContextMock.Setup(x => x.MonitorManager).Returns(monitorManagerMock.Object);

		WorkspaceManager workspaceManager = new(configContextMock.Object);
		workspaceManager.Add(workspaceMock.Object);
		workspaceManager.Initialize();

		// Move the window to the workspace
		workspaceManager.MoveWindowToWorkspace(secondWorkspaceMock.Object);

		workspaceMock.Verify(w => w.RemoveWindow(windowMock.Object), Times.Never);
		secondWorkspaceMock.Verify(w => w.AddWindow(windowMock.Object), Times.Never);
	}

	[Fact]
	public void MoveWindowToWorkspace_ActiveWorkspace()
	{
		Mock<IWindowManager> windowManagerMock = new();
		Mock<IWindow> windowMock = new();
		Mock<IMonitor> monitorMock = new();

		Mock<IMonitorManager> monitorManagerMock = new();
		monitorManagerMock.Setup(m => m.GetEnumerator()).Returns(new List<IMonitor> { monitorMock.Object }.GetEnumerator());
		monitorManagerMock.Setup(m => m.FocusedMonitor).Returns(monitorMock.Object);

		Mock<IWorkspace> workspaceMock = new();

		// Setup config context
		Mock<IConfigContext> configContextMock = new();
		configContextMock.Setup(x => x.WindowManager).Returns(windowManagerMock.Object);
		configContextMock.Setup(x => x.MonitorManager).Returns(monitorManagerMock.Object);

		WorkspaceManager workspaceManager = new(configContextMock.Object);
		workspaceManager.Add(workspaceMock.Object);
		workspaceManager.Initialize();

		// Setup the ActiveWorkspace
		workspaceManager.Activate(workspaceMock.Object, monitorMock.Object);

		// Move the window to the workspace
		workspaceManager.MoveWindowToWorkspace(workspaceMock.Object, windowMock.Object);

		workspaceMock.Verify(w => w.RemoveWindow(windowMock.Object), Times.Never);
		workspaceMock.Verify(w => w.AddWindow(windowMock.Object), Times.Never);
	}
}
