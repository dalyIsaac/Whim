using System;
using System.Text.Json;

namespace Whim.FloatingLayout;

/// <inheritdoc />
public class FloatingLayoutPlugin : IFloatingLayoutPlugin, IDisposable
{
	private readonly IContext _context;
	private bool disposedValue;

	/// <inheritdoc />
	public string Name => "whim.floating_layout";

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
		_context.WindowManager.WindowMoved += WindowManager_WindowMoved;
		_context.WorkspaceManager.AddProxyLayoutEngine(layout => new FloatingLayoutEngine(_context, layout));
	}

	/// <inheritdoc />
	public void PostInitialize() { }

	/// <inheritdoc />
	public IPluginCommands PluginCommands => new FloatingLayoutCommands(this);

	private void WindowManager_WindowMoved(object? sender, WindowEventArgs e)
	{
		IWorkspace? workspace = _context.WorkspaceManager.GetWorkspaceForWindow(e.Window);
		if (workspace == null)
		{
			Logger.Error($"Could not find workspace for window {e.Window}");
			return;
		}

		ILayoutEngine rootEngine = workspace.ActiveLayoutEngine;
		IFloatingLayoutEngine? floatingLayoutEngine = rootEngine.GetLayoutEngine<IFloatingLayoutEngine>();
		if (floatingLayoutEngine == null)
		{
			Logger.Debug("Could not find floating layout engine");
			return;
		}

		floatingLayoutEngine.UpdateWindowLocation(e.Window);
	}

	/// <summary>
	/// Mark the given <paramref name="window"/> as a floating window
	/// </summary>
	/// <param name="window"></param>
	public void MarkWindowAsFloating(IWindow? window = null)
	{
		// Get the currently active floating layout engine.
		ILayoutEngine rootEngine = _context.WorkspaceManager.ActiveWorkspace.ActiveLayoutEngine;
		IFloatingLayoutEngine? floatingLayoutEngine = rootEngine.GetLayoutEngine<IFloatingLayoutEngine>();
		if (floatingLayoutEngine == null)
		{
			Logger.Error("Could not find floating layout engine");
			return;
		}

		floatingLayoutEngine.MarkWindowAsFloating(window);
		_context.WorkspaceManager.ActiveWorkspace.DoLayout();
	}

	/// <summary>
	/// Update the floating window location.
	/// </summary>
	/// <param name="window"></param>
	public void MarkWindowAsDocked(IWindow? window = null)
	{
		// Get the currently active floating layout engine.
		ILayoutEngine rootEngine = _context.WorkspaceManager.ActiveWorkspace.ActiveLayoutEngine;
		IFloatingLayoutEngine? floatingLayoutEngine = rootEngine.GetLayoutEngine<IFloatingLayoutEngine>();
		if (floatingLayoutEngine == null)
		{
			Logger.Error("Could not find floating layout engine");
			return;
		}

		floatingLayoutEngine.MarkWindowAsDocked(window);
		_context.WorkspaceManager.ActiveWorkspace.DoLayout();
	}

	/// <summary>
	/// Toggle the floating state of the given <paramref name="window"/>.
	/// </summary>
	/// <param name="window"></param>
	public void ToggleWindowFloating(IWindow? window = null)
	{
		// Get the currently active floating layout engine.
		ILayoutEngine rootEngine = _context.WorkspaceManager.ActiveWorkspace.ActiveLayoutEngine;
		IFloatingLayoutEngine? floatingLayoutEngine = rootEngine.GetLayoutEngine<IFloatingLayoutEngine>();
		if (floatingLayoutEngine == null)
		{
			Logger.Error("Could not find floating layout engine");
			return;
		}

		floatingLayoutEngine.ToggleWindowFloating(window);
		_context.WorkspaceManager.ActiveWorkspace.DoLayout();
	}

	/// <inheritdoc />
	public void LoadState(JsonElement pluginSavedState) { }

	/// <inheritdoc />
	public JsonElement? SaveState() => null;

	/// <inheritdoc />
	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_context.WindowManager.WindowMoved -= WindowManager_WindowMoved;
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			disposedValue = true;
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
