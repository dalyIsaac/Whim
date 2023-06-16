using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Whim.Tests;

public class WorkspaceManagerTests
{
	private class WorkspaceManagerTestWrapper : WorkspaceManager
	{
		// Yes, I know it's bad to have `_triggers` be `internal` in `WorkspaceManager`.
		public WorkspaceManagerTriggers InternalTriggers => _triggers;

		public WorkspaceManagerTestWrapper(IContext context)
			: base(context) { }

		public void Add(IWorkspace workspace) => _workspaces.Add(workspace);
	}

	private class Wrapper
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<IMonitorManager> MonitorManager { get; } = new();
		public Mock<IMonitor>[] Monitors { get; }
		public Mock<IRouterManager> RouterManager { get; } = new();
		public Mock<INativeManager> NativeManager { get; } = new();
		public WorkspaceManagerTestWrapper WorkspaceManager { get; }

		public Wrapper(
			Mock<IWorkspace>[]? workspaces = null,
			Mock<IMonitor>[]? monitors = null,
			int activeMonitorIndex = 0
		)
		{
			Context.Setup(c => c.MonitorManager).Returns(MonitorManager.Object);
			Context.Setup(c => c.NativeManager).Returns(NativeManager.Object);
			Context.Setup(c => c.RouterManager).Returns(RouterManager.Object);

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
				mon.Setup(m => m.WorkingArea).Returns(new Location<int>());
			}

			RouterManager.Setup(r => r.RouteWindow(It.IsAny<IWindow>())).Returns(null as IWorkspace);

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
		Wrapper wrapper = new();

