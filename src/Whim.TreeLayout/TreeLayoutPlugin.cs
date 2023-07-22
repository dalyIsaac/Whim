using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Whim.TreeLayout;

/// <inheritdoc/>
public class TreeLayoutPlugin : ITreeLayoutPlugin
{
	private readonly IContext _context;

	private readonly Dictionary<LayoutEngineIdentity, Direction> _addNodeDirections = new();
	private const Direction DefaultAddNodeDirection = Direction.Right;

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

	/// <inheritdoc />
	public Direction? GetAddWindowDirection(IMonitor monitor)
	{
		ILayoutEngine? layoutEngine = _context.WorkspaceManager.GetWorkspaceForMonitor(monitor)?.ActiveLayoutEngine;
		if (layoutEngine is not ILayoutEngine treeLayoutEngine)
		{
			return null;
		}

		if (!_addNodeDirections.TryGetValue(treeLayoutEngine.Identity, out Direction direction))
		{
			// Lazy initialization
			_addNodeDirections[treeLayoutEngine.Identity] = direction = DefaultAddNodeDirection;
		}

		return direction;
	}

	/// <inheritdoc />
	public void SplitFocusedWindow()
	{
		IWorkspace? workspace = _context.WorkspaceManager.ActiveWorkspace;

		PhantomWindow phantomWindow = new(_context);
		_phantomWindows.Add(phantomWindow.Window);

		_context.WorkspaceManager.AddPhantomWindow(workspace, phantomWindow.Window);
	}

	/// <inheritdoc />
	public void SetAddWindowDirection(IMonitor monitor, Direction direction)
	{
		ILayoutEngine? layoutEngine = _context.WorkspaceManager.GetWorkspaceForMonitor(monitor)?.ActiveLayoutEngine;
		if (layoutEngine is not ILayoutEngine treeLayoutEngine)
		{
			return;
		}

		Direction previousDirection = _addNodeDirections.TryGetValue(treeLayoutEngine.Identity, out Direction previous)
			? previous
			: DefaultAddNodeDirection;

		_addNodeDirections[treeLayoutEngine.Identity] = direction;
		AddWindowDirectionChanged?.Invoke(
			this,
			new AddWindowDirectionChangedEventArgs()
			{
				CurrentDirection = direction,
				PreviousDirection = previousDirection,
				TreeLayoutEngine = treeLayoutEngine
			}
		);
	}

	/// <inheritdoc />
	public void LoadState(JsonElement state) { }

	/// <inheritdoc />
	public JsonElement? SaveState() => null;
}
