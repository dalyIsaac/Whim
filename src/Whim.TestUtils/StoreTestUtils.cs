using System;
using System.Collections.Generic;
using NSubstitute;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Whim.TestUtils;

internal static class StoreTestUtils
{
	private static int _workspaceCounter = 1;
	private static int _monitorCounter = 1;

	public static Workspace CreateWorkspace(IContext ctx, Guid? providedId = null)
	{
		Guid workspaceId;

		if (providedId is null)
		{
			byte[] bytes = new byte[16];
			BitConverter.GetBytes(_workspaceCounter).CopyTo(bytes, 0);
			workspaceId = new(bytes);
			_workspaceCounter++;
		}
		else
		{
			workspaceId = providedId.Value;
		}

		ILayoutEngine engine = Substitute.For<ITestLayoutEngine>();

		return new Workspace(ctx, workspaceId) { LayoutEngines = [engine], ActiveLayoutEngineIndex = 0 };
	}

	public static IMonitor CreateMonitor(HMONITOR? handle = null)
	{
		IMonitor monitor = Substitute.For<IMonitor>();

		if (handle is null)
		{
			handle = new HMONITOR(_monitorCounter);
			_monitorCounter++;
		}

		monitor.Handle.Returns(handle.Value);
		monitor.WorkingArea.Returns(new Rectangle<int>(0, 0, 1920, 1080));
		monitor.ScaleFactor.Returns(100);
		return monitor;
	}

	public static IWindow CreateWindow(HWND handle)
	{
		IWindow window = Substitute.For<IWindow>();
		window.Handle.Returns(handle);
		return window;
	}

	public static void AddWorkspaceToManager(IContext ctx, MutableRootSector rootSector, Workspace workspace)
	{
		WorkspaceSector workspaceSector = rootSector.WorkspaceSector;

		if (workspaceSector.WorkspaceOrder.Contains(workspace.Id))
		{
			return;
		}

		List<IWorkspace> workspaces = [.. ctx.WorkspaceManager];
		workspaces.Add(workspace);
		ctx.WorkspaceManager.GetEnumerator().Returns(_ => workspaces.GetEnumerator());

		workspaceSector.Workspaces = workspaceSector.Workspaces.Add(workspace.Id, workspace);
		workspaceSector.WorkspaceOrder = workspaceSector.WorkspaceOrder.Add(workspace.Id);
	}

	public static void AddWorkspacesToManager(
		IContext ctx,
		MutableRootSector rootSector,
		params Workspace[] newWorkspaces
	)
	{
		foreach (Workspace workspace in newWorkspaces)
		{
			if (!rootSector.WorkspaceSector.Workspaces.ContainsKey(workspace.Id))
			{
				AddWorkspaceToManager(ctx, rootSector, workspace);
			}
		}
	}

	/// <summary>
	/// Adds the given workspace, sets it to the active workspace using an existing monitor.
	/// If an existing monitor doesn't exist, it creates one.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="root"></param>
	/// <param name="workspace"></param>
	public static void AddActiveWorkspace(IContext ctx, MutableRootSector root, Workspace workspace)
	{
		AddWorkspaceToManager(ctx, root, workspace);
		if (root.MonitorSector.Monitors.Length == 0)
		{
			PopulateMonitorWorkspaceMap(ctx, root, CreateMonitor(), workspace);
		}
		else
		{
			root.MapSector.MonitorWorkspaceMap.SetItem(root.MonitorSector.ActiveMonitorHandle, workspace.Id);
		}
	}

	public static void AddMonitorsToManager(IContext ctx, MutableRootSector rootSector, params IMonitor[] newMonitors)
	{
		List<IMonitor> monitors = [.. rootSector.MonitorSector.Monitors];
		monitors.AddRange(newMonitors);

		ctx.MonitorManager.GetEnumerator().Returns(_ => monitors.GetEnumerator());
		rootSector.MonitorSector.Monitors = [.. newMonitors];
	}

	public static void AddWindowToSector(MutableRootSector rootSector, IWindow window)
	{
		rootSector.WindowSector.Windows = rootSector.WindowSector.Windows.SetItem(window.Handle, window);
	}

	public static Workspace PopulateWindowWorkspaceMap(
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
		AddWorkspaceToManager(ctx, rootSector, workspace);

		if (!workspace.WindowPositions.ContainsKey(window.Handle))
		{
			workspace = workspace with
			{
				WindowPositions = workspace.WindowPositions.SetItem(window.Handle, new WindowPosition()),
			};
		}

		rootSector.WorkspaceSector.Workspaces = rootSector.WorkspaceSector.Workspaces.SetItem(workspace.Id, workspace);

		return workspace;
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

		AddWorkspaceToManager(ctx, rootSector, workspace);
	}

	public static Workspace PopulateThreeWayMap(
		IContext ctx,
		MutableRootSector rootSector,
		IMonitor monitor,
		Workspace workspace,
		IWindow window
	)
	{
		workspace = PopulateWindowWorkspaceMap(ctx, rootSector, window, workspace);
		PopulateMonitorWorkspaceMap(ctx, rootSector, monitor, workspace);
		AddWorkspaceToManager(ctx, rootSector, workspace);
		return workspace;
	}

	internal static void SetupMonitorAtPoint(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		IPoint<int> point,
		IMonitor monitor
	)
	{
		// We need to compare the coordinates by value, so we can't pass in the `point` directly.
		internalCtx
			.CoreNativeManager.MonitorFromPoint(
				Arg.Any<System.Drawing.Point>(),
				MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST
			)
			.Returns(callInfo =>
			{
				System.Drawing.Point calledPoint = callInfo.Arg<System.Drawing.Point>();
				if (calledPoint.X == point.X && calledPoint.Y == point.Y)
				{
					return monitor.Handle;
				}

				return default;
			});

		AddMonitorsToManager(ctx, rootSector, monitor);
	}
}
