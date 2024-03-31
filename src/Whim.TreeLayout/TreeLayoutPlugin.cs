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

	/// <summary>
	/// <c>whim.tree_layout</c>
	/// </summary>
	public string Name => "whim.tree_layout";

	/// <inheritdoc/>
	public IPluginCommands PluginCommands => new TreeLayoutCommands(_context, this);

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
	public void PreInitialize() { }

	/// <inheritdoc	/>
	public void PostInitialize() { }

	/// <inheritdoc />
	public void Subscribe() { }

	/// <inheritdoc />
	public Direction? GetAddWindowDirection(IMonitor monitor)
	{
		ILayoutEngine? layoutEngine = _context.Butler.Pantry.GetWorkspaceForMonitor(monitor)?.ActiveLayoutEngine;
		if (layoutEngine?.GetLayoutEngine<TreeLayoutEngine>() is not TreeLayoutEngine treeLayoutEngine)
		{
			return null;
		}

		return GetAddWindowDirection(treeLayoutEngine);
	}

	/// <inheritdoc />
	public Direction GetAddWindowDirection(TreeLayoutEngine engine)
	{
		if (!_addNodeDirections.TryGetValue(engine.Identity, out Direction direction))
		{
			// Lazy initialization
			_addNodeDirections[engine.Identity] = direction = DefaultAddNodeDirection;
		}

		return direction;
	}

	/// <inheritdoc />
	public void SetAddWindowDirection(IMonitor monitor, Direction direction)
	{
		ILayoutEngine? layoutEngine = _context.Butler.Pantry.GetWorkspaceForMonitor(monitor)?.ActiveLayoutEngine;
		if (layoutEngine?.GetLayoutEngine<TreeLayoutEngine>() is not TreeLayoutEngine treeLayoutEngine)
		{
			return;
		}

		SetAddWindowDirection(treeLayoutEngine, direction);
	}

	/// <inheritdoc />
	public void SetAddWindowDirection(TreeLayoutEngine engine, Direction direction)
	{
		Direction previousDirection = _addNodeDirections.TryGetValue(engine.Identity, out Direction previous)
			? previous
			: DefaultAddNodeDirection;

		_addNodeDirections[engine.Identity] = direction;
		AddWindowDirectionChanged?.Invoke(
			this,
			new AddWindowDirectionChangedEventArgs()
			{
				CurrentDirection = direction,
				PreviousDirection = previousDirection,
				TreeLayoutEngine = engine
			}
		);
	}

	/// <inheritdoc />
	public void LoadState(JsonElement state) { }

	/// <inheritdoc />
	public JsonElement? SaveState() => null;
}
