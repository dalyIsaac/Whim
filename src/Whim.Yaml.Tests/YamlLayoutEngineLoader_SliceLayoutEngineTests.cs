using Whim.SliceLayout;
using Whim.TestUtils;
using Xunit;

namespace Whim.Yaml.Tests;

public class YamlLayoutEngineLoader_SliceLayoutEngineTests
{
	public static TheoryData<string, bool> ColumnConfig =>
		new()
		{
			{
				"""
					layout_engines:
					  entries:
					    - type: SliceLayoutEngine
					      variant:
					        type: column
					""",
				true
			},
			{
				"""
					{
						"layout_engines": {
							"entries": [
								{
									"type": "SliceLayoutEngine",
									"variant": {
										"type": "column"
									}
								}
							]
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(ColumnConfig))]
	public void Load_ColumnLayoutEngine(string config, bool isYaml, IContext ctx, ISliceLayoutPlugin plugin)
	{
		// Given a valid config with layout engine set
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the layout engine
		bool result = YamlLoader.Load(ctx);

		// Then the layout engine is loaded
		Assert.True(result);

		ILayoutEngine[] engines = YamlLoaderTestUtils.GetLayoutEngines(ctx);
		Assert.Single(engines);
		Assert.Equal(SliceLayouts.CreateColumnLayout(ctx, plugin, engines[0].Identity), engines[0]);
	}

	public static TheoryData<string, bool> RowConfig =>
		new()
		{
			{
				"""
					layout_engines:
					  entries:
					    - type: SliceLayoutEngine
					      variant:
					        type: row
					""",
				true
			},
			{
				"""
					{
						"layout_engines": {
							"entries": [
								{
									"type": "SliceLayoutEngine",
									"variant": {
										"type": "row"
									}
								}
							]
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(RowConfig))]
	public void Load_RowLayoutEngine(string config, bool isYaml, IContext ctx, ISliceLayoutPlugin plugin)
	{
		// Given a valid config with layout engine set
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the layout engine
		bool result = YamlLoader.Load(ctx);

		// Then the layout engine is loaded
		Assert.True(result);

		ILayoutEngine[] engines = YamlLoaderTestUtils.GetLayoutEngines(ctx);
		Assert.Single(engines);
		Assert.Equal(SliceLayouts.CreateRowLayout(ctx, plugin, engines[0].Identity), engines[0]);
	}

	public static TheoryData<string, bool> PrimaryStackConfig =>
		new()
		{
			{
				"""
					layout_engines:
					  entries:
					    - type: SliceLayoutEngine
					      variant:
					        type: primary_stack
					""",
				true
			},
			{
				"""
					{
						"layout_engines": {
							"entries": [
								{
									"type": "SliceLayoutEngine",
									"variant": {
										"type": "primary_stack"
									}
								}
							]
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(PrimaryStackConfig))]
	public void Load_PrimaryStackLayoutEngine(string config, bool isYaml, IContext ctx, ISliceLayoutPlugin plugin)
	{
		// Given a valid config with layout engine set
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the layout engine
		bool result = YamlLoader.Load(ctx);

		// Then the layout engine is loaded
		Assert.True(result);

		ILayoutEngine[] engines = YamlLoaderTestUtils.GetLayoutEngines(ctx);
		Assert.Single(engines);
		Assert.Equal(SliceLayouts.CreatePrimaryStackLayout(ctx, plugin, engines[0].Identity), engines[0]);
	}

	public static TheoryData<string, uint[], bool> MultiColumnConfig =>
		new()
		{
			{
				"""
					layout_engines:
					  entries:
					    - type: SliceLayoutEngine
					      variant:
					        type: multi_column_stack
					        columns: [1, 2, 0, 3]
					""",
				new uint[] { 1, 2, 0, 3 },
				true
			},
			{
				"""
					{
						"layout_engines": {
							"entries": [
								{
									"type": "SliceLayoutEngine",
									"variant": {
										"type": "multi_column_stack",
										"columns": [1, 2, 0, 3]
									}
								}
							]
						}
					}
					""",
				new uint[] { 1, 2, 0, 3 },
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(MultiColumnConfig))]
	public void Load_MultiColumnLayoutEngine(
		string config,
		uint[] columns,
		bool isYaml,
		IContext ctx,
		ISliceLayoutPlugin plugin
	)
	{
		// Given a valid config with layout engine set
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the layout engine
		bool result = YamlLoader.Load(ctx);

		// Then the layout engine is loaded
		Assert.True(result);

		ILayoutEngine[] engines = YamlLoaderTestUtils.GetLayoutEngines(ctx);
		Assert.Single(engines);
		Assert.Equal(SliceLayouts.CreateMultiColumnLayout(ctx, plugin, engines[0].Identity, columns), engines[0]);
	}

	public static TheoryData<string, uint, uint, bool> SecondaryPrimaryConfig =>
		new()
		{
			{
				"""
					layout_engines:
					  entries:
					    - type: SliceLayoutEngine
					      variant:
					        type: secondary_primary_stack
					        primary_capacity: 1
					        secondary_capacity: 2
					""",
				1u,
				2u,
				true
			},
			{
				"""
					{
						"layout_engines": {
							"entries": [
								{
									"type": "SliceLayoutEngine",
									"variant": {
										"type": "secondary_primary_stack",
										"primary_capacity": 1,
										"secondary_capacity": 2
									}
								}
							]
						}
					}
					""",
				1u,
				2u,
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(SecondaryPrimaryConfig))]
	public void Load_SecondaryPrimaryLayoutEngine(
		string config,
		uint primaryCount,
		uint secondaryCount,
		bool isYaml,
		IContext ctx,
		ISliceLayoutPlugin plugin
	)
	{
		// Given a valid config with layout engine set
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the layout engine
		bool result = YamlLoader.Load(ctx);

		// Then the layout engine is loaded
		Assert.True(result);

		ILayoutEngine[] engines = YamlLoaderTestUtils.GetLayoutEngines(ctx);
		Assert.Single(engines);
		Assert.Equal(
			SliceLayouts.CreateSecondaryPrimaryLayout(ctx, plugin, engines[0].Identity, primaryCount, secondaryCount),
			engines[0]
		);
	}
}
