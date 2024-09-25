using Corvus.Json;
using Whim.SliceLayout;
using Whim.TreeLayout;

namespace Whim.Yaml;

internal static class YamlLayoutEngineLoader
{
	public static void UpdateLayoutEngines(IContext ctx, Schema schema)
	{
		if (!schema.LayoutEngines.IsValid())
		{
			Logger.Debug("LayoutEngines plugin is not valid.");
			return;
		}

		if (schema.LayoutEngines.AsOptional() is not { } layoutEngines)
		{
			Logger.Debug("No layout engines found.");
			return;
		}

		List<CreateLeafLayoutEngine> leafLayoutEngineCreators = [];
		foreach (var engine in layoutEngines.Entries)
		{
			engine.Match<object?>(
				(in Schema.ALayoutEngineThatDisplaysOneWindowAtATime focusLayoutEngine) =>
				{
					leafLayoutEngineCreators.Add((id) => new FocusLayoutEngine(id));
					return null;
				},
				(in Schema.RequiredTypeAndVariant sliceLayoutEngine) =>
				{
					CreateSliceLayoutEngineCreator(ctx, leafLayoutEngineCreators, sliceLayoutEngine);
					return null;
				},
				(in Schema.ALayoutEngineThatArrangesWindowsInATreeStructure treeLayoutEngine) =>
				{
					CreateTreeLayoutEngineCreator(ctx, leafLayoutEngineCreators, treeLayoutEngine);
					return null;
				},
				// TODO: Throw an error for an unmatched type.
				(in Schema.RequiredType _) => null
			);
		}

		ctx.Store.Dispatch(new SetCreateLayoutEnginesTransform(() => [.. leafLayoutEngineCreators]));
	}

	private static void CreateSliceLayoutEngineCreator(
		IContext ctx,
		List<CreateLeafLayoutEngine> leafLayoutEngineCreators,
		Schema.RequiredTypeAndVariant sliceLayoutEngine
	)
	{
		if (!sliceLayoutEngine.IsValid())
		{
			return;
		}

		if (
			ctx.PluginManager.LoadedPlugins.FirstOrDefault(p => p.Name == "whim.slice_layout")
			is not SliceLayoutPlugin plugin
		)
		{
			plugin = new(ctx);
			ctx.PluginManager.AddPlugin(plugin);
		}

		if (sliceLayoutEngine.Variant == "row")
		{
			leafLayoutEngineCreators.Add((id) => SliceLayouts.CreateRowLayout(ctx, plugin, id));
		}
		else if (sliceLayoutEngine.Variant == "column")
		{
			leafLayoutEngineCreators.Add((id) => SliceLayouts.CreateColumnLayout(ctx, plugin, id));
		}
	}

	private static void CreateTreeLayoutEngineCreator(
		IContext ctx,
		List<CreateLeafLayoutEngine> leafLayoutEngineCreators,
		Schema.ALayoutEngineThatArrangesWindowsInATreeStructure treeLayoutEngine
	)
	{
		if (!treeLayoutEngine.IsValid())
		{
			return;
		}

		if (
			ctx.PluginManager.LoadedPlugins.FirstOrDefault(p => p.Name == "whim.tree_layout")
			is not TreeLayoutPlugin plugin
		)
		{
			plugin = new(ctx);
			ctx.PluginManager.AddPlugin(plugin);
		}

		Direction defaultAddNodeDirection = (string)treeLayoutEngine.InitialDirection switch
		{
			"left" => Direction.Left,
			"right" => Direction.Right,
			"up" => Direction.Up,
			"down" => Direction.Down,
			_ => Direction.Right,
		};

		leafLayoutEngineCreators.Add(
			(id) =>
			{
				TreeLayoutEngine engine = new(ctx, plugin, id);
				plugin.SetAddWindowDirection(engine, defaultAddNodeDirection);
				return engine;
			}
		);
	}
}
