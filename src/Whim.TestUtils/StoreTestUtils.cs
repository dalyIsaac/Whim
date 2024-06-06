using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NSubstitute;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Whim.TestUtils;

internal static class StoreTestUtils
{
	private static int _workspaceCounter = 1;

	public static Workspace CreateWorkspace(IContext ctx)
	{
		byte[] bytes = new byte[16];
		BitConverter.GetBytes(_workspaceCounter).CopyTo(bytes, 0);
		Guid workspaceId = new(bytes);
		_workspaceCounter++;

		ILayoutEngine engine = Substitute.For<ITestLayoutEngine>();

		return new Workspace(ctx, workspaceId)
		{
			LayoutEngines = ImmutableList.Create(engine),
			ActiveLayoutEngineIndex = 0
		};
	}

	public static IMonitor CreateMonitor(HMONITOR handle)
	{
		IMonitor monitor = Substitute.For<IMonitor>();
		monitor.Handle.Returns(handle);
		monitor.WorkingArea.X.Returns(0);
		monitor.WorkingArea.Y.Returns(0);
		monitor.WorkingArea.Width.Returns(1920);
		monitor.WorkingArea.Height.Returns(1080);
		monitor.ScaleFactor.Returns(100);
		return monitor;
	}

	public static IWindow CreateWindow(HWND handle)
	{
		IWindow window = Substitute.For<IWindow>();
		window.Handle.Returns(handle);
		return window;
	}

	public static void AddWorkspacesToManager(
		IContext ctx,
		MutableRootSector rootSector,
		params Workspace[] newWorkspaces
	)
	{
		List<IWorkspace> workspaces = ctx.WorkspaceManager.ToList();
		workspaces.AddRange(newWorkspaces);

		ctx.WorkspaceManager.GetEnumerator().Returns(_ => workspaces.GetEnumerator());

		foreach (Workspace w in newWorkspaces)
		{
			rootSector.WorkspaceSector.Workspaces = rootSector.WorkspaceSector.Workspaces.Add(w.Id, w);
			rootSector.WorkspaceSector.WorkspaceOrder = rootSector.WorkspaceSector.WorkspaceOrder.Add(w.Id);
		}
	}

	public static void AddMonitorsToManager(IContext ctx, MutableRootSector rootSector, params IMonitor[] newMonitors)
	{
		List<IMonitor> monitors = ctx.MonitorManager.ToList();
		monitors.AddRange(newMonitors);

		ctx.MonitorManager.GetEnumerator().Returns(_ => monitors.GetEnumerator());
		rootSector.MonitorSector.Monitors = newMonitors.ToImmutableArray();
	}

	public static void AddWindowToSector(MutableRootSector rootSector, IWindow window)
	{
		rootSector.WindowSector.Windows = rootSector.WindowSector.Windows.Add(window.Handle, window);
	}

	public static void PopulateWindowWorkspaceMap(
		IContext ctx,
		MutableRootSector rootSector,
		IWindow window,
		Workspace workspace
	)
	{
		rootSector.MapSector.WindowWorkspaceMap = rootSector.MapSector.WindowWorkspaceMap.SetItem(
			window.Handle,
			workspace.Id
		);
		AddWindowToSector(rootSector, window);

		AddWorkspacesToManager(ctx, rootSector, workspace);
	}

	public static void PopulateMonitorWorkspaceMap(
		IContext ctx,
		MutableRootSector rootSector,
		IMonitor monitor,
		Workspace workspace
	)
	{
		rootSector.MapSector.MonitorWorkspaceMap = rootSector.MapSector.MonitorWorkspaceMap.SetItem(
			monitor.Handle,
			workspace.Id
		);
		rootSector.MonitorSector.Monitors = rootSector.MonitorSector.Monitors.Add(monitor);

		if (rootSector.MonitorSector.Monitors.Length == 1)
		{
			rootSector.MonitorSector.ActiveMonitorHandle = monitor.Handle;
		}

		AddWorkspacesToManager(ctx, rootSector, workspace);
	}

	public static void PopulateThreeWayMap(
		IContext ctx,
		MutableRootSector rootSector,
		IMonitor monitor,
		Workspace workspace,
		IWindow window
	)
	{
		PopulateWindowWorkspaceMap(ctx, rootSector, window, workspace);
		PopulateMonitorWorkspaceMap(ctx, rootSector, monitor, workspace);
		AddWorkspacesToManager(ctx, rootSector, workspace);
	}

	// TODO: Remove
	public static void SetupMonitorAtPoint(IContext ctx, IPoint<int> point, IMonitor monitor)
	{
		ctx.MonitorManager.GetMonitorAtPoint(point).Returns(monitor);
	}
}
