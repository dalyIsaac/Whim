using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Win32.Foundation;

namespace Whim;

internal class Workspace : IWorkspace, IInternalWorkspace
{
	/// <summary>
	/// Lock for mutating the layout engines.
	/// </summary>
	private readonly object _workspaceLock = new();
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;
	private readonly WorkspaceManagerTriggers _triggers;

	private string _name;
	public string Name
	{
		get => _name;
		set
		{
			string oldName = _name;
			_name = value;
			_triggers.WorkspaceRenamed(
				new WorkspaceRenamedEventArgs()
				{
					Workspace = this,
					PreviousName = oldName,
					CurrentName = _name
				}
			);
		}
	}

	private IWindow? _lastFocusedWindow;

	public IWindow? LastFocusedWindow
	{
		get
		{
			lock (_workspaceLock)
			{
				return _lastFocusedWindow;
			}
		}
		private set
		{
			lock (_workspaceLock)
			{
				_lastFocusedWindow = value;
			}
		}
	}

	protected readonly ILayoutEngine[] _layoutEngines;
	private int _activeLayoutEngineIndex;
	private bool _disposedValue;

	// NOTE: Don't set this directly. Usually all the layout engines will need to be updated.
	public ILayoutEngine ActiveLayoutEngine
	{
		get
		{
			lock (_workspaceLock)
			{
				return _layoutEngines[_activeLayoutEngineIndex];
			}
		}
	}

	/// <summary>
	/// All the windows in this workspace.
	/// </summary>
	private readonly HashSet<IWindow> _windows = new();

	public IEnumerable<IWindow> Windows
	{
		get
		{
			lock (_workspaceLock)
			{
				return _windows;
			}
		}
	}

	/// <summary>
	/// Map of window handles to their current <see cref="IWindowState"/>.
	/// </summary>
	private Dictionary<HWND, IWindowState> _windowStates = new();

	public Workspace(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		string name,
		IEnumerable<ILayoutEngine> layoutEngines
	)
	{
		_context = context;
		_internalContext = internalContext;
		_triggers = triggers;

		_name = name;
		_layoutEngines = layoutEngines.ToArray();

		if (_layoutEngines.Length == 0)
		{
			throw new ArgumentException("At least one layout engine must be provided.");
		}
	}

	public void WindowFocused(IWindow? window)
	{
		lock (_workspaceLock)
		{
			if (window != null && _windows.Contains(window))
			{
				LastFocusedWindow = window;
				Logger.Debug($"Focused window {window} in workspace {Name}");
			}
		}
	}

	public void MinimizeWindowStart(IWindow window)
	{
		lock (_workspaceLock)
		{
			Logger.Debug($"Minimizing window {window} in workspace {Name}");

			// If the window is already in the workspace, minimize it in just the active layout engine.
			// If it isn't, then we assume it was provided during startup and minimize it in all layouts.
			if (_windows.Contains(window))
			{
				_layoutEngines[_activeLayoutEngineIndex] = _layoutEngines[_activeLayoutEngineIndex].MinimizeWindowStart(
					window
				);
			}
			else
			{
				_windows.Add(window);

				for (int idx = 0; idx < _layoutEngines.Length; idx++)
				{
					_layoutEngines[idx] = _layoutEngines[idx].MinimizeWindowStart(window);
				}
			}
		}

		DoLayout();
	}

	public void MinimizeWindowEnd(IWindow window)
	{
		lock (_workspaceLock)
		{
			Logger.Debug($"Minimizing window {window} in workspace {Name}");
			_windows.Add(window);

			// Restore in just the active layout engine. MinimizeWindowEnd is not called as part of
			// Whim starting up.
			_layoutEngines[_activeLayoutEngineIndex] = _layoutEngines[_activeLayoutEngineIndex].MinimizeWindowEnd(
				window
			);
		}

		DoLayout();
	}

	public void FocusFirstWindow()
	{
		Logger.Debug($"Focusing first window in workspace {Name}");
		ActiveLayoutEngine.GetFirstWindow()?.Focus();
	}

