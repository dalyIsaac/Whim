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
		MocksBuilder mocks = new();

		using WorkspaceManager workspaceManager = new(mocks.ConfigContext.Object);
		Assert.Throws<InvalidOperationException>(workspaceManager.Initialize);
	}

	[Fact]
	public void Initialize_Success()
	{
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();

		// Given the workspace manager has two workspaces
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
		Mock<IMonitor>[] monitorMocks = new[] { new Mock<IMonitor>() };
		MocksBuilder mocks = new(monitorMocks);

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
		MocksBuilder mocks = new();

		using WorkspaceManager workspaceManager = new(mocks.ConfigContext.Object);
		Assert.Null(workspaceManager.TryGet("not found"));
	}

	[Fact]
	public void TryGet_Success()
	{
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		workspace.Setup(w => w.Name).Returns("workspace");
		using WorkspaceManager workspaceManager = new(mocks.ConfigContext.Object) { workspace.Object };

		Assert.Equal(workspace.Object, workspaceManager.TryGet("workspace"));
	}

	[Fact]
	public void Activate_NoOldWorkspace()
	{
		Mock<IMonitor>[] monitorMocks = new[] { new Mock<IMonitor>() };
		MocksBuilder mocks = new(monitorMocks);

		Mock<IWorkspace> workspace = new();
		using WorkspaceManager workspaceManager = new(mocks.ConfigContext.Object) { workspace.Object };

		// When a workspace is activated, it is focused on the focused monitor
		var result = Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => workspaceManager.MonitorWorkspaceChanged += h,
			h => workspaceManager.MonitorWorkspaceChanged -= h,
			() => workspaceManager.Activate(workspace.Object)
		);
		Assert.Equal(workspace.Object, result.Arguments.NewWorkspace);
		Assert.Null(result.Arguments.OldWorkspace);

		workspace.Verify(w => w.DoLayout(), Times.Once);
		workspace.Verify(w => w.FocusFirstWindow(), Times.Once);
	}

	[Fact]
	public void Activate_WithOldWorkspace()
	{
		Mock<IMonitor>[] monitorMocks = new[] { new Mock<IMonitor>() };
		MocksBuilder mocks = new(monitorMocks);

		Mock<IWorkspace> oldWorkspace = new();
		Mock<IWorkspace> newWorkspace = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { oldWorkspace.Object, newWorkspace.Object };

		workspaceManager.Activate(oldWorkspace.Object);

		// When a workspace is activated, it is focused on the focused monitor
		var result = Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => workspaceManager.MonitorWorkspaceChanged += h,
			h => workspaceManager.MonitorWorkspaceChanged -= h,
			() => workspaceManager.Activate(newWorkspace.Object)
		);
		Assert.Equal(newWorkspace.Object, result.Arguments.NewWorkspace);
		Assert.Equal(oldWorkspace.Object, result.Arguments.OldWorkspace);

		oldWorkspace.Verify(w => w.Deactivate(), Times.Once);
		newWorkspace.Verify(w => w.DoLayout(), Times.Once);
		newWorkspace.Verify(w => w.FocusFirstWindow(), Times.Once);
	}

	[Fact]
	public void GetMonitorForWorkspace_NoWorkspace()
	{
		Mock<IMonitor>[] monitorMocks = new[] { new Mock<IMonitor>() };
		MocksBuilder mocks = new(monitorMocks);

		Mock<IWorkspace> workspace = new();
		using WorkspaceManager workspaceManager = new(mocks.ConfigContext.Object) { workspace.Object, };
		workspaceManager.Activate(workspace.Object);

		// Get the monitor for a workspace which isn't in the workspace manager
		Assert.Null(workspaceManager.GetMonitorForWorkspace(new Mock<IWorkspace>().Object));
	}

	[Fact]
	public void GetMonitorForWorkspace_Success()
	{
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };
		workspaceManager.Activate(workspace.Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// Get the monitor for a workspace which is in the workspace manager
		Assert.Equal(mocks.Monitors[0].Object, workspaceManager.GetMonitorForWorkspace(workspace.Object));
	}

	[Fact]
	public void LayoutAllActiveWorkspaces()
	{
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };
		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);
		workspace.Reset();
		workspace2.Reset();

		workspaceManager.LayoutAllActiveWorkspaces();

		workspace.Verify(w => w.DoLayout(), Times.Once());
		workspace2.Verify(w => w.DoLayout(), Times.Once());
	}

	[Fact]
	public void WindowAdded_NoRouter()
	{
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };
		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);
		workspace.Reset();
		workspace2.Reset();

		Mock<IWindow> window = new();
		workspaceManager.WindowAdded(window.Object);

		workspace.Verify(w => w.AddWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void WindowAdded_Router()
	{
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };
		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);
		workspace.Reset();
		workspace2.Reset();

		Mock<IWindow> window = new();
		mocks.RouterManager.Setup(r => r.RouteWindow(window.Object)).Returns(workspace2.Object);

		workspaceManager.WindowAdded(window.Object);

		workspace.Verify(w => w.AddWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Once());
	}

	[Fact]
	public void WindowAdded_RouterToActive()
	{
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };
		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);
		workspace.Reset();
		workspace2.Reset();

		Mock<IWindow> window = new();
		mocks.RouterManager.Setup(r => r.RouteWindow(window.Object)).Returns<IWorkspace?>(null);
		mocks.RouterManager.Setup(r => r.RouteToActiveWorkspace).Returns(true);

		workspaceManager.WindowAdded(window.Object);

		workspace.Verify(w => w.AddWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void WindowRemoved_NotFound()
	{
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };
		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);
		workspace.Reset();
		workspace2.Reset();

		Mock<IWindow> window = new();
		workspaceManager.WindowRemoved(window.Object);

		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.RemoveWindow(window.Object), Times.Never());
	}

	[Fact]
	public void WindowRemoved_Found()
	{
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

		workspaceManager.WindowRemoved(window.Object);

		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.RemoveWindow(window.Object), Times.Never());
	}

	[Fact]
	public void MoveWindowToWorkspace_NoWindow()
	{
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };
		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);
		workspace.Reset();
		workspace2.Reset();

		workspaceManager.MoveWindowToWorkspace(workspace.Object);

		workspace.Verify(w => w.RemoveWindow(It.IsAny<IWindow>()), Times.Never());
		workspace2.Verify(w => w.AddWindow(It.IsAny<IWindow>()), Times.Never());
	}

	[Fact]
	public void MoveWindowToWorkspace_PhantomWindow()
	{
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };
		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		Mock<IWindow> window = new();
		workspaceManager.AddPhantomWindow(workspace.Object, window.Object);
		workspace.Reset();
		workspace2.Reset();

		workspaceManager.MoveWindowToWorkspace(workspace.Object, window.Object);

		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void MoveWindowToWorkspace_CannotFindWindow()
	{
		// Given
		MocksBuilder mocks = new();

		// Workspaces
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		Mock<IWorkspace> workspace3 = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object, workspace3.Object };

		// Windows
		Mock<IWindow> window = new();
		workspace.Reset();
		workspace2.Reset();
		workspace3.Reset();

		// When
		workspaceManager.MoveWindowToWorkspace(workspace.Object, window.Object);

		// Then
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void MoveWindowToWorkspace_Success()
	{
		// Given
		MocksBuilder mocks = new();

		// Workspaces
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		Mock<IWorkspace> workspace3 = new();

		// Workspace manager
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };
		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);

		// Windows
		Mock<IWindow> window = new();
		workspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();
		workspace3.Reset();

		// When
		workspaceManager.MoveWindowToWorkspace(workspace2.Object, window.Object);

		// Then
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Once());
	}

	[Fact]
	public void MoveWindowToMonitor_NoWindow()
	{
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };
		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);
		workspace.Reset();
		workspace2.Reset();

		workspaceManager.MoveWindowToMonitor(mocks.Monitors[0].Object);

		workspace.Verify(w => w.RemoveWindow(It.IsAny<IWindow>()), Times.Never());
		workspace2.Verify(w => w.AddWindow(It.IsAny<IWindow>()), Times.Never());
	}

	[Fact]
	public void MoveWindowToMonitor_NoOldMonitor()
	{
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager =
			new(mocks.ConfigContext.Object) { workspace.Object, workspace2.Object };
		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, mocks.Monitors[1].Object);
		workspace.Reset();
		workspace2.Reset();

		Mock<IWindow> window = new();
		workspaceManager.MoveWindowToMonitor(mocks.Monitors[0].Object, window.Object);

		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void MoveWindowToMonitor_OldMonitorIsNewMonitor()
	{
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

		workspaceManager.MoveWindowToMonitor(mocks.Monitors[0].Object, window.Object);

		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void MoveWindowToMonitor_WorkspaceForMonitorNotFound()
	{
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

		workspaceManager.MoveWindowToMonitor(new Mock<IMonitor>().Object, window.Object);

		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void MoveWindowToMonitor_Success()
	{
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

		workspaceManager.MoveWindowToMonitor(mocks.Monitors[1].Object, window.Object);

		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Once());
	}

	[Fact]
	public void MoveWindowToPreviousMonitor_Success()
	{
		MocksBuilder mocks = new();

		mocks.MonitorManager.Setup(m => m.GetPreviousMonitor(mocks.Monitors[0].Object)).Returns(mocks.Monitors[1].Object);

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

		workspaceManager.MoveWindowToPreviousMonitor(window.Object);

		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Once());
	}

	[Fact]
	public void MoveWindowToNextMonitor_Success()
	{
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

		workspaceManager.MoveWindowToNextMonitor(window.Object);

		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Once());
	}

	[Fact]
	public void MoveWindowToPoint_TargetWorkspaceNotFound()
	{
		MocksBuilder mocks = new();

		Mock<IWorkspace> workspace = new();
		using WorkspaceManager workspaceManager = new(mocks.ConfigContext.Object) { workspace.Object };
		workspaceManager.Activate(workspace.Object, mocks.Monitors[0].Object);

		Mock<IWindow> window = new();
		workspaceManager.WindowAdded(window.Object);
		workspace.Reset();

		mocks.MonitorManager.Setup(m => m.GetMonitorAtPoint(It.IsAny<IPoint<int>>())).Returns(new Mock<IMonitor>().Object);

		workspaceManager.MoveWindowToPoint(window.Object, new Point<int>() { X = 0, Y = 0 });

		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace.Verify(w => w.MoveWindowToPoint(window.Object, It.IsAny<Point<double>>(), false), Times.Never());

		workspace.Verify(w => w.DoLayout(), Times.Never());
	}

	[Fact]
	public void MoveWindowToPoint_PhantomWindow()
	{
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

		workspaceManager.MoveWindowToPoint(window.Object, new Point<int>() { X = 0, Y = 0 });

		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace.Verify(w => w.MoveWindowToPoint(window.Object, It.IsAny<Point<double>>(), false), Times.Never());

		workspace.Verify(w => w.DoLayout(), Times.Never());
		workspace2.Verify(w => w.DoLayout(), Times.Never());
	}

	[Fact]
	public void MoveWindowToPoint_CannotRemoveWindow()
	{
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

		workspaceManager.MoveWindowToPoint(window.Object, new Point<int>() { X = 0, Y = 0 });

		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace.Verify(w => w.MoveWindowToPoint(window.Object, It.IsAny<Point<double>>(), false), Times.Never());

		workspace.Verify(w => w.DoLayout(), Times.Never());
		workspace2.Verify(w => w.DoLayout(), Times.Never());
	}

	[Fact]
	public void MoveWindowToPoint_Success()
	{
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

		workspaceManager.MoveWindowToPoint(window.Object, new Point<int>() { X = 0, Y = 0 });

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

		// When
		workspaceManager.WindowFocused(window.Object);

		// Then
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

		// When
		workspaceManager.WindowMinimizeStart(window.Object);

		// Then
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

		// When
		workspaceManager.WindowMinimizeStart(window.Object);

		// Then
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

		// When
		workspaceManager.WindowMinimizeEnd(window.Object);

		// Then
		routerManager.Verify(r => r.RouteWindow(window.Object), Times.Once());
	}
}
