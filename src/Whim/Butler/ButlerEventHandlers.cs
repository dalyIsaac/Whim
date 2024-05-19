using Windows.Win32.Graphics.Gdi;

namespace Whim;

internal class ButlerEventHandlers : IButlerEventHandlers
{
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;
	private readonly IButlerPantry _pantry;
	private readonly IButlerChores _chores;

	private int _monitorsChangingTasks;
	public bool AreMonitorsChanging => _monitorsChangingTasks > 0;

	public int MonitorsChangedDelay { init; get; } = 3 * 1000;

	public ButlerEventHandlers(
		IContext context,
		IInternalContext internalContext,
		IButlerPantry pantry,
		IButlerChores chores
	)
	{
		_context = context;
		_internalContext = internalContext;
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

		_internalContext.Butler.TriggerWindowRouted(RouteEventArgs.WindowAdded(window, workspace));

		workspace.DoLayout();
		window.Focus();
		Logger.Debug($"Window {window} added to workspace {workspace.Name}");
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
}