	public void FocusLastFocusedWindow()
	{
		Logger.Debug($"Focusing last focused window in workspace {Name}");
		if (LastFocusedWindow != null)
		{
			LastFocusedWindow.Focus();
		}
		else
		{
			Logger.Debug($"No windows in workspace {Name} to focus, focusing desktop");

			// Get the bounds of the monitor for this workspace.
			IMonitor? monitor = _context.WorkspaceManager.GetMonitorForWorkspace(this);
			if (monitor == null)
			{
				Logger.Debug($"No active monitors found for workspace {Name}.");
				return;
			}

			// Focus the desktop.
			HWND desktop = _internalContext.CoreNativeManager.GetDesktopWindow();
			_internalContext.CoreNativeManager.SetForegroundWindow(desktop);
			_internalContext.WindowManager.OnWindowFocused(null);

			_internalContext.MonitorManager.ActivateEmptyMonitor(monitor);
		}
	}

	private void UpdateLayoutEngine(int delta)
	{
		ILayoutEngine prevLayoutEngine;
		ILayoutEngine nextLayoutEngine;

		lock (_workspaceLock)
		{
			int prevIdx = _activeLayoutEngineIndex;
			_activeLayoutEngineIndex = (_activeLayoutEngineIndex + delta).Mod(_layoutEngines.Length);

			prevLayoutEngine = _layoutEngines[prevIdx];
			nextLayoutEngine = _layoutEngines[_activeLayoutEngineIndex];
		}

		DoLayout();
		_triggers.ActiveLayoutEngineChanged(
			new ActiveLayoutEngineChangedEventArgs()
			{
				Workspace = this,
				PreviousLayoutEngine = prevLayoutEngine,
				CurrentLayoutEngine = nextLayoutEngine
			}
		);
	}

	public void NextLayoutEngine()
	{
		Logger.Debug(Name);
		UpdateLayoutEngine(1);
	}

	public void PreviousLayoutEngine()
	{
		Logger.Debug(Name);
		UpdateLayoutEngine(-1);
	}

	public bool TrySetLayoutEngine(string name)
	{
		Logger.Debug($"Trying to set layout engine {name} for workspace {Name}");

		ILayoutEngine prevLayoutEngine;
		ILayoutEngine nextLayoutEngine;

		lock (_workspaceLock)
		{
			int nextIdx = -1;
			for (int idx = 0; idx < _layoutEngines.Length; idx++)
			{
				ILayoutEngine engine = _layoutEngines[idx];
				if (engine.Name == name)
				{
					nextIdx = idx;
					break;
				}
			}

			if (nextIdx == -1)
			{
				Logger.Error($"Layout engine {name} not found for workspace {Name}");
				return false;
			}
			else if (_activeLayoutEngineIndex == nextIdx)
			{
				Logger.Debug($"Layout engine {name} is already active for workspace {Name}");
				return true;
			}

			int prevIdx = _activeLayoutEngineIndex;
			_activeLayoutEngineIndex = nextIdx;

			prevLayoutEngine = _layoutEngines[prevIdx];
			nextLayoutEngine = _layoutEngines[_activeLayoutEngineIndex];
		}

		DoLayout();
		_triggers.ActiveLayoutEngineChanged(
			new ActiveLayoutEngineChangedEventArgs()
			{
				Workspace = this,
				PreviousLayoutEngine = prevLayoutEngine,
				CurrentLayoutEngine = nextLayoutEngine
			}
		);
		return true;
	}

	public void AddWindow(IWindow window)
	{
		lock (_workspaceLock)
		{
			Logger.Debug($"Adding window {window} to workspace {Name}");

			if (_windows.Contains(window))
			{
				Logger.Error($"Window {window} already exists in workspace {Name}");
				window.Focus();
				return;
			}

			_windows.Add(window);
			for (int i = 0; i < _layoutEngines.Length; i++)
			{
				_layoutEngines[i] = _layoutEngines[i].AddWindow(window);
			}
		}

		DoLayout();
		window.Focus();
		LastFocusedWindow = window;
	}

	public bool RemoveWindow(IWindow window)
	{
		bool success;
		lock (_workspaceLock)
		{
			Logger.Debug($"Removing window {window} from workspace {Name}");

			if (window.Equals(LastFocusedWindow))
			{
				// Find the next window to focus.
				foreach (IWindow nextWindow in Windows)
				{
					if (nextWindow.Equals(window))
					{
						continue;
					}

					LastFocusedWindow = nextWindow;
					break;
				}

				// If there are no other windows, set the last focused window to null.
				if (LastFocusedWindow.Equals(window))
				{
					LastFocusedWindow = null;
				}
			}

			bool isWindow = _windows.Contains(window);
			if (!isWindow)
			{
				Logger.Error($"Window {window} already does not exist in workspace {Name}");
				return false;
			}

			success = true;

			for (int idx = 0; idx < _layoutEngines.Length; idx++)
			{
				ILayoutEngine oldEngine = _layoutEngines[idx];
				ILayoutEngine newEngine = oldEngine.RemoveWindow(window);

				if (newEngine == oldEngine)
				{
					Logger.Error($"Window {window} could not be removed from layout engine {oldEngine}");
					success = false;
				}
				else
				{
					_layoutEngines[idx] = newEngine;
				}
			}

			if (success)
			{
				_windows.Remove(window);
			}
		}

		if (success)
		{
			DoLayout();
			return true;
		}

		return false;
	}

