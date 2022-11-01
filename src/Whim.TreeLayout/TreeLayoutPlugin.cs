using System.Collections.Generic;

namespace Whim.TreeLayout;

/// <inheritdoc/>
public class TreeLayoutPlugin : ITreeLayoutPlugin
{
	private readonly IConfigContext _configContext;

	/// <inheritdoc/>
	public string Name => "whim.tree_layout";

	/// <summary>
	/// Initializes a new instance of the <see cref="TreeLayoutPlugin"/> class.
	/// </summary>
	public TreeLayoutPlugin(IConfigContext configContext)
	{
		_configContext = configContext;
	}

	/// <inheritdoc />
	public void PreInitialize() { }

	/// <inheritdoc />
	public void PostInitialize() { }

	/// <inheritdoc />
	public IEnumerable<CommandItem> Commands => new TreeLayoutCommands(this);

	/// <inheritdoc/>
	public TreeLayoutEngine? GetTreeLayoutEngine()
	{
		ILayoutEngine rootEngine = _configContext.WorkspaceManager.ActiveWorkspace.ActiveLayoutEngine;
		return ILayoutEngine.GetLayoutEngine<TreeLayoutEngine>(rootEngine);
	}

	/// <inheritdoc />
	public void SetAddWindowDirection(Direction direction)
	{
		TreeLayoutEngine? engine = GetTreeLayoutEngine();
		if (engine != null)
		{
			engine.AddNodeDirection = direction;
		}
	}
}
