using Moq;
using System;
using System.Linq;
using Xunit;

namespace Whim.Tests;

public class WorkspaceManagerTests
{
	public static (Mock<IConfigContext>, Mock<IMonitorManager>, Mock<IMonitor>[], Mock<IRouterManager>) CreateMocks(
		Mock<IMonitor>[]? monitors = null,
		int focusedMonitorIndex = 0
	)
	{
		Mock<IConfigContext> configContext = new();

		Mock<IMonitorManager> monitorManager = new();
		configContext.Setup(c => c.MonitorManager).Returns(monitorManager.Object);

		monitors ??= new[] { new Mock<IMonitor>(), new Mock<IMonitor>() };
		monitorManager.Setup(m => m.Length).Returns(monitors.Length);
		monitorManager.Setup(m => m.GetEnumerator()).Returns(monitors.Select(m => m.Object).GetEnumerator());

		if (monitors.Length > 0)
		{
			monitorManager.Setup(m => m.FocusedMonitor).Returns(monitors[focusedMonitorIndex].Object);
		}

		Mock<IRouterManager> routerManager = new();
		routerManager.Setup(r => r.RouteWindow(It.IsAny<IWindow>())).Returns(null as IWorkspace);
		configContext.Setup(c => c.RouterManager).Returns(routerManager.Object);

		return (configContext, monitorManager, monitors, routerManager);
	}

	[Fact]
	public void Initialize_RequireAtLeastNWorkspace()
	{
		(var configContext, var monitorManager, var monitors, _) = CreateMocks();

		using WorkspaceManager workspaceManager = new(configContext.Object);
		Assert.Throws<InvalidOperationException>(workspaceManager.Initialize);
	}

