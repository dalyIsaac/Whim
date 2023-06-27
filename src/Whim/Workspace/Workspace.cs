using System;
using System.Collections.Generic;
using System.Linq;

namespace Whim;

internal class Workspace : IWorkspace, IInternalWorkspace
{
	private bool _initialized;
	private readonly IContext _context;
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

	/// <summary>
	/// The last focused window in this workspace.
	/// </summary>
	public IWindow? LastFocusedWindow { get; private set; }

	private readonly ILayoutEngine[] _layoutEngines;
	private int _activeLayoutEngineIndex;
	private bool _disposedValue;

	public ILayoutEngine ActiveLayoutEngine => _layoutEngines[_activeLayoutEngineIndex];

	/// <summary>
	/// All the windows in this workspace which are common to every layout engine.
	/// The intersection of <see cref="_normalWindows"/> and <see cref="_phantomWindows"/>
	/// is the empty set.
	/// </summary>
	private readonly HashSet<IWindow> _normalWindows = new();

	/// <summary>
	/// All the minimized windows in this workspace.
	/// </summary>
	private readonly HashSet<IWindow> _minimizedWindows = new();

	public IEnumerable<IWindow> Windows => _normalWindows.Concat(_minimizedWindows);

	/// <summary>
	/// Phantom windows are specific to a single layout engine.
	/// The intersection of <see cref="_normalWindows"/> and <see cref="_phantomWindows"/>
	/// is the empty set.
	/// </summary>
	private readonly Dictionary<IWindow, ILayoutEngine> _phantomWindows = new();

	/// <summary>
	/// Map of windows to their current location.
	/// </summary>
	private readonly Dictionary<IWindow, IWindowState> _windowLocations = new();

	public Workspace(
		IContext context,
		WorkspaceManagerTriggers triggers,
		string name,
		IEnumerable<ILayoutEngine> layoutEngines
	)
	{
		_context = context;
		_triggers = triggers;

		_name = name;
		_layoutEngines = layoutEngines.ToArray();

		if (_layoutEngines.Length == 0)
		{
			throw new ArgumentException("At least one layout engine must be provided.");
		}
	}

	public void Initialize()
	{
		if (_initialized)
		{
			Logger.Error($"Workspace {Name} has already been initialized.");
			return;
		}

		_initialized = true;

		// Apply the proxy layout engines
		foreach (ProxyLayoutEngine proxyLayout in _context.WorkspaceManager.ProxyLayoutEngines)
		{
			for (int i = 0; i < _layoutEngines.Length; i++)
			{
				_layoutEngines[i] = proxyLayout(_layoutEngines[i]);
			}
		}
	}

	public void WindowFocused(IWindow window)
	{
		if (
			_normalWindows.Contains(window)
			|| (
				_phantomWindows.TryGetValue(window, out ILayoutEngine? layoutEngine)
				&& layoutEngine != null
				&& ActiveLayoutEngine.ContainsEqual(layoutEngine)
			)
		)
		{
			LastFocusedWindow = window;
			Logger.Debug($"Focused window {window} in workspace {Name}");
		}
	}

	public void WindowMinimizeStart(IWindow window)
	{
		if (!_normalWindows.Contains(window))
		{
			Logger.Error($"Window {window} is not a normal window in workspace {Name}");
			return;
		}

		_normalWindows.Remove(window);
		_minimizedWindows.Add(window);

		foreach (ILayoutEngine layoutEngine in _layoutEngines)
		{
			layoutEngine.Remove(window);
		}

		DoLayout();
	}

	public void WindowMinimizeEnd(IWindow window)
	{
		if (!_minimizedWindows.Contains(window))
		{
			Logger.Error($"Window {window} is not a minimized window in workspace {Name}");
			return;
		}

		_minimizedWindows.Remove(window);
		AddWindow(window);
	}

	public void FocusFirstWindow()
	{
		Logger.Debug($"Focusing first window in workspace {Name}");
		ActiveLayoutEngine.GetFirstWindow()?.Focus();
	}

