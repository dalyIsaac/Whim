using System;
using System.Collections.Generic;
using Windows.Win32.Foundation;

namespace Whim;

internal partial class Butler : IButler
{
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;

	private bool _initialized;

	public Butler(IContext context, IInternalContext internalContext)
	{
		_context = context;
		_internalContext = internalContext;
	}

	public event EventHandler<RouteEventArgs>? WindowRouted;

	public event EventHandler<MonitorWorkspaceChangedEventArgs>? MonitorWorkspaceChanged;

	public void TriggerWindowRouted(RouteEventArgs args) => WindowRouted?.Invoke(this, args);

	public void TriggerMonitorWorkspaceChanged(MonitorWorkspaceChangedEventArgs args) =>
		MonitorWorkspaceChanged?.Invoke(this, args);

	public void Initialize()
	{
		_initialized = true;

		// Add the saved windows at their saved locations inside their saved workspaces.
		// Other windows are routed to the monitor they're on.
		List<HWND> processedWindows = new();

		// Route windows to their saved workspaces.
		foreach (
			SavedWorkspace savedWorkspace in _internalContext.CoreSavedStateManager.SavedState?.Workspaces ?? new()
		)
		{
			IWorkspace? workspace = _context.WorkspaceManager.TryGet(savedWorkspace.Name);
			if (workspace == null)
			{
				Logger.Debug($"Could not find workspace {savedWorkspace.Name}");
				continue;
			}

			foreach (SavedWindow savedWindow in savedWorkspace.Windows)
			{
				HWND hwnd = (HWND)savedWindow.Handle;
				if (!_context.WindowManager.CreateWindow(hwnd).TryGet(out IWindow window))
				{
					Logger.Debug($"Could not find window with handle {savedWindow.Handle}");
					continue;
				}

				_pantry.SetWindowWorkspace(window, workspace);
				workspace.MoveWindowToPoint(window, savedWindow.Rectangle.Center, deferLayout: false);
				processedWindows.Add(hwnd);

				// Fire the window added event.
				_context.Store.Dispatch(new WindowAddedTransform(window.Handle));
			}
		}

		// Route the rest of the windows to the monitor they're on.
		// Don't route to the active workspace while we're adding all the windows.
		RouterOptions routerOptions = _context.RouterManager.RouterOptions;
		_context.RouterManager.RouterOptions = RouterOptions.RouteToLaunchedWorkspace;

		// Add all existing windows.
		foreach (HWND hwnd in _internalContext.CoreNativeManager.GetAllWindows())
		{
			if (processedWindows.Contains(hwnd))
			{
				continue;
			}

			_context.Store.Dispatch(new WindowAddedTransform(hwnd));
		}

		// Restore the route to active workspace setting.
		_context.RouterManager.RouterOptions = routerOptions;
	}

	#region Chores
	public void Activate(IWorkspace workspace, IMonitor? monitor = null) =>
		_context.Store.Dispatch(new ActivateWorkspaceTransform(workspace, monitor));

	public void ActivateAdjacent(IMonitor? monitor = null, bool reverse = false, bool skipActive = false) =>
		_context.Store.Dispatch(new ActivateAdjacentTransform(monitor, reverse, skipActive));

	public void LayoutAllActiveWorkspaces() => _context.Store.Dispatch(new LayoutAllActiveWorkspacesTransform());

	public void FocusMonitorDesktop(IMonitor monitor) =>
		_context.Store.Dispatch(new FocusMonitorDesktopTransform(monitor));

	public bool MoveWindowEdgesInDirection(Direction edges, IPoint<int> pixelsDeltas, IWindow? window = null) =>
		_context.Store.Dispatch(new MoveWindowEdgesInDirectionTransform(edges, pixelsDeltas, window)).IsSuccessful;

	public void MoveWindowToAdjacentWorkspace(IWindow? window = null, bool reverse = false, bool skipActive = false) =>
		_context.Store.Dispatch(new MoveWindowToAdjacentWorkspaceTransform(window, reverse, skipActive));

	public void MoveWindowToMonitor(IMonitor monitor, IWindow? window = null) =>
		_context.Store.Dispatch(new MoveWindowToMonitorTransform(monitor, window));

	public void MoveWindowToPreviousMonitor(IWindow? window = null) =>
		_context.Store.Dispatch(new MoveWindowToAdjacentMonitorTransform(window, Reverse: true));

	public void MoveWindowToNextMonitor(IWindow? window = null) =>
		_context.Store.Dispatch(new MoveWindowToAdjacentMonitorTransform(window, Reverse: false));

	public void MoveWindowToPoint(IWindow window, IPoint<int> point) =>
		_context.Store.Dispatch(new MoveWindowToPointTransform(window, point));

	public void MoveWindowToWorkspace(IWorkspace targetWorkspace, IWindow? window = null) =>
		_context.Store.Dispatch(new MoveWindowToWorkspaceTransform(targetWorkspace, window));

	public void MergeWorkspaceWindows(IWorkspace source, IWorkspace target) =>
		_context.Store.Dispatch(new MergeWorkspaceWindowsTransform(source, target));

	public void SwapWorkspaceWithAdjacentMonitor(IWorkspace? workspace = null, bool reverse = false) =>
		_context.Store.Dispatch(new SwapWorkspaceWithAdjacentMonitorTransform(workspace, reverse));
	#endregion
}