	/// <summary>
	/// Returns the window to process. If the window is null, the last focused window is used.
	/// If the given window is not null, it is checked if it exists in the workspace.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	private IWindow? GetValidVisibleWindow(IWindow? window)
	{
		window ??= LastFocusedWindow;

		if (window == null)
		{
			Logger.Error($"Could not find a valid window in workspace {Name} to perform action");
			return null;
		}

		if (!_windows.Contains(window))
		{
			Logger.Error($"Window {window} does not exist in workspace {Name}");
			return null;
		}

		return window;
	}

	public void FocusWindowInDirection(Direction direction, IWindow? window = null)
	{
		Logger.Debug($"Focusing window {window} in workspace {Name}");

		lock (_workspaceLock)
		{
			if (GetValidVisibleWindow(window) is not IWindow validWindow)
			{
				return;
			}

			ILayoutEngine newEngine = ActiveLayoutEngine.FocusWindowInDirection(direction, validWindow);

			if (ActiveLayoutEngine != newEngine)
			{
				_layoutEngines[_activeLayoutEngineIndex] = newEngine;
				DoLayout();
			}
		}
	}

	public void SwapWindowInDirection(Direction direction, IWindow? window = null)
	{
		bool success = false;

		lock (_workspaceLock)
		{
			Logger.Debug($"Swapping window {window} in workspace {Name} in direction {direction}");
			if (GetValidVisibleWindow(window) is IWindow validWindow)
			{
				_layoutEngines[_activeLayoutEngineIndex] = ActiveLayoutEngine.SwapWindowInDirection(
					direction,
					validWindow
				);
				success = true;
			}
		}

		if (success)
		{
			DoLayout();
		}
		return;
	}

	public void MoveWindowEdgesInDirection(Direction edges, IPoint<double> deltas, IWindow? window = null)
	{
		bool success = false;

		lock (_workspaceLock)
		{
			Logger.Debug($"Moving window {window} in workspace {Name} in direction {edges} by {deltas}");
			if (GetValidVisibleWindow(window) is IWindow validWindow)
			{
				_layoutEngines[_activeLayoutEngineIndex] = ActiveLayoutEngine.MoveWindowEdgesInDirection(
					edges,
					deltas,
					validWindow
				);
				success = true;
			}
		}

		if (success)
		{
			DoLayout();
		}
		return;
	}

	public void MoveWindowToPoint(IWindow window, IPoint<double> point)
	{
		lock (_workspaceLock)
		{
			Logger.Debug($"Moving window {window} to point {point} in workspace {Name}");

			if (_windows.Contains(window))
			{
				// The window is already in the workspace, so move it in just the active layout engine
				_layoutEngines[_activeLayoutEngineIndex] = ActiveLayoutEngine.MoveWindowToPoint(window, point);
			}
			else
			{
				// The window is new to the workspace, so add it to all layout engines
				_windows.Add(window);

				for (int idx = 0; idx < _layoutEngines.Length; idx++)
				{
					_layoutEngines[idx] = _layoutEngines[idx].MoveWindowToPoint(window, point);
				}
			}
		}

		DoLayout();
		window.Focus();
	}

	public override string ToString() => Name;

	public void Deactivate()
	{
		Logger.Debug($"Deactivating workspace {Name}");

		foreach (IWindow window in Windows)
		{
			window.Hide();
		}

		_windowStates.Clear();
	}

	public IWindowState? TryGetWindowState(IWindow window)
	{
		_windowStates.TryGetValue(window.Handle, out IWindowState? rect);
		return rect;
	}

