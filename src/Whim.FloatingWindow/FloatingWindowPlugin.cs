using System.Collections.Generic;
using System.Text.Json;

namespace Whim.FloatingWindow;

/// <inheritdoc />
/// <summary>
/// Creates a new instance of the floating window plugin.
/// </summary>
/// <param name="context"></param>
public class FloatingWindowPlugin(IContext context) : IFloatingWindowPlugin, IInternalFloatingWindowPlugin
{
	private readonly IContext _context = context;

	/// <summary>
	/// <c>whim.floating_window</c>
	/// </summary>
	public string Name => "whim.floating_window";

	private readonly Dictionary<IWindow, ISet<LayoutEngineIdentity>> _floatingWindows = [];

	/// <inheritdoc/>
	public IReadOnlyDictionary<IWindow, ISet<LayoutEngineIdentity>> FloatingWindows => _floatingWindows;

	/// <inheritdoc />
	public void PreInitialize()
	{
		_context.WorkspaceManager.AddProxyLayoutEngine(layout => new ProxyFloatingLayoutEngine(_context, this, layout));
		_context.WindowManager.WindowRemoved += WindowManager_WindowRemoved;
	}

	/// <inheritdoc />
	public void PostInitialize() { }

	/// <inheritdoc />
	public IPluginCommands PluginCommands => new FloatingWindowCommands(this);

	private void WindowManager_WindowRemoved(object? sender, WindowEventArgs e) => _floatingWindows.Remove(e.Window);

	private void UpdateWindow(IWindow? window, bool markAsFloating)
	{
		window ??= _context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;

		if (window == null)
		{
			Logger.Error("Could not find window");
			return;
		}

		if (!markAsFloating && !FloatingWindows.ContainsKey(window))
		{
			Logger.Debug($"Window {window} is not floating");
			return;
		}

		if (_context.Butler.Pantry.GetWorkspaceForWindow(window) is not IWorkspace workspace)
		{
			Logger.Error($"Window {window} is not in a workspace");
			return;
		}

		if (workspace.TryGetWindowState(window) is not IWindowState windowState)
		{
			Logger.Error($"Could not get window state for window {window}");
			return;
		}

		LayoutEngineIdentity layoutEngineIdentity = workspace.ActiveLayoutEngine.Identity;
		ISet<LayoutEngineIdentity> layoutEngines = FloatingWindows.TryGetValue(
			window,
			out ISet<LayoutEngineIdentity>? existingLayoutEngines
		)
			? existingLayoutEngines
			: new HashSet<LayoutEngineIdentity>();

		if (markAsFloating)
		{
			Logger.Debug($"Marking window {window} as floating");
			layoutEngines.Add(layoutEngineIdentity);
		}
		else
		{
			Logger.Debug($"Marking window {window} as docked");
			layoutEngines.Remove(layoutEngineIdentity);
		}

		if (layoutEngines.Count == 0)
		{
			_floatingWindows.Remove(window);
		}
		else
		{
			_floatingWindows[window] = layoutEngines;
		}

		// Convert the rectangle to a unit square rectangle.
		IMonitor monitor = _context.MonitorManager.GetMonitorAtPoint(windowState.Rectangle);
		IRectangle<double> unitSquareRect = monitor.WorkingArea.NormalizeRectangle(windowState.Rectangle);

		workspace.MoveWindowToPoint(window, unitSquareRect);
	}

	/// <inheritdoc />
	public void MarkWindowAsDockedInLayoutEngine(IWindow window, LayoutEngineIdentity layoutEngineIdentity)
	{
		if (_floatingWindows.TryGetValue(window, out ISet<LayoutEngineIdentity>? layoutEngines))
		{
			layoutEngines.Remove(layoutEngineIdentity);

			if (layoutEngines.Count == 0)
			{
				_floatingWindows.Remove(window);
			}
		}
	}

	/// <inheritdoc />
	public void MarkWindowAsFloating(IWindow? window = null) => UpdateWindow(window, true);

	/// <inheritdoc />
	public void MarkWindowAsDocked(IWindow? window = null) => UpdateWindow(window, false);

	/// <inheritdoc />
	public void ToggleWindowFloating(IWindow? window = null)
	{
		window ??= _context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;
		if (window == null)
		{
			Logger.Error("Could not find window");
			return;
		}

		if (FloatingWindows.ContainsKey(window))
		{
			MarkWindowAsDocked(window);
		}
		else
		{
			MarkWindowAsFloating(window);
		}
	}

	/// <inheritdoc />
	public void LoadState(JsonElement pluginSavedState) { }

	/// <inheritdoc />
	public JsonElement? SaveState() => null;
}
