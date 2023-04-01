using Moq;
using System;
using System.Linq;
using Xunit;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Reliability",
	"CA2000:Dispose objects before losing scope",
	Justification = "Unnecessary for tests"
)]
public class WorkspaceManagerTests
{
	private class MocksBuilder
	{
		public Mock<IConfigContext> ConfigContext { get; } = new();
		public Mock<IMonitorManager> MonitorManager { get; } = new();
		public Mock<IMonitor>[] Monitors { get; }
		public Mock<IRouterManager> RouterManager { get; } = new();

		public MocksBuilder(Mock<IMonitor>[]? monitors = null, int focusedMonitorIndex = 0)
		{
			ConfigContext.Setup(c => c.MonitorManager).Returns(MonitorManager.Object);

			Monitors = monitors ?? new[] { new Mock<IMonitor>(), new Mock<IMonitor>() };
			MonitorManager.Setup(m => m.Length).Returns(Monitors.Length);
			MonitorManager.Setup(m => m.GetEnumerator()).Returns(Monitors.Select(m => m.Object).GetEnumerator());

			if (Monitors.Length > 0)
			{
				MonitorManager.Setup(m => m.FocusedMonitor).Returns(Monitors[focusedMonitorIndex].Object);
			}

			// Set up IEquatable for the monitors.
			foreach (Mock<IMonitor> mon in Monitors)
			{
				mon.Setup(m => m.Equals(It.Is((IMonitor m) => mon.Object == m))).Returns(true);
			}

			RouterManager.Setup(r => r.RouteWindow(It.IsAny<IWindow>())).Returns(null as IWorkspace);
			ConfigContext.Setup(c => c.RouterManager).Returns(RouterManager.Object);
		}
	}

	[Fact]
	public void Initialize_RequireAtLeastNWorkspace()
	{
		// Given
		MocksBuilder mocks = new();

		// When
		using WorkspaceManager workspaceManager = new(mocks.ConfigContext.Object);

		// Then
		Assert.Throws<InvalidOperationException>(workspaceManager.Initialize);
	}

	[Fact]
	public void Initialize_Success()
	{
		// Given
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();

		// the workspace manager has two workspaces
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };

