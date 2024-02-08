using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Win32.Foundation;

namespace Whim;

internal class Workspace : IWorkspace, IInternalWorkspace
{
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

	public IWindow? LastFocusedWindow { get; private set; }

	protected readonly ILayoutEngine[] _layoutEngines;
	private int _prevLayoutEngineIndex;
	private int _activeLayoutEngineIndex;
	private bool _disposedValue;

	// NOTE: Don't set this directly. Usually all the layout engines will need to be updated.
	public ILayoutEngine ActiveLayoutEngine => _layoutEngines[_activeLayoutEngineIndex];

	/// <summary>
	/// All the windows in this workspace.
	/// </summary>
	private readonly HashSet<IWindow> _windows = new();

	public IEnumerable<IWindow> Windows => _windows;

	/// <summary>
	/// Map of window handles to their current <see cref="IWindowState"/>.
	/// </summary>
	private readonly Dictionary<HWND, IWindowState> _windowStates = new();

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
		if (window != null && _windows.Contains(window))
		{
			LastFocusedWindow = window;
			Logger.Debug($"Focused window {window} in workspace {Name}");
		}
	}

	public void MinimizeWindowStart(IWindow window)
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

	public void MinimizeWindowEnd(IWindow window)
	{
		Logger.Debug($"Minimizing window {window} in workspace {Name}");
		_windows.Add(window);

		// Restore in just the active layout engine. MinimizeWindowEnd is not called as part of
		// Whim starting up.
		_layoutEngines[_activeLayoutEngineIndex] = _layoutEngines[_activeLayoutEngineIndex].MinimizeWindowEnd(window);
	}

	public void FocusLastFocusedWindow()
	{
		Logger.Debug($"Focusing last focused window in workspace {Name}");
		if (LastFocusedWindow != null && !LastFocusedWindow.IsMinimized)
		{
			LastFocusedWindow.Focus();
		}
		else
		{
			Logger.Debug($"No windows in workspace {Name} to focus, focusing desktop");

			// Get the bounds of the monitor for this workspace.
			IMonitor? monitor = _context.Butler.GetMonitorForWorkspace(this);
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

	public bool TrySetLayoutEngineFromIndex(int nextIdx)
	{
		Logger.Debug($"Trying to set layout engine with index {nextIdx} for workspace");

		ILayoutEngine prevLayoutEngine;
		ILayoutEngine nextLayoutEngine;

		if (nextIdx >= _layoutEngines.Length)
		{
			Logger.Error($"Index {nextIdx} exceeds number of layout engines for workspace");
			return false;
		}
		if (nextIdx < 0)
		{
			Logger.Error($"Index {nextIdx} is negative");
			return false;
		}

		if (_activeLayoutEngineIndex == nextIdx)
		{
			Logger.Debug($"Layout engine with index {nextIdx} is already active for workspace");
			return true;
		}

		_prevLayoutEngineIndex = _activeLayoutEngineIndex;
		_activeLayoutEngineIndex = nextIdx;

		prevLayoutEngine = _layoutEngines[_prevLayoutEngineIndex];
		nextLayoutEngine = _layoutEngines[_activeLayoutEngineIndex];

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

	public void CycleLayoutEngine(bool reverse = false)
	{
		Logger.Debug($"Cycling layout engine on workspace {Name}");

		int delta = reverse ? -1 : 1;

		int nextIdx;

		nextIdx = (_activeLayoutEngineIndex + delta).Mod(_layoutEngines.Length);

		TrySetLayoutEngineFromIndex(nextIdx);
	}

	public void ActivatePreviouslyActiveLayoutEngine() => TrySetLayoutEngineFromIndex(_prevLayoutEngineIndex);

	public bool TrySetLayoutEngineFromName(string name)
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

		TrySetLayoutEngineFromIndex(nextIdx);
		return true;
	}

	public bool AddWindow(IWindow window)
	{
		Logger.Debug($"Adding window {window} to workspace {Name}");

		if (_windows.Contains(window))
		{
			Logger.Error($"Window {window} already exists in workspace {Name}");
			return false;
		}

		_windows.Add(window);
		for (int i = 0; i < _layoutEngines.Length; i++)
		{
			_layoutEngines[i] = _layoutEngines[i].AddWindow(window);
		}
		return true;
	}

	public bool RemoveWindow(IWindow window)
	{
		bool success;
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

				if (!nextWindow.IsMinimized)
				{
					LastFocusedWindow = nextWindow;
					break;
				}
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

		if (!_windows.Contains(window))
		{
			Logger.Error($"Window {window} does not exist in workspace {Name}");
			return null;
		}

		return window;
	}

	public bool FocusWindowInDirection(Direction direction, IWindow? window = null, bool deferLayout = false)
	{
		Logger.Debug($"Focusing window {window} in workspace {Name}");

		if (GetValidVisibleWindow(window) is not IWindow validWindow)
		{
			return false;
		}

		ILayoutEngine oldEngine = ActiveLayoutEngine;
		_layoutEngines[_activeLayoutEngineIndex] = ActiveLayoutEngine.FocusWindowInDirection(direction, validWindow);
		bool changed = ActiveLayoutEngine != oldEngine;

		if (changed && !deferLayout)
		{
			DoLayout();
		}

		return changed;
	}

	public bool SwapWindowInDirection(Direction direction, IWindow? window = null, bool deferLayout = false)
	{
		Logger.Debug($"Swapping window {window} in workspace {Name} in direction {direction}");

		if (GetValidVisibleWindow(window) is not IWindow validWindow)
		{
			return false;
		}

		ILayoutEngine newEngine = ActiveLayoutEngine.SwapWindowInDirection(direction, validWindow);
		bool changed = ActiveLayoutEngine != newEngine;
		_layoutEngines[_activeLayoutEngineIndex] = newEngine;

		if (changed && !deferLayout)
		{
			DoLayout();
		}

		return changed;
	}

	public bool MoveWindowEdgesInDirection(
		Direction edges,
		IPoint<double> deltas,
		IWindow? window = null,
		bool deferLayout = false
	)
	{
		Logger.Debug($"Moving window {window} in workspace {Name} in direction {edges} by {deltas}");
		if (GetValidVisibleWindow(window) is not IWindow validWindow)
		{
			return false;
		}

		ILayoutEngine newEngine = ActiveLayoutEngine.MoveWindowEdgesInDirection(edges, deltas, validWindow);
		bool changed = ActiveLayoutEngine != newEngine;
		_layoutEngines[_activeLayoutEngineIndex] = newEngine;

		if (changed && !deferLayout)
		{
			DoLayout();
		}

		return changed;
	}

	public bool MoveWindowToPoint(IWindow window, IPoint<double> point, bool deferLayout = false)
	{
		Logger.Debug($"Moving window {window} to point {point} in workspace {Name}");

		ILayoutEngine oldEngine = ActiveLayoutEngine;

		if (_windows.Contains(window))
		{
			// The window is already in the workspace, so move it in just the active layout engine
			_layoutEngines[_activeLayoutEngineIndex] = _layoutEngines[_activeLayoutEngineIndex].MoveWindowToPoint(
				window,
				point
			);
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

		bool changed = ActiveLayoutEngine != oldEngine;
		if (changed && !deferLayout)
		{
			DoLayout();
		}

		return changed;
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

		_internalContext.DeferWorkspacePosManager.DoLayout(this, _triggers, _windowStates);
	}

	public bool ContainsWindow(IWindow window) => _windows.Contains(window);

	public bool PerformCustomLayoutEngineAction(LayoutEngineCustomAction action) =>
		PerformCustomLayoutEngineAction(
			new LayoutEngineCustomAction<IWindow?>()
			{
				Name = action.Name,
				Payload = action.Window,
				Window = action.Window
			}
		);

	public bool PerformCustomLayoutEngineAction<T>(LayoutEngineCustomAction<T> action)
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

		if (doLayout && !action.DeferLayout)
		{
			DoLayout();
		}

		return doLayout;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				Logger.Debug($"Disposing workspace {Name}");

				// dispose managed state (managed objects)
				bool isWorkspaceActive = _context.Butler.GetMonitorForWorkspace(this) != null;

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
