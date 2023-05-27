using Moq;
using System;
using System.Linq;
using Xunit;

namespace Whim.Tests;

public class WorkspaceManagerTests
{
	private class MocksBuilder
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<IMonitorManager> MonitorManager { get; } = new();
		public Mock<IMonitor>[] Monitors { get; }
		public Mock<IRouterManager> RouterManager { get; } = new();
		public WorkspaceManager WorkspaceManager { get; }

		public MocksBuilder(
			Mock<IWorkspace>[]? workspaces = null,
			Mock<IMonitor>[]? monitors = null,
			int activeMonitorIndex = 0
		)
		{
			Context.Setup(c => c.MonitorManager).Returns(MonitorManager.Object);

			Monitors = monitors ?? new[] { new Mock<IMonitor>(), new Mock<IMonitor>() };
			MonitorManager.Setup(m => m.Length).Returns(Monitors.Length);
			MonitorManager.Setup(m => m.GetEnumerator()).Returns(Monitors.Select(m => m.Object).GetEnumerator());

			if (Monitors.Length > 0)
			{
				MonitorManager.Setup(m => m.ActiveMonitor).Returns(Monitors[activeMonitorIndex].Object);
			}

			// Set up IEquatable for the monitors.
			foreach (Mock<IMonitor> mon in Monitors)
			{
				mon.Setup(m => m.Equals(It.Is((IMonitor m) => mon.Object == m))).Returns(true);
			}

			RouterManager.Setup(r => r.RouteWindow(It.IsAny<IWindow>())).Returns(null as IWorkspace);
			Context.Setup(c => c.RouterManager).Returns(RouterManager.Object);

			WorkspaceManager = new(Context.Object);
			foreach (Mock<IWorkspace> workspace in workspaces ?? Array.Empty<Mock<IWorkspace>>())
			{
				WorkspaceManager.Add(workspace.Object);
			}

			Context.Setup(c => c.WorkspaceManager).Returns(WorkspaceManager);
		}
	}

	[Fact]
	public void Initialize_RequireAtLeastNWorkspace()
	{
		// Given
		MocksBuilder mocks = new();

		// Then
		Assert.Throws<InvalidOperationException>(mocks.WorkspaceManager.Initialize);
	}

	[Fact]
	public void Initialize_Success()
	{
		// Given the workspace manager has two workspaces
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		// When the workspace manager is initialized, then a MonitorWorkspaceChanged event is raised
		Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => mocks.WorkspaceManager.MonitorWorkspaceChanged += h,
			h => mocks.WorkspaceManager.MonitorWorkspaceChanged -= h,
			mocks.WorkspaceManager.Initialize
		);

		// The workspaces are initialized
		workspace.Verify(w => w.Initialize(), Times.Once);
		workspace2.Verify(w => w.Initialize(), Times.Once);
	}

	[Fact]
	public void Add_AfterInitialize()
	{
		// Given the workspace manager is already initialized
		MocksBuilder mocks = new(new[] { new Mock<IWorkspace>(), new Mock<IWorkspace>() });
		mocks.WorkspaceManager.Initialize();
		Mock<IWorkspace> workspace = new();

		// When a workspace is added, then WorkspaceAdded is raised
		Assert.Raises<WorkspaceEventArgs>(
			h => mocks.WorkspaceManager.WorkspaceAdded += h,
			h => mocks.WorkspaceManager.WorkspaceAdded -= h,
			() => mocks.WorkspaceManager.Add(workspace.Object)
		);

		workspace.Verify(w => w.Initialize(), Times.Once);
	}

	[Fact]
	public void Add_BeforeInitialize()
	{
		// Given the workspace manager is not initialized
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace1 = new();
		MocksBuilder mocks = new(new[] { workspace, workspace1 });

		// When
		mocks.WorkspaceManager.Initialize();

		// Then
		workspace.Verify(w => w.Initialize(), Times.Once);
	}

	[Fact]
	public void Add_SameWorkspaceTwice()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		MocksBuilder mocks = new(new[] { workspace });

		// When a workspace is added
		mocks.WorkspaceManager.Add(workspace.Object);

		// Then the length is still 1
		Assert.Single(mocks.WorkspaceManager);
	}

	[Fact]
	public void Remove_Workspace_RequireAtLeastNWorkspace()
	{
		// Given the workspace manager has two workspaces and there are two monitors
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		// When a workspace is removed
		bool result = mocks.WorkspaceManager.Remove(workspace.Object);

		//Then it returns false, as there must be at least two workspaces, since there are two monitors
		Assert.False(result);
		workspace2.Verify(w => w.DoLayout(), Times.Never);
	}

	[Fact]
	public void Remove_Workspace_NotFound()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		Mock<IWorkspace> workspace3 = new();
		MocksBuilder mocks = new(new[] { workspace, workspace2, workspace3 });

		// When a workspace is removed
		bool result = mocks.WorkspaceManager.Remove(new Mock<IWorkspace>().Object);

		// Then it returns false, as the workspace was not found
		Assert.False(result);
	}

	[Fact]
	public void Remove_Workspace_Success()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		Mock<IMonitor>[] monitorMocks = new[] { new Mock<IMonitor>() };
		MocksBuilder mocks = new(new Mock<IWorkspace>[] { workspace, workspace2 }, monitorMocks);

		Mock<IWindow> window = new();
		workspace.Setup(w => w.Windows).Returns(new[] { window.Object });

		// When a workspace is removed, it returns true, WorkspaceRemoved is raised
		var result = Assert.Raises<WorkspaceEventArgs>(
			h => mocks.WorkspaceManager.WorkspaceRemoved += h,
			h => mocks.WorkspaceManager.WorkspaceRemoved -= h,
			() => Assert.True(mocks.WorkspaceManager.Remove(workspace.Object))
		);
		Assert.Equal(workspace.Object, result.Arguments.Workspace);

		// and the window is added to the last remaining workspace
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Once);
		workspace2.Verify(w => w.DoLayout(), Times.Once);
	}

	[Fact]
	public void Remove_String_NotFound()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		// When a workspace is removed, it returns false, as the workspace was not found
		Assert.False(mocks.WorkspaceManager.Remove("not found"));
	}

	[Fact]
	public void Remove_String_Success()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		MocksBuilder mocks =
			new(new Mock<IWorkspace>[] { workspace, new Mock<IWorkspace>() }, new[] { new Mock<IMonitor>() });

		workspace.Setup(w => w.Name).Returns("workspace");

		// When a workspace is removed, it returns true, and WorkspaceRemoved is raised
		var result = Assert.Raises<WorkspaceEventArgs>(
			h => mocks.WorkspaceManager.WorkspaceRemoved += h,
			h => mocks.WorkspaceManager.WorkspaceRemoved -= h,
			() => mocks.WorkspaceManager.Remove("workspace")
		);
		Assert.Equal(workspace.Object, result.Arguments.Workspace);
	}

	[Fact]
	public void TryGet_Null()
	{
		// Given
		MocksBuilder mocks = new();

		// When getting a workspace which does not exist, then null is returned
		Assert.Null(mocks.WorkspaceManager.TryGet("not found"));
	}

	[Fact]
	public void TryGet_Success()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		workspace.Setup(w => w.Name).Returns("workspace");
		MocksBuilder mocks = new(new[] { workspace });

		// When getting a workspace which does exist, then the workspace is returned
		Assert.Equal(workspace.Object, mocks.WorkspaceManager.TryGet("workspace"));
	}

	[Fact]
	public void SquareBracket_Get()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		workspace.Setup(w => w.Name).Returns("workspace");
		MocksBuilder mocks = new(new[] { workspace });

		// When getting a workspace which does exist, then the workspace is returned
		Assert.Equal(workspace.Object, mocks.WorkspaceManager["workspace"]);
	}

	[Fact]
	public void GetEnumerator()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		// When enumerating the workspaces, then the workspaces are returned
		Assert.Equal(new[] { workspace.Object, workspace2.Object }, mocks.WorkspaceManager);
	}

	[Fact]
	public void IEnumerable_GetEnumerator()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		// When enumerating the workspaces, then the workspaces are returned
		Assert.Equal(new[] { workspace.Object, workspace2.Object }, mocks.WorkspaceManager);
	}

	[Fact]
	public void Activate_NoOldWorkspace()
	{
		// Given
		Mock<IMonitor>[] monitorMocks = new[] { new Mock<IMonitor>() };
		Mock<IWorkspace> workspace = new();
		MocksBuilder mocks = new(new[] { workspace }, monitorMocks);

		// When a workspace is activated when there are no other workspaces activated, then it is
		// focused on the active monitor and raises an event,
		var result = Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => mocks.WorkspaceManager.MonitorWorkspaceChanged += h,
			h => mocks.WorkspaceManager.MonitorWorkspaceChanged -= h,
			() => mocks.WorkspaceManager.Activate(workspace.Object)
		);
		Assert.Equal(workspace.Object, result.Arguments.NewWorkspace);
		Assert.Null(result.Arguments.OldWorkspace);

		// Layout is done, and the first window is focused.
		workspace.Verify(w => w.DoLayout(), Times.Once);
		workspace.Verify(w => w.FocusFirstWindow(), Times.Once);
	}

	[Fact]
	public void Activate_WithOldWorkspace()
	{
		// Given
		Mock<IWorkspace> oldWorkspace = new();
		Mock<IWorkspace> newWorkspace = new();
		Mock<IMonitor>[] monitorMocks = new[] { new Mock<IMonitor>() };
		MocksBuilder mocks = new(new[] { oldWorkspace, newWorkspace }, monitorMocks);

		mocks.WorkspaceManager.Activate(oldWorkspace.Object);

		// Reset mocks
		oldWorkspace.Reset();
		newWorkspace.Reset();

		// When a workspace is activated when there is another workspace activated, then the old
		// workspace is deactivated, the new workspace is activated, and an event is raised
		var result = Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => mocks.WorkspaceManager.MonitorWorkspaceChanged += h,
			h => mocks.WorkspaceManager.MonitorWorkspaceChanged -= h,
			() => mocks.WorkspaceManager.Activate(newWorkspace.Object)
		);
		Assert.Equal(newWorkspace.Object, result.Arguments.NewWorkspace);
		Assert.Equal(oldWorkspace.Object, result.Arguments.OldWorkspace);

		// The old workspace is deactivated, the new workspace is laid out, and the first window is
		// focused.
		oldWorkspace.Verify(w => w.Deactivate(), Times.Once);
		oldWorkspace.Verify(w => w.DoLayout(), Times.Never);
		newWorkspace.Verify(w => w.DoLayout(), Times.Once);
		newWorkspace.Verify(w => w.FocusFirstWindow(), Times.Once);
	}

	[Fact]
	public void Activate_MultipleMonitors()
	{
		// Given there are two workspaces and monitors
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		MocksBuilder mocks = new(new[] { workspace, workspace2 });
		IMonitor monitor = mocks.Monitors[0].Object;
		IMonitor monitor2 = mocks.Monitors[1].Object;

		mocks.WorkspaceManager.Activate(workspace.Object, monitor);
		mocks.WorkspaceManager.Activate(workspace2.Object, monitor2);

		// Reset mocks
		workspace.Reset();
		workspace2.Reset();

		// When a workspace is activated on a monitor which already has a workspace activated, then
		// an event is raised
		var result = Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => mocks.WorkspaceManager.MonitorWorkspaceChanged += h,
			h => mocks.WorkspaceManager.MonitorWorkspaceChanged -= h,
			() => mocks.WorkspaceManager.Activate(workspace2.Object, monitor)
		);

		Assert.Equal(monitor, result.Arguments.Monitor);
		Assert.Equal(workspace2.Object, result.Arguments.NewWorkspace);
		Assert.Equal(workspace.Object, result.Arguments.OldWorkspace);

		workspace.Verify(w => w.Deactivate(), Times.Never);
		workspace.Verify(w => w.DoLayout(), Times.Once);
		workspace.Verify(w => w.FocusFirstWindow(), Times.Never);

		workspace2.Verify(w => w.Deactivate(), Times.Never);
		workspace2.Verify(w => w.DoLayout(), Times.Once);
		workspace2.Verify(w => w.FocusFirstWindow(), Times.Once);
	}

	[InlineData(0, 2)]
	[InlineData(2, 1)]
	[Theory]
	public void ActivatePrevious(int currentIdx, int prevIdx)
	{
		// Given
		Mock<IWorkspace>[] workspaces = new[]
		{
			new Mock<IWorkspace>(),
			new Mock<IWorkspace>(),
			new Mock<IWorkspace>()
		};

		Mock<IMonitor>[] monitors = new[] { new Mock<IMonitor>() };
		MocksBuilder mocks = new(workspaces, monitors);

		mocks.WorkspaceManager.Activate(workspaces[currentIdx].Object);

		// Reset mocks
		workspaces[currentIdx].Reset();

		// When the previous workspace is activated, then the previous workspace is activated
		mocks.WorkspaceManager.ActivatePrevious();

		workspaces[currentIdx].Verify(w => w.Deactivate(), Times.Once);
		workspaces[currentIdx].Verify(w => w.DoLayout(), Times.Never);
		workspaces[currentIdx].Verify(w => w.FocusFirstWindow(), Times.Never);

		workspaces[prevIdx].Verify(w => w.Deactivate(), Times.Never);
		workspaces[prevIdx].Verify(w => w.DoLayout(), Times.Once);
		workspaces[prevIdx].Verify(w => w.FocusFirstWindow(), Times.Once);
	}

	[InlineData(0, 1)]
	[InlineData(2, 0)]
	[Theory]
	public void ActivateNext(int currentIdx, int nextIdx)
	{
		// Given
		Mock<IWorkspace>[] workspaces = new[]
		{
			new Mock<IWorkspace>(),
			new Mock<IWorkspace>(),
			new Mock<IWorkspace>()
		};

		Mock<IMonitor>[] monitors = new[] { new Mock<IMonitor>() };
		MocksBuilder mocks = new(workspaces, monitors);

		mocks.WorkspaceManager.Activate(workspaces[currentIdx].Object);

		// Reset mocks
		workspaces[currentIdx].Reset();

		// When the next workspace is activated, then the next workspace is activated
		mocks.WorkspaceManager.ActivateNext();

		workspaces[currentIdx].Verify(w => w.Deactivate(), Times.Once);
		workspaces[currentIdx].Verify(w => w.DoLayout(), Times.Never);
		workspaces[currentIdx].Verify(w => w.FocusFirstWindow(), Times.Never);

		workspaces[nextIdx].Verify(w => w.Deactivate(), Times.Never);
		workspaces[nextIdx].Verify(w => w.DoLayout(), Times.Once);
		workspaces[nextIdx].Verify(w => w.FocusFirstWindow(), Times.Once);
	}

	[Fact]
	public void GetMonitorForWorkspace_NoWorkspace()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IMonitor>[] monitorMocks = new[] { new Mock<IMonitor>() };
		MocksBuilder mocks = new(new[] { workspace }, monitorMocks);

		mocks.WorkspaceManager.Activate(workspace.Object);

		// When we get the monitor for a workspace which isn't in the workspace manager
		IMonitor? monitor = mocks.WorkspaceManager.GetMonitorForWorkspace(new Mock<IWorkspace>().Object);

		// Then null is returned
		Assert.Null(monitor);
	}

	[Fact]
	public void GetMonitorForWorkspace_Success()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		mocks.WorkspaceManager.Activate(workspace.Object);
		mocks.WorkspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// When we get the monitor for a workspace which is in the workspace manager
		IMonitor? monitor = mocks.WorkspaceManager.GetMonitorForWorkspace(workspace.Object);

		// Then the monitor is returned
		Assert.Equal(mocks.Monitors[0].Object, monitor);
	}

	[Fact]
	public void LayoutAllActiveWorkspaces()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		mocks.WorkspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// Reset the mocks
		workspace.Reset();
		workspace2.Reset();

		// When we layout all active workspaces
		mocks.WorkspaceManager.LayoutAllActiveWorkspaces();

		// Then all active workspaces are laid out
		workspace.Verify(w => w.DoLayout(), Times.Once());
		workspace2.Verify(w => w.DoLayout(), Times.Once());
	}

	[Fact]
	public void WindowAdded_NoRouter()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		mocks.WorkspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// Reset the mocks
		workspace.Reset();
		workspace2.Reset();

		// When a window is added
		Mock<IWindow> window = new();
		mocks.WorkspaceManager.WindowAdded(window.Object);

		// Then the window is added to the active workspace
		workspace.Verify(w => w.AddWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void WindowAdded_NoRouter_GetMonitorAtWindowCenter()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		mocks.MonitorManager.Setup(m => m.GetMonitorAtPoint(It.IsAny<IPoint<int>>())).Returns(mocks.Monitors[0].Object);

		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		mocks.WorkspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// Reset the mocks
		workspace.Reset();
		workspace2.Reset();

		// When a window is added
		Mock<IWindow> window = new();
		mocks.WorkspaceManager.WindowAdded(window.Object);

		// Then the window is added to the active workspace
		workspace.Verify(w => w.AddWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void WindowAdded_Router()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		mocks.WorkspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// Reset the mocks
		workspace.Reset();
		workspace2.Reset();

		// There is a router which routes the window to a different workspace
		Mock<IWindow> window = new();
		mocks.RouterManager.Setup(r => r.RouteWindow(window.Object)).Returns(workspace2.Object);

		// When a window is added
		mocks.WorkspaceManager.WindowAdded(window.Object);

		// Then the window is added to the workspace returned by the router
		workspace.Verify(w => w.AddWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Once());
	}

	[Fact]
	public void WindowAdded_RouterToActive()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		mocks.WorkspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// Reset the mocks
		workspace.Reset();
		workspace2.Reset();

		// There is a router which routes the window to the active workspace
		Mock<IWindow> window = new();
		mocks.RouterManager.Setup(r => r.RouteWindow(window.Object)).Returns<IWorkspace?>(null);
		mocks.RouterManager.Setup(r => r.RouteToActiveWorkspace).Returns(true);

		// When a window is added
		mocks.WorkspaceManager.WindowAdded(window.Object);

		// Then the window is added to the active workspace
		workspace.Verify(w => w.AddWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void WindowRemoved_NotFound()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		mocks.WorkspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// Reset the mocks
		workspace.Reset();
		workspace2.Reset();

		// When a window is removed
		Mock<IWindow> window = new();
		mocks.WorkspaceManager.WindowRemoved(window.Object);

		// Then the window is removed from all workspaces
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.RemoveWindow(window.Object), Times.Never());
	}

	[Fact]
	public void WindowRemoved_Found()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		mocks.WorkspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		Mock<IWindow> window = new();
		mocks.WorkspaceManager.WindowAdded(window.Object);
		// Reset the mocks
		workspace.Reset();
		workspace2.Reset();

		// When a window which is in a workspace is removed
		mocks.WorkspaceManager.WindowRemoved(window.Object);

		// Then the window is removed from the workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.RemoveWindow(window.Object), Times.Never());
	}

	[Fact]
	public void MoveWindowToWorkspace_NoWindow()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		mocks.WorkspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// Reset the mocks
		workspace.Reset();
		workspace2.Reset();

		// When a window which is not in a workspace is moved to a workspace
		mocks.WorkspaceManager.MoveWindowToWorkspace(workspace.Object);

		// Then the window is not added to the workspace
		workspace.Verify(w => w.RemoveWindow(It.IsAny<IWindow>()), Times.Never());
		workspace2.Verify(w => w.AddWindow(It.IsAny<IWindow>()), Times.Never());
	}

	[Fact]
	public void MoveWindowToWorkspace_PhantomWindow()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		mocks.WorkspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		Mock<IWindow> window = new();
		// When a phantom window is added
		mocks.WorkspaceManager.AddPhantomWindow(workspace.Object, window.Object);
		workspace.Reset();
		workspace2.Reset();

		// and moved to a workspace
		mocks.WorkspaceManager.MoveWindowToWorkspace(workspace.Object, window.Object);

		// Then the window is not removed or added to any workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void MoveWindowToWorkspace_CannotFindWindow()
	{
		// Given
		Mock<IWindow> window = new();

		// there are 3 workspaces
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		Mock<IWorkspace> workspace3 = new();

		MocksBuilder mocks = new(new[] { workspace, workspace2, workspace3 });

		// Reset the mocks
		workspace.Reset();
		workspace2.Reset();
		workspace3.Reset();

		// When a window not in any workspace is moved to a workspace
		mocks.WorkspaceManager.MoveWindowToWorkspace(workspace.Object, window.Object);

		// Then the window is not removed or added to any workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void MoveWindowToWorkspace_Success_WindowNotHidden()
	{
		// Given
		Mock<IWindow> window = new();

		// there are 3 workspaces
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		Mock<IWorkspace> workspace3 = new();

		MocksBuilder mocks = new(new[] { workspace, workspace2, workspace3 });

		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		mocks.WorkspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// and the window is added
		mocks.WorkspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();
		workspace3.Reset();
		window.Reset();

		// When a window in a workspace is moved to another workspace
		mocks.WorkspaceManager.MoveWindowToWorkspace(workspace2.Object, window.Object);

		// Then the window is removed from the first workspace and added to the second
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Once());
		window.Verify(w => w.Hide(), Times.Never());
	}

	[Fact]
	public void MoveWindowToWorkspace_Success_WindowHidden()
	{
		// Given
		Mock<IWindow> window = new();

		// there are 3 workspaces
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		Mock<IWorkspace> workspace3 = new();

		// and there are 3 monitors
		Mock<IMonitor>[] monitors = new[] { new Mock<IMonitor>(), new Mock<IMonitor>(), new Mock<IMonitor>(), };

		MocksBuilder mocks = new(new[] { workspace, workspace2, workspace3 }, monitors);

		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[1].Object);

		// and the window is added
		mocks.WorkspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();
		workspace3.Reset();
		window.Reset();

		// When a window in a workspace is moved to another workspace
		mocks.WorkspaceManager.MoveWindowToWorkspace(workspace2.Object, window.Object);

		// Then the window is removed from the first workspace and added to the third
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Once());
		window.Verify(w => w.Hide(), Times.Once());
	}

	[Fact]
	public void MoveWindowToMonitor_NoWindow()
	{
		// Given there are 2 workspaces
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();

		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		mocks.WorkspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// Reset the mocks
		workspace.Reset();
		workspace2.Reset();

		// When there is no focused window
		mocks.WorkspaceManager.MoveWindowToMonitor(mocks.Monitors[0].Object);

		// Then the window is not added to the workspace
		workspace.Verify(w => w.RemoveWindow(It.IsAny<IWindow>()), Times.Never());
		workspace2.Verify(w => w.AddWindow(It.IsAny<IWindow>()), Times.Never());
	}

	[Fact]
	public void MoveWindowToMonitor_NoOldMonitor()
	{
		// Given there are 2 workspaces
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();

		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		mocks.WorkspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// Reset the mocks
		workspace.Reset();
		workspace2.Reset();

		Mock<IWindow> window = new();

		// When a window which is not in a workspace is moved to a monitor
		mocks.WorkspaceManager.MoveWindowToMonitor(mocks.Monitors[0].Object, window.Object);

		// Then the window is not added to the workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void MoveWindowToMonitor_OldMonitorIsNewMonitor()
	{
		// Given there are 2 workspaces, and the window has been added
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();

		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		mocks.WorkspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		Mock<IWindow> window = new();
		mocks.WorkspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();

		// When a window which is in a workspace is moved to the same monitor
		mocks.WorkspaceManager.MoveWindowToMonitor(mocks.Monitors[0].Object, window.Object);

		// Then the window is not added to the workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void MoveWindowToMonitor_WorkspaceForMonitorNotFound()
	{
		// Given there are 2 workspaces, and the window has been added
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();

		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		mocks.WorkspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		Mock<IWindow> window = new();
		mocks.WorkspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();

		// When a window which is in a workspace is moved to a monitor which isn't registered
		mocks.WorkspaceManager.MoveWindowToMonitor(new Mock<IMonitor>().Object, window.Object);

		// Then the window is not added to the workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void MoveWindowToMonitor_Success()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		mocks.WorkspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		Mock<IWindow> window = new();
		mocks.WorkspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();

		// When a window which is in a workspace is moved to a monitor
		mocks.WorkspaceManager.MoveWindowToMonitor(mocks.Monitors[1].Object, window.Object);

		// Then the window is removed from the old workspace and added to the new workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Once());
	}

	[Fact]
	public void MoveWindowToPreviousMonitor_Success()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();

		MocksBuilder mocks = new(new[] { workspace, workspace2 });
		mocks.MonitorManager
			.Setup(m => m.GetPreviousMonitor(mocks.Monitors[0].Object))
			.Returns(mocks.Monitors[1].Object);

		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		mocks.WorkspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		Mock<IWindow> window = new();
		mocks.WorkspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();

		// When a window which is in a workspace is moved to the previous monitor
		mocks.WorkspaceManager.MoveWindowToPreviousMonitor(window.Object);

		// Then the window is removed from the old workspace and added to the new workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Once());
	}

	[Fact]
	public void MoveWindowToNextMonitor_Success()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();

		MocksBuilder mocks = new(new[] { workspace, workspace2 });
		mocks.MonitorManager.Setup(m => m.GetNextMonitor(mocks.Monitors[0].Object)).Returns(mocks.Monitors[1].Object);

		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		mocks.WorkspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		Mock<IWindow> window = new();
		mocks.WorkspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();

		// When a window which is in a workspace is moved to the next monitor
		mocks.WorkspaceManager.MoveWindowToNextMonitor(window.Object);

		// Then the window is removed from the old workspace and added to the new workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Once());
	}

	[Fact]
	public void MoveWindowToPoint_TargetWorkspaceNotFound()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		MocksBuilder mocks = new(new[] { workspace });

		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);

		Mock<IWindow> window = new();
		mocks.WorkspaceManager.WindowAdded(window.Object);
		workspace.Reset();

		mocks.MonitorManager
			.Setup(m => m.GetMonitorAtPoint(It.IsAny<IPoint<int>>()))
			.Returns(new Mock<IMonitor>().Object);

		// When a window which is in a workspace is moved to a point which doesn't correspond to any workspaces
		mocks.WorkspaceManager.MoveWindowToPoint(window.Object, new Point<int>() { X = 0, Y = 0 });

		// Then the window is not removed from the old workspace and not added to the new workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace.Verify(w => w.MoveWindowToPoint(window.Object, It.IsAny<Point<double>>()), Times.Never());
	}

	[Fact]
	public void MoveWindowToPoint_PhantomWindow()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		mocks.WorkspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		Mock<IWindow> window = new();
		mocks.WorkspaceManager.AddPhantomWindow(workspace.Object, window.Object);
		workspace.Reset();
		workspace2.Reset();

		mocks.MonitorManager.Setup(m => m.GetMonitorAtPoint(It.IsAny<IPoint<int>>())).Returns(mocks.Monitors[1].Object);

		// When a phantom is moved
		mocks.WorkspaceManager.MoveWindowToPoint(window.Object, new Point<int>() { X = 0, Y = 0 });

		// Then nothing happens
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace.Verify(w => w.MoveWindowToPoint(window.Object, It.IsAny<Point<double>>()), Times.Never());
	}

	[Fact]
	public void MoveWindowToPoint_CannotRemoveWindow()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		mocks.WorkspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		Mock<IWindow> window = new();
		mocks.WorkspaceManager.WindowAdded(window.Object);
		workspace.Reset();

		mocks.MonitorManager.Setup(m => m.GetMonitorAtPoint(It.IsAny<IPoint<int>>())).Returns(mocks.Monitors[1].Object);
		workspace.Setup(w => w.RemoveWindow(window.Object)).Returns(false);

		// When a window is moved to a point, but the window cannot be removed from the old workspace
		mocks.WorkspaceManager.MoveWindowToPoint(window.Object, new Point<int>() { X = 0, Y = 0 });

		// Then nothing happens
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace.Verify(w => w.MoveWindowToPoint(window.Object, It.IsAny<Point<double>>()), Times.Never());
	}

	[Fact]
	public void MoveWindowToPoint_Success()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		mocks.WorkspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		Mock<IWindow> window = new();
		mocks.WorkspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();

		mocks.MonitorManager.Setup(m => m.GetMonitorAtPoint(It.IsAny<IPoint<int>>())).Returns(mocks.Monitors[1].Object);
		mocks.Monitors[1].Setup(m => m.WorkingArea).Returns(new Location<int>());
		workspace.Setup(w => w.RemoveWindow(window.Object)).Returns(true);

		// When a window is moved to a point
		mocks.WorkspaceManager.MoveWindowToPoint(window.Object, new Point<int>() { X = 0, Y = 0 });

		// Then the window is removed from the old workspace and added to the new workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace.Verify(w => w.MoveWindowToPoint(window.Object, It.IsAny<Point<double>>()), Times.Never());

		window.Verify(w => w.Focus(), Times.Once());
	}

	[Fact]
	public void WindowFocused()
	{
		// Given
		MocksBuilder mocks = new();

		Mock<ILayoutEngine> layoutEngine = new();
		Mock<Workspace> workspace = new(mocks.Context.Object, "test", layoutEngine.Object);
		Mock<Workspace> workspace2 = new(mocks.Context.Object, "test", layoutEngine.Object);

		mocks.WorkspaceManager.Add(workspace.Object);
		mocks.WorkspaceManager.Add(workspace2.Object);

		Mock<IWindow> window = new();

		// When a window is focused
		mocks.WorkspaceManager.WindowFocused(window.Object);

		// Then the window is focused in all workspaces
		workspace.Verify(w => w.WindowFocused(window.Object), Times.Once());
		workspace2.Verify(w => w.WindowFocused(window.Object), Times.Once());
	}

	[Fact]
	public void WindowFocused_ActivateWorkspace()
	{
		// Given
		Mock<IWorkspace>[] workspaces = new[] { new Mock<IWorkspace>(), new Mock<IWorkspace>() };
		Mock<IMonitor> monitor = new();
		MocksBuilder mocks = new(workspaces, new Mock<IMonitor>[] { monitor });

		Mock<ILayoutEngine> layoutEngine = new();
		Mock<IWindow> window = new();

		mocks.WorkspaceManager.Activate(workspaces[0].Object, monitor.Object);
		workspaces[0].Reset();

		// When a window is added to the first workspace, the second workspace is activated, and the window is focused
		mocks.WorkspaceManager.WindowAdded(window.Object);
		mocks.WorkspaceManager.Activate(workspaces[1].Object, monitor.Object);
		mocks.WorkspaceManager.WindowFocused(window.Object);

		// Then the first workspace is activated
		workspaces[0].Verify(w => w.DoLayout(), Times.Once());
	}

	[Fact]
	public void WindowMinimizeStart_CouldNotFindWindow()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		MocksBuilder mocks = new(new[] { workspace });
		Mock<IWindow> window = new();

		// When a window is minimized, but the window is not found in any workspace
		mocks.WorkspaceManager.WindowMinimizeStart(window.Object);

		// Then nothing happens
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
	}

	[Fact]
	public void WindowMinimizeStart()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		MocksBuilder mocks = new(new[] { workspace });

		Mock<IRouterManager> routerManager = new();
		routerManager.Setup(r => r.RouteWindow(It.IsAny<IWindow>())).Returns(workspace.Object);
		mocks.Context.Setup(c => c.RouterManager).Returns(routerManager.Object);

		// A window is added to the workspace
		Mock<IWindow> window = new();
		mocks.WorkspaceManager.WindowAdded(window.Object);

		// When a window is minimized
		mocks.WorkspaceManager.WindowMinimizeStart(window.Object);

		// Then the window is removed from the workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
	}

	[Fact]
	public void WindowMinimizeEnd()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		MocksBuilder mocks = new(new[] { workspace });

		Mock<IRouterManager> routerManager = new();
		routerManager.Setup(r => r.RouteWindow(It.IsAny<IWindow>())).Returns(workspace.Object);
		mocks.Context.Setup(c => c.RouterManager).Returns(routerManager.Object);

		Mock<IWindow> window = new();

		// When a window is restored
		mocks.WorkspaceManager.WindowMinimizeEnd(window.Object);

		// Then the window is routed to the workspace
		routerManager.Verify(r => r.RouteWindow(window.Object), Times.Once());
	}

	[Fact]
	public void MonitorManager_MonitorsChanged_Removed()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();

		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		mocks.WorkspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// When a monitor is removed
		mocks.WorkspaceManager.MonitorManager_MonitorsChanged(
			this,
			new MonitorsChangedEventArgs()
			{
				AddedMonitors = Array.Empty<IMonitor>(),
				RemovedMonitors = new IMonitor[] { mocks.Monitors[0].Object },
				UnchangedMonitors = new IMonitor[] { mocks.Monitors[1].Object }
			}
		);

		// Then the workspace is deactivated
		workspace.Verify(w => w.Deactivate(), Times.Once());
	}

	[Fact]
	public void MonitorManager_MonitorsChanged_Added_CreateWorkspace()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();

		Mock<IMonitor> monitor = new();

		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		Mock<Func<IContext, string, IWorkspace>> workspaceFactory = new();
		workspaceFactory.Setup(f => f(mocks.Context.Object, It.IsAny<string>())).Returns(new Mock<IWorkspace>().Object);
		mocks.WorkspaceManager.WorkspaceFactory = workspaceFactory.Object;

		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		mocks.WorkspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// When a monitor is added
		mocks.WorkspaceManager.MonitorManager_MonitorsChanged(
			this,
			new MonitorsChangedEventArgs()
			{
				AddedMonitors = new IMonitor[] { monitor.Object },
				RemovedMonitors = Array.Empty<IMonitor>(),
				UnchangedMonitors = new IMonitor[] { mocks.Monitors[0].Object, mocks.Monitors[1].Object }
			}
		);

		// Then a new workspace is created
		workspaceFactory.Verify(f => f(mocks.Context.Object, "Workspace 3"), Times.Once());
	}

	[Fact]
	public void MonitorManager_MonitorsChanged_Added_UseExistingWorkspace()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		Mock<IWorkspace> workspace3 = new();

		Mock<IMonitor> monitor = new();

		MocksBuilder mocks = new(new[] { workspace, workspace2, workspace3 });

		mocks.WorkspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		mocks.WorkspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// Reset the mocks
		workspace.Reset();
		workspace2.Reset();
		workspace3.Reset();

		// When a monitor is added
		mocks.WorkspaceManager.MonitorManager_MonitorsChanged(
			this,
			new MonitorsChangedEventArgs()
			{
				AddedMonitors = new IMonitor[] { monitor.Object },
				RemovedMonitors = Array.Empty<IMonitor>(),
				UnchangedMonitors = new IMonitor[] { mocks.Monitors[0].Object, mocks.Monitors[1].Object }
			}
		);

		// Then the workspace is activated
		workspace.Verify(w => w.DoLayout(), Times.Once());
		workspace2.Verify(w => w.DoLayout(), Times.Once());
		workspace3.Verify(w => w.DoLayout(), Times.Exactly(2));
	}

	[Fact]
	public void AddProxyLayoutEngine()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<ProxyLayoutEngine> proxyLayoutEngine = new();

		// When a proxy layout engine is added
		mocks.WorkspaceManager.AddProxyLayoutEngine(proxyLayoutEngine.Object);

		// Then the proxy layout engine is added to the list
		Assert.Contains(proxyLayoutEngine.Object, mocks.WorkspaceManager.ProxyLayoutEngines);
	}

	[Fact]
	public void TriggerActiveLayoutEngineChanged()
	{
		// Given
		MocksBuilder mocks = new();
		ActiveLayoutEngineChangedEventArgs eventArgs =
			new()
			{
				Workspace = new Mock<IWorkspace>().Object,
				PreviousLayoutEngine = new Mock<ILayoutEngine>().Object,
				CurrentLayoutEngine = new Mock<ILayoutEngine>().Object
			};

		// When the active layout engine is changed, then the event is triggered
		Assert.Raises<ActiveLayoutEngineChangedEventArgs>(
			h => mocks.WorkspaceManager.ActiveLayoutEngineChanged += h,
			h => mocks.WorkspaceManager.ActiveLayoutEngineChanged -= h,
			() => mocks.WorkspaceManager.TriggerActiveLayoutEngineChanged(eventArgs)
		);
	}

	[Fact]
	public void TriggerWorkspaceRenamed()
	{
		// Given
		MocksBuilder mocks = new();
		WorkspaceRenamedEventArgs eventArgs =
			new()
			{
				Workspace = new Mock<IWorkspace>().Object,
				PreviousName = "Previous",
				CurrentName = "Current"
			};

		// When the workspace is renamed, then the event is triggered
		Assert.Raises<WorkspaceRenamedEventArgs>(
			h => mocks.WorkspaceManager.WorkspaceRenamed += h,
			h => mocks.WorkspaceManager.WorkspaceRenamed -= h,
			() => mocks.WorkspaceManager.TriggerWorkspaceRenamed(eventArgs)
		);
	}

	[Fact]
	public void AddPhantomWindow()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWindow> window = new();
		MocksBuilder mocks = new(new[] { workspace });

		// When a phantom window is added
		mocks.WorkspaceManager.AddPhantomWindow(workspace.Object, window.Object);

		// Then the phantom window is added to the list
		Assert.Contains(window.Object, mocks.WorkspaceManager.PhantomWindows);
	}

	[Fact]
	public void RemovePhantomWindow()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWindow> window = new();
		MocksBuilder mocks = new(new[] { workspace });

		mocks.WorkspaceManager.AddPhantomWindow(workspace.Object, window.Object);

		// When a phantom window is removed
		mocks.WorkspaceManager.RemovePhantomWindow(window.Object);

		// Then the phantom window is removed from the list
		Assert.DoesNotContain(window.Object, mocks.WorkspaceManager.PhantomWindows);
		Assert.Null(mocks.WorkspaceManager.GetMonitorForWindow(window.Object));
	}

	[Fact]
	public void DoesDispose()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		MocksBuilder mocks = new(new[] { workspace, workspace2 });

		// When the workspace manager is disposed
		mocks.WorkspaceManager.Dispose();

		// Then the workspaces are disposed
		workspace.Verify(w => w.Dispose(), Times.Once());
		workspace2.Verify(w => w.Dispose(), Times.Once());
	}
}
