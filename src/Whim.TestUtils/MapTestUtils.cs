using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Whim.TestUtils;

internal static class MapTestUtils
{
	public static void SetupWindowWorkspaceMapping(
		IContext ctx,
		MutableRootSector rootSector,
		IWindow window,
		IWorkspace workspace
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
		ctx.WorkspaceManager.GetEnumerator().Returns(_ => new List<IWorkspace>() { workspace }.GetEnumerator());
	}

	public static IWorkspace CreateWorkspace()
	{
		IWorkspace workspace = Substitute.For<IWorkspace, IInternalWorkspace>();
		workspace.Id.Returns(Guid.NewGuid());
		return workspace;
	}

	public static IMonitor CreateMonitor(HMONITOR handle)
	{
		IMonitor monitor = Substitute.For<IMonitor>();
		monitor.Handle.Returns(handle);
		return monitor;
	}

	public static IWindow CreateWindow(HWND handle)
	{
		IWindow window = Substitute.For<IWindow>();
		window.Handle.Returns(handle);
		return window;
	}

	public static void AddWorkspaceToManager(IContext ctx, IWorkspace workspace)
	{
		List<IWorkspace> workspaces = ctx.WorkspaceManager.ToList();
		workspaces.Add(workspace);

		ctx.WorkspaceManager.GetEnumerator().Returns(_ => workspaces.GetEnumerator());
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
		rootSector.WindowSector.Windows = rootSector.WindowSector.Windows.Add(window.Handle, window);

		AddWorkspaceToManager(ctx, workspace);
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

		AddWorkspaceToManager(ctx, workspace);
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

		AddWorkspaceToManager(ctx, workspace);
	}
}
