using System.Text.Json;

namespace Whim.Gaps;

/// <inheritdoc />
public class GapsPlugin : IGapsPlugin
{
	private readonly IContext _context;

	/// <summary>
	/// The configuration for the gaps plugin.
	/// </summary>
	public GapsConfig GapsConfig { get; }

	/// <inheritdoc />
	public string Name => "whim.gaps";

	/// <summary>
	/// Creates a new instance of the gaps plugin.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="gapsConfig"></param>
	public GapsPlugin(IContext context, GapsConfig gapsConfig)
	{
		_context = context;
		GapsConfig = gapsConfig;
	}

	/// <inheritdoc />
	public void PreInitialize()
	{
		_context.WorkspaceManager.AddProxyLayoutEngine(layout => new ImmutableGapsLayoutEngine(GapsConfig, layout));
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
		GapsConfig.OuterGap += delta;
		_context.WorkspaceManager.LayoutAllActiveWorkspaces();
	}

	/// <summary>
	/// Update the gap between windows by the <paramref name="delta"/>.
	/// </summary>
	/// <param name="delta"></param>
	public void UpdateInnerGap(int delta)
	{
		GapsConfig.InnerGap += delta;
		_context.WorkspaceManager.LayoutAllActiveWorkspaces();
	}

	/// <inheritdoc />
	public IPluginCommands PluginCommands => new GapsCommands(this);

	/// <inheritdoc />
	public void LoadState(JsonElement state) { }

	/// <inheritdoc />
	public JsonElement? SaveState() => null;
}
