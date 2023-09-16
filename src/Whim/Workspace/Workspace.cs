using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

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

	/// <summary>
	/// The last focused window in this workspace.
	/// </summary>
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

	private readonly ILayoutEngine[] _layoutEngines;
	private int _activeLayoutEngineIndex;
	private bool _disposedValue;

	private readonly ConcurrentQueue<(ILayoutEngine layoutEngine, IMonitor monitor)> _layoutQueue = new();
	private CancellationTokenSource? _cancellationTokenSource;
	private readonly object _layoutLock = new();

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
	/// All the windows in this workspace which are <see cref="WindowSize.Normal"/>.
	/// </summary>
	private readonly HashSet<IWindow> _normalWindows = new();

	/// <summary>
	/// All the minimized windows in this workspace.
	/// </summary>
	private readonly HashSet<IWindow> _minimizedWindows = new();

	public IEnumerable<IWindow> Windows
	{
		get
		{
			lock (_workspaceLock)
			{
				return _normalWindows.Concat(_minimizedWindows);
			}
		}
	}

	/// <summary>
	/// Map of window handles to their current location.
	/// </summary>
	private Dictionary<HWND, IWindowState> _windowLocations = new();

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
			if (window != null && _normalWindows.Contains(window))
			{
				LastFocusedWindow = window;
				Logger.Debug($"Focused window {window} in workspace {Name}");
			}
		}
	}

	public void WindowMinimizeStart(IWindow window)
	{
		lock (_workspaceLock)
		{
			if (!_normalWindows.Contains(window))
			{
				Logger.Error($"Window {window} is not a normal window in workspace {Name}");
				return;
			}

			_normalWindows.Remove(window);
			_minimizedWindows.Add(window);

			for (int i = 0; i < _layoutEngines.Length; i++)
			{
				_layoutEngines[i] = _layoutEngines[i].RemoveWindow(window);
			}
		}

		DoLayout();
	}

	public void WindowMinimizeEnd(IWindow window)
	{
		lock (_workspaceLock)
		{
			if (!_minimizedWindows.Contains(window))
			{
				Logger.Error($"Window {window} is not a minimized window in workspace {Name}");
				return;
			}

			_minimizedWindows.Remove(window);
			AddWindow(window);
		}
	}

	public void FocusFirstWindow()
	{
		Logger.Debug($"Focusing first window in workspace {Name}");
		ActiveLayoutEngine.GetFirstWindow()?.Focus();
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

		DoLayout(
			() =>
				_triggers.ActiveLayoutEngineChanged(
					new ActiveLayoutEngineChangedEventArgs()
					{
						Workspace = this,
						PreviousLayoutEngine = prevLayoutEngine,
						CurrentLayoutEngine = nextLayoutEngine
					}
				)
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

		DoLayout(
			() =>
				_triggers.ActiveLayoutEngineChanged(
					new ActiveLayoutEngineChangedEventArgs()
					{
						Workspace = this,
						PreviousLayoutEngine = prevLayoutEngine,
						CurrentLayoutEngine = nextLayoutEngine
					}
				)
		);

		return true;
	}

	public void AddWindow(IWindow window)
	{
		lock (_workspaceLock)
		{
			Logger.Debug($"Adding window {window} to workspace {Name}");

			if (_normalWindows.Contains(window))
			{
				Logger.Error($"Window {window} already exists in workspace {Name}");
				return;
			}

			_normalWindows.Add(window);
			for (int i = 0; i < _layoutEngines.Length; i++)
			{
				_layoutEngines[i] = _layoutEngines[i].AddWindow(window);
			}
		}

		DoLayout(window.Focus);
	}

	public bool RemoveWindow(IWindow window)
	{
		bool success;
		lock (_workspaceLock)
		{
			Logger.Debug($"Removing window {window} from workspace {Name}");

			if (window.Equals(LastFocusedWindow))
			{
				LastFocusedWindow = null;
			}

			bool isNormalWindow = _normalWindows.Contains(window);
			if (!isNormalWindow && !_minimizedWindows.Contains(window))
			{
				Logger.Error($"Window {window} already does not exist in workspace {Name}");
				return false;
			}

			success = true;
			if (isNormalWindow)
			{
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
					_normalWindows.Remove(window);
				}
			}
			else
			{
				_minimizedWindows.Remove(window);
			}
		}

		if (success)
		{
			DoLayout();
		}

		return success;
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

		if (!ContainsWindow(window))
		{
			Logger.Error($"Window {window} does not exist in workspace {Name}");
			return null;
		}

		if (_minimizedWindows.Contains(window))
		{
			Logger.Error($"Window {window} is minimized in workspace {Name}");
			return null;
		}

		return window;
	}

	public void FocusWindowInDirection(Direction direction, IWindow? window = null)
	{
		Logger.Debug($"Focusing window {window} in workspace {Name}");

		lock (_workspaceLock)
		{
			if (GetValidVisibleWindow(window) is IWindow validWindow)
			{
				ActiveLayoutEngine.FocusWindowInDirection(direction, validWindow);
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
	}

	public void MoveWindowToPoint(IWindow window, IPoint<double> point)
	{
		lock (_workspaceLock)
		{
			Logger.Debug($"Moving window {window} to point {point} in workspace {Name}");

			if (_normalWindows.Contains(window))
			{
				// The window is already in the workspace, so move it in just the active layout engine
				_layoutEngines[_activeLayoutEngineIndex] = ActiveLayoutEngine.MoveWindowToPoint(window, point);
			}
			else
			{
				// If the window is minimized, remove it from the minimized list, and treat it as a new window
				if (_minimizedWindows.Contains(window))
				{
					_minimizedWindows.Remove(window);
				}

				// The window is new to the workspace, so add it to all layout engines
				_normalWindows.Add(window);

				for (int idx = 0; idx < _layoutEngines.Length; idx++)
				{
					_layoutEngines[idx] = _layoutEngines[idx].MoveWindowToPoint(window, point);
				}
			}
		}

		DoLayout();
	}

	public override string ToString() => Name;

	public void Deactivate()
	{
		Logger.Debug($"Deactivating workspace {Name}");

		foreach (IWindow window in Windows)
		{
			window.Hide();
		}

		_windowLocations.Clear();
	}

	public IWindowState? TryGetWindowLocation(IWindow window)
	{
		_windowLocations.TryGetValue(window.Handle, out IWindowState? location);

		if (location is null && _minimizedWindows.Contains(window))
		{
			location = new WindowState()
			{
				Window = window,
				Location = new Location<int>(),
				WindowSize = WindowSize.Minimized
			};
		}

		return location;
	}

	public void DoLayout(Action? afterLayout = null)
	{
		Logger.Debug($"Workspace {Name}");

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

		lock (_layoutLock)
		{
			_layoutQueue.Enqueue((ActiveLayoutEngine, monitor));
			if (_layoutQueue.Count > 1)
			{
				// Cancel the previous layout tasks
				_cancellationTokenSource?.Cancel();
				Logger.Debug($"Cancelling previous layout tasks for workspace {Name}");
			}

			// Set the window positions in another thread. Doing this in the same thread can block
			// the UI thread, which can delay the handling of messages in the window manager - see #446.
			_cancellationTokenSource = new();
		}

		// Execute the layout task
		TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
		Task.Run(
				() => SetWindowPos(engine: ActiveLayoutEngine, monitor, _cancellationTokenSource.Token),
				_cancellationTokenSource.Token
			)
			.ContinueWith(
				t =>
				{
					// Remove the completed task from the queue
					_layoutQueue.TryDequeue(out (ILayoutEngine layoutEngine, IMonitor monitor) _);

					// If there are no more tasks in the queue, dispose the cancellation token source
					lock (_layoutLock)
					{
						if (_layoutQueue.IsEmpty)
						{
							_cancellationTokenSource?.Dispose();
							_cancellationTokenSource = null;
						}

						// Add the window locations to the map
						_windowLocations = t.Result;
					}

					// Trigger the layout completed event
					afterLayout?.Invoke();
					_triggers.WorkspaceLayoutCompleted(new WorkspaceEventArgs() { Workspace = this });
				},
				uiScheduler
			);
	}

	private Dictionary<HWND, IWindowState> SetWindowPos(
		ILayoutEngine engine,
		IMonitor monitor,
		CancellationToken cancellationToken
	)
	{
		_internalContext.LayoutLock.EnterReadLock();
		Logger.Debug($"Setting window positions for workspace {Name}");
		List<(IWindowState windowState, HWND hwndInsertAfter, SET_WINDOW_POS_FLAGS? flags)> windowStates = new();
		Dictionary<HWND, IWindowState> windowLocations = new();

		foreach (IWindowState loc in engine.DoLayout(monitor.WorkingArea, monitor))
		{
			cancellationToken.ThrowIfCancellationRequested();
			windowStates.Add((loc, (HWND)1, null));
			windowLocations.Add(loc.Window.Handle, loc);
		}

		WindowDeferPosHandle handle = new(_context, windowStates, cancellationToken);
		handle.Dispose();

		_internalContext.LayoutLock.ExitReadLock();
		return windowLocations;
	}

	public bool ContainsWindow(IWindow window)
	{
		lock (_workspaceLock)
		{
			return _normalWindows.Contains(window) || _minimizedWindows.Contains(window);
		}
	}

	/// <summary>
	/// Garbage collects windows that are no longer valid.
	/// </summary>
	/// <returns></returns>
	private bool GarbageCollect()
	{
		IInternalWindowManager windowManager = (IInternalWindowManager)_context.WindowManager;

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
			else if (!windowManager.Windows.ContainsKey(window.Handle))
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
			windowManager.OnWindowRemoved(window);
		}

		return garbageCollected;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				Logger.Debug($"Disposing workspace {Name}");

				// dispose managed state (managed objects)
				_cancellationTokenSource?.Dispose();

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
