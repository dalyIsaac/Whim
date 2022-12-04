using System;
using System.Collections.Generic;
using System.Linq;

namespace Whim;

internal class Workspace : IWorkspace
{
	private readonly IConfigContext _configContext;

	private string _name;
	public string Name
	{
		get => _name;
		set
		{
			string oldName = _name;
			_name = value;
			(_configContext.WorkspaceManager as WorkspaceManager)?.TriggerWorkspaceRenamed(
				new WorkspaceRenamedEventArgs()
				{
					Workspace = this,
					OldName = oldName,
					NewName = _name
				}
			);
		}
	}

	/// <summary>
	/// The last focused window in this workspace.
	/// </summary>
	public IWindow? LastFocusedWindow { get; private set; }

	private readonly List<ILayoutEngine> _layoutEngines = new();
	private int _activeLayoutEngineIndex;
	private bool _disposedValue;

	public ILayoutEngine ActiveLayoutEngine => _layoutEngines[_activeLayoutEngineIndex];

	/// <summary>
	/// All the windows in this workspace which are common to every layout engine.
	/// The intersection of <see cref="_windows"/> and <see cref="_phantomWindows"/>
	/// is the empty set.
	/// </summary>
	private readonly HashSet<IWindow> _windows = new();
	public IEnumerable<IWindow> Windows => _windows;

	/// <summary>
	/// Phantom windows are specific to a single layout engine.
	/// The intersection of <see cref="_windows"/> and <see cref="_phantomWindows"/>
	/// is the empty set.
	/// </summary>
	private readonly Dictionary<IWindow, ILayoutEngine> _phantomWindows = new();

	/// <summary>
	/// Map of windows to their current location.
	/// </summary>
	private readonly Dictionary<IWindow, IWindowState> _windowLocations = new();

