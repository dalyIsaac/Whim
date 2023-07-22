using System.Collections.Generic;
using System.Text.Json;

namespace Whim.FloatingLayout;

/// <inheritdoc />
public class FloatingLayoutPlugin : IFloatingLayoutPlugin, IInternalFloatingLayoutPlugin
{
	private readonly IContext _context;

	/// <inheritdoc />
	public string Name => "whim.floating_layout";

	/// <inheritdoc />
	public ISet<IWindow> MutableFloatingWindows { get; } = new HashSet<IWindow>();

	/// <inheritdoc />
	public IReadOnlySet<IWindow> FloatingWindows => (IReadOnlySet<IWindow>)MutableFloatingWindows;

	/// <summary>
	/// Creates a new instance of the floating layout plugin.
	/// </summary>
	/// <param name="context"></param>
	public FloatingLayoutPlugin(IContext context)
	{
		_context = context;
	}

	/// <inheritdoc />
	public void PreInitialize()
	{
		_context.WorkspaceManager.AddProxyLayoutEngine(layout => new FloatingLayoutEngine(_context, this, layout));
	}

	/// <inheritdoc />
	public void PostInitialize() { }

	/// <inheritdoc />
	public IPluginCommands PluginCommands => new FloatingLayoutCommands(this);

	private void UpdateWindow(IWindow? window, bool markAsFloating)
	{
		window ??= _context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;
		if (window == null)
		{
			Logger.Error("Could not find window");
			return;
		}

		if (_context.WorkspaceManager.GetWorkspaceForWindow(window) is not IWorkspace workspace)
		{
			Logger.Error($"Window {window} is not in a workspace");
			return;
		}

		if (workspace.TryGetWindowLocation(window) is not IWindowState windowState)
		{
			Logger.Error($"Could not get location for window {window}");
			return;
		}

		if (markAsFloating)
		{
			Logger.Debug($"Marking window {window} as floating");
			MutableFloatingWindows.Add(window);
		}
		else
		{
			Logger.Debug($"Marking window {window} as docked");
			MutableFloatingWindows.Remove(window);
		}

		// Convert the location to a unit square location.
		IMonitor monitor = _context.MonitorManager.GetMonitorAtPoint(windowState.Location);
		ILocation<double> unitSquareLocation = monitor.WorkingArea.ToUnitSquare(windowState.Location);

		workspace.MoveWindowToPoint(window, unitSquareLocation);
	}

	/// <summary>
	/// Mark the given <paramref name="window"/> as a floating window
	/// </summary>
	/// <param name="window"></param>
	public void MarkWindowAsFloating(IWindow? window = null) => UpdateWindow(window, true);

	/// <summary>
	/// Update the floating window location.
	/// </summary>
	/// <param name="window"></param>
	public void MarkWindowAsDocked(IWindow? window = null) => UpdateWindow(window, false);

	/// <summary>
	/// Toggle the floating state of the given <paramref name="window"/>.
	/// </summary>
	/// <param name="window"></param>
	public void ToggleWindowFloating(IWindow? window = null)
	{
		window ??= _context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;
		if (window == null)
		{
			Logger.Error("Could not find window");
			return;
		}

		if (MutableFloatingWindows.Contains(window))
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
