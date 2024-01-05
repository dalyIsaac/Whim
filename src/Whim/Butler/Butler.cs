using System;
using System.Collections.Generic;
using Windows.Win32.Foundation;

namespace Whim;

// TODO: Order
internal partial class Butler : IButler
{
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;

	private readonly IButlerPantry _pantry;
	private readonly ButlerTriggers _triggers;
	private readonly IButlerChores _chores;
	private readonly ButlerEventHandlers _eventHandlers;
	private bool _disposedValue;

	public Butler(IContext context, IInternalContext internalContext)
	{
		_context = context;
		_internalContext = internalContext;
		_triggers = new ButlerTriggers()
		{
			WindowRouted = (args) => WindowRouted?.Invoke(this, args),
			MonitorWorkspaceChanged = (args) => MonitorWorkspaceChanged?.Invoke(this, args),
		};
		_pantry = new ButlerPantry();
		_chores = new ButlerChores(_context, _triggers, _pantry);
		_eventHandlers = new ButlerEventHandlers(_context, _internalContext, _triggers, _pantry, _chores);
	}

	public event EventHandler<RouteEventArgs>? WindowRouted;

	public event EventHandler<MonitorWorkspaceChangedEventArgs>? MonitorWorkspaceChanged;

	#region Initialize
	public void PreInitialize()
	{
		Logger.Debug("Pre-initializing Butler");
		_eventHandlers.PreInitialize();
	}

	public void Initialize()
	{
		// Add the saved windows at their saved locations inside their saved workspaces.
		// Other windows are routed to the monitor they're on.

		List<HWND> processedWindows = new();

		// Route windows to their saved workspaces.
		foreach (
			SavedWorkspace savedWorkspace in _internalContext.CoreSavedStateManager.SavedState?.Workspaces ?? new()
		)
		{
			IWorkspace? workspace = _context.WorkspaceManager2.TryGet(savedWorkspace.Name);
			if (workspace == null)
			{
				Logger.Debug($"Could not find workspace {savedWorkspace.Name}");
				continue;
			}

			foreach (SavedWindow savedWindow in savedWorkspace.Windows)
			{
				HWND hwnd = (HWND)savedWindow.Handle;
				IWindow? window = _context.WindowManager.CreateWindow(hwnd);
				if (window == null)
				{
					Logger.Debug($"Could not find window with handle {savedWindow.Handle}");
					continue;
				}

				_pantry.SetWindowWorkspace(window, workspace);
				workspace.MoveWindowToPoint(window, savedWindow.Rectangle.Center);
				processedWindows.Add(hwnd);

				// Fire the window added event.
				_internalContext.WindowManager.OnWindowAdded(window);
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

			_internalContext.WindowManager.AddWindow(hwnd);
		}

		// Restore the route to active workspace setting.
		_context.RouterManager.RouterOptions = routerOptions;
	}

	#endregion

	#region Pantry
	public IMonitor? GetMonitorForWindow(IWindow window) => _pantry.GetMonitorForWindow(window);

	public IMonitor? GetMonitorForWorkspace(IWorkspace workspace) => _pantry.GetMonitorForWorkspace(workspace);

	public IWorkspace? GetWorkspaceForMonitor(IMonitor monitor) => _pantry.GetWorkspaceForMonitor(monitor);

	public IWorkspace? GetWorkspaceForWindow(IWindow window) => _pantry.GetWorkspaceForWindow(window);
	#endregion

	#region Chores
	public void Activate(IWorkspace workspace, IMonitor? monitor = null) => _chores.Activate(workspace, monitor);

	public void ActivateAdjacent(IMonitor? monitor = null, bool reverse = false, bool skipActive = false) =>
		_chores.ActivateAdjacent(monitor, reverse, skipActive);

	public void LayoutAllActiveWorkspaces() => _chores.LayoutAllActiveWorkspaces();

	public void RemoveWorkspace(IWorkspace workspaceToDelete, IWorkspace workspaceMergeTarget) =>
		_chores.RemoveWorkspace(workspaceToDelete, workspaceMergeTarget);

	public bool MoveWindowEdgesInDirection(Direction edges, IPoint<int> pixelsDeltas, IWindow? window = null) =>
		_chores.MoveWindowEdgesInDirection(edges, pixelsDeltas, window);

	public void MoveWindowToAdjacentWorkspace(IWindow? window = null, bool reverse = false, bool skipActive = false) =>
		_chores.MoveWindowToAdjacentWorkspace(window, reverse, skipActive);

	public void MoveWindowToWorkspace(IWorkspace workspace, IWindow? window = null) =>
		_chores.MoveWindowToWorkspace(workspace, window);

	public void MoveWindowToMonitor(IMonitor monitor, IWindow? window = null) =>
		_chores.MoveWindowToMonitor(monitor, window);

	public void MoveWindowToNextMonitor(IWindow? window = null) => _chores.MoveWindowToNextMonitor(window);

	public void MoveWindowToPreviousMonitor(IWindow? window = null) => _chores.MoveWindowToPreviousMonitor(window);

	public void MoveWindowToPoint(IWindow window, IPoint<int> point) => _chores.MoveWindowToPoint(window, point);

	public void SwapWorkspaceWithAdjacentMonitor(IWorkspace? workspace = null, bool reverse = false) =>
		_chores.SwapWorkspaceWithAdjacentMonitor(workspace, reverse);
	#endregion

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_eventHandlers.Dispose();
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
