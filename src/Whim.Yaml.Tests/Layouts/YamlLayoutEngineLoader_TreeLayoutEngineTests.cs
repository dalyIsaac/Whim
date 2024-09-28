using NSubstitute;
using Whim.TestUtils;
using Whim.TreeLayout;
using Xunit;

namespace Whim.Yaml.Tests;

public class YamlLayoutEngineLoader_TreeLayoutEngineTests
{
	public static TheoryData<string, bool, Direction> TreeConfig =>
		new()
		{
			{
				"""
					layout_engines:
					  entries:
					    - type: TreeLayoutEngine
					      initial_direction: left
					""",
				true,
				Direction.Left
			},
			{
				"""
					{
						"layout_engines": {
							"entries": [
								{
									"type": "TreeLayoutEngine",
									"initial_direction": "left"
								}
							]
						}
					}
					""",
				false,
				Direction.Left
			},
			{
				"""
					layout_engines:
					  entries:
					    - type: TreeLayoutEngine
					      initial_direction: right
					""",
				true,
				Direction.Right
			},
			{
				"""
					{
						"layout_engines": {
							"entries": [
								{
									"type": "TreeLayoutEngine",
									"initial_direction": "right"
								}
							]
						}
					}
					""",
				false,
				Direction.Right
			},
			{
				"""
					layout_engines:
					  entries:
					    - type: TreeLayoutEngine
					      initial_direction: up
					""",
				true,
				Direction.Up
			},
			{
				"""
					{
						"layout_engines": {
							"entries": [
								{
									"type": "TreeLayoutEngine",
									"initial_direction": "up"
								}
							]
						}
					}
					""",
				false,
				Direction.Up
			},
			{
				"""
					layout_engines:
					  entries:
					    - type: TreeLayoutEngine
					      initial_direction: down
					""",
				true,
				Direction.Down
			},
			{
				"""
					{
						"layout_engines": {
							"entries": [
								{
									"type": "TreeLayoutEngine",
									"initial_direction": "down"
								}
							]
						}
					}
					""",
				false,
				Direction.Down
			},
		};

	[Theory]
	[MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(TreeConfig))]
	public void LoadTreeLayoutEngine(
		string yaml,
		bool isValid,
		Direction direction,
		IContext ctx,
		ITreeLayoutPlugin plugin
	)
	{
		// Given a valid config with the tree layout engine set
		ctx.PluginManager.LoadedPlugins.Returns([]);
		YamlLoaderTestUtils.SetupFileConfig(ctx, yaml, isValid);

		// When loading the layout engine
		bool result = YamlLoader.Load(ctx);

		// Then the tree layout engine should be updated
		Assert.True(result);

		ILayoutEngine[] engines = YamlLoaderTestUtils.GetLayoutEngines(ctx);
		Assert.Single(engines);
		Assert.Equal(new TreeLayoutEngine(ctx, plugin, engines[0].Identity), engines[0]);

		ctx.PluginManager.Received(1).AddPlugin(Arg.Any<ITreeLayoutPlugin>());

		plugin.Received(1).SetAddWindowDirection(Arg.Any<TreeLayoutEngine>(), direction);
	}
}
