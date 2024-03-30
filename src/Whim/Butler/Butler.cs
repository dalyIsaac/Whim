using System;
using System.Collections.Generic;
using Windows.Win32.Foundation;

namespace Whim;

internal partial class Butler : IButler, IInternalButler
{
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;

	private readonly IButlerChores _chores;
	private bool _initialized;
	internal readonly ButlerEventHandlers EventHandlers;

	private IButlerPantry _pantry;
	public IButlerPantry Pantry
	{
		get => _pantry;
		set
		{
			if (_initialized)
			{
				Logger.Error("Cannot set the pantry after initialization.");
				return;
			}

			_pantry = value;
		}
	}

	public Butler(IContext context, IInternalContext internalContext)
	{
		_context = context;
		_internalContext = internalContext;

		_pantry = new ButlerPantry(_context);
		_chores = new ButlerChores(_context, _internalContext);
		EventHandlers = new ButlerEventHandlers(_context, _internalContext, _pantry, _chores);

		_windowRoutedEvent = new(context);
		_monitorWorkspaceChangedEvent = new(context);
	}

	private readonly ThreadSafeEvent<RouteEventArgs> _windowRoutedEvent;
	public event EventHandler<RouteEventArgs>? WindowRouted
	{
		add => _windowRoutedEvent.Add(value);
		remove => _windowRoutedEvent.Remove(value);
	}

	private readonly ThreadSafeEvent<MonitorWorkspaceChangedEventArgs> _monitorWorkspaceChangedEvent;
	public event EventHandler<MonitorWorkspaceChangedEventArgs>? MonitorWorkspaceChanged
	{
		add => _monitorWorkspaceChangedEvent.Add(value);
		remove => _monitorWorkspaceChangedEvent.Remove(value);
	}

	public void TriggerWindowRouted(RouteEventArgs args) => _windowRoutedEvent.Invoke(this, args);

	public void TriggerMonitorWorkspaceChanged(MonitorWorkspaceChangedEventArgs args) =>
		_monitorWorkspaceChangedEvent?.Invoke(this, args);

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
				IWindow? window = _context.WindowManager.CreateWindow(hwnd);
				if (window == null)
				{
					Logger.Debug($"Could not find window with handle {savedWindow.Handle}");
					continue;
				}

				_pantry.SetWindowWorkspace(window, workspace);
				workspace.MoveWindowToPoint(window, savedWindow.Rectangle.Center, deferLayout: false);
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

	#region Chores
	public void Activate(IWorkspace workspace, IMonitor? monitor = null) => _chores.Activate(workspace, monitor);

	public void ActivateAdjacent(IMonitor? monitor = null, bool reverse = false, bool skipActive = false) =>
		_chores.ActivateAdjacent(monitor, reverse, skipActive);

	public void LayoutAllActiveWorkspaces() => _chores.LayoutAllActiveWorkspaces();

	public void FocusMonitorDesktop(IMonitor monitor) => _chores.FocusMonitorDesktop(monitor);

	public void MergeWorkspaceWindows(IWorkspace workspaceToDelete, IWorkspace workspaceMergeTarget) =>
		_chores.MergeWorkspaceWindows(workspaceToDelete, workspaceMergeTarget);

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
}