	[Fact]
	public void Initialize_Success()
	{
		(var configContext, var monitorManager, var monitors, _) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();

		// Given the workspace manager has two workspaces
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			workspace2.Object
		};

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
		(var configContext, var monitorManager, var monitors, _) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		using WorkspaceManager workspaceManager = new(configContext.Object);

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
		(var configContext, _, _, _) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			workspace2.Object
		};

		// When a workspace is removed, it returns false, as there must be at least N workspaces,
		// where N is the number of monitors
		Assert.False(workspaceManager.Remove(workspace.Object));
	}

	[Fact]
	public void Remove_Workspace_NotFound()
	{
		(var configContext, _, _, _) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			workspace2.Object
		};

		// When a workspace is removed, it returns false, as the workspace was not found
		Assert.False(workspaceManager.Remove(new Mock<IWorkspace>().Object));
	}

	[Fact]
	public void Remove_Workspace_Success()
	{
		Mock<IMonitor>[] monitorMocks = new[] { new Mock<IMonitor>() };
		(var configContext, _, _, _) = CreateMocks(monitorMocks);

		Mock<IWorkspace> workspace = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			new Mock<IWorkspace>().Object,
		};

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
		(var configContext, _, _, _) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			workspace2.Object
		};

		// When a workspace is removed, it returns false, as the workspace was not found
		Assert.False(workspaceManager.Remove("not found"));
	}

	[Fact]
	public void Remove_String_Success()
	{
		Mock<IMonitor>[] monitorMocks = new[] { new Mock<IMonitor>() };
		(var configContext, _, _, _) = CreateMocks(monitorMocks);

		Mock<IWorkspace> workspace = new();
		workspace.Setup(w => w.Name).Returns("workspace");
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			new Mock<IWorkspace>().Object,
		};

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
		(var configContext, _, _, _) = CreateMocks();

		using WorkspaceManager workspaceManager = new(configContext.Object);
		Assert.Null(workspaceManager.TryGet("not found"));
	}

	[Fact]
	public void TryGet_Success()
	{
		(var configContext, _, _, _) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		workspace.Setup(w => w.Name).Returns("workspace");
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object
		};

		Assert.Equal(workspace.Object, workspaceManager.TryGet("workspace"));
	}

	[Fact]
	public void Activate_NoOldWorkspace()
	{
		Mock<IMonitor>[] monitorMocks = new[] { new Mock<IMonitor>() };
		(var configContext, _, _, _) = CreateMocks(monitorMocks);

		Mock<IWorkspace> workspace = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object
		};

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
		(var configContext, _, _, _) = CreateMocks(monitorMocks);

		Mock<IWorkspace> workspace0 = new();
		Mock<IWorkspace> workspace1 = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace0.Object,
			workspace1.Object
		};

		workspaceManager.Activate(workspace0.Object);

		// When a workspace is activated, it is focused on the focused monitor
		var result = Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => workspaceManager.MonitorWorkspaceChanged += h,
			h => workspaceManager.MonitorWorkspaceChanged -= h,
			() => workspaceManager.Activate(workspace1.Object)
		);
		Assert.Equal(workspace1.Object, result.Arguments.NewWorkspace);
		Assert.Equal(workspace0.Object, result.Arguments.OldWorkspace);

		workspace0.Verify(w => w.Deactivate(), Times.Once);
		workspace1.Verify(w => w.DoLayout(), Times.Once);
		workspace1.Verify(w => w.FocusFirstWindow(), Times.Once);
	}

	[Fact]
	public void GetMonitorForWorkspace_NoWorkspace()
	{
		Mock<IMonitor>[] monitorMocks = new[] { new Mock<IMonitor>() };
		(var configContext, _, _, _) = CreateMocks(monitorMocks);

		Mock<IWorkspace> workspace = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
		};
		workspaceManager.Activate(workspace.Object);

		// Get the monitor for a workspace which isn't in the workspace manager
		Assert.Null(workspaceManager.GetMonitorForWorkspace(new Mock<IWorkspace>().Object));
	}

	[Fact]
	public void GetMonitorForWorkspace_Success()
	{
		(var configContext, _, var monitors, _) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			workspace2.Object
		};
		workspaceManager.Activate(workspace.Object);
		workspaceManager.Activate(workspace2.Object, monitors[1].Object);

		// Get the monitor for a workspace which is in the workspace manager
		Assert.Equal(monitors[0].Object, workspaceManager.GetMonitorForWorkspace(workspace.Object));
	}

	[Fact]
	public void LayoutAllActiveWorkspaces()
	{
		(var configContext, _, var monitors, _) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			workspace2.Object
		};
		workspaceManager.Activate(workspace.Object, monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, monitors[1].Object);
		workspace.Reset();
		workspace2.Reset();

		workspaceManager.LayoutAllActiveWorkspaces();

		workspace.Verify(w => w.DoLayout(), Times.Once());
		workspace2.Verify(w => w.DoLayout(), Times.Once());
	}

	[Fact]
	public void WindowAdded_NoRouter()
	{
		(var configContext, _, var monitors, _) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			workspace2.Object
		};
		workspaceManager.Activate(workspace.Object, monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, monitors[1].Object);
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
		(var configContext, _, var monitors, var router) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			workspace2.Object
		};
		workspaceManager.Activate(workspace.Object, monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, monitors[1].Object);
		workspace.Reset();
		workspace2.Reset();

		Mock<IWindow> window = new();
		router.Setup(r => r.RouteWindow(window.Object)).Returns(workspace2.Object);

		workspaceManager.WindowAdded(window.Object);

		workspace.Verify(w => w.AddWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Once());
	}

	[Fact]
	public void WindowAdded_RouterToActive()
	{
		(var configContext, _, var monitors, var router) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			workspace2.Object
		};
		workspaceManager.Activate(workspace.Object, monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, monitors[1].Object);
		workspace.Reset();
		workspace2.Reset();

		Mock<IWindow> window = new();
		router.Setup(r => r.RouteWindow(window.Object)).Returns<IWorkspace?>(null);
		router.Setup(r => r.RouteToActiveWorkspace).Returns(true);

		workspaceManager.WindowAdded(window.Object);

		workspace.Verify(w => w.AddWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void WindowRemoved_NotFound()
	{
		(var configContext, _, var monitors, _) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			workspace2.Object
		};
		workspaceManager.Activate(workspace.Object, monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, monitors[1].Object);
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
		(var configContext, _, var monitors, _) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			workspace2.Object
		};
		workspaceManager.Activate(workspace.Object, monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, monitors[1].Object);

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
		(var configContext, _, var monitors, _) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			workspace2.Object
		};
		workspaceManager.Activate(workspace.Object, monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, monitors[1].Object);
		workspace.Reset();
		workspace2.Reset();

		workspaceManager.MoveWindowToWorkspace(workspace.Object);

		workspace.Verify(w => w.RemoveWindow(It.IsAny<IWindow>()), Times.Never());
		workspace2.Verify(w => w.AddWindow(It.IsAny<IWindow>()), Times.Never());
	}

	[Fact]
	public void MoveWindowToWorkspace_PhantomWindow()
	{
		(var configContext, _, var monitors, _) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			workspace2.Object
		};
		workspaceManager.Activate(workspace.Object, monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, monitors[1].Object);

		Mock<IWindow> window = new();
		workspaceManager.AddPhantomWindow(workspace.Object, window.Object);
		workspace.Reset();
		workspace2.Reset();

		workspaceManager.MoveWindowToWorkspace(workspace.Object, window.Object);

		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void MoveWindowToWorkspace_AlreadyInWorkspace()
	{
		(var configContext, _, var monitors, _) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			workspace2.Object
		};
		workspaceManager.Activate(workspace.Object, monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, monitors[1].Object);

		Mock<IWindow> window = new();
		workspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();

		workspaceManager.MoveWindowToWorkspace(workspace.Object, window.Object);

		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void MoveWindowToWorkspace_Success()
	{
		(var configContext, _, var monitors, _) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			workspace2.Object
		};
		workspaceManager.Activate(workspace.Object, monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, monitors[1].Object);

		Mock<IWindow> window = new();
		workspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();

		workspaceManager.MoveWindowToWorkspace(workspace2.Object, window.Object);

		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Once());

		// Verify that the window was hidden.
		window.Verify(w => w.Hide(), Times.Once());

		// Verify that DoLayout was called.
		workspace2.Verify(w => w.DoLayout(), Times.Once());
	}

	[Fact]
	public void MoveWindowToMonitor_NoWindow()
	{
		(var configContext, _, var monitors, _) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			workspace2.Object
		};
		workspaceManager.Activate(workspace.Object, monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, monitors[1].Object);
		workspace.Reset();
		workspace2.Reset();

		workspaceManager.MoveWindowToMonitor(monitors[0].Object);

		workspace.Verify(w => w.RemoveWindow(It.IsAny<IWindow>()), Times.Never());
		workspace2.Verify(w => w.AddWindow(It.IsAny<IWindow>()), Times.Never());
	}

	[Fact]
	public void MoveWindowToMonitor_NoOldMonitor()
	{
		(var configContext, _, var monitors, _) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			workspace2.Object
		};
		workspaceManager.Activate(workspace.Object, monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, monitors[1].Object);
		workspace.Reset();
		workspace2.Reset();

		Mock<IWindow> window = new();
		workspaceManager.MoveWindowToMonitor(monitors[0].Object, window.Object);

		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void MoveWindowToMonitor_OldMonitorIsNewMonitor()
	{
		(var configContext, _, var monitors, _) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			workspace2.Object
		};
		workspaceManager.Activate(workspace.Object, monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, monitors[1].Object);

		Mock<IWindow> window = new();
		workspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();

		workspaceManager.MoveWindowToMonitor(monitors[0].Object, window.Object);

		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Never());
	}

	[Fact]
	public void MoveWindowToMonitor_WorkspaceForMonitorNotFound()
	{
		(var configContext, _, var monitors, _) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			workspace2.Object
		};
		workspaceManager.Activate(workspace.Object, monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, monitors[1].Object);

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
		(var configContext, _, var monitors, _) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			workspace2.Object
		};
		workspaceManager.Activate(workspace.Object, monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, monitors[1].Object);

		Mock<IWindow> window = new();
		workspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();

		workspaceManager.MoveWindowToMonitor(monitors[1].Object, window.Object);

		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace2.Verify(w => w.AddWindow(window.Object), Times.Once());
	}

	[Fact]
	public void MoveWindowToPreviousMonitor_Success()
	{
		(var configContext, var monitorManager, var monitors, _) = CreateMocks();

		monitorManager.Setup(m => m.GetPreviousMonitor(monitors[0].Object)).Returns(monitors[1].Object);

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			workspace2.Object
		};
		workspaceManager.Activate(workspace.Object, monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, monitors[1].Object);

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
		(var configContext, var monitorManager, var monitors, _) = CreateMocks();

		monitorManager.Setup(m => m.GetNextMonitor(monitors[0].Object)).Returns(monitors[1].Object);

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			workspace2.Object
		};
		workspaceManager.Activate(workspace.Object, monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, monitors[1].Object);

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
		(var configContext, var monitorManager, var monitors, _) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object
		};
		workspaceManager.Activate(workspace.Object, monitors[0].Object);

		Mock<IWindow> window = new();
		workspaceManager.WindowAdded(window.Object);
		workspace.Reset();

		monitorManager.Setup(m => m.GetMonitorAtPoint(It.IsAny<IPoint<int>>())).Returns(new Mock<IMonitor>().Object);

		workspaceManager.MoveWindowToPoint(window.Object, new Point<int>(0, 0));

		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace.Verify(w => w.MoveWindowToPoint(window.Object, It.IsAny<Point<double>>(), false), Times.Never());

		workspace.Verify(w => w.DoLayout(), Times.Never());
	}

	[Fact]
	public void MoveWindowToPoint_PhantomWindow()
	{
		(var configContext, var monitorManager, var monitors, _) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			workspace2.Object
		};
		workspaceManager.Activate(workspace.Object, monitors[0].Object);

		Mock<IWindow> window = new();
		workspaceManager.AddPhantomWindow(workspace.Object, window.Object);
		workspace.Reset();
		workspace2.Reset();

		monitorManager.Setup(m => m.GetMonitorAtPoint(It.IsAny<IPoint<int>>())).Returns(monitors[1].Object);

		workspaceManager.MoveWindowToPoint(window.Object, new Point<int>(0, 0));

		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace.Verify(w => w.MoveWindowToPoint(window.Object, It.IsAny<Point<double>>(), false), Times.Never());

		workspace.Verify(w => w.DoLayout(), Times.Never());
		workspace2.Verify(w => w.DoLayout(), Times.Never());
	}

	[Fact]
	public void MoveWindowToPoint_CannotRemoveWindow()
	{
		(var configContext, var monitorManager, var monitors, _) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			workspace2.Object
		};
		workspaceManager.Activate(workspace.Object, monitors[0].Object);

		Mock<IWindow> window = new();
		workspaceManager.WindowAdded(window.Object);
		workspace.Reset();

		monitorManager.Setup(m => m.GetMonitorAtPoint(It.IsAny<IPoint<int>>())).Returns(monitors[1].Object);
		workspace.Setup(w => w.RemoveWindow(window.Object)).Returns(false);

		workspaceManager.MoveWindowToPoint(window.Object, new Point<int>(0, 0));

		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Never());
		workspace.Verify(w => w.MoveWindowToPoint(window.Object, It.IsAny<Point<double>>(), false), Times.Never());

		workspace.Verify(w => w.DoLayout(), Times.Never());
		workspace2.Verify(w => w.DoLayout(), Times.Never());
	}

	[Fact]
	public void MoveWindowToPoint_Success()
	{
		(var configContext, var monitorManager, var monitors, _) = CreateMocks();

		Mock<IWorkspace> workspace = new();
		Mock<IWorkspace> workspace2 = new();
		using WorkspaceManager workspaceManager = new(configContext.Object)
		{
			workspace.Object,
			workspace2.Object
		};
		workspaceManager.Activate(workspace.Object, monitors[0].Object);
		workspaceManager.Activate(workspace2.Object, monitors[1].Object);

		Mock<IWindow> window = new();
		workspaceManager.WindowAdded(window.Object);
		workspace.Reset();
		workspace2.Reset();

		monitorManager.Setup(m => m.GetMonitorAtPoint(It.IsAny<IPoint<int>>())).Returns(monitors[1].Object);
		workspace.Setup(w => w.RemoveWindow(window.Object)).Returns(true);

		workspaceManager.MoveWindowToPoint(window.Object, new Point<int>(0, 0));

		workspace.Verify(w => w.RemoveWindow(window.Object), Times.Once());
		workspace.Verify(w => w.MoveWindowToPoint(window.Object, It.IsAny<Point<double>>(), false), Times.Never());

		workspace.Verify(w => w.DoLayout(), Times.Once());
		workspace2.Verify(w => w.DoLayout(), Times.Once());

		window.Verify(w => w.Focus(), Times.Once());
	}
}



