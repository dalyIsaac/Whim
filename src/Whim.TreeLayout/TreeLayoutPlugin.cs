using System.Collections.Generic;

namespace Whim.TreeLayout;

/// <inheritdoc/>
public class TreeLayoutPlugin : ITreeLayoutPlugin
{
	private readonly IContext _context;

	/// <inheritdoc/>
	public string Name => "whim.tree_layout";

	/// <summary>
	/// Initializes a new instance of the <see cref="TreeLayoutPlugin"/> class.
	/// </summary>
	public TreeLayoutPlugin(IContext context)
	{
		_context = context;
	}

	/// <inheritdoc />
	public void PreInitialize() { }

	/// <inheritdoc />
	public void PostInitialize() { }

	/// <inheritdoc />
	public IEnumerable<CommandItem> Commands => new TreeLayoutCommands(this);

	/// <inheritdoc/>
	public ITreeLayoutEngine? GetTreeLayoutEngine()
	{
		ILayoutEngine rootEngine = _context.WorkspaceManager.ActiveWorkspace.ActiveLayoutEngine;
		return rootEngine.GetLayoutEngine<ITreeLayoutEngine>();
	}

	/// <inheritdoc />
	public void SetAddWindowDirection(Direction direction)
	{
		ITreeLayoutEngine? engine = GetTreeLayoutEngine();
		if (engine != null)
		{
			engine.AddNodeDirection = direction;
		}
	}
}
