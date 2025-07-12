using Whim.FloatingWindow;
using Whim.SliceLayout;
using Whim.TreeLayout;

namespace Whim.Yaml;

internal static class YamlLayoutEngineLoader
{
	public static void UpdateLayoutEngines(IContext ctx, Schema schema)
	{
		if (schema.LayoutEngines is not { } layoutEngines)
		{
			Logger.Debug("No layout engines found.");
			return;
		}

		CreateLeafLayoutEngine[]? engineCreators = GetCreateLeafLayoutEngines(ctx, layoutEngines.Entries);

		if (engineCreators is not null)
		{
			ctx.Store.Dispatch(new SetCreateLayoutEnginesTransform(() => engineCreators));
		}
	}

	public static CreateLeafLayoutEngine[]? GetCreateLeafLayoutEngines(
		IContext ctx,
		Schema.LayoutEngineListEntity.EntriesArray layoutEngines
	)
	{
		List<CreateLeafLayoutEngine> leafLayoutEngineCreators = [];

		foreach (var engine in layoutEngines)
		{
			engine.Match<object?>(
				(in Schema.FloatingWindowEngineEntity floatingWindow) =>
				{
					CreateFloatingLayoutEngineCreator(ctx, leafLayoutEngineCreators);
					return null;
				},
				(in Schema.FocusLayoutEngineEntity focusLayoutEngine) =>
				{
					CreateFocusLayoutEngineCreator(ctx, leafLayoutEngineCreators, focusLayoutEngine);
					return null;
				},
				(in Schema.SliceLayoutEngineEntity sliceLayoutEngine) =>
				{
					CreateSliceLayoutEngineCreator(ctx, leafLayoutEngineCreators, sliceLayoutEngine);
					return null;
				},
				(in Schema.TreeLayoutEngineEntity treeLayoutEngine) =>
				{
					CreateTreeLayoutEngineCreator(ctx, leafLayoutEngineCreators, treeLayoutEngine);
					return null;
				},
				(in Schema.LayoutEngineEntity fallback) => null
			);
		}

		return leafLayoutEngineCreators.Count == 0 ? null : [.. leafLayoutEngineCreators];
	}

	private static void CreateFloatingLayoutEngineCreator(
		IContext ctx,
		List<CreateLeafLayoutEngine> leafLayoutEngineCreators
	)
	{
		// The floating layout leaf engine doesn't require the FloatingWindowPlugin.
		leafLayoutEngineCreators.Add((id) => new FloatingLayoutEngine(ctx, id));
	}

	private static void CreateFocusLayoutEngineCreator(
		IContext ctx,
		List<CreateLeafLayoutEngine> leafLayoutEngineCreators,
		Schema.FocusLayoutEngineEntity focusLayoutEngine
	)
	{
		bool maximize = focusLayoutEngine.Maximize is { } m && m;
		leafLayoutEngineCreators.Add((id) => new FocusLayoutEngine(id, maximize));
	}

	private static void CreateSliceLayoutEngineCreator(
		IContext ctx,
		List<CreateLeafLayoutEngine> leafLayoutEngineCreators,
		Schema.SliceLayoutEngineEntity sliceLayoutEngine
	)
	{
		if (
			ctx.PluginManager.LoadedPlugins.FirstOrDefault(p => p.Name == "whim.slice_layout")
			is not SliceLayoutPlugin plugin
		)
		{
			plugin = new(ctx);
			ctx.PluginManager.AddPlugin(plugin);
		}

		sliceLayoutEngine.Variant.Match<object?>(
			(in Schema.SliceLayoutEngineEntity.VariantEntity.AnyOf0Entity _) =>
			{
				leafLayoutEngineCreators.Add((id) => SliceLayouts.CreateColumnLayout(ctx, plugin, id));
				return null;
			},
			(in Schema.SliceLayoutEngineEntity.VariantEntity.AnyOf1Entity _) =>
			{
				leafLayoutEngineCreators.Add((id) => SliceLayouts.CreateRowLayout(ctx, plugin, id));
				return null;
			},
			(in Schema.SliceLayoutEngineEntity.VariantEntity.AnyOf2Entity _) =>
			{
				leafLayoutEngineCreators.Add((id) => SliceLayouts.CreatePrimaryStackLayout(ctx, plugin, id));
				return null;
			},
			(in Schema.SliceLayoutEngineEntity.VariantEntity.AnyOf3Entity multiColumnLayout) =>
			{
				var columns = multiColumnLayout.Columns;
				uint[] unsignedColumns = [.. columns.Select(c => (uint)c)];
				leafLayoutEngineCreators.Add(
					(id) => SliceLayouts.CreateMultiColumnLayout(ctx, plugin, id, unsignedColumns)
				);
				return null;
			},
			(in Schema.SliceLayoutEngineEntity.VariantEntity.AnyOf4Entity secondaryPrimaryStack) =>
			{
				uint primaryCapacity = secondaryPrimaryStack.PrimaryCapacity is { } pc ? (uint)pc : 1;
				uint secondaryCapacity = secondaryPrimaryStack.SecondaryCapacity is { } sc ? (uint)sc : 2;

				leafLayoutEngineCreators.Add(
					(id) =>
						SliceLayouts.CreateSecondaryPrimaryLayout(ctx, plugin, id, primaryCapacity, secondaryCapacity)
				);
				return null;
			},
			(in Schema.SliceLayoutEngineEntity.VariantEntity _) => null
		);
	}

	private static void CreateTreeLayoutEngineCreator(
		IContext ctx,
		List<CreateLeafLayoutEngine> leafLayoutEngineCreators,
		Schema.TreeLayoutEngineEntity treeLayoutEngine
	)
	{
		if (
			ctx.PluginManager.LoadedPlugins.FirstOrDefault(p => p.Name == "whim.tree_layout")
			is not TreeLayoutPlugin plugin
		)
		{
			plugin = new(ctx);
			ctx.PluginManager.AddPlugin(plugin);
		}

		Direction defaultAddNodeDirection = Direction.Right;
		if (((string?)treeLayoutEngine.InitialDirection) is { } initialDirection)
		{
			defaultAddNodeDirection = initialDirection switch
			{
				"left" => Direction.Left,
				"right" => Direction.Right,
				"up" => Direction.Up,
				"down" => Direction.Down,
				_ => Direction.Right,
			};
		}

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
