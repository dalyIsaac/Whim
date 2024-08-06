using System.Text.Json;

namespace Whim.Gaps;

/// <inheritdoc />
/// <summary>
/// Creates a new instance of the gaps plugin.
/// </summary>
/// <param name="context"></param>
/// <param name="gapsConfig"></param>
public class GapsPlugin(IContext context, GapsConfig gapsConfig) : IGapsPlugin
{
	private readonly IContext _context = context;

	/// <summary>
	/// The configuration for the gaps plugin.
	/// </summary>
	public GapsConfig GapsConfig { get; } = gapsConfig;

	/// <summary>
	/// <c>whim.gaps</c>
	/// </summary>
	public string Name => "whim.gaps";

	/// <inheritdoc />
	public void PreInitialize()
	{
		_context.WorkspaceManager.AddProxyLayoutEngine(layout => new GapsLayoutEngine(GapsConfig, layout));
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
		_context.Butler.LayoutAllActiveWorkspaces();
	}

	/// <summary>
	/// Update the gap between windows by the <paramref name="delta"/>.
	/// </summary>
	/// <param name="delta"></param>
	public void UpdateInnerGap(int delta)
	{
		GapsConfig.InnerGap += delta;
		_context.Butler.LayoutAllActiveWorkspaces();
	}

	/// <inheritdoc />
	public IPluginCommands PluginCommands => new GapsCommands(this);

	/// <inheritdoc />
	public void LoadState(JsonElement state) { }

	/// <inheritdoc />
	public JsonElement? SaveState() => null;
}