	public void DoLayout()
	{
		Logger.Debug($"Workspace {Name}");

		if (_disposedValue)
		{
			Logger.Debug($"Workspace {Name} is disposed, skipping layout");
			return;
		}

		if (GarbageCollect())
		{
			Logger.Debug($"Garbage collected windows, skipping layout for workspace {Name}");
			return;
		}

		// Get the monitor for this workspace
		IMonitor? monitor = _context.WorkspaceManager.GetMonitorForWorkspace(this);
		if (monitor == null)
		{
			Logger.Debug($"No active monitors found for workspace {Name}.");
			return;
		}

		Logger.Debug($"Starting layout for workspace {Name}");
		_triggers.WorkspaceLayoutStarted(new WorkspaceEventArgs() { Workspace = this });

		// Execute the layout task
		_windowStates = SetWindowPos(engine: ActiveLayoutEngine, monitor);
		_triggers.WorkspaceLayoutCompleted(new WorkspaceEventArgs() { Workspace = this });
	}

	private Dictionary<HWND, IWindowState> SetWindowPos(ILayoutEngine engine, IMonitor monitor)
	{
		Logger.Debug($"Setting window positions for workspace {Name}");
		List<WindowPosState> windowStates = new();
		Dictionary<HWND, IWindowState> windowRects = new();

		foreach (IWindowState loc in engine.DoLayout(monitor.WorkingArea, monitor))
		{
			windowStates.Add(new(loc, (HWND)1, null));
			windowRects.Add(loc.Window.Handle, loc);
		}

		using DeferWindowPosHandle handle = _context.NativeManager.DeferWindowPos(windowStates);

		return windowRects;
	}

	public bool ContainsWindow(IWindow window)
	{
		lock (_workspaceLock)
		{
			return _windows.Contains(window);
		}
	}

	/// <summary>
	/// Garbage collects windows that are no longer valid.
	/// </summary>
	/// <returns></returns>
	private bool GarbageCollect()
	{
		List<IWindow> garbageWindows = new();
		bool garbageCollected = false;

		foreach (IWindow window in Windows)
		{
			bool removeWindow = false;
			if (!_internalContext.CoreNativeManager.IsWindow(window.Handle))
			{
				Logger.Debug($"Window {window.Handle} is no longer a window.");
				removeWindow = true;
			}
			else if (!_internalContext.WindowManager.HandleWindowMap.ContainsKey(window.Handle))
			{
				Logger.Debug($"Window {window.Handle} is somehow no longer managed.");
				removeWindow = true;
			}

			if (removeWindow)
			{
				garbageWindows.Add(window);
				garbageCollected = true;
			}
		}

		// Remove the windows by doing a sneaky call.
		foreach (IWindow window in garbageWindows)
		{
			_internalContext.WindowManager.OnWindowRemoved(window);
		}

		return garbageCollected;
	}

	public void PerformCustomLayoutEngineAction(LayoutEngineCustomAction action)
	{
		PerformCustomLayoutEngineAction(
			new LayoutEngineCustomAction<IWindow?>()
			{
				Name = action.Name,
				Payload = action.Window,
				Window = action.Window
			}
		);
	}

	public void PerformCustomLayoutEngineAction<T>(LayoutEngineCustomAction<T> action)
	{
		Logger.Debug($"Attempting to perform custom layout engine action {action.Name} for workspace {Name}");

		bool doLayout = false;

		// Update the layout engine for a given index can change ActiveLayoutEngine, which breaks the
		// doLayout test.
		ILayoutEngine prevActiveLayoutEngine = ActiveLayoutEngine;

		for (int idx = 0; idx < _layoutEngines.Length; idx++)
		{
			ILayoutEngine oldEngine = _layoutEngines[idx];
			ILayoutEngine newEngine = oldEngine.PerformCustomAction(action);

			if (newEngine.Equals(oldEngine))
			{
				Logger.Debug($"Layout engine {oldEngine} could not perform action {action.Name}");
			}
			else
			{
				_layoutEngines[idx] = newEngine;

				if (oldEngine == prevActiveLayoutEngine)
				{
					doLayout = true;
				}
			}
		}

		if (doLayout)
		{
			DoLayout();
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				Logger.Debug($"Disposing workspace {Name}");

				// dispose managed state (managed objects)
				bool isWorkspaceActive = _context.WorkspaceManager.GetMonitorForWorkspace(this) != null;

				// If the workspace isn't active on the monitor, show all the windows in as minimized.
				if (!isWorkspaceActive)
				{
					foreach (IWindow window in Windows)
					{
						window.ShowMinimized();
					}
				}
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