	private void UpdateLayoutEngine(int nextIdx)
	{
		int prevIdx = _activeLayoutEngineIndex;

		// If the LastFocusedWindow is a phantom window, remove it.
		// This is because phantom windows belong to a specific layout engine.
		if (LastFocusedWindow != null && _phantomWindows.ContainsKey(LastFocusedWindow))
		{
			LastFocusedWindow = null;
		}

		_layoutEngines[prevIdx].HidePhantomWindows();
		_activeLayoutEngineIndex = nextIdx;
		DoLayout();

		_triggers.ActiveLayoutEngineChanged(
			new ActiveLayoutEngineChangedEventArgs()
			{
				Workspace = this,
				PreviousLayoutEngine = _layoutEngines[prevIdx],
				CurrentLayoutEngine = _layoutEngines[_activeLayoutEngineIndex]
			}
		);
	}

	public void NextLayoutEngine()
	{
		Logger.Debug(Name);
		UpdateLayoutEngine((_activeLayoutEngineIndex + 1).Mod(_layoutEngines.Length));
	}

	public void PreviousLayoutEngine()
	{
		Logger.Debug(Name);
		UpdateLayoutEngine((_activeLayoutEngineIndex - 1).Mod(_layoutEngines.Length));
	}

	public bool TrySetLayoutEngine(string name)
	{
		Logger.Debug($"Trying to set layout engine {name} for workspace {Name}");

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

		UpdateLayoutEngine(nextIdx);
		return true;
	}

	public void AddWindow(IWindow window)
	{
		Logger.Debug($"Adding window {window} to workspace {Name}");

		if (_phantomWindows.ContainsKey(window))
		{
			Logger.Verbose($"Phantom window {window} is already in workspace {Name}");
			return;
		}

		if (_normalWindows.Contains(window))
		{
			Logger.Error($"Window {window} already exists in workspace {Name}");
			return;
		}

		_normalWindows.Add(window);
		foreach (ILayoutEngine layoutEngine in _layoutEngines)
		{
			layoutEngine.Add(window);
		}
		DoLayout();
		window.Focus();
	}