	public void DoLayout()
	{
		Logger.Debug($"Workspace {Name}");

		// Get the monitor for this workspace
		IMonitor? monitor = _configContext.WorkspaceManager.GetMonitorForWorkspace(this);
		if (monitor == null)
		{
			Logger.Debug($"No active monitors found for workspace {Name}.");
			return;
		}

		// Ensure there's at least one layout engine
		if (_layoutEngines.Count == 0)
		{
			Logger.Fatal($"No layout engines found for workspace {Name}");
			throw new InvalidOperationException("No layout engines found for workspace " + Name);
		}

		_windowLocations.Clear();

		Logger.Verbose($"Starting layout for workspace {Name} with layout engine {ActiveLayoutEngine.Name}");
		IEnumerable<IWindowState> locations = ActiveLayoutEngine.DoLayout(
			new Location<int>()
			{
				X = 0,
				Y = 0,
				Width = monitor.Width,
				Height = monitor.Height
			},
			monitor
		);

		using WindowDeferPosHandle handle = new(Windows.Count());
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
				X = loc.Location.X + monitor.X,
				Y = loc.Location.Y + monitor.Y,
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

	public Workspace(IConfigContext configContext, string name, params ILayoutEngine[] layoutEngines)
	{
		_configContext = configContext;
		_name = name;

		if (layoutEngines.Length == 0)
		{
			throw new ArgumentException("At least one layout engine must be provided.");
		}

		_layoutEngines = layoutEngines.ToList();
	}

	public void Initialize()
	{
		// Apply the proxy layout engines
		foreach (ProxyLayoutEngine proxyLayout in _configContext.WorkspaceManager.ProxyLayoutEngines)
		{
			for (int i = 0; i < _layoutEngines.Count; i++)
			{
				_layoutEngines[i] = proxyLayout(_layoutEngines[i]);
			}
		}
	}

	/// <summary>
	/// Called when a window is focused, regardless of whether it's in this workspace.
	/// </summary>
	/// <param name="window"></param>
	internal void WindowFocused(IWindow window)
	{
		if (
			_windows.Contains(window)
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

	public void FocusFirstWindow()
	{
		Logger.Debug($"Focusing first window in workspace {Name}");
		ActiveLayoutEngine.GetFirstWindow()?.Focus();
	}

	private void UpdateLayoutEngine(int delta)
	{
		int prevIdx = _activeLayoutEngineIndex;
		_activeLayoutEngineIndex = (_activeLayoutEngineIndex + delta).Mod(_layoutEngines.Count);

		// If the LastFocusedWindow is a phantom window, remove it.
		// This is because phantom windows belong to a specific layout engine.
		if (LastFocusedWindow != null && _phantomWindows.ContainsKey(LastFocusedWindow))
		{
			LastFocusedWindow = null;
		}

		_configContext.WorkspaceManager.TriggerActiveLayoutEngineChanged(
			new ActiveLayoutEngineChangedEventArgs()
			{
				Workspace = this,
				PreviousLayoutEngine = _layoutEngines[prevIdx],
				CurrentLayoutEngine = _layoutEngines[_activeLayoutEngineIndex]
			}
		);

		_layoutEngines[prevIdx].HidePhantomWindows();
		DoLayout();
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

		int prevIdx = -1;
		for (int idx = 0; idx < _layoutEngines.Count; idx++)
		{
			ILayoutEngine engine = _layoutEngines[idx];
			if (engine.Name == name)
			{
				prevIdx = _activeLayoutEngineIndex;
				_activeLayoutEngineIndex = idx;
				break;
			}
		}

		if (prevIdx == -1)
		{
			Logger.Error($"Layout engine {name} not found for workspace {Name}");
			return false;
		}
		else if (_activeLayoutEngineIndex == prevIdx)
		{
			Logger.Debug($"Layout engine {name} is already active for workspace {Name}");
			return true;
		}

		_configContext.WorkspaceManager.TriggerActiveLayoutEngineChanged(
			new ActiveLayoutEngineChangedEventArgs()
			{
				Workspace = this,
				PreviousLayoutEngine = _layoutEngines[prevIdx],
				CurrentLayoutEngine = _layoutEngines[_activeLayoutEngineIndex]
			}
		);

		DoLayout();
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

		if (_windows.Contains(window))
		{
			Logger.Error($"Window {window} already exists in workspace {Name}");
			return;
		}

		_windows.Add(window);
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

		if (_phantomWindows.ContainsKey(window))
		{
			bool removePhantomSuccess = RemovePhantomWindow(window);
			DoLayout();
			return removePhantomSuccess;
		}

		if (!_windows.Contains(window))
		{
			Logger.Error($"Window {window} already does not exist in workspace {Name}");
			return false;
		}

		bool success = true;
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
			_windows.Remove(window);
			DoLayout();
		}

		return success;
	}

	private bool RemovePhantomWindow(IWindow window)
	{
		Logger.Debug($"Removing phantom window {window} from workspace {Name}");

		if (!_phantomWindows.TryGetValue(window, out ILayoutEngine? layoutEngine))
		{
			Logger.Error($"Phantom window {window} does not exist in workspace {Name}");
			return false;
		}

		if (!ActiveLayoutEngine.ContainsEqual(layoutEngine))
		{
			Logger.Error($"Phantom window {window} is not in the active layout engine {ActiveLayoutEngine}");
			return false;
		}

		if (!layoutEngine.Remove(window))
		{
			Logger.Error($"Phantom window {window} could not be removed from layout engine {layoutEngine}");
			return false;
		}

		return true;
	}

	public void FocusWindowInDirection(Direction direction, IWindow window)
	{
		Logger.Debug($"Focusing window {window} in workspace {Name}");

		if (!ContainsWindow(window))
		{
			Logger.Error($"Window {window} does not exist in workspace {Name}");
			return;
		}

		ActiveLayoutEngine.FocusWindowInDirection(direction, window);
	}

	public void SwapWindowInDirection(Direction direction, IWindow? window = null)
	{
		window ??= LastFocusedWindow;
		if (window == null)
		{
			Logger.Error($"No window to swap in workspace {Name}");
			return;
		}

		Logger.Debug($"Swapping window {window} in workspace {Name} in direction {direction}");

		if (!ContainsWindow(window))
		{
			Logger.Error($"Window {window} does not exist in workspace {Name}");
			return;
		}

		ActiveLayoutEngine.SwapWindowInDirection(direction, window);
		DoLayout();
	}

	public void MoveWindowEdgeInDirection(Direction edge, double delta, IWindow? window = null)
	{
		window ??= LastFocusedWindow;
		if (window == null)
		{
			Logger.Error($"No window to move in workspace {Name}");
			return;
		}

		Logger.Debug($"Moving window {window} in workspace {Name} in direction {edge} by {delta}");

		if (!ContainsWindow(window))
		{
			Logger.Error($"Window {window} does not exist in workspace {Name}");
			return;
		}

		ActiveLayoutEngine.MoveWindowEdgeInDirection(edge, delta, window);
		DoLayout();
	}

	public void MoveWindowToPoint(IWindow window, IPoint<double> point, bool isPhantom)
	{
		Logger.Debug($"Moving window {window} to point {point} in workspace {Name}");

		// Double check isPhantom.
		if (_phantomWindows.ContainsKey(window) && !isPhantom)
		{
			Logger.Error($"Window {window} is a phantom window but is not being moved to a phantom point");
			return;
		}

		_windows.Add(window);

		foreach (ILayoutEngine layoutEngine in _layoutEngines)
		{
			layoutEngine.AddWindowAtPoint(window, point, isPhantom);
		}
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
		_windowLocations.TryGetValue(window, out IWindowState? location);
		return location;
	}

	#region Phantom Windows
	public void AddPhantomWindow(ILayoutEngine engine, IWindow window)
	{
		Logger.Debug($"Adding phantom window {window} in workspace {Name}");

		if (engine.ContainsEqual(ActiveLayoutEngine))
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
		_configContext.WorkspaceManager.AddPhantomWindow(this, window);
		DoLayout();
	}

	public void RemovePhantomWindow(ILayoutEngine engine, IWindow window, bool doLayout = false)
	{
		Logger.Debug($"Removing phantom window {window} in workspace {Name}");

		if (engine.ContainsEqual(ActiveLayoutEngine))
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
		_configContext.WorkspaceManager.RemovePhantomWindow(window);

		if (doLayout)
		{
			DoLayout();
		}
	}
	#endregion

	/// <summary>
	/// Returns true when the workspace contains the provided <paramref name="window"/>.
	/// </summary>
	/// <param name="window">The window to check for.</param>
	/// <returns>True when the workspace contains the provided <paramref name="window"/>.</returns>
	public bool ContainsWindow(IWindow window) =>
		_windows.Contains(window)
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
				bool isWorkspaceActive = _configContext.WorkspaceManager.GetMonitorForWorkspace(this) != null;

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
