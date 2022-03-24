namespace Whim.FloatingLayout;

public class FloatingLayoutPlugin : IPlugin
{
	private readonly IConfigContext _configContext;
	private readonly FloatingLayoutConfig _floatingLayoutConfig;

	public FloatingLayoutPlugin(IConfigContext configContext, FloatingLayoutConfig? floatingLayoutConfig = null)
	{
		_configContext = configContext;
		_floatingLayoutConfig = floatingLayoutConfig ?? new FloatingLayoutConfig();
		_configContext.WorkspaceManager.AddProxyLayoutEngine(layout => new FloatingLayoutEngine(_configContext, _floatingLayoutConfig, layout));
	}

	public void Initialize() { }

	public void MarkWindowAsFloating(IWindow? window = null)
	{
		// Get the currently active floating layout engine.
		ILayoutEngine rootEngine = _configContext.WorkspaceManager.ActiveWorkspace.ActiveLayoutEngine;
		FloatingLayoutEngine? floatingLayoutEngine = ILayoutEngine.GetLayoutEngine<FloatingLayoutEngine>(rootEngine);
		if (floatingLayoutEngine == null)
		{
			Logger.Error("Could not find floating layout engine");
			return;
		}

		floatingLayoutEngine.MarkWindowAsFloating(window);
		_configContext.WorkspaceManager.ActiveWorkspace.DoLayout();
	}

	public void MarkWindowAsDocked(IWindow? window = null)
	{
		// Get the currently active floating layout engine.
		ILayoutEngine rootEngine = _configContext.WorkspaceManager.ActiveWorkspace.ActiveLayoutEngine;
		FloatingLayoutEngine? floatingLayoutEngine = ILayoutEngine.GetLayoutEngine<FloatingLayoutEngine>(rootEngine);
		if (floatingLayoutEngine == null)
		{
			Logger.Error("Could not find floating layout engine");
			return;
		}

		floatingLayoutEngine.MarkWindowAsDocked(window);
		_configContext.WorkspaceManager.ActiveWorkspace.DoLayout();
	}

	public void ToggleWindowFloating(IWindow? window = null)
	{
		// Get the currently active floating layout engine.
		ILayoutEngine rootEngine = _configContext.WorkspaceManager.ActiveWorkspace.ActiveLayoutEngine;
		FloatingLayoutEngine? floatingLayoutEngine = ILayoutEngine.GetLayoutEngine<FloatingLayoutEngine>(rootEngine);
		if (floatingLayoutEngine == null)
		{
			Logger.Error("Could not find floating layout engine");
			return;
		}

		floatingLayoutEngine.ToggleWindowFloating(window);
		_configContext.WorkspaceManager.ActiveWorkspace.DoLayout();
	}
}
