using System.Collections.Generic;

namespace Whim.FloatingLayout;

public class FloatingLayoutEngine : BaseProxyLayoutEngine
{
	private readonly IConfigContext _configContext;
	private readonly FloatingLayoutConfig _floatingLayoutConfig;

	public FloatingLayoutEngine(IConfigContext configContext, FloatingLayoutConfig floatingLayoutConfig, ILayoutEngine innerLayoutEngine) : base(innerLayoutEngine)
	{
		_configContext = configContext;
		_floatingLayoutConfig = floatingLayoutConfig;
	}

	public override IEnumerable<IWindowLocation> DoLayout(ILocation<int> location)
	{
		foreach (IWindowLocation loc in _innerLayoutEngine.DoLayout(location))
		{
			yield return new WindowLocation(
				window: loc.Window,
				location: loc.Location,
				windowState: loc.WindowState
			);
		}

		foreach (IWindowLocation windowLocation in _floatingLayoutConfig.GetWindows(_configContext.WorkspaceManager.ActiveWorkspace))
		{
			yield return windowLocation;
		}
	}
}
