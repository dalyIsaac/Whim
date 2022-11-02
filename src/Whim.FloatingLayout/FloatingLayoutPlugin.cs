using System.Collections.Generic;

namespace Whim.FloatingLayout;

/// <inheritdoc />
public class FloatingLayoutPlugin : IFloatingLayoutPlugin
{
	private readonly IConfigContext _configContext;
	private readonly FloatingLayoutConfig _floatingLayoutConfig;

	/// <inheritdoc />
	public string Name => "whim.floating_layout";

	/// <summary>
	/// Creates a new instance of the floating layout plugin.
	/// </summary>
	/// <param name="configContext"></param>
	/// <param name="floatingLayoutConfig"></param>
	public FloatingLayoutPlugin(IConfigContext configContext, FloatingLayoutConfig? floatingLayoutConfig = null)
	{
		_configContext = configContext;
		_floatingLayoutConfig = floatingLayoutConfig ?? new FloatingLayoutConfig();
	}

	/// <inheritdoc />
	public void PreInitialize()
	{
		_configContext.WorkspaceManager.AddProxyLayoutEngine(layout => new FloatingLayoutEngine(_configContext, _floatingLayoutConfig, layout));
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
		ILayoutEngine rootEngine = _configContext.WorkspaceManager.ActiveWorkspace.ActiveLayoutEngine;
		FloatingLayoutEngine? floatingLayoutEngine = rootEngine.GetLayoutEngine<FloatingLayoutEngine>();
		if (floatingLayoutEngine == null)
		{
			Logger.Error("Could not find floating layout engine");
			return;
		}

		floatingLayoutEngine.MarkWindowAsFloating(window);
		_configContext.WorkspaceManager.ActiveWorkspace.DoLayout();
	}

	/// <summary>
	/// Update the floating window location.
	/// </summary>
	/// <param name="window"></param>
	public void MarkWindowAsDocked(IWindow? window = null)
	{
		// Get the currently active floating layout engine.
		ILayoutEngine rootEngine = _configContext.WorkspaceManager.ActiveWorkspace.ActiveLayoutEngine;
		FloatingLayoutEngine? floatingLayoutEngine = rootEngine.GetLayoutEngine<FloatingLayoutEngine>();
		if (floatingLayoutEngine == null)
		{
			Logger.Error("Could not find floating layout engine");
			return;
		}

		floatingLayoutEngine.MarkWindowAsDocked(window);
		_configContext.WorkspaceManager.ActiveWorkspace.DoLayout();
	}

	/// <summary>
	/// Toggle the floating state of the given <paramref name="window"/>.
	/// </summary>
	/// <param name="window"></param>
	public void ToggleWindowFloating(IWindow? window = null)
	{
		// Get the currently active floating layout engine.
		ILayoutEngine rootEngine = _configContext.WorkspaceManager.ActiveWorkspace.ActiveLayoutEngine;
		FloatingLayoutEngine? floatingLayoutEngine = rootEngine.GetLayoutEngine<FloatingLayoutEngine>();
		if (floatingLayoutEngine == null)
		{
			Logger.Error("Could not find floating layout engine");
			return;
		}

		floatingLayoutEngine.ToggleWindowFloating(window);
		_configContext.WorkspaceManager.ActiveWorkspace.DoLayout();
	}
}
