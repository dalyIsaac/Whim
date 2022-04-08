namespace Whim.Gaps;

public class GapsPlugin : IPlugin
{
	private readonly IConfigContext _configContext;
	private readonly GapsConfig _gapsConfig;

	public GapsPlugin(IConfigContext configContext, GapsConfig gapsConfig)
	{
		_configContext = configContext;
		_gapsConfig = gapsConfig;
	}

	public void PreInitialize()
	{
		_configContext.WorkspaceManager.AddProxyLayoutEngine(layout => new GapsLayoutEngine(_gapsConfig, layout));
	}

	public void PostInitialize() { }

	public void UpdateOuterGap(int delta)
	{
		_gapsConfig.OuterGap += delta;
		_configContext.WorkspaceManager.LayoutAllActiveWorkspaces();
	}

	public void UpdateInnerGap(int delta)
	{
		_gapsConfig.InnerGap += delta;
		_configContext.WorkspaceManager.LayoutAllActiveWorkspaces();
	}
}