		// Then
		Assert.Throws<InvalidOperationException>(wrapper.WorkspaceManager.Initialize);
	}

	[Fact]
	public void Initialize_Success()
	{
		// Given the workspace manager has two workspaces
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		Wrapper wrapper = new(new[] { workspace, workspace2 });

		// When the workspace manager is initialized, then a MonitorWorkspaceChanged event is raised
		Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => wrapper.WorkspaceManager.MonitorWorkspaceChanged += h,
			h => wrapper.WorkspaceManager.MonitorWorkspaceChanged -= h,
			wrapper.WorkspaceManager.Initialize
		);

		// The workspaces are initialized
		workspace.Verify(w => w.Initialize(), Times.Once);
		workspace2.Verify(w => w.Initialize(), Times.Once);
	}

	[Fact]
	public void Add_AfterInitialize()
	{
		// Given the workspace manager is already initialized
		Wrapper wrapper = new(new[] { new Mock<IWorkspace>(), new Mock<IWorkspace>() });
		Mock<ProxyLayoutEngine> proxyLayoutEngine = new();

		// When a workspace is added, then WorkspaceAdded is raised
		wrapper.WorkspaceManager.AddProxyLayoutEngine(proxyLayoutEngine.Object);
		wrapper.WorkspaceManager.Initialize();
		Assert.Raises<WorkspaceEventArgs>(
			h => wrapper.WorkspaceManager.WorkspaceAdded += h,
			h => wrapper.WorkspaceManager.WorkspaceAdded -= h,
			() => wrapper.WorkspaceManager.Add("new workspace")
		);

		// Verify that the workspace was initialized
		proxyLayoutEngine.Verify(p => p(It.IsAny<ILayoutEngine>()), Times.Once);
	}

	[Fact]
	public void Add_BeforeInitialize()
	{
		// Given the workspace manager is not initialized
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace1 = new();
		Wrapper wrapper = new(new[] { workspace, workspace1 });

		// When
		wrapper.WorkspaceManager.Initialize();

		// Then
		workspace.Verify(w => w.Initialize(), Times.Once);
	}

	[Fact]
	public void Remove_Workspace_RequireAtLeastNWorkspace()
	{
		// Given the workspace manager has two workspaces and there are two monitors
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		Wrapper wrapper = new(new[] { workspace, workspace2 });

		// When a workspace is removed
		bool result = wrapper.WorkspaceManager.Remove(workspace.Object);

		// Then it returns false, as there must be at least two workspaces, since there are two monitors
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
		Wrapper wrapper = new(new[] { workspace, workspace2, workspace3 });

		// When a workspace is removed
		bool result = wrapper.WorkspaceManager.Remove(new Mock<IWorkspace>().Object);

		// Then it returns false, as the workspace was not found
		Assert.False(result);
	}

	[Fact]
	public void Remove_Workspace_Success()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		Mock<IMonitor>[] monitorWrapper = new[] { new Mock<IMonitor>() };
		Wrapper wrapper = new(new Mock<IWorkspace>[] { workspace, workspace2 }, monitorWrapper);

		Mock<IWindow> window = new();
		workspace.Setup(w => w.Windows).Returns(new[] { window.Object });

		// When a workspace is removed, it returns true, WorkspaceRemoved is raised
		var result = Assert.Raises<WorkspaceEventArgs>(
			h => wrapper.WorkspaceManager.WorkspaceRemoved += h,
			h => wrapper.WorkspaceManager.WorkspaceRemoved -= h,
			() => Assert.True(wrapper.WorkspaceManager.Remove(workspace.Object))
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
		Wrapper wrapper = new(new[] { workspace, workspace2 });

		// When a workspace is removed, it returns false, as the workspace was not found
		Assert.False(wrapper.WorkspaceManager.Remove("not found"));
	}

	[Fact]
	public void Remove_String_Success()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Wrapper wrapper =
			new(new Mock<IWorkspace>[] { workspace, new Mock<IWorkspace>() }, new[] { new Mock<IMonitor>() });

		workspace.Setup(w => w.Name).Returns("workspace");

		// When a workspace is removed, it returns true, and WorkspaceRemoved is raised
		var result = Assert.Raises<WorkspaceEventArgs>(
			h => wrapper.WorkspaceManager.WorkspaceRemoved += h,
			h => wrapper.WorkspaceManager.WorkspaceRemoved -= h,
			() => wrapper.WorkspaceManager.Remove("workspace")
		);
		Assert.Equal(workspace.Object, result.Arguments.Workspace);
	}

	[Fact]
	public void TryGet_Null()
	{
		// Given
		Wrapper wrapper = new();

		// When getting a workspace which does not exist, then null is returned
		Assert.Null(wrapper.WorkspaceManager.TryGet("not found"));
	}

	[Fact]
	public void TryGet_Success()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		workspace.Setup(w => w.Name).Returns("workspace");
		Wrapper wrapper = new(new[] { workspace });

		// When getting a workspace which does exist, then the workspace is returned
		Assert.Equal(workspace.Object, wrapper.WorkspaceManager.TryGet("workspace"));
	}

	[Fact]
	public void SquareBracket_Get()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		workspace.Setup(w => w.Name).Returns("workspace");
		Wrapper wrapper = new(new[] { workspace });

		// When getting a workspace which does exist, then the workspace is returned
		Assert.Equal(workspace.Object, wrapper.WorkspaceManager["workspace"]);
	}

	[Fact]
	public void GetEnumerator()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		Wrapper wrapper = new(new[] { workspace, workspace2 });

		// When enumerating the workspaces, then the workspaces are returned
		Assert.Equal(new[] { workspace.Object, workspace2.Object }, wrapper.WorkspaceManager);
	}

	[Fact]
	public void IEnumerable_GetEnumerator()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		Wrapper wrapper = new(new[] { workspace, workspace2 });

		// When enumerating the workspaces, then the workspaces are returned
		Assert.Equal(new[] { workspace.Object, workspace2.Object }, wrapper.WorkspaceManager);
	}

	[Fact]
	public void Activate_NoPreviousWorkspace()
	{
		// Given
		Mock<IMonitor>[] monitorWrapper = new[] { new Mock<IMonitor>() };
		Mock<IWorkspace> workspace = new();
		Wrapper wrapper = new(new[] { workspace }, monitorWrapper);

		// When a workspace is activated when there are no other workspaces activated, then it is
		// focused on the active monitor and raises an event,
		var result = Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => wrapper.WorkspaceManager.MonitorWorkspaceChanged += h,
			h => wrapper.WorkspaceManager.MonitorWorkspaceChanged -= h,
			() => wrapper.WorkspaceManager.Activate(workspace.Object)
		);
		Assert.Equal(workspace.Object, result.Arguments.CurrentWorkspace);
		Assert.Null(result.Arguments.PreviousWorkspace);

		// Layout is done, and the first window is focused.
		workspace.Verify(w => w.DoLayout(), Times.Once);
		workspace.Verify(w => w.FocusFirstWindow(), Times.Once);
	}

	[Fact]
	public void Activate_WithPreviousWorkspace()
	{
		// Given
		Mock<IWorkspace> previousWorkspace = new();
		Mock<IWorkspace> currentWorkspace = new();
		Mock<IMonitor>[] monitorWrapper = new[] { new Mock<IMonitor>() };
		Wrapper wrapper = new(new[] { previousWorkspace, currentWorkspace }, monitorWrapper);

		wrapper.WorkspaceManager.Activate(previousWorkspace.Object);

		// Reset wrapper
		previousWorkspace.Reset();
		currentWorkspace.Reset();

		// When a workspace is activated when there is another workspace activated, then the old
		// workspace is deactivated, the new workspace is activated, and an event is raised
		var result = Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => wrapper.WorkspaceManager.MonitorWorkspaceChanged += h,
			h => wrapper.WorkspaceManager.MonitorWorkspaceChanged -= h,
			() => wrapper.WorkspaceManager.Activate(currentWorkspace.Object)
		);
		Assert.Equal(currentWorkspace.Object, result.Arguments.CurrentWorkspace);
		Assert.Equal(previousWorkspace.Object, result.Arguments.PreviousWorkspace);

		// The old workspace is deactivated, the new workspace is laid out, and the first window is
		// focused.
		previousWorkspace.Verify(w => w.Deactivate(), Times.Once);
		previousWorkspace.Verify(w => w.DoLayout(), Times.Never);
		currentWorkspace.Verify(w => w.DoLayout(), Times.Once);
		currentWorkspace.Verify(w => w.FocusFirstWindow(), Times.Once);
	}

	[Fact]
	public void Activate_MultipleMonitors()
	{
		// Given there are two workspaces and monitors
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		Wrapper wrapper = new(new[] { workspace, workspace2 });
		IMonitor monitor = wrapper.Monitors[0].Object;
		IMonitor monitor2 = wrapper.Monitors[1].Object;

		wrapper.WorkspaceManager.Activate(workspace.Object, monitor);
		wrapper.WorkspaceManager.Activate(workspace2.Object, monitor2);

		// Reset wrapper
		workspace.Reset();
		workspace2.Reset();

		// When a workspace is activated on a monitor which already has a workspace activated, then
		// an event is raised
		var result = Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => wrapper.WorkspaceManager.MonitorWorkspaceChanged += h,
			h => wrapper.WorkspaceManager.MonitorWorkspaceChanged -= h,
			() => wrapper.WorkspaceManager.Activate(workspace2.Object, monitor)
		);

		Assert.Equal(monitor, result.Arguments.Monitor);
		Assert.Equal(workspace2.Object, result.Arguments.CurrentWorkspace);
		Assert.Equal(workspace.Object, result.Arguments.PreviousWorkspace);

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
		Wrapper wrapper = new(workspaces, monitors);

		wrapper.WorkspaceManager.Activate(workspaces[currentIdx].Object);

		// Reset wrapper
		workspaces[currentIdx].Reset();

		// When the previous workspace is activated, then the previous workspace is activated
		wrapper.WorkspaceManager.ActivatePrevious();

		workspaces[currentIdx].Verify(w => w.Deactivate(), Times.Once);
		workspaces[currentIdx].Verify(w => w.DoLayout(), Times.Never);
		workspaces[currentIdx].Verify(w => w.FocusFirstWindow(), Times.Never);

		workspaces[prevIdx].Verify(w => w.Deactivate(), Times.Never);
		workspaces[prevIdx].Verify(w => w.DoLayout(), Times.Once);
		workspaces[prevIdx].Verify(w => w.FocusFirstWindow(), Times.Once);
	}

	[Fact]
	public void ActivePrevious_CannotFindActiveMonitor()
	{
		// Given
		Mock<IWorkspace>[] workspaces = new[]
		{
			new Mock<IWorkspace>(),
			new Mock<IWorkspace>(),
			new Mock<IWorkspace>()
		};

		Mock<IMonitor>[] monitors = new[] { new Mock<IMonitor>() };
		Wrapper wrapper = new(workspaces, monitors);

		wrapper.WorkspaceManager.Activate(workspaces[0].Object);

		// Reset wrapper
		workspaces[0].Reset();
		wrapper.MonitorManager.Setup(m => m.ActiveMonitor).Returns(new Mock<IMonitor>().Object);

		// When the previous workspace is activated, then the previous workspace is activated
		wrapper.WorkspaceManager.ActivatePrevious();

		workspaces[0].Verify(w => w.Deactivate(), Times.Never);
		workspaces[0].Verify(w => w.DoLayout(), Times.Never);
		workspaces[0].Verify(w => w.FocusFirstWindow(), Times.Never);

		workspaces[1].Verify(w => w.Deactivate(), Times.Never);
		workspaces[1].Verify(w => w.DoLayout(), Times.Never);
		workspaces[1].Verify(w => w.FocusFirstWindow(), Times.Never);
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
		Wrapper wrapper = new(workspaces, monitors);

		wrapper.WorkspaceManager.Activate(workspaces[currentIdx].Object);

		// Reset wrapper
		workspaces[currentIdx].Reset();

		// When the next workspace is activated, then the next workspace is activated
		wrapper.WorkspaceManager.ActivateNext();

		workspaces[currentIdx].Verify(w => w.Deactivate(), Times.Once);
		workspaces[currentIdx].Verify(w => w.DoLayout(), Times.Never);
		workspaces[currentIdx].Verify(w => w.FocusFirstWindow(), Times.Never);

		workspaces[nextIdx].Verify(w => w.Deactivate(), Times.Never);
		workspaces[nextIdx].Verify(w => w.DoLayout(), Times.Once);
		workspaces[nextIdx].Verify(w => w.FocusFirstWindow(), Times.Once);
	}

	[Fact]
	public void ActivateNext_CannotFindActiveMonitor()
	{
		// Given
		Mock<IWorkspace>[] workspaces = new[]
		{
			new Mock<IWorkspace>(),
			new Mock<IWorkspace>(),
			new Mock<IWorkspace>()
		};

		Mock<IMonitor>[] monitors = new[] { new Mock<IMonitor>() };
		Wrapper wrapper = new(workspaces, monitors);

		wrapper.WorkspaceManager.Activate(workspaces[0].Object);

		// Reset wrapper
		workspaces[0].Reset();
		wrapper.MonitorManager.Setup(m => m.ActiveMonitor).Returns(new Mock<IMonitor>().Object);

		// When the next workspace is activated, then the next workspace is activated
		wrapper.WorkspaceManager.ActivateNext();

		workspaces[0].Verify(w => w.Deactivate(), Times.Never);
		workspaces[0].Verify(w => w.DoLayout(), Times.Never);
		workspaces[0].Verify(w => w.FocusFirstWindow(), Times.Never);

		workspaces[1].Verify(w => w.Deactivate(), Times.Never);
		workspaces[1].Verify(w => w.DoLayout(), Times.Never);
		workspaces[1].Verify(w => w.FocusFirstWindow(), Times.Never);
	}

	[Fact]
	public void GetMonitorForWorkspace_NoWorkspace()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IMonitor>[] monitorWrapper = new[] { new Mock<IMonitor>() };
		Wrapper wrapper = new(new[] { workspace }, monitorWrapper);

		wrapper.WorkspaceManager.Activate(workspace.Object);

		// When we get the monitor for a workspace which isn't in the workspace manager
		IMonitor? monitor = wrapper.WorkspaceManager.GetMonitorForWorkspace(new Mock<IWorkspace>().Object);

		// Then null is returned
		Assert.Null(monitor);
	}

	[Fact]
	public void GetMonitorForWorkspace_Success()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		Wrapper wrapper = new(new[] { workspace, workspace2 });

		wrapper.WorkspaceManager.Activate(workspace.Object);
		wrapper.WorkspaceManager.Activate(workspace2.Object, wrapper.Monitors[1].Object);

		// When we get the monitor for a workspace which is in the workspace manager
		IMonitor? monitor = wrapper.WorkspaceManager.GetMonitorForWorkspace(workspace.Object);

		// Then the monitor is returned
		Assert.Equal(wrapper.Monitors[0].Object, monitor);
	}

	[Fact]
	public void LayoutAllActiveWorkspaces()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		Wrapper wrapper = new(new[] { workspace, workspace2 });

		wrapper.WorkspaceManager.Activate(workspace.Object, wrapper.Monitors[0].Object);
		wrapper.WorkspaceManager.Activate(workspace2.Object, wrapper.Monitors[1].Object);

		// Reset the wrapper
		workspace.Reset();
		workspace2.Reset();

		// When we layout all active workspaces
		wrapper.WorkspaceManager.LayoutAllActiveWorkspaces();

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
		Wrapper wrapper = new(new[] { workspace, workspace2 });

		wrapper.WorkspaceManager.Activate(workspace.Object, wrapper.Monitors[0].Object);
		wrapper.WorkspaceManager.Activate(workspace2.Object, wrapper.Monitors[1].Object);

		// Reset the wrapper
		workspace.Reset();
		workspace2.Reset();

		// When a window is added
		Mock<IWindow> window = new();
		wrapper.WorkspaceManager.WindowAdded(window.Object);

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
		Wrapper wrapper = new(new[] { workspace, workspace2 });

		wrapper.MonitorManager
			.Setup(m => m.GetMonitorAtPoint(It.IsAny<IPoint<int>>()))
			.Returns(wrapper.Monitors[0].Object);

		wrapper.WorkspaceManager.Activate(workspace.Object, wrapper.Monitors[0].Object);
		wrapper.WorkspaceManager.Activate(workspace2.Object, wrapper.Monitors[1].Object);

		// Reset the wrapper
		workspace.Reset();
		workspace2.Reset();

		// When a window is added
		Mock<IWindow> window = new();
		wrapper.WorkspaceManager.WindowAdded(window.Object);

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
		Wrapper wrapper = new(new[] { workspace, workspace2 });

		wrapper.WorkspaceManager.Activate(workspace.Object, wrapper.Monitors[0].Object);
		wrapper.WorkspaceManager.Activate(workspace2.Object, wrapper.Monitors[1].Object);

		// Reset the wrapper
		workspace.Reset();
		workspace2.Reset();

		// There is a router which routes the window to a different workspace
		Mock<IWindow> window = new();
		wrapper.RouterManager.Setup(r => r.RouteWindow(window.Object)).Returns(workspace2.Object);

		// When a window is added
		wrapper.WorkspaceManager.WindowAdded(window.Object);

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
		Wrapper wrapper = new(new[] { workspace, workspace2 });

		wrapper.WorkspaceManager.Activate(workspace.Object, wrapper.Monitors[0].Object);
		wrapper.WorkspaceManager.Activate(workspace2.Object, wrapper.Monitors[1].Object);

		// Reset the wrapper
		workspace.Reset();
		workspace2.Reset();

		// There is a router which routes the window to the active workspace
		Mock<IWindow> window = new();
		wrapper.RouterManager.Setup(r => r.RouteWindow(window.Object)).Returns<IWorkspace?>(null);
		wrapper.RouterManager.Setup(r => r.RouteToActiveWorkspace).Returns(true);

		// When a window is added
		wrapper.WorkspaceManager.WindowAdded(window.Object);

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
		Wrapper wrapper = new(new[] { workspace, workspace2 });

		wrapper.WorkspaceManager.Activate(workspace.Object, wrapper.Monitors[0].Object);
		wrapper.WorkspaceManager.Activate(workspace2.Object, wrapper.Monitors[1].Object);

		// Reset the wrapper
		workspace.Reset();
		workspace2.Reset();

		// When a window is removed
		Mock<IWindow> window = new();
		wrapper.WorkspaceManager.WindowRemoved(window.Object);

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
		Wrapper wrapper = new(new[] { workspace, workspace2 });

		wrapper.WorkspaceManager.Activate(workspace.Object, wrapper.Monitors[0].Object);
		wrapper.WorkspaceManager.Activate(workspace2.Object, wrapper.Monitors[1].Object);

		Mock<IWindow> window = new();
		wrapper.WorkspaceManager.WindowAdded(window.Object);
		// Reset the wrapper
		workspace.Reset();
		workspace2.Reset();

		// When a window which is in a workspace is removed
		wrapper.WorkspaceManager.WindowRemoved(window.Object);

		// Then the window is removed from the workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.RemoveWindow(window.Object), Times.Never());
	}

	[Fact]
	public void MoveWindowToWorkspace_PhantomWindow()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		Wrapper wrapper = new(new[] { workspace, workspace2 });

		wrapper.WorkspaceManager.Activate(workspace.Object, wrapper.Monitors[0].Object);
		wrapper.WorkspaceManager.Activate(workspace2.Object, wrapper.Monitors[1].Object);

		Mock<IWindow> window = new();
		// When a phantom window is added
		wrapper.WorkspaceManager.AddPhantomWindow(workspace.Object, window.Object);
		workspace.Reset();
		workspace2.Reset();

		// and moved to a workspace
		wrapper.WorkspaceManager.MoveWindowToWorkspace(window.Object, workspace.Object);

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

		Wrapper wrapper = new(new[] { workspace, workspace2, workspace3 });

		// Reset the wrapper
		workspace.Reset();
		workspace2.Reset();
		workspace3.Reset();

		// When a window not in any workspace is moved to a workspace
		wrapper.WorkspaceManager.MoveWindowToWorkspace(window.Object, workspace.Object);

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

		Wrapper wrapper = new(new[] { workspace, workspace2, workspace3 });

		wrapper.WorkspaceManager.Activate(workspace.Object, wrapper.Monitors[0].Object);
		wrapper.WorkspaceManager.Activate(workspace2.Object, wrapper.Monitors[1].Object);

		// and the window is added
		wrapper.WorkspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();
		workspace3.Reset();
		window.Reset();

		// When a window in a workspace is moved to another workspace
		wrapper.WorkspaceManager.MoveWindowToWorkspace(window.Object, workspace2.Object);

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

		Wrapper wrapper = new(new[] { workspace, workspace2, workspace3 }, monitors);

		wrapper.WorkspaceManager.Activate(workspace.Object, wrapper.Monitors[0].Object);
		wrapper.WorkspaceManager.Activate(workspace.Object, wrapper.Monitors[1].Object);

		// and the window is added
		wrapper.WorkspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();
		workspace3.Reset();
		window.Reset();

		// When a window in a workspace is moved to another workspace
		wrapper.WorkspaceManager.MoveWindowToWorkspace(window.Object, workspace2.Object);

		// Then the window is removed from the first workspace and added to the third
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Once());
		window.Verify(w => w.Hide(), Times.Once());
	}

	[Fact]
	public void MoveWindowToMonitor_NoPreviousMonitor()
	{
		// Given there are 2 workspaces
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();

		Wrapper wrapper = new(new[] { workspace, workspace2 });

		wrapper.WorkspaceManager.Activate(workspace.Object, wrapper.Monitors[0].Object);
		wrapper.WorkspaceManager.Activate(workspace2.Object, wrapper.Monitors[1].Object);

		// Reset the wrapper
		workspace.Reset();
		workspace2.Reset();

		Mock<IWindow> window = new();

		// When a window which is not in a workspace is moved to a monitor
		wrapper.WorkspaceManager.MoveWindowToMonitor(window.Object, wrapper.Monitors[0].Object);

		// Then the window is not added to the workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void MoveWindowToMonitor_PreviousMonitorIsNewMonitor()
	{
		// Given there are 2 workspaces, and the window has been added
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();

		Wrapper wrapper = new(new[] { workspace, workspace2 });

		wrapper.WorkspaceManager.Activate(workspace.Object, wrapper.Monitors[0].Object);
		wrapper.WorkspaceManager.Activate(workspace2.Object, wrapper.Monitors[1].Object);

		Mock<IWindow> window = new();
		wrapper.WorkspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();

		// When a window which is in a workspace is moved to the same monitor
		wrapper.WorkspaceManager.MoveWindowToMonitor(window.Object, wrapper.Monitors[0].Object);

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

		Wrapper wrapper = new(new[] { workspace, workspace2 });

		wrapper.WorkspaceManager.Activate(workspace.Object, wrapper.Monitors[0].Object);
		wrapper.WorkspaceManager.Activate(workspace2.Object, wrapper.Monitors[1].Object);

		Mock<IWindow> window = new();
		wrapper.WorkspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();

		// When a window which is in a workspace is moved to a monitor which isn't registered
		wrapper.WorkspaceManager.MoveWindowToMonitor(window.Object, new Mock<IMonitor>().Object);

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
		Wrapper wrapper = new(new[] { workspace, workspace2 });

		wrapper.WorkspaceManager.Activate(workspace.Object, wrapper.Monitors[0].Object);
		wrapper.WorkspaceManager.Activate(workspace2.Object, wrapper.Monitors[1].Object);

		Mock<IWindow> window = new();
		wrapper.WorkspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();

		// When a window which is in a workspace is moved to a monitor
		wrapper.WorkspaceManager.MoveWindowToMonitor(window.Object, wrapper.Monitors[1].Object);

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

		Wrapper wrapper = new(new[] { workspace, workspace2 });
		wrapper.MonitorManager
			.Setup(m => m.GetPreviousMonitor(wrapper.Monitors[0].Object))
			.Returns(wrapper.Monitors[1].Object);

		wrapper.WorkspaceManager.Activate(workspace.Object, wrapper.Monitors[0].Object);
		wrapper.WorkspaceManager.Activate(workspace2.Object, wrapper.Monitors[1].Object);

		Mock<IWindow> window = new();
		wrapper.WorkspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();

		// When a window which is in a workspace is moved to the previous monitor
		wrapper.WorkspaceManager.MoveWindowToPreviousMonitor(window.Object);

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

		Wrapper wrapper = new(new[] { workspace, workspace2 });
		wrapper.MonitorManager
			.Setup(m => m.GetNextMonitor(wrapper.Monitors[0].Object))
			.Returns(wrapper.Monitors[1].Object);

		wrapper.WorkspaceManager.Activate(workspace.Object, wrapper.Monitors[0].Object);
		wrapper.WorkspaceManager.Activate(workspace2.Object, wrapper.Monitors[1].Object);

		Mock<IWindow> window = new();
		wrapper.WorkspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();

		// When a window which is in a workspace is moved to the next monitor
		wrapper.WorkspaceManager.MoveWindowToNextMonitor(window.Object);

		// Then the window is removed from the old workspace and added to the new workspace
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Once());
	}

	[Fact]
	public void MoveWindowToPoint_TargetWorkspaceNotFound()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Wrapper wrapper = new(new[] { workspace });

		wrapper.WorkspaceManager.Activate(workspace.Object, wrapper.Monitors[0].Object);

		Mock<IWindow> window = new();
		wrapper.WorkspaceManager.WindowAdded(window.Object);
		workspace.Reset();

		wrapper.MonitorManager
			.Setup(m => m.GetMonitorAtPoint(It.IsAny<IPoint<int>>()))
			.Returns(new Mock<IMonitor>().Object);

		// When a window which is in a workspace is moved to a point which doesn't correspond to any workspaces
		wrapper.WorkspaceManager.MoveWindowToPoint(window.Object, new Point<int>() { X = 0, Y = 0 });

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
		Wrapper wrapper = new(new[] { workspace, workspace2 });

		wrapper.WorkspaceManager.Activate(workspace.Object, wrapper.Monitors[0].Object);
		wrapper.WorkspaceManager.Activate(workspace2.Object, wrapper.Monitors[1].Object);

		Mock<IWindow> window = new();
		wrapper.WorkspaceManager.AddPhantomWindow(workspace.Object, window.Object);
		workspace.Reset();
		workspace2.Reset();

		wrapper.MonitorManager
			.Setup(m => m.GetMonitorAtPoint(It.IsAny<IPoint<int>>()))
			.Returns(wrapper.Monitors[1].Object);

		// When a phantom is moved
		wrapper.WorkspaceManager.MoveWindowToPoint(window.Object, new Point<int>() { X = 0, Y = 0 });

		// Then nothing happens
		wrapper.MonitorManager.Verify(m => m.GetMonitorAtPoint(It.IsAny<IPoint<int>>()), Times.Never());
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace.Verify(w => w.MoveWindowToPoint(window.Object, It.IsAny<Point<double>>()), Times.Never());
	}

	[Fact]
	public void MoveWindowToPoint_CannotRemoveWindow()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		Wrapper wrapper = new(new[] { workspace, workspace2 });

		wrapper.WorkspaceManager.Activate(workspace.Object, wrapper.Monitors[0].Object);
		wrapper.WorkspaceManager.Activate(workspace2.Object, wrapper.Monitors[1].Object);

		Mock<IWindow> window = new();
		wrapper.WorkspaceManager.WindowAdded(window.Object);
		workspace.Reset();

		wrapper.MonitorManager
			.Setup(m => m.GetMonitorAtPoint(It.IsAny<IPoint<int>>()))
			.Returns(wrapper.Monitors[1].Object);
		workspace.Setup(w => w.RemoveWindow(window.Object)).Returns(false);

		// When a window is moved to a point, but the window cannot be removed from the old workspace
		wrapper.WorkspaceManager.MoveWindowToPoint(window.Object, new Point<int>() { X = 0, Y = 0 });

		// Then nothing happens
		wrapper.Monitors[0].VerifyGet(m => m.WorkingArea, Times.Never());
		wrapper.Monitors[1].VerifyGet(m => m.WorkingArea, Times.Never());
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace.Verify(w => w.MoveWindowToPoint(window.Object, It.IsAny<Point<double>>()), Times.Never());
	}

	[Fact]
	public void MoveWindowToPoint_Success_DifferentWorkspace()
	{
		// Given
		Mock<IWorkspace> activeWorkspace = new();
		Mock<IWorkspace> targetWorkspace = new();
		Wrapper wrapper = new(new[] { activeWorkspace, targetWorkspace });

		wrapper.WorkspaceManager.Activate(activeWorkspace.Object, wrapper.Monitors[0].Object);
		wrapper.WorkspaceManager.Activate(targetWorkspace.Object, wrapper.Monitors[1].Object);

		Mock<IWindow> window = new();
		wrapper.WorkspaceManager.WindowAdded(window.Object);
		activeWorkspace.Reset();
		targetWorkspace.Reset();

		wrapper.MonitorManager
			.Setup(m => m.GetMonitorAtPoint(It.IsAny<IPoint<int>>()))
			.Returns(wrapper.Monitors[1].Object);
		activeWorkspace.Setup(w => w.RemoveWindow(window.Object)).Returns(true);

		// When a window is moved to a point
		wrapper.WorkspaceManager.MoveWindowToPoint(window.Object, new Point<int>() { X = 0, Y = 0 });

		// Then the window is removed from the old workspace and added to the new workspace
		activeWorkspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		activeWorkspace.Verify(w => w.MoveWindowToPoint(window.Object, It.IsAny<Point<double>>()), Times.Never());

		Assert.Equal(targetWorkspace.Object, wrapper.WorkspaceManager.GetWorkspaceForWindow(window.Object));

		window.Verify(w => w.Focus(), Times.Once());
	}

	[Fact]
	public void MoveWindowToPoint_Success_SameWorkspace()
	{
		// Given
		Mock<IWorkspace> activeWorkspace = new();
		Mock<IWorkspace> anotherWorkspace = new();
		Wrapper wrapper = new(new[] { activeWorkspace, anotherWorkspace });

		wrapper.WorkspaceManager.Activate(activeWorkspace.Object, wrapper.Monitors[0].Object);

		Mock<IWindow> window = new();
		wrapper.WorkspaceManager.WindowAdded(window.Object);
		activeWorkspace.Reset();

		wrapper.MonitorManager
			.Setup(m => m.GetMonitorAtPoint(It.IsAny<IPoint<int>>()))
			.Returns(wrapper.Monitors[0].Object);
		activeWorkspace.Setup(w => w.RemoveWindow(window.Object)).Returns(true);

		// When a window is moved to a point
		wrapper.WorkspaceManager.MoveWindowToPoint(window.Object, new Point<int>() { X = 0, Y = 0 });

		// Then the window is removed from the old workspace and added to the new workspace
		activeWorkspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		activeWorkspace.Verify(w => w.MoveWindowToPoint(window.Object, It.IsAny<Point<double>>()), Times.Once());
		anotherWorkspace.Verify(w => w.MoveWindowToPoint(window.Object, It.IsAny<Point<double>>()), Times.Never());

		Assert.Equal(activeWorkspace.Object, wrapper.WorkspaceManager.GetWorkspaceForWindow(window.Object));

		window.Verify(w => w.Focus(), Times.Once());
	}

	[Fact]
	public void WindowFocused()
	{
		// Given
		Wrapper wrapper = new();

		Mock<IWorkspace> workspace = new();
		Mock<IInternalWorkspace> internalWorkspace = workspace.As<IInternalWorkspace>();

		Mock<IWorkspace> workspace2 = new();
		Mock<IInternalWorkspace> internalWorkspace2 = workspace2.As<IInternalWorkspace>();

		wrapper.WorkspaceManager.Add(workspace.Object);
		wrapper.WorkspaceManager.Add(workspace2.Object);

		Mock<IWindow> window = new();

		// When a window is focused
		wrapper.WorkspaceManager.WindowFocused(window.Object);

		// Then the window is focused in all workspaces
		internalWorkspace.Verify(w => w.WindowFocused(window.Object), Times.Once());
		internalWorkspace2.Verify(w => w.WindowFocused(window.Object), Times.Once());
	}

	[Fact]
	public void WindowFocused_ActivateWorkspace()
	{
		// Given
		Mock<IWorkspace>[] workspaces = new[] { new Mock<IWorkspace>(), new Mock<IWorkspace>() };
		Mock<IMonitor> monitor = new();
		Wrapper wrapper = new(workspaces, new Mock<IMonitor>[] { monitor });

		Mock<ILayoutEngine> layoutEngine = new();
		Mock<IWindow> window = new();

		wrapper.WorkspaceManager.Activate(workspaces[0].Object, monitor.Object);
		workspaces[0].Reset();

		// When a window is added to the first workspace, the second workspace is activated, and the window is focused
		wrapper.WorkspaceManager.WindowAdded(window.Object);
		wrapper.WorkspaceManager.Activate(workspaces[1].Object, monitor.Object);
		wrapper.WorkspaceManager.WindowFocused(window.Object);

		// Then the first workspace is activated
		workspaces[0].Verify(w => w.DoLayout(), Times.Once());
	}

	[Fact]
	public void WindowMinimizeStart_CouldNotFindWindow()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Wrapper wrapper = new(new[] { workspace });
		Mock<IWindow> window = new();

		// When a window is minimized, but the window is not found in any workspace
		wrapper.WorkspaceManager.WindowMinimizeStart(window.Object);

		// Then nothing happens
		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
	}

	[Fact]
	public void WindowMinimizeStart()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IInternalWorkspace> internalWorkspace = workspace.As<IInternalWorkspace>();

		Wrapper wrapper = new(new[] { workspace });

		Mock<IRouterManager> routerManager = new();
		routerManager.Setup(r => r.RouteWindow(It.IsAny<IWindow>())).Returns(workspace.Object);
		wrapper.Context.Setup(c => c.RouterManager).Returns(routerManager.Object);

		// A window is added to the workspace
		Mock<IWindow> window = new();
		wrapper.WorkspaceManager.WindowAdded(window.Object);

		// When a window is minimized
		wrapper.WorkspaceManager.WindowMinimizeStart(window.Object);

		// Then the workspace is notified
		internalWorkspace.Verify(w => w.WindowMinimizeStart(window.Object), Times.Once());
	}

	[Fact]
	public void WindowMinimizeEnd()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IInternalWorkspace> internalWorkspace = workspace.As<IInternalWorkspace>();

		Wrapper wrapper = new(new[] { workspace });

		Mock<IRouterManager> routerManager = new();
		routerManager.Setup(r => r.RouteWindow(It.IsAny<IWindow>())).Returns(workspace.Object);
		wrapper.Context.Setup(c => c.RouterManager).Returns(routerManager.Object);

		// A window is added to the workspace
		Mock<IWindow> window = new();
		wrapper.WorkspaceManager.WindowAdded(window.Object);

		// When a window is restored
		wrapper.WorkspaceManager.WindowMinimizeEnd(window.Object);

		// Then the window is routed to the workspace
		internalWorkspace.Verify(w => w.WindowMinimizeEnd(window.Object), Times.Once());
	}

	[Fact]
	public void WindowMinizeEnd_Fail()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IInternalWorkspace> internalWorkspace = workspace.As<IInternalWorkspace>();

		Wrapper wrapper = new(new[] { workspace });

		Mock<IRouterManager> routerManager = new();
		routerManager.Setup(r => r.RouteWindow(It.IsAny<IWindow>())).Returns(workspace.Object);
		wrapper.Context.Setup(c => c.RouterManager).Returns(routerManager.Object);

		Mock<IWindow> window = new();

		// When a window which isn't tracked is restored
		wrapper.WorkspaceManager.WindowMinimizeEnd(window.Object);

		// Then the workspace is not notified
		internalWorkspace.Verify(w => w.WindowMinimizeEnd(window.Object), Times.Never());
	}

	[Fact]
	public void MonitorManager_MonitorsChanged_Removed()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();

		Wrapper wrapper = new(new[] { workspace, workspace2 });

		wrapper.WorkspaceManager.Activate(workspace.Object, wrapper.Monitors[0].Object);
		wrapper.WorkspaceManager.Activate(workspace2.Object, wrapper.Monitors[1].Object);

		// When a monitor is removed, and a monitor not tracked in the WorkspaceManager is removed
		wrapper.WorkspaceManager.MonitorManager_MonitorsChanged(
			this,
			new MonitorsChangedEventArgs()
			{
				AddedMonitors = Array.Empty<IMonitor>(),
				RemovedMonitors = new IMonitor[] { wrapper.Monitors[0].Object, new Mock<IMonitor>().Object },
				UnchangedMonitors = new IMonitor[] { wrapper.Monitors[1].Object }
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
		monitor.Setup(m => m.WorkingArea).Returns(new Location<int>());

		Wrapper wrapper = new(new[] { workspace, workspace2 });

		Mock<Func<IList<ILayoutEngine>>> CreateLayoutEngines = new();
		CreateLayoutEngines.Setup(c => c()).Returns(new ILayoutEngine[] { new Mock<ILayoutEngine>().Object });
		wrapper.WorkspaceManager.CreateLayoutEngines = CreateLayoutEngines.Object;

		wrapper.WorkspaceManager.Activate(workspace.Object, wrapper.Monitors[0].Object);
		wrapper.WorkspaceManager.Activate(workspace2.Object, wrapper.Monitors[1].Object);

		// When a monitor is added
		wrapper.WorkspaceManager.MonitorManager_MonitorsChanged(
			this,
			new MonitorsChangedEventArgs()
			{
				AddedMonitors = new IMonitor[] { monitor.Object },
				RemovedMonitors = Array.Empty<IMonitor>(),
				UnchangedMonitors = new IMonitor[] { wrapper.Monitors[0].Object, wrapper.Monitors[1].Object }
			}
		);

		// Then a new workspace is created
		CreateLayoutEngines.Verify(f => f(), Times.Once());
	}

	[Fact]
	public void MonitorManager_MonitorsChanged_Added_UseExistingWorkspace()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		Mock<IWorkspace> workspace3 = new();

		Mock<IMonitor> monitor = new();

		Wrapper wrapper = new(new[] { workspace, workspace2, workspace3 });

		wrapper.WorkspaceManager.Activate(workspace.Object, wrapper.Monitors[0].Object);
		wrapper.WorkspaceManager.Activate(workspace2.Object, wrapper.Monitors[1].Object);

		// Reset the wrapper
		workspace.Reset();
		workspace2.Reset();
		workspace3.Reset();

		// When a monitor is added
		wrapper.WorkspaceManager.MonitorManager_MonitorsChanged(
			this,
			new MonitorsChangedEventArgs()
			{
				AddedMonitors = new IMonitor[] { monitor.Object },
				RemovedMonitors = Array.Empty<IMonitor>(),
				UnchangedMonitors = new IMonitor[] { wrapper.Monitors[0].Object, wrapper.Monitors[1].Object }
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
		Wrapper wrapper = new();
		Mock<ProxyLayoutEngine> proxyLayoutEngine = new();

		// When a proxy layout engine is added
		wrapper.WorkspaceManager.AddProxyLayoutEngine(proxyLayoutEngine.Object);

		// Then the proxy layout engine is added to the list
		Assert.Contains(proxyLayoutEngine.Object, wrapper.WorkspaceManager.ProxyLayoutEngines);
	}

	[Fact]
	public void AddPhantomWindow()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWindow> window = new();
		Wrapper wrapper = new(new[] { workspace });

		// When a phantom window is added
		wrapper.WorkspaceManager.AddPhantomWindow(workspace.Object, window.Object);

		// Then the phantom window is added to the list
		Assert.Contains(window.Object, wrapper.WorkspaceManager.PhantomWindows);
	}

	[Fact]
	public void RemovePhantomWindow()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWindow> window = new();
		Wrapper wrapper = new(new[] { workspace });

		wrapper.WorkspaceManager.AddPhantomWindow(workspace.Object, window.Object);

		// When a phantom window is removed
		wrapper.WorkspaceManager.RemovePhantomWindow(window.Object);

		// Then the phantom window is removed from the list
		Assert.DoesNotContain(window.Object, wrapper.WorkspaceManager.PhantomWindows);
		Assert.Null(wrapper.WorkspaceManager.GetMonitorForWindow(window.Object));
	}

	[Fact]
	public void DoesDispose()
	{
		// Given
		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		Wrapper wrapper = new(new[] { workspace, workspace2 });

		// When the workspace manager is disposed
		wrapper.WorkspaceManager.Dispose();

		// Then the workspaces are disposed
		workspace.Verify(w => w.Dispose(), Times.Once());
		workspace2.Verify(w => w.Dispose(), Times.Once());
	}

	[Fact]
	public void WorkspaceManagerTriggers_ActiveLayoutEngineChanged()
	{
		// Given
		Wrapper wrapper = new(Array.Empty<Mock<IWorkspace>>());

		// When creating a workspace
		wrapper.WorkspaceManager.Add("workspace");
		IWorkspace workspace = wrapper.WorkspaceManager["workspace"]!;

		// Then changing the layout engine should trigger the event
		Assert.Raises<ActiveLayoutEngineChangedEventArgs>(
			h => wrapper.WorkspaceManager.ActiveLayoutEngineChanged += h,
			h => wrapper.WorkspaceManager.ActiveLayoutEngineChanged -= h,
			workspace.NextLayoutEngine
		);
	}

	[Fact]
	public void WorkspaceManagerTriggers_WorkspaceRenamed()
	{
		// Given
		Wrapper wrapper = new(Array.Empty<Mock<IWorkspace>>());

		// When creating a workspace
		wrapper.WorkspaceManager.Add("workspace");
		IWorkspace workspace = wrapper.WorkspaceManager["workspace"]!;

		// Then renaming the workspace should trigger the event
		Assert.Raises<WorkspaceRenamedEventArgs>(
			h => wrapper.WorkspaceManager.WorkspaceRenamed += h,
			h => wrapper.WorkspaceManager.WorkspaceRenamed -= h,
			() => workspace.Name = "new name"
		);
	}

	[Fact]
	public void WorkspaceManagerTriggers_WorkspaceLayoutStarted()
	{
		// Given
		Wrapper wrapper = new(Array.Empty<Mock<IWorkspace>>());
		Mock<Func<IList<ILayoutEngine>>> CreateLayoutEngines = new();
		CreateLayoutEngines.Setup(c => c()).Returns(new ILayoutEngine[] { new Mock<ILayoutEngine>().Object });
		wrapper.WorkspaceManager.CreateLayoutEngines = CreateLayoutEngines.Object;

		// When creating a workspace
		wrapper.WorkspaceManager.Add("workspace");
		IWorkspace workspace = wrapper.WorkspaceManager["workspace"]!;
		wrapper.WorkspaceManager.Activate(workspace);

		// Then starting the layout should trigger the event
		Assert.Raises<WorkspaceEventArgs>(
			h => wrapper.WorkspaceManager.WorkspaceLayoutStarted += h,
			h => wrapper.WorkspaceManager.WorkspaceLayoutStarted -= h,
			workspace.DoLayout
		);
	}

	[Fact]
	public void WorkspaceManagerTriggers_WorkspaceLayoutCompleted()
	{
		// Given
		Wrapper wrapper = new(Array.Empty<Mock<IWorkspace>>());
		Mock<Func<IList<ILayoutEngine>>> CreateLayoutEngines = new();
		CreateLayoutEngines.Setup(c => c()).Returns(new ILayoutEngine[] { new Mock<ILayoutEngine>().Object });
		wrapper.WorkspaceManager.CreateLayoutEngines = CreateLayoutEngines.Object;

		// When
		wrapper.WorkspaceManager.Add("workspace");
		IWorkspace workspace = wrapper.WorkspaceManager["workspace"]!;
		wrapper.WorkspaceManager.Activate(workspace);

		// Then completing the layout should trigger the event
		Assert.Raises<WorkspaceEventArgs>(
			h => wrapper.WorkspaceManager.WorkspaceLayoutCompleted += h,
			h => wrapper.WorkspaceManager.WorkspaceLayoutCompleted -= h,
			workspace.DoLayout
		);
	}

	[Fact]
	public void ActiveWorkspace_CannotFindMonitor()
	{
		// Given
		Mock<IWorkspace>[] workspaces = new[] { new Mock<IWorkspace>(), new Mock<IWorkspace>() };
		Wrapper wrapper = new(workspaces);

		wrapper.MonitorManager.Setup(m => m.ActiveMonitor).Returns(new Mock<IMonitor>().Object);

		// When the active monitor can't be found inside the WorkspaceManager
		IWorkspace activeWorkspace = wrapper.WorkspaceManager.ActiveWorkspace;

		// Then the first workspace is returned
		Assert.Equal(workspaces[0].Object, activeWorkspace);
	}

	[Fact]
	public void ActiveWorkspace_CanFindMonitor()
	{
		// Given
		Mock<IWorkspace>[] workspaces = new[] { new Mock<IWorkspace>(), new Mock<IWorkspace>() };
		Wrapper wrapper = new(workspaces);

		wrapper.WorkspaceManager.Initialize();
		wrapper.MonitorManager.Setup(m => m.ActiveMonitor).Returns(wrapper.Monitors[1].Object);

		// When the active monitor can be found inside the WorkspaceManager
		IWorkspace activeWorkspace = wrapper.WorkspaceManager.ActiveWorkspace;

		// Then the workspace is returned
		Assert.Equal(workspaces[1].Object, activeWorkspace);
	}
}
