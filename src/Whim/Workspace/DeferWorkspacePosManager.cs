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

	public Dictionary<HWND, IWindowState>? DoLayout(IWorkspace workspace, WorkspaceManagerTriggers triggers)
	{
		Logger.Debug($"Layout {workspace}");

		if (GarbageCollect(workspace))
		{
			Logger.Debug($"Garbage collected windows, skipping layout for workspace {workspace}");
			return null;
		}

		// Get the monitor for this workspace
		IMonitor? monitor = _context.Butler.GetMonitorForWorkspace(workspace);
		if (monitor == null)
		{
			Logger.Debug($"No active monitors found for workspace {workspace}.");
			return null;
		}

		Logger.Debug($"Starting layout for workspace {workspace}");
		triggers.WorkspaceLayoutStarted(new WorkspaceEventArgs() { Workspace = workspace });

		// Execute the layout task
		Dictionary<HWND, IWindowState> windowStates = SetWindowPos(workspace, monitor);
		triggers.WorkspaceLayoutCompleted(new WorkspaceEventArgs() { Workspace = workspace });

		return windowStates;
	}

	private Dictionary<HWND, IWindowState> SetWindowPos(IWorkspace workspace, IMonitor monitor)
	{
		Logger.Debug($"Setting window positions for workspace {workspace}");

		List<WindowPosState> windowStates = new();
		Dictionary<HWND, IWindowState> windowRects = new();

		foreach (IWindowState loc in workspace.ActiveLayoutEngine.DoLayout(monitor.WorkingArea, monitor))
		{
			windowStates.Add(new(loc, (HWND)1, null));
			windowRects.Add(loc.Window.Handle, loc);
			Logger.Debug($"Window {loc.Window} has rectangle {loc.Rectangle}");
		}

		using DeferWindowPosHandle handle = _context.NativeManager.DeferWindowPos(windowStates);

		return windowRects;
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
}
