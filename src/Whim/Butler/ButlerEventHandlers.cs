using System.Threading.Tasks;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

internal class ButlerEventHandlers : IButlerEventHandlers
{
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;
	private readonly ButlerTriggers _triggers;
	private readonly IButlerPantry _pantry;
	private readonly IButlerChores _chores;

	private int _monitorsChangingTasks;
	public bool AreMonitorsChanging => _monitorsChangingTasks > 0;

	public ButlerEventHandlers(
		IContext context,
		IInternalContext internalContext,
		ButlerTriggers triggers,
		IButlerPantry pantry,
		IButlerChores chores
	)
	{
		_context = context;
		_internalContext = internalContext;
		_triggers = triggers;
		_pantry = pantry;
		_chores = chores;
	}

	public void OnWindowAdded(WindowEventArgs args)
	{
		IWindow window = args.Window;
		Logger.Debug($"Adding window {window}");

		IWorkspace? workspace = null;

		// RouteWindow takes precedence over RouterOptions.
		if (_context.RouterManager.RouteWindow(window) is IWorkspace routedWorkspace)
		{
			workspace = routedWorkspace;
		}
		else if (_context.RouterManager.RouterOptions == RouterOptions.RouteToActiveWorkspace)
		{
			workspace = _context.WorkspaceManager.ActiveWorkspace;
		}
		else if (_context.RouterManager.RouterOptions == RouterOptions.RouteToLastTrackedActiveWorkspace)
		{
			workspace = _internalContext.MonitorManager.LastWhimActiveMonitor is IMonitor lastWhimActiveMonitor
				? _pantry.GetWorkspaceForMonitor(lastWhimActiveMonitor)
				: _context.WorkspaceManager.ActiveWorkspace;
		}

		// Check the workspace exists. If it doesn't, clear the workspace.
		if (workspace != null && !_context.WorkspaceManager.Contains(workspace))
		{
			Logger.Error($"Workspace {workspace} was not found");
			workspace = null;
		}

		// If the workspace is still null, try to find a workspace for the window's monitor.
		if (workspace == null)
		{
			HMONITOR hmonitor = _internalContext.CoreNativeManager.MonitorFromWindow(
				window.Handle,
				MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST
			);

			if (_internalContext.MonitorManager.GetMonitorByHandle(hmonitor) is IMonitor monitor)
			{
				workspace = _pantry.GetWorkspaceForMonitor(monitor);
			}
		}

		// If that fails too, route the window to the active workspace.
		workspace ??= _context.WorkspaceManager.ActiveWorkspace;

		_pantry.SetWindowWorkspace(window, workspace);

		if (window.IsMinimized)
		{
			workspace.MinimizeWindowStart(window);
		}
		else
		{
			workspace.AddWindow(window);
		}

		_triggers.WindowRouted(RouteEventArgs.WindowAdded(window, workspace));

		workspace.DoLayout();
		window.Focus();
		Logger.Debug($"Window {window} added to workspace {workspace.Name}");
	}

	public void OnWindowRemoved(WindowEventArgs args)
	{
		IWindow window = args.Window;
		Logger.Debug($"Window removed: {window}");

		if (_pantry.GetWorkspaceForWindow(window) is not IWorkspace workspace)
		{
			Logger.Error($"Window {window} was not found in any workspace");
			return;
		}

		_pantry.RemoveWindow(window);
		workspace.RemoveWindow(window);

		_triggers.WindowRouted(RouteEventArgs.WindowRemoved(window, workspace));
		workspace.DoLayout();
	}

	public void OnWindowFocused(WindowFocusedEventArgs args)
	{
		IWindow? window = args.Window;
		Logger.Debug($"Window focused: {window}");

		foreach (IWorkspace workspace in _context.WorkspaceManager)
		{
			((IInternalWorkspace)workspace).WindowFocused(window);
		}

		if (window is null)
		{
			return;
		}

		if (_pantry.GetWorkspaceForWindow(window) is not IWorkspace workspaceForWindow)
		{
			Logger.Debug($"Window {window} was not found in any workspace");
			return;
		}

		if (_pantry.GetMonitorForWorkspace(workspaceForWindow) is null)
		{
			Logger.Debug($"Window {window} is not in an active workspace");
			_chores.Activate(workspaceForWindow);
			return;
		}
	}

	public void OnWindowMinimizeStart(WindowEventArgs args)
	{
		IWindow window = args.Window;
		Logger.Debug($"Window minimize start: {window}");

		if (_pantry.GetWorkspaceForWindow(window) is not IWorkspace workspace)
		{
			Logger.Error($"Window {window} was not found in any workspace");
			return;
		}

		workspace.MinimizeWindowStart(window);
		workspace.DoLayout();
	}

	public void OnWindowMinimizeEnd(WindowEventArgs args)
	{
		IWindow window = args.Window;
		Logger.Debug($"Window minimize end: {window}");

		if (_pantry.GetWorkspaceForWindow(window) is not IWorkspace workspace)
		{
			Logger.Error($"Window {window} was not found in any workspace");
			return;
		}

		workspace.MinimizeWindowEnd(window);
		workspace.DoLayout();
	}

	public void OnMonitorsChanged(MonitorsChangedEventArgs e)
	{
		Logger.Debug($"Monitors changed: {e}");

		_monitorsChangingTasks++;

		// Deactivate all workspaces.
		foreach (IWorkspace visibleWorkspace in _pantry.GetAllActiveWorkspaces())
		{
			visibleWorkspace.Deactivate();
		}

		// If a monitor was removed, remove the workspace from the map.
		foreach (IMonitor monitor in e.RemovedMonitors)
		{
			IWorkspace? workspace = _pantry.GetWorkspaceForMonitor(monitor);
			_pantry.RemoveMonitor(monitor);

			if (workspace is null)
			{
				Logger.Error($"Could not find workspace for monitor {monitor}");
				continue;
			}

			workspace.Deactivate();
		}

		// If a monitor was added, set it to an inactive workspace.
		foreach (IMonitor monitor in e.AddedMonitors)
		{
			// Try find a workspace which doesn't have a monitor.
			IWorkspace? workspace = null;
			foreach (IWorkspace w in _context.WorkspaceManager)
			{
				if (_pantry.GetMonitorForWorkspace(w) is null)
				{
					workspace = w;
					_pantry.SetMonitorWorkspace(monitor, w);
					break;
				}
			}

			// If there's no workspace, create one.
			if (workspace is null)
			{
				if (_context.WorkspaceManager.Add() is IWorkspace newWorkspace)
				{
					_pantry.SetMonitorWorkspace(monitor, newWorkspace);
				}
				else
				{
					continue;
				}
			}
		}

		// Hack to only accept window events after Windows has been given a chance to stop moving
		// windows around after a monitor change.
		_context.NativeManager.TryEnqueue(async () =>
		{
			await Task.Delay(3 * 1000).ConfigureAwait(true);

			_monitorsChangingTasks--;
			if (_monitorsChangingTasks > 0)
			{
				Logger.Debug("Monitors changed: More tasks are pending");
				return;
			}

			Logger.Debug("Cleared AreMonitorsChanging");

			// For each workspace which is active in a monitor, do a layout.
			// This will handle cases when the monitor's properties have changed.
			_chores.LayoutAllActiveWorkspaces();
		});
	}
}
