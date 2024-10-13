using FluentAssertions;
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
					    - type: tree
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
									"type": "tree",
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
					    - type: tree
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
									"type": "tree",
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
					    - type: tree
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
									"type": "tree",
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
					    - type: tree
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
									"type": "tree",
									"initial_direction": "down"
								}
							]
						}
					}
					""",
				false,
				Direction.Down
			},
			// Use the default direction.
			{
				"""
					layout_engines:
					  entries:
					    - type: tree
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
									"type": "tree"
								}
							]
						}
					}
					""",
				false,
				Direction.Right
			},
		};

	[Theory]
	[MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(TreeConfig))]
	public void LoadTreeLayoutEngine(string yaml, bool isValid, Direction direction, IContext ctx)
	{
		// Given a valid config with the tree layout engine set
		ctx.PluginManager.LoadedPlugins.Returns([]);
		YamlLoaderTestUtils.SetupFileConfig(ctx, yaml, isValid);

		TreeLayoutPlugin? plugin = null;
		ctx.PluginManager.AddPlugin(Arg.Do<TreeLayoutPlugin>(t => plugin = t));

		// When loading the layout engine
		bool result = YamlLoader.Load(ctx);

		// Then the tree layout engine should be updated
		Assert.True(result);

		ILayoutEngine[] engines = YamlLoaderTestUtils.GetLayoutEngines(ctx)!;
		Assert.Single(engines);
		new TreeLayoutEngine(ctx, Substitute.For<ITreeLayoutPlugin>(), engines[0].Identity)
			.Should()
			.BeEquivalentTo(engines[0]);

		// ...and the tree layout plugin should be added...
		ctx.PluginManager.Received(1).AddPlugin(Arg.Any<TreeLayoutPlugin>());
		Assert.IsType<TreeLayoutPlugin>(plugin);

		// ...and the direction was set.
		Assert.Equal(plugin.GetAddWindowDirection((TreeLayoutEngine)engines[0]), direction);
	}
}
