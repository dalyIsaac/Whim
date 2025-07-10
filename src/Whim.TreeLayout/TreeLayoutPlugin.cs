using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Whim.TreeLayout;

/// <inheritdoc/>
/// <summary>
/// Initializes a new instance of the <see cref="TreeLayoutPlugin"/> class.
/// </summary>
public class TreeLayoutPlugin : ITreeLayoutPlugin
{
	private readonly IContext _ctx;

	/// <summary>
	/// Creates a new instance of the <see cref="TreeLayoutPlugin"/> class.
	/// </summary>
	/// <param name="context"></param>
	[SuppressMessage("Style", "IDE0290:Use primary constructor")]
	public TreeLayoutPlugin(IContext context)
	{
		_ctx = context;
	}

	private readonly Dictionary<LayoutEngineIdentity, Direction> _addNodeDirections = [];
	private const Direction DefaultAddNodeDirection = Direction.Right;

	/// <summary>
	/// <c>whim.tree_layout</c>
	/// </summary>
	public string Name => "whim.tree_layout";

	/// <inheritdoc/>
	public IPluginCommands PluginCommands => new TreeLayoutCommands(_ctx, this);

	/// <inheritdoc	/>
	public event EventHandler<AddWindowDirectionChangedEventArgs>? AddWindowDirectionChanged;

	/// <inheritdoc	/>
	public void PreInitialize() { }

	/// <inheritdoc	/>
	public void PostInitialize() { }

	/// <inheritdoc />
	public Direction? GetAddWindowDirection(IMonitor monitor)
	{
		if (
			!_ctx
				.Store.Pick(Pickers.PickActiveLayoutEngineByMonitor(monitor.Handle))
				.TryGet(out ILayoutEngine? layoutEngine)
		)
		{
			return null;
		}

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
		if (
			!_ctx
				.Store.Pick(Pickers.PickActiveLayoutEngineByMonitor(monitor.Handle))
				.TryGet(out ILayoutEngine? layoutEngine)
		)
		{
			return;
		}

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

		_ctx.NativeManager.TryEnqueue(() =>
			AddWindowDirectionChanged?.Invoke(
				this,
				new AddWindowDirectionChangedEventArgs()
				{
					CurrentDirection = direction,
					PreviousDirection = previousDirection,
					TreeLayoutEngine = engine,
				}
			)
		);
	}

	/// <inheritdoc />
	public void LoadState(JsonElement state) { }

	/// <inheritdoc />
	public JsonElement? SaveState() => null;
}