	public bool RemoveWindow(IWindow window)
	{
		Logger.Debug($"Removing window {window} from workspace {Name}");

		if (LastFocusedWindow == window)
		{
			LastFocusedWindow = null;
		}

		if (_phantomWindows.TryGetValue(window, out ILayoutEngine? phantomLayoutEngine))
		{
			bool removePhantomSuccess = phantomLayoutEngine.Remove(window);
			if (removePhantomSuccess)
			{
				_phantomWindows.Remove(window);
				DoLayout();
			}
			return removePhantomSuccess;
		}

		bool isNormalWindow = _normalWindows.Contains(window);
		if (!isNormalWindow && !_minimizedWindows.Contains(window))
		{
			Logger.Error($"Window {window} already does not exist in workspace {Name}");
			return false;
		}

		bool success = true;
		if (isNormalWindow)
		{
			foreach (ILayoutEngine layoutEngine in _layoutEngines)
			{
				if (!layoutEngine.Remove(window))
				{
					Logger.Error($"Window {window} could not be removed from layout engine {layoutEngine}");
					success = false;
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

		if (GetValidVisibleWindow(window) is IWindow validWindow)
		{
			ActiveLayoutEngine.FocusWindowInDirection(direction, validWindow);
		}
	}

	public void SwapWindowInDirection(Direction direction, IWindow? window = null)
	{
		Logger.Debug($"Swapping window {window} in workspace {Name} in direction {direction}");

		if (GetValidVisibleWindow(window) is IWindow validWindow)
		{
			ActiveLayoutEngine.SwapWindowInDirection(direction, validWindow);
			DoLayout();
		}
	}

	public void MoveWindowEdgesInDirection(Direction edges, IPoint<double> deltas, IWindow? window = null)
	{
		Logger.Debug($"Moving window {window} in workspace {Name} in direction {edges} by {deltas}");

		if (GetValidVisibleWindow(window) is IWindow validWindow)
		{
			ActiveLayoutEngine.MoveWindowEdgesInDirection(edges, deltas, validWindow);
			DoLayout();
		}
	}

	public void MoveWindowToPoint(IWindow window, IPoint<double> point)
	{
		Logger.Debug($"Moving window {window} to point {point} in workspace {Name}");

		if (_phantomWindows.ContainsKey(window))
		{
			return;
		}
		if (_normalWindows.Contains(window))
		{
			// The window is already in the workspace, so move it in just the active layout engine
			ActiveLayoutEngine.Remove(window);
			ActiveLayoutEngine.AddWindowAtPoint(window, point);
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

			foreach (ILayoutEngine layoutEngine in _layoutEngines)
			{
				layoutEngine.AddWindowAtPoint(window, point);
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

		foreach (IWindow window in _phantomWindows.Keys)
		{
			window.Hide();
		}

		_windowLocations.Clear();
	}

	public IWindowState? TryGetWindowLocation(IWindow window)
	{
		_windowLocations.TryGetValue(window, out IWindowState? location);

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

	public void DoLayout()
	{
		Logger.Debug($"Workspace {Name}");

		// Get the monitor for this workspace
		IMonitor? monitor = _context.WorkspaceManager.GetMonitorForWorkspace(this);
		if (monitor == null)
		{
			Logger.Debug($"No active monitors found for workspace {Name}.");
			return;
		}

		_triggers.WorkspaceLayoutStarted(new WorkspaceEventArgs() { Workspace = this });
		_windowLocations.Clear();

		Logger.Verbose($"Starting layout for workspace {Name} with layout engine {ActiveLayoutEngine.Name}");
		IEnumerable<IWindowState> locations = ActiveLayoutEngine.DoLayout(
			new Location<int>() { Width = monitor.WorkingArea.Width, Height = monitor.WorkingArea.Height },
			monitor
		);

		using (WindowDeferPosHandle handle = new(_context))
		{
			foreach (IWindowState loc in locations)
			{
				Logger.Verbose($"Setting location of window {loc.Window}");
				if (loc.Window.IsMouseMoving)
				{
					continue;
				}

				// Adjust the window location to the monitor's coordinates
				loc.Location = new Location<int>()
				{
					X = loc.Location.X + monitor.WorkingArea.X,
					Y = loc.Location.Y + monitor.WorkingArea.Y,
					Width = loc.Location.Width,
					Height = loc.Location.Height
				};

				Logger.Verbose($"{loc.Window} at {loc.Location}");
				handle.DeferWindowPos(loc);

				// Update the window location
				_windowLocations[loc.Window] = loc;
			}
			Logger.Verbose($"Layout for workspace {Name} complete");
		}

		foreach (IWindow window in _minimizedWindows)
		{
			window.ShowMinimized();
		}

		_triggers.WorkspaceLayoutCompleted(new WorkspaceEventArgs() { Workspace = this });
	}

	#region Phantom Windows
	public void AddPhantomWindow(ILayoutEngine engine, IWindow window)
	{
		Logger.Debug($"Adding phantom window {window} in workspace {Name}");

		if (!ActiveLayoutEngine.ContainsEqual(engine))
		{
			Logger.Error($"Layout engine {engine} is not active in workspace {Name}");
			return;
		}

		if (_phantomWindows.ContainsKey(window))
		{
			Logger.Error($"Phantom window {window} already exists in workspace {Name}");
			return;
		}

		_phantomWindows.Add(window, engine);
		_context.WorkspaceManager.AddPhantomWindow(this, window);
		DoLayout();
	}

	public void RemovePhantomWindow(ILayoutEngine engine, IWindow window)
	{
		Logger.Debug($"Removing phantom window {window} in workspace {Name}");

		if (!ActiveLayoutEngine.ContainsEqual(engine))
		{
			Logger.Error($"Layout engine {engine} is not active in workspace {Name}");
			return;
		}

		if (!_phantomWindows.TryGetValue(window, out ILayoutEngine? phantomEngine))
		{
			Logger.Error($"Phantom window {window} does not exist in workspace {Name}");
			return;
		}

		if (phantomEngine != engine)
		{
			Logger.Error($"Phantom window {window} does not belong to layout engine {engine} in workspace {Name}");
			return;
		}

		_phantomWindows.Remove(window);
		_context.WorkspaceManager.RemovePhantomWindow(window);

		DoLayout();
	}
	#endregion

	public bool ContainsWindow(IWindow window) =>
		_normalWindows.Contains(window)
		|| _minimizedWindows.Contains(window)
		|| (
			_phantomWindows.TryGetValue(window, out ILayoutEngine? phantomEngine)
			&& ActiveLayoutEngine.ContainsEqual(phantomEngine)
		);

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