		// When the workspace manager is initialized, then MonitorWorkspaceChanged events are raised, and
		Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => workspaceManager.MonitorWorkspaceChanged += h,
			h => workspaceManager.MonitorWorkspaceChanged -= h,
			workspaceManager.Initialize
		);

		// The workspaces are initialized
		workspace.Verify(w => w.Initialize(), Times.Once);
		workspace2.Verify(w => w.Initialize(), Times.Once);
	}

	[Fact]
	public void Add()
	{
		// Given
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		using WorkspaceManager workspaceManager = new(mocks.ConfigContext.Object);

		// When a workspace is added, then WorkspaceAdded is raised
		Assert.Raises<WorkspaceEventArgs>(
			h => workspaceManager.WorkspaceAdded += h,
			h => workspaceManager.WorkspaceAdded -= h,
			() => workspaceManager.Add(workspace.Object)
		);
	}

	[Fact]
	public void Remove_Workspace_RequireAtLeastNWorkspace()
	{
		// Given
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };

		// When a workspace is removed, it returns false, as there must be at least N workspaces,
		// where N is the number of monitors
		Assert.False(workspaceManager.Remove(workspace.Object));
	}

	[Fact]
	public void Remove_Workspace_NotFound()
	{
		// Given
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };

		// When a workspace is removed, it returns false, as the workspace was not found
		Assert.False(workspaceManager.Remove(new Mock<IWorkspace>().Object));
	}

	[Fact]
	public void Remove_Workspace_Success()
	{
		Mock<IMonitor>[] monitorMocks = new[] { new Mock<IMonitor>() };
		// Given
		MocksBuilder mocks = new(monitorMocks);

		Mock<IWorkspace> workspace = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, new Mock<IWorkspace>().Object, };

		// When a workspace is removed, it returns true, and WorkspaceRemoved is raised
		var result = Assert.Raises<WorkspaceEventArgs>(
			h => workspaceManager.WorkspaceRemoved += h,
			h => workspaceManager.WorkspaceRemoved -= h,
			() => workspaceManager.Remove(workspace.Object)
		);
		Assert.Equal(workspace.Object, result.Arguments.Workspace);
	}

	[Fact]
	public void Remove_String_NotFound()
	{
		// Given
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };

		// When a workspace is removed, it returns false, as the workspace was not found
		Assert.False(workspaceManager.Remove("not found"));
	}

	[Fact]
	public void Remove_String_Success()
	{
		// Given
		MocksBuilder mocks = new(new[] { new Mock<IMonitor>() });

		Mock<IWorkspace> workspace = new();
		workspace.Setup(w => w.Name).Returns("workspace");
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, new Mock<IWorkspace>().Object, };

		// When a workspace is removed, it returns true, and WorkspaceRemoved is raised
		var result = Assert.Raises<WorkspaceEventArgs>(
			h => workspaceManager.WorkspaceRemoved += h,
			h => workspaceManager.WorkspaceRemoved -= h,
			() => workspaceManager.Remove("workspace")
		);
		Assert.Equal(workspace.Object, result.Arguments.Workspace);
	}

	[Fact]
	public void TryGet_Null()
	{
		// Given
		MocksBuilder mocks = new();
		using WorkspaceManager workspaceManager = new(mocks.ConfigContext.Object);

		// When getting a workspace which does not exist, then null is returned
		Assert.Null(workspaceManager.TryGet("not found"));
	}

	[Fact]
	public void TryGet_Success()
	{
		// Given
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		workspace.Setup(w => w.Name).Returns("workspace");
		using WorkspaceManager workspaceManager = new(mocks.ConfigContext.Object) { workspace.Object };

		// When getting a workspace which does exist, then the workspace is returned
		Assert.Equal(workspace.Object, workspaceManager.TryGet("workspace"));
	}

	[Fact]
	public void Activate_NoOldWorkspace()
	{
		// Given
		Mock<IMonitor>[] monitorMocks = new[] { new Mock<IMonitor>() };
		MocksBuilder mocks = new(monitorMocks);

		Mock<IWorkspace> workspace = new();
		using WorkspaceManager workspaceManager = new(mocks.ConfigContext.Object) { workspace.Object };

		// When a workspace is activated when there are no other workspaces activated, then it is
		// focused on the focused monitor and raises an event,
		var result = Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => workspaceManager.MonitorWorkspaceChanged += h,
			h => workspaceManager.MonitorWorkspaceChanged -= h,
			() => workspaceManager.Activate(workspace.Object)
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
		Mock<IMonitor>[] monitorMocks = new[] { new Mock<IMonitor>() };
		MocksBuilder mocks = new(monitorMocks);

		Mock<IWorkspace> oldWorkspace = new();
		Mock<IWorkspace> newWorkspace = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { oldWorkspace.Object, newWorkspace.Object };

		workspaceManager.Activate(oldWorkspace.Object);

		// When a workspace is activated when there is another workspace activated, then the old
		// workspace is deactivated, the new workspace is activated, and an event is raised
		var result = Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => workspaceManager.MonitorWorkspaceChanged += h,
			h => workspaceManager.MonitorWorkspaceChanged -= h,
			() => workspaceManager.Activate(newWorkspace.Object)
		);
		Assert.Equal(newWorkspace.Object, result.Arguments.NewWorkspace);
		Assert.Equal(oldWorkspace.Object, result.Arguments.OldWorkspace);

		// The old workspace is deactivated, the new workspace is laid out, and the first window is
		// focused.
		oldWorkspace.Verify(w => w.Deactivate(), Times.Once);
		newWorkspace.Verify(w => w.DoLayout(), Times.Once);
		newWorkspace.Verify(w => w.FocusFirstWindow(), Times.Once);
	}

	[Fact]
	public void GetMonitorForWorkspace_NoWorkspace()
	{
		// Given
		Mock<IMonitor>[] monitorMocks = new[] { new Mock<IMonitor>() };
		MocksBuilder mocks = new(monitorMocks);

		Mock<IWorkspace> workspace = new();
		using WorkspaceManager workspaceManager = new(mocks.ConfigContext.Object) { workspace.Object, };
		workspaceManager.Activate(workspace.Object);

		// When we get the monitor for a workspace which isn't in the workspace manager
		IMonitor? monitor = workspaceManager.GetMonitorForWorkspace(new Mock<IWorkspace>().Object);

		// Then null is returned
		Assert.Null(monitor);
	}

	[Fact]
	public void GetMonitorForWorkspace_Success()
	{
		// Given
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();

		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };
		workspaceManager.Activate(workspace.Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// When we get the monitor for a workspace which is in the workspace manager
		IMonitor? monitor = workspaceManager.GetMonitorForWorkspace(workspace.Object);

		// Then the monitor is returned
		Assert.Equal(mocks.Monitors[0].Object, monitor);
	}

	[Fact]
	public void LayoutAllActiveWorkspaces()
	{
		// Given
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();

		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };
		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// Reset the mocks
		workspace.Reset();
		workspace2.Reset();

		// When we layout all active workspaces
		workspaceManager.LayoutAllActiveWorkspaces();

		// Then all active workspaces are laid out
		workspace.Verify(w => w.DoLayout(), Times.Once());
		workspace2.Verify(w => w.DoLayout(), Times.Once());
	}

	[Fact]
	public void WindowAdded_NoRouter()
	{
		// Given
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();

		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };

		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// Reset the mocks
		workspace.Reset();
		workspace2.Reset();

		// When a window is added
		Mock<IWindow> window = new();
		workspaceManager.WindowAdded(window.Object);

		// Then the window is added to the active workspace
		workspace.Verify(w => w.AddWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void WindowAdded_Router()
	{
		// Given
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();

		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };

		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// Reset the mocks
		workspace.Reset();
		workspace2.Reset();

		// There is a router which routes the window to a different workspace
		Mock<IWindow> window = new();
		mocks.RouterManager.Setup(r => r.RouteWindow(window.Object)).Returns(workspace2.Object);

		// When a window is added
		workspaceManager.WindowAdded(window.Object);

		// Then the window is added to the workspace returned by the router
		workspace.Verify(w => w.AddWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Once());
	}

	[Fact]
	public void WindowAdded_RouterToActive()
	{
		// Given
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();

		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };

		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// Reset the mocks
		workspace.Reset();
		workspace2.Reset();

		// There is a router which routes the window to the active workspace
		Mock<IWindow> window = new();
		mocks.RouterManager.Setup(r => r.RouteWindow(window.Object)).Returns<IWorkspace?>(null);
		mocks.RouterManager.Setup(r => r.RouteToActiveWorkspace).Returns(true);

		// When a window is added
		workspaceManager.WindowAdded(window.Object);

		// Then the window is added to the active workspace
		workspace.Verify(w => w.AddWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void WindowRemoved_NotFound()
	{
		// Given
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();

		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };

		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// Reset the mocks
		workspace.Reset();
		workspace2.Reset();

		// When a window is removed
		Mock<IWindow> window = new();
		workspaceManager.WindowRemoved(window.Object);

		// Then the window is removed from all workspaces
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.RemoveWindow(window.Object), Times.Never());
	}

	[Fact]
	public void WindowRemoved_Found()
	{
		// Given
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();

		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };

		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		Mock<IWindow> window = new();
		workspaceManager.WindowAdded(window.Object);
		// Reset the mocks
		workspace.Reset();
		workspace2.Reset();

		// When a window which is in a workspace is removed
		workspaceManager.WindowRemoved(window.Object);

		// Then the window is removed from the workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.RemoveWindow(window.Object), Times.Never());
	}

	[Fact]
	public void MoveWindowToWorkspace_NoWindow()
	{
		// Given
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();

		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };

		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// Reset the mocks
		workspace.Reset();
		workspace2.Reset();

		// When a window which is not in a workspace is moved to a workspace
		workspaceManager.MoveWindowToWorkspace(workspace.Object);

		// Then the window is not added to the workspace
		workspace.Verify(w => w.RemoveWindow(It.IsAny<IWindow>()), Times.Never());
		workspace2.Verify(w => w.AddWindow(It.IsAny<IWindow>()), Times.Never());
	}

	[Fact]
	public void MoveWindowToWorkspace_PhantomWindow()
	{
		// Given
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };
		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		Mock<IWindow> window = new();
		// When a phantom window is added
		workspaceManager.AddPhantomWindow(workspace.Object, window.Object);
		workspace.Reset();
		workspace2.Reset();

		// and moved to a workspace
		workspaceManager.MoveWindowToWorkspace(workspace.Object, window.Object);

		// Then the window is not removed or added to any workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void MoveWindowToWorkspace_CannotFindWindow()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();

		// there are 3 workspaces
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		Mock<IWorkspace> workspace3 = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object, workspace3.Object };

		// and 3 workspaces
		workspace.Reset();
		workspace2.Reset();
		workspace3.Reset();

		// When a window not in any workspace is moved to a workspace
		workspaceManager.MoveWindowToWorkspace(workspace.Object, window.Object);

		// Then the window is not removed or added to any workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void MoveWindowToWorkspace_Success()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();

		// there are 3 workspaces
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		Mock<IWorkspace> workspace3 = new();

		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };
		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// and the window is added
		workspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();
		workspace3.Reset();

		// When a window in a workspace is moved to another workspace
		workspaceManager.MoveWindowToWorkspace(workspace2.Object, window.Object);

		// Then the window is removed from the first workspace and added to the second
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Once());
	}

	[Fact]
	public void MoveWindowToMonitor_NoWindow()
	{
		// Given
		MocksBuilder mocks = new();

		// there are 2 workspaces
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();

		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };

		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// Reset the mocks
		workspace.Reset();
		workspace2.Reset();

		// When there is no focused window
		workspaceManager.MoveWindowToMonitor(mocks.Monitors[0].Object);

		// Then the window is not added to the workspace
		workspace.Verify(w => w.RemoveWindow(It.IsAny<IWindow>()), Times.Never());
		workspace2.Verify(w => w.AddWindow(It.IsAny<IWindow>()), Times.Never());
	}

	[Fact]
	public void MoveWindowToMonitor_NoOldMonitor()
	{
		// Given
		MocksBuilder mocks = new();

		// there are 2 workspaces
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };

		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// Reset the mocks
		workspace.Reset();
		workspace2.Reset();

		Mock<IWindow> window = new();

		// When a window which is not in a workspace is moved to a monitor
		workspaceManager.MoveWindowToMonitor(mocks.Monitors[0].Object, window.Object);

		// Then the window is not added to the workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void MoveWindowToMonitor_OldMonitorIsNewMonitor()
	{
		// Given
		MocksBuilder mocks = new();

		// there are 2 workspaces
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };
		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		Mock<IWindow> window = new();
		workspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();

		// When a window which is in a workspace is moved to the same monitor
		workspaceManager.MoveWindowToMonitor(mocks.Monitors[0].Object, window.Object);

		// Then the window is not added to the workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void MoveWindowToMonitor_WorkspaceForMonitorNotFound()
	{
		// Given
		MocksBuilder mocks = new();

		// there are 2 workspaces
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };
		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		Mock<IWindow> window = new();
		workspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();

		// When a window which is in a workspace is moved to a monitor which isn't registered
		workspaceManager.MoveWindowToMonitor(new Mock<IMonitor>().Object, window.Object);

		// Then the window is not added to the workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void MoveWindowToMonitor_Success()
	{
		// Given
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };
		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		Mock<IWindow> window = new();
		workspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();

		// When a window which is in a workspace is moved to a monitor
		workspaceManager.MoveWindowToMonitor(mocks.Monitors[1].Object, window.Object);

		// Then the window is removed from the old workspace and added to the new workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Once());
	}

	[Fact]
	public void MoveWindowToPreviousMonitor_Success()
	{
		// Given
		MocksBuilder mocks = new();

		mocks.MonitorManager
			.Setup(m => m.GetPreviousMonitor(mocks.Monitors[0].Object))
			.Returns(mocks.Monitors[1].Object);

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };
		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		Mock<IWindow> window = new();
		workspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();

		// When a window which is in a workspace is moved to the previous monitor
		workspaceManager.MoveWindowToPreviousMonitor(window.Object);

		// Then the window is removed from the old workspace and added to the new workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Once());
	}

	[Fact]
	public void MoveWindowToNextMonitor_Success()
	{
		// Given
		MocksBuilder mocks = new();

		mocks.MonitorManager.Setup(m => m.GetNextMonitor(mocks.Monitors[0].Object)).Returns(mocks.Monitors[1].Object);

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };
		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		Mock<IWindow> window = new();
		workspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();

		// When a window which is in a workspace is moved to the next monitor
		workspaceManager.MoveWindowToNextMonitor(window.Object);

		// Then the window is removed from the old workspace and added to the new workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Once());
	}

	[Fact]
	public void MoveWindowToPoint_TargetWorkspaceNotFound()
	{
		// Given
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		using WorkspaceManager workspaceManager = new(mocks.ConfigContext.Object) { workspace.Object };
		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);

		Mock<IWindow> window = new();
		workspaceManager.WindowAdded(window.Object);
		workspace.Reset();

		mocks.MonitorManager
			.Setup(m => m.GetMonitorAtPoint(It.IsAny<IPoint<int>>()))
			.Returns(new Mock<IMonitor>().Object);

		// When a window which is in a workspace is moved to a point which doesn't correspond to any workspaces
		workspaceManager.MoveWindowToPoint(window.Object, new Point<int>() { X = 0, Y = 0 });

		// Then the window is not removed from the old workspace and not added to the new workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace.Verify(w => w.MoveWindowToPoint(window.Object, It.IsAny<Point<double>>(), false), Times.Never());
	}

	[Fact]
	public void MoveWindowToPoint_PhantomWindow()
	{
		// Given
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };
		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);

		Mock<IWindow> window = new();
		workspaceManager.AddPhantomWindow(workspace.Object, window.Object);
		workspace.Reset();
		workspace2.Reset();

		mocks.MonitorManager.Setup(m => m.GetMonitorAtPoint(It.IsAny<IPoint<int>>())).Returns(mocks.Monitors[1].Object);

		// When a phantom is moved
		workspaceManager.MoveWindowToPoint(window.Object, new Point<int>() { X = 0, Y = 0 });

		// Then nothing happens
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace.Verify(w => w.MoveWindowToPoint(window.Object, It.IsAny<Point<double>>(), false), Times.Never());
	}

	[Fact]
	public void MoveWindowToPoint_CannotRemoveWindow()
	{
		// Given
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };
		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);

		Mock<IWindow> window = new();
		workspaceManager.WindowAdded(window.Object);
		workspace.Reset();

		mocks.MonitorManager.Setup(m => m.GetMonitorAtPoint(It.IsAny<IPoint<int>>())).Returns(mocks.Monitors[1].Object);
		workspace.Setup(w => w.RemoveWindow(window.Object)).Returns(false);

		// When a window is moved to a point, but the window cannot be removed from the old workspace
		workspaceManager.MoveWindowToPoint(window.Object, new Point<int>() { X = 0, Y = 0 });

		// Then nothing happens
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace.Verify(w => w.MoveWindowToPoint(window.Object, It.IsAny<Point<double>>(), false), Times.Never());
	}

	[Fact]
	public void MoveWindowToPoint_Success()
	{
		// Given
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };
		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		Mock<IWindow> window = new();
		workspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();

		mocks.MonitorManager.Setup(m => m.GetMonitorAtPoint(It.IsAny<IPoint<int>>())).Returns(mocks.Monitors[1].Object);
		mocks.Monitors[1].Setup(m => m.WorkingArea).Returns(new Location<int>());
		workspace.Setup(w => w.RemoveWindow(window.Object)).Returns(true);

		// When a window is moved to a point
		workspaceManager.MoveWindowToPoint(window.Object, new Point<int>() { X = 0, Y = 0 });

		// Then the window is removed from the old workspace and added to the new workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace.Verify(w => w.MoveWindowToPoint(window.Object, It.IsAny<Point<double>>(), false), Times.Never());

		window.Verify(w => w.Focus(), Times.Once());
	}

	[Fact]
	public void WindowFocused()
	{
		// Given
		Mock<IConfigContext> configContext = new();

		Mock<ILayoutEngine> layoutEngine = new();
		Mock<Workspace> workspace = new(configContext.Object, "test", layoutEngine.Object);
		Mock<Workspace> workspace2 = new(configContext.Object, "test", layoutEngine.Object);

		WorkspaceManager workspaceManager = new(configContext.Object) { workspace.Object, workspace2.Object };

		Mock<IWindow> window = new();

		// When a window is focused
		workspaceManager.WindowFocused(window.Object);

		// Then the window is focused in all workspaces
		workspace.Verify(w => w.WindowFocused(window.Object), Times.Once());
		workspace2.Verify(w => w.WindowFocused(window.Object), Times.Once());
	}

	[Fact]
	public void WindowMinimizeStart_CouldNotFindWindow()
	{
		// Given
		Mock<IConfigContext> configContext = new();
		Mock<IWorkspace> workspace = new();

		WorkspaceManager workspaceManager = new(configContext.Object) { workspace.Object };

		Mock<IWindow> window = new();

		// When a window is minimized, but the window is not found in any workspace
		workspaceManager.WindowMinimizeStart(window.Object);

		// Then nothing happens
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
	}

	[Fact]
	public void WindowMinimizeStart()
	{
		// Given
		Mock<IWorkspace> workspace = new();

		Mock<IRouterManager> routerManager = new();
		routerManager.Setup(r => r.RouteWindow(It.IsAny<IWindow>())).Returns(workspace.Object);

		Mock<IConfigContext> configContext = new();
		configContext.Setup(c => c.RouterManager).Returns(routerManager.Object);

		WorkspaceManager workspaceManager = new(configContext.Object) { workspace.Object };

		Mock<IWindow> window = new();
		workspaceManager.WindowAdded(window.Object);

		// When a window is minimized
		workspaceManager.WindowMinimizeStart(window.Object);

		// Then the window is removed from the workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
	}

	[Fact]
	public void WindowMinimizeEnd()
	{
		// Given
		Mock<IWorkspace> workspace = new();

		Mock<IRouterManager> routerManager = new();
		routerManager.Setup(r => r.RouteWindow(It.IsAny<IWindow>())).Returns(workspace.Object);

		Mock<IConfigContext> configContext = new();
		configContext.Setup(c => c.RouterManager).Returns(routerManager.Object);

		WorkspaceManager workspaceManager = new(configContext.Object) { workspace.Object };

		Mock<IWindow> window = new();

		// When a window is restored
		workspaceManager.WindowMinimizeEnd(window.Object);

		// Then the window is routed to the workspace
		routerManager.Verify(r => r.RouteWindow(window.Object), Times.Once());
	}
}
