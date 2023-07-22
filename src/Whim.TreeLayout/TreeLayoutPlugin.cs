using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Whim.TreeLayout;

/// <inheritdoc/>
public class TreeLayoutPlugin : ITreeLayoutPlugin
{
	private readonly IContext _context;

	/// <inheritdoc/>
	public string Name => "whim.tree_layout";

	/// <inheritdoc/>
	public IPluginCommands PluginCommands => new TreeLayoutCommands(_context, this);

	private readonly HashSet<IWindow> _phantomWindows = new();

	/// <inheritdoc/>
	public IReadOnlySet<IWindow> PhantomWindows => _phantomWindows;

	/// <inheritdoc	/>
	public event EventHandler<AddWindowDirectionChangedEventArgs>? AddWindowDirectionChanged;

	/// <summary>
	/// Initializes a new instance of the <see cref="TreeLayoutPlugin"/> class.
	/// </summary>
	public TreeLayoutPlugin(IContext context)
	{
		_context = context;
	}

	/// <inheritdoc	/>
	public void PreInitialize()
	{
		_context.WindowManager.WindowRemoved += WindowManager_WindowRemoved;
	}

	/// <inheritdoc	/>
	public void PostInitialize() { }

	private void WindowManager_WindowRemoved(object? sender, WindowEventArgs e)
	{
		if (_phantomWindows.Contains(e.Window))
		{
			_phantomWindows.Remove(e.Window);
		}
	}

	// TODO
	// private TreeLayoutEngine? GetTreeLayoutEngine(IMonitor monitor)
	// {
	// 	IWorkspace? workspace = _context.WorkspaceManager.GetWorkspaceForMonitor(monitor);
	// 	return workspace?.ActiveLayoutEngine.GetLayoutEngine<TreeLayoutEngine>();
	// }

	/// <inheritdoc />
	public Direction? GetAddWindowDirection(IMonitor monitor) =>
		// GetTreeLayoutEngine(monitor) is TreeLayoutEngine treeLayoutEngine ? treeLayoutEngine.AddNodeDirection : null;
		throw new NotImplementedException();

	/// <inheritdoc />
	public void SplitFocusedWindow()
		// {
		// TODO: Add phantom window to _phantomWindows, and use the TreeLayoutEngine.Add method.
		// TODO: Call AddPhantomWindow and RemovePhantomWindow.
		=>
		throw new NotImplementedException();

	/// <inheritdoc />
	public void SetAddWindowDirection(IMonitor monitor, Direction direction)
	{
		throw new NotImplementedException();
		// if (GetTreeLayoutEngine(monitor) is TreeLayoutEngine treeLayoutEngine)
		// {
		// 	treeLayoutEngine.AddNodeDirection = direction;
		// 	AddWindowDirectionChanged?.Invoke(
		// 		this,
		// 		new AddWindowDirectionChangedEventArgs()
		// 		{
		// 			TreeLayoutEngine = treeLayoutEngine,
		// 			CurrentDirection = direction,
		// 			PreviousDirection = treeLayoutEngine.AddNodeDirection
		// 		}
		// 	);
		// }
	}

	/// <inheritdoc />
	public void LoadState(JsonElement state) { }

	/// <inheritdoc />
	public JsonElement? SaveState() => null;
}
