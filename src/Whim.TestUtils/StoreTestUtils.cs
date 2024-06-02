using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Whim.TestUtils;

internal static class StoreTestUtils
{
	public static void SetupWindowWorkspaceMapping(
		IContext ctx,
		MutableRootSector rootSector,
		IWindow window,
		Workspace workspace
	)
	{
		if (workspace.Id == default)
		{
			workspace.Id.Returns(Guid.NewGuid());
		}

		if (window.Handle == default)
		{
			window.Handle.Returns((HWND)2);
		}

		rootSector.MapSector.WindowWorkspaceMap = rootSector.MapSector.WindowWorkspaceMap.SetItem(
			window.Handle,
			workspace.Id
		);

		rootSector.WorkspaceSector.Workspaces = rootSector.WorkspaceSector.Workspaces.Add(workspace.Id, workspace);
		ctx.WorkspaceManager.GetEnumerator().Returns(_ => new List<IWorkspace>() { workspace }.GetEnumerator());
	}

	private static int _workspaceCounter = 1;

	public static Workspace CreateWorkspace(IContext ctx)
	{
		byte[] bytes = new byte[16];
		BitConverter.GetBytes(_workspaceCounter).CopyTo(bytes, 0);
		Guid workspaceId = new(bytes);
		_workspaceCounter++;

		return new Workspace(ctx, workspaceId);
	}

	public static IMonitor CreateMonitor(HMONITOR handle)
	{
		IMonitor monitor = Substitute.For<IMonitor>();
		monitor.Handle.Returns(handle);
		monitor.WorkingArea.X.Returns(0);
		monitor.WorkingArea.Y.Returns(0);
		monitor.WorkingArea.Width.Returns(1920);
		monitor.WorkingArea.Height.Returns(1080);
		return monitor;
	}

	public static IWindow CreateWindow(HWND handle)
	{
		IWindow window = Substitute.For<IWindow>();
		window.Handle.Returns(handle);
		return window;
	}

	public static void AddWorkspacesToManager(IContext ctx, params IWorkspace[] newWorkspaces)
	{
		List<IWorkspace> workspaces = ctx.WorkspaceManager.ToList();
		workspaces.AddRange(newWorkspaces);

		ctx.WorkspaceManager.GetEnumerator().Returns(_ => workspaces.GetEnumerator());
	}

	public static void AddWindowToSector(MutableRootSector rootSector, IWindow window)
	{
		rootSector.WindowSector.Windows = rootSector.WindowSector.Windows.Add(window.Handle, window);
	}

	public static void PopulateWindowWorkspaceMap(
		IContext ctx,
		MutableRootSector rootSector,
		IWindow window,
		IWorkspace workspace
	)
	{
		rootSector.MapSector.WindowWorkspaceMap = rootSector.MapSector.WindowWorkspaceMap.SetItem(
			window.Handle,
			workspace.Id
		);
		AddWindowToSector(rootSector, window);

		AddWorkspacesToManager(ctx, workspace);
	}

	public static void PopulateMonitorWorkspaceMap(
		IContext ctx,
		MutableRootSector rootSector,
		IMonitor monitor,
		IWorkspace workspace
	)
	{
		rootSector.MapSector.MonitorWorkspaceMap = rootSector.MapSector.MonitorWorkspaceMap.SetItem(
			monitor.Handle,
			workspace.Id
		);
		rootSector.MonitorSector.Monitors = rootSector.MonitorSector.Monitors.Add(monitor);

		AddWorkspacesToManager(ctx, workspace);
	}

	public static void PopulateThreeWayMap(
		IContext ctx,
		MutableRootSector rootSector,
		IMonitor monitor,
		IWorkspace workspace,
		IWindow window
	)
	{
		rootSector.MapSector.MonitorWorkspaceMap = rootSector.MapSector.MonitorWorkspaceMap.SetItem(
			monitor.Handle,
			workspace.Id
		);
		rootSector.MapSector.WindowWorkspaceMap = rootSector.MapSector.WindowWorkspaceMap.SetItem(
			window.Handle,
			workspace.Id
		);
		rootSector.WindowSector.Windows = rootSector.WindowSector.Windows.Add(window.Handle, window);

		rootSector.MonitorSector.Monitors = rootSector.MonitorSector.Monitors.Add(monitor);

		AddWorkspacesToManager(ctx, workspace);
	}

	public static void SetupMonitorAtPoint(IContext ctx, IPoint<int> point, IMonitor monitor)
	{
		ctx.MonitorManager.GetMonitorAtPoint(point).Returns(monitor);
	}
}
