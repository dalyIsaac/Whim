using System;

namespace Whim;

internal class ButlerEventHandlers : IDisposable
{
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;
	private readonly ButlerTriggers _triggers;
	private readonly IButlerPantry _pantry;
	private readonly IButlerChores _chores;
	private bool _disposedValue;

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

	public void PreInitialize()
	{
		_context.WindowManager.WindowAdded += WindowManager_WindowAdded;
		_context.WindowManager.WindowRemoved += WindowManager_WindowRemoved;
		_context.WindowManager.WindowFocused += WindowManager_WindowFocused;
		_context.WindowManager.WindowMinimizeStart += WindowManager_WindowMinimizeStart;
		_context.WindowManager.WindowMinimizeEnd += WindowManager_WindowMinimizeEnd;
		_context.MonitorManager.MonitorsChanged += MonitorManager_MonitorsChanged;
	}

	private void WindowManager_WindowAdded(object? sender, WindowEventArgs args)
	{
		IWindow window = args.Window;
		Logger.Debug($"Adding window {window}");

		IWorkspace? workspace = null;
		if (_context.RouterManager.RouterOptions == RouterOptions.RouteToActiveWorkspace)
		{
			workspace = _context.WorkspaceManager.ActiveWorkspace;
		}
		else if (_context.RouterManager.RouterOptions == RouterOptions.RouteToLastTrackedActiveWorkspace)
		{
			workspace = _internalContext.MonitorManager.LastWhimActiveMonitor is IMonitor lastWhimActiveMonitor
				? _pantry.GetWorkspaceForMonitor(lastWhimActiveMonitor)
				: _context.WorkspaceManager.ActiveWorkspace;
		}

		// RouteWindow takes precedence over RouterOptions.
		if (_context.RouterManager.RouteWindow(window) is IWorkspace routedWorkspace)
		{
			workspace = routedWorkspace;
		}

		// Check the workspace exists. If it doesn't, clear the workspace.
		if (workspace != null && !_context.WorkspaceManager.Contains(workspace))
		{
			Logger.Error($"Workspace {workspace} was not found");
			workspace = null;
		}

		// If no workspace has been selected, route the window to the monitor it's on.
		if (workspace == null)
		{
			IMonitor? monitor = _context.MonitorManager.GetMonitorAtPoint(window.Rectangle.Center);
			if (monitor is not null)
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
		Logger.Debug($"Window {window} added to workspace {workspace.Name}");
	}

	private void WindowManager_WindowRemoved(object? sender, WindowEventArgs args)
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
	}

	private void WindowManager_WindowFocused(object? sender, WindowFocusedEventArgs args)
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

	private void WindowManager_WindowMinimizeStart(object? sender, WindowEventArgs args)
	{
		IWindow window = args.Window;
		Logger.Debug($"Window minimize start: {window}");

		if (_pantry.GetWorkspaceForWindow(window) is not IWorkspace workspace)
		{
			Logger.Error($"Window {window} was not found in any workspace");
			return;
		}

		workspace.MinimizeWindowStart(window);
	}

	private void WindowManager_WindowMinimizeEnd(object? sender, WindowEventArgs args)
	{
		IWindow window = args.Window;
		Logger.Debug($"Window minimize end: {window}");

		if (_pantry.GetWorkspaceForWindow(window) is not IWorkspace workspace)
		{
			Logger.Error($"Window {window} was not found in any workspace");
			return;
		}

		workspace.MinimizeWindowEnd(window);
	}

	private void MonitorManager_MonitorsChanged(object? sender, MonitorsChangedEventArgs e)
	{
		Logger.Debug($"MonitorManager_MonitorsChanged: {e}");

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
					workspace = newWorkspace;
				}
				else
				{
					continue;
				}
			}

			// Add the workspace to the map.
			_chores.Activate(workspace, monitor);
		}

		// For each workspace which is active in a monitor, do a layout.
		// This will handle cases when the monitor's properties have changed.
		_chores.LayoutAllActiveWorkspaces();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_context.WindowManager.WindowAdded -= WindowManager_WindowAdded;
				_context.WindowManager.WindowRemoved -= WindowManager_WindowRemoved;
				_context.WindowManager.WindowFocused -= WindowManager_WindowFocused;
				_context.WindowManager.WindowMinimizeStart -= WindowManager_WindowMinimizeStart;
				_context.WindowManager.WindowMinimizeEnd -= WindowManager_WindowMinimizeEnd;
				_context.MonitorManager.MonitorsChanged -= MonitorManager_MonitorsChanged;
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
