using System;
using System.Collections.Generic;

namespace Whim.TreeLayout;

/// <inheritdoc/>
public class TreeLayoutPlugin : ITreeLayoutPlugin
{
	private readonly IContext _context;

	/// <inheritdoc/>
	public string Name => "whim.tree_layout";

	/// <inheritdoc/>
	public event EventHandler<AddWindowDirectionChangedEventArgs>? AddWindowDirectionChanged;

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
	public IEnumerable<CommandItem> Commands => new TreeLayoutCommands(_context, this);

	private ITreeLayoutEngine? GetTreeLayoutEngine(IMonitor monitor)
	{
		IWorkspace? workspace = _context.WorkspaceManager.GetWorkspaceForMonitor(monitor);
		return workspace?.ActiveLayoutEngine.GetLayoutEngine<ITreeLayoutEngine>();
	}

	/// <inheritdoc />
	public void SetAddWindowDirection(IMonitor monitor, Direction direction)
	{
		if (GetTreeLayoutEngine(monitor) is TreeLayoutEngine treeLayoutEngine)
		{
			treeLayoutEngine.AddNodeDirection = direction;
			AddWindowDirectionChanged?.Invoke(
				this,
				new AddWindowDirectionChangedEventArgs()
				{
					TreeLayoutEngine = treeLayoutEngine,
					CurrentDirection = direction,
					PreviousDirection = treeLayoutEngine.AddNodeDirection
				}
			);
		}
	}

	/// <inheritdoc />
	public Direction? GetAddWindowDirection(IMonitor monitor) =>
		GetTreeLayoutEngine(monitor) is TreeLayoutEngine treeLayoutEngine ? treeLayoutEngine.AddNodeDirection : null;

	/// <inheritdoc />
	public void SplitFocusedWindow()
	{
		IMonitor monitor = _context.MonitorManager.FocusedMonitor;
		if (GetTreeLayoutEngine(monitor) is TreeLayoutEngine treeLayoutEngine)
		{
			treeLayoutEngine.SplitFocusedWindow();
		}
	}
}
