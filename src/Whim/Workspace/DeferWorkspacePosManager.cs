using System.Collections.Generic;
using Windows.Win32.Foundation;

namespace Whim;

internal class DeferWorkspacePosManager : IDeferWorkspacePosManager
{
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;

	public DeferWorkspacePosManager(IContext context, IInternalContext internalContext)
	{
		_context = context;
		_internalContext = internalContext;
	}

	public void DoLayout(
		IWorkspace workspace,
		WorkspaceManagerTriggers triggers,
		Dictionary<HWND, IWindowState> windowStates
	)
	{
		Logger.Debug($"Layout {workspace}");

		if (GarbageCollect(workspace))
		{
			Logger.Debug($"Garbage collected windows, skipping layout for workspace {workspace}");
			return;
		}

		// Get the monitor for this workspace
		if (!_context.Store.Pick(Pickers.GetMonitorForWorkspace(workspace)).TryGet(out IMonitor monitor))
		{
			Logger.Debug($"No active monitors found for workspace {workspace}.");
			return;
		}

		Logger.Debug($"Starting layout for workspace {workspace}");
		triggers.WorkspaceLayoutStarted(new WorkspaceEventArgs() { Workspace = workspace });

		// Execute the layout task, and update the window states before the completed event.
		SetWindowPos(workspace, monitor, windowStates);

		triggers.WorkspaceLayoutCompleted(new WorkspaceEventArgs() { Workspace = workspace });
	}

	private void SetWindowPos(IWorkspace workspace, IMonitor monitor, Dictionary<HWND, IWindowState> windowStates)
	{
		Logger.Debug($"Setting window positions for workspace {workspace}");

		List<WindowPosState> windowStatesList = new();
		windowStates.Clear();

		foreach (IWindowState loc in workspace.ActiveLayoutEngine.DoLayout(monitor.WorkingArea, monitor))
		{
			windowStatesList.Add(new(loc, (HWND)1, null));
			windowStates.Add(loc.Window.Handle, loc);
			Logger.Debug($"Window {loc.Window} has rectangle {loc.Rectangle}");
		}

		using DeferWindowPosHandle handle = _context.NativeManager.DeferWindowPos(windowStatesList);
	}

	/// <summary>
	/// Garbage collects windows that are no longer valid.
	/// </summary>
	/// <returns></returns>
	private bool GarbageCollect(IWorkspace workspace)
	{
		List<IWindow> garbageWindows = new();
		bool garbageCollected = false;

		foreach (IWindow window in workspace.Windows)
		{
			bool removeWindow = false;
			if (!_internalContext.CoreNativeManager.IsWindow(window.Handle))
			{
				Logger.Debug($"Window {window.Handle} is no longer a window.");
				removeWindow = true;
			}
			else if (_context.Store.Pick(new TryGetWindowPicker(window.Handle)).IsSuccessful == false)
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
			_context.Store.Dispatch(new WindowRemovedTransform(window));
		}

		return garbageCollected;
	}
}
