namespace Whim.Gaps;

/// <summary>
/// GapsPlugin is a plugin to add gaps between windows in the layout.
/// </summary>
public class GapsPlugin : IPlugin
{
	private readonly IConfigContext _configContext;
	private readonly GapsConfig _gapsConfig;

	/// <summary>
	/// Creates a new instance of the gaps plugin.
	/// </summary>
	/// <param name="configContext"></param>
	/// <param name="gapsConfig"></param>
	public GapsPlugin(IConfigContext configContext, GapsConfig gapsConfig)
	{
		_configContext = configContext;
		_gapsConfig = gapsConfig;
	}

	/// <inheritdoc />
	public void PreInitialize()
	{
		_configContext.WorkspaceManager.AddProxyLayoutEngine(layout => new GapsLayoutEngine(_gapsConfig, layout));
	}

	/// <inheritdoc />
	public void PostInitialize() { }

	/// <summary>
	/// Update the gap between the parent layout engine and the area where windows are placed by
	/// the <paramref name="delta"/>.
	/// </summary>
	/// <param name="delta"></param>
	public void UpdateOuterGap(int delta)
	{
		_gapsConfig.OuterGap += delta;
		_configContext.WorkspaceManager.LayoutAllActiveWorkspaces();
	}

	/// <summary>
	/// Update the gap between windows by the <paramref name="delta"/>.
	/// </summary>
	/// <param name="delta"></param>
	public void UpdateInnerGap(int delta)
	{
		_gapsConfig.InnerGap += delta;
		_configContext.WorkspaceManager.LayoutAllActiveWorkspaces();
	}
}
