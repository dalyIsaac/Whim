using Corvus.Json;
using Whim.SliceLayout;
using Whim.TreeLayout;

namespace Whim.Yaml;

internal static class YamlLayoutEngineLoader
{
	public static void UpdateLayoutEngines(IContext ctx, Schema schema)
	{
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
					CreateFocusLayoutEngineCreator(ctx, leafLayoutEngineCreators, focusLayoutEngine);
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

	private static void CreateFocusLayoutEngineCreator(
		IContext ctx,
		List<CreateLeafLayoutEngine> leafLayoutEngineCreators,
		Schema.ALayoutEngineThatDisplaysOneWindowAtATime focusLayoutEngine
	)
	{
		if (!focusLayoutEngine.IsValid())
		{
			return;
		}

		bool maximize = focusLayoutEngine.Maximize.AsOptional() ?? false;
		leafLayoutEngineCreators.Add((id) => new FocusLayoutEngine(id, maximize));
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

		sliceLayoutEngine.Variant.Match<object?>(
			(in Schema.RequiredTypeAndVariant.VariantEntity.CreatesAColumnLayoutWhereWindowsAreStackedVertically _) =>
			{
				leafLayoutEngineCreators.Add((id) => SliceLayouts.CreateColumnLayout(ctx, plugin, id));
				return null;
			},
			(in Schema.RequiredTypeAndVariant.VariantEntity.CreatesARowLayoutWhereWindowsAreStackedHorizontally _) =>
			{
				leafLayoutEngineCreators.Add((id) => SliceLayouts.CreateRowLayout(ctx, plugin, id));
				return null;
			},
			(in Schema.RequiredTypeAndVariant.VariantEntity.RequiredType _) =>
			{
				leafLayoutEngineCreators.Add((id) => SliceLayouts.CreatePrimaryStackLayout(ctx, plugin, id));
				return null;
			},
			(in Schema.RequiredTypeAndVariant.VariantEntity.RequiredTypeAndColumns multiColumnLayout) =>
			{
				var columns = multiColumnLayout.Columns;
				uint[] unsignedColumns = columns.Select(c => (uint)c).ToArray();
				leafLayoutEngineCreators.Add(
					(id) => SliceLayouts.CreateMultiColumnLayout(ctx, plugin, id, unsignedColumns)
				);
				return null;
			},
			(in Schema.RequiredTypeAndVariant.VariantEntity.AnyOfRequiredType secondaryPrimaryStack) =>
			{
				var primaryCapacity = (uint?)secondaryPrimaryStack.PrimaryCapacity.AsOptional() ?? 1;
				var secondaryCapacity = (uint?)secondaryPrimaryStack.SecondaryCapacity.AsOptional() ?? 2;

				leafLayoutEngineCreators.Add(
					(id) =>
						SliceLayouts.CreateSecondaryPrimaryLayout(ctx, plugin, id, primaryCapacity, secondaryCapacity)
				);
				return null;
			},
			(in Schema.RequiredTypeAndVariant.VariantEntity _) => null
		);
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
