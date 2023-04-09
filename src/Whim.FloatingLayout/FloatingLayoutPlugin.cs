using System.Collections.Generic;

namespace Whim.FloatingLayout;

/// <inheritdoc />
public class FloatingLayoutPlugin : IFloatingLayoutPlugin
{
	private readonly IContext _context;
	private readonly FloatingLayoutConfig _floatingLayoutConfig;

	/// <inheritdoc />
	public string Name => "whim.floating_layout";

	/// <summary>
	/// Creates a new instance of the floating layout plugin.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="floatingLayoutConfig"></param>
	public FloatingLayoutPlugin(IContext context, FloatingLayoutConfig? floatingLayoutConfig = null)
	{
		_context = context;
		_floatingLayoutConfig = floatingLayoutConfig ?? new FloatingLayoutConfig();
	}

	/// <inheritdoc />
	public void PreInitialize()
	{
		_context.WorkspaceManager.AddProxyLayoutEngine(
			layout => new FloatingLayoutEngine(_context, _floatingLayoutConfig, layout)
		);
	}

	/// <inheritdoc />
	public void PostInitialize() { }

	/// <inheritdoc />
	public IEnumerable<CommandItem> Commands => new FloatingLayoutCommands(this);

	/// <summary>
	/// Mark the given <paramref name="window"/> as a floating window
	/// </summary>
	/// <param name="window"></param>
	public void MarkWindowAsFloating(IWindow? window = null)
	{
		// Get the currently active floating layout engine.
		ILayoutEngine rootEngine = _context.WorkspaceManager.ActiveWorkspace.ActiveLayoutEngine;
		FloatingLayoutEngine? floatingLayoutEngine = rootEngine.GetLayoutEngine<FloatingLayoutEngine>();
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
		FloatingLayoutEngine? floatingLayoutEngine = rootEngine.GetLayoutEngine<FloatingLayoutEngine>();
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
		FloatingLayoutEngine? floatingLayoutEngine = rootEngine.GetLayoutEngine<FloatingLayoutEngine>();
		if (floatingLayoutEngine == null)
		{
			Logger.Error("Could not find floating layout engine");
			return;
		}

		floatingLayoutEngine.ToggleWindowFloating(window);
		_context.WorkspaceManager.ActiveWorkspace.DoLayout();
	}
}
