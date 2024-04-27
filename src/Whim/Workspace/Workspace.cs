using System;
using System.Collections.Generic;
using System.Linq;
using DotNext;
using Windows.Win32.Foundation;

namespace Whim;

internal class Workspace : IWorkspace, IInternalWorkspace
{
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;

	public Guid Id { get; }

	public string Name
	{
		get => _context.Store.Pick(new GetWorkspaceByIdPicker(Id))!.Value.Name;
		set => _context.Store.Dispatch(new SetWorkspaceNameTransform(Id, value));
	}

	private bool _disposedValue;

	private ImmutableWorkspace GetThis() => _context.Store.Pick(new GetWorkspaceByIdPicker(Id))!.Value;

	public IWindow? LastFocusedWindow => _context.Store.Pick(new GetLastFocusedWindowPicker(GetThis())).Value!;

	public ILayoutEngine ActiveLayoutEngine => _context.Store.Pick(new GetActiveLayoutEnginePicker(GetThis()));

	public IEnumerable<IWindow> Windows => _context.Store.Pick(new GetAllWorkspaceWindowsPicker(GetThis())).Value!;

	/// <summary>
	/// Map of window handles to their current <see cref="IWindowState"/>.
	/// </summary>
	private readonly Dictionary<HWND, IWindowState> _windowStates = new();

	public Workspace(IContext context, IInternalContext internalContext, Guid id)
	{
		_context = context;
		_internalContext = internalContext;
		Id = id;
	}

	public void WindowFocused(IWindow? window)
	{
		if (window != null && Windows.Contains(window))
		{
			_context.Store.Dispatch(new SetLastFocusedWindowTransform(GetThis(), window));
		}
	}

	public void MinimizeWindowStart(IWindow window)
	{
		Logger.Debug($"Minimizing window {window} in workspace {Name}");

		// If the window is already in the workspace, minimize it in just the active layout engine.
		// If it isn't, then we assume it was provided during startup and minimize it in all layouts.
		if (_windows.Contains(window))
		{
			_layoutEngines[_activeLayoutEngineIndex] = _layoutEngines[_activeLayoutEngineIndex]
				.MinimizeWindowStart(window);
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
			if (_context.Store.Pick(Pickers.GetMonitorForWorkspace(this)).TryGet(out IMonitor? monitor))
			{
				Logger.Debug($"No active monitors found for workspace {Name}.");
				return;
			}

			_context.Store.Dispatch(new FocusMonitorDesktopTransform(monitor));
		}
	}

	public bool TrySetLayoutEngineFromIndex(int nextIdx) =>
		_context.Store.Dispatch(new ActivateLayoutEngineTransform(GetThis(), (_, idx) => idx == nextIdx)).IsSuccessful;

	public void CycleLayoutEngine(bool reverse = false) =>
		_context.Store.Dispatch(new CycleLayoutEngineTransform(GetThis(), reverse));

	public void ActivatePreviouslyActiveLayoutEngine()
	{
		ImmutableWorkspace workspace = GetThis();

		_context.Store.Dispatch(
			new ActivateLayoutEngineTransform(workspace, (_, idx) => idx == workspace.PreviousLayoutEngineIndex)
		);
	}

	public bool TrySetLayoutEngineFromName(string name) =>
		_context.Store.Dispatch(new ActivateLayoutEngineTransform(GetThis(), (l, _) => l.Name == name)).IsSuccessful;

	public bool AddWindow(IWindow window) =>
		_context.Store.Dispatch(new AddWindowToWorkspaceTransform(GetThis(), window)).IsSuccessful;

	public bool RemoveWindow(IWindow window) =>
		_context.Store.Dispatch(new RemoveWindowFromWorkspaceTransform(GetThis(), window)).IsSuccessful;

	public bool FocusWindowInDirection(Direction direction, IWindow? window = null, bool deferLayout = false)
	{
		Result<bool> result = _context.Store.Dispatch(
			new FocusWindowInDirectionTransform(GetThis(), window, direction)
		);
		return result.IsSuccessful && result.TryGet(out bool isChanged) && isChanged;
	}

	public bool SwapWindowInDirection(Direction direction, IWindow? window = null, bool deferLayout = false)
	{
		Result<bool> result = _context.Store.Dispatch(new SwapWindowInDirectionTransform(GetThis(), window, direction));
		return result.IsSuccessful && result.TryGet(out bool isChanged) && isChanged;
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

		if (!deferLayout)
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
			_layoutEngines[_activeLayoutEngineIndex] = _layoutEngines[_activeLayoutEngineIndex]
				.MoveWindowToPoint(window, point);
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
		if (!deferLayout)
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
				bool isWorkspaceActive = _context.Store.Pick(Pickers.GetMonitorForWorkspace(this)).IsSuccessful;

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
