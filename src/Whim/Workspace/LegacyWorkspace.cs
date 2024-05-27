using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Win32.Foundation;

namespace Whim;

internal partial record Workspace : IInternalWorkspace
{
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;

	/// <summary>
	/// The latest instance of <see cref="IWorkspace"/> for the given <see cref="WorkspaceId"/>.
	/// This is used for compatibility with the old <see cref="Workspace"/> implementation.
	/// </summary>
	private Workspace LatestWorkspace => _context.Store.Pick(Pickers.PickWorkspaceById(Id))!.Value!;

	public string Name
	{
		get => LatestWorkspace.Name;
		set => _context.Store.Dispatch(new SetWorkspaceNameTransform(Id, value));
	}

	private bool _disposedValue;

	public IWindow? LastFocusedWindow => _context.Store.Pick(Pickers.PickLastFocusedWindow(Id)).Value!;

	public ILayoutEngine ActiveLayoutEngine => _context.Store.Pick(Pickers.PickActiveLayoutEngine(Id)).Value!;

	public IEnumerable<IWindow> Windows => _context.Store.Pick(Pickers.PickAllWindowsInWorkspace(Id)).Value!;

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
			_context.Store.Dispatch(new SetLastFocusedWindowTransform(Id, window.Handle));
		}
	}

	public void MinimizeWindowStart(IWindow window) =>
		_context.Store.Dispatch(new MinimizeWindowStartTransform(Id, window.Handle));

	public void MinimizeWindowEnd(IWindow window) =>
		_context.Store.Dispatch(new MinimizeWindowEndTransform(Id, window.Handle));

	public void FocusLastFocusedWindow() => _context.Store.Dispatch(new FocusWindowTransform(Id));

	public bool TrySetLayoutEngineFromIndex(int nextIdx) =>
		_context.Store.Dispatch(new ActivateLayoutEngineTransform(Id, (_, idx) => idx == nextIdx)).IsSuccessful;

	public void CycleLayoutEngine(bool reverse = false) =>
		_context.Store.Dispatch(new CycleLayoutEngineTransform(Id, reverse));

	public void ActivatePreviouslyActiveLayoutEngine() =>
		_context.Store.Dispatch(
			new ActivateLayoutEngineTransform(Id, (_, idx) => idx == LatestWorkspace.PreviousLayoutEngineIndex)
		);

	public bool TrySetLayoutEngineFromName(string name) =>
		_context.Store.Dispatch(new ActivateLayoutEngineTransform(Id, (l, _) => l.Name == name)).IsSuccessful;

	public bool AddWindow(IWindow window) =>
		_context.Store.Dispatch(new AddWindowToWorkspaceTransform(Id, window)).IsSuccessful;

	public bool RemoveWindow(IWindow window) =>
		_context.Store.Dispatch(new RemoveWindowFromWorkspaceTransform(Id, window.Handle)).IsSuccessful;

	public bool FocusWindowInDirection(Direction direction, IWindow? window = null, bool deferLayout = false) =>
		_context
			.Store.Dispatch(new FocusWindowInDirectionTransform(Id, window?.Handle ?? default, direction))
			.TryGet(out bool isChanged) && isChanged;

	public bool SwapWindowInDirection(Direction direction, IWindow? window = null, bool deferLayout = false) =>
		_context
			.Store.Dispatch(new SwapWindowInDirectionTransform(Id, window?.Handle ?? default, direction))
			.TryGet(out bool isChanged) && isChanged;

	public bool MoveWindowEdgesInDirection(
		Direction edges,
		IPoint<double> deltas,
		IWindow? window = null,
		bool deferLayout = false
	) =>
		_context
			.Store.Dispatch(
				new MoveWindowEdgesInDirectionWorkspaceTransform(Id, edges, deltas, window?.Handle ?? default)
			)
			.TryGet(out bool isChanged) && isChanged;

	public bool MoveWindowToPoint(IWindow window, IPoint<double> point, bool deferLayout = false) =>
		_context
			.Store.Dispatch(new MoveWindowToPointInWorkspaceTransform(Id, window.Handle, point))
			.TryGet(out bool isChanged) && isChanged;

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

	public bool ContainsWindow(IWindow window) => LatestWorkspace.WindowHandles.Contains(window.Handle);

	public bool PerformCustomLayoutEngineAction(LayoutEngineCustomAction action) =>
		_context.Store.Dispatch(new PerformCustomLayoutEngineActionTransform(Id, action)).TryGet(out bool isChanged)
		&& isChanged;

	public bool PerformCustomLayoutEngineAction<T>(LayoutEngineCustomAction<T> action) =>
		_context
			.Store.Dispatch(new PerformCustomLayoutEnginePayloadActionTransform<T>(Id, action))
			.TryGet(out bool isChanged) && isChanged;

	// TODO: Remove entirely
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
