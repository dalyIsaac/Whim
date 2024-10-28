using NSubstitute;
using Whim.TestUtils;
using Whim.TreeLayout;
using Xunit;

namespace Whim.Yaml.Tests;

public class YamlPluginLoader_TreeLayoutPluginTests
{
	public static TheoryData<string, bool> TreeLayoutConfig =>
		new()
		{
			// YAML, not explicitly enabled
			{
				"""
					plugins:
					  tree_layout: {}
					""",
				true
			},
			// JSON, not explicitly enabled
			{
				"""
					{
						"plugins": {
							"tree_layout": {}
						}
					}
					""",
				false
			},
			// YAML, explicitly enabled
			{
				"""
					plugins:
					  tree_layout:
					    is_enabled: true
					""",
				true
			},
			// JSON, explicitly enabled
			{
				"""
					{
						"plugins": {
							"tree_layout": {
								"is_enabled": true
							}
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(TreeLayoutConfig))]
	public void LoadTreeLayoutPlugin_ConfigIsSet_PluginIsLoaded(string schema, bool isYaml, IContext ctx)
	{
		// Given a valid config which loads the tree layout plugin
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx, showErrorWindow: false);

		// Then the result is true, and the tree layout plugin is set
		Assert.True(result);
		ctx.PluginManager.Received(1).AddPlugin(Arg.Any<TreeLayoutPlugin>());
	}

	public static TheoryData<string, bool> TreeLayoutPluginNotEnabledConfig =>
		new()
		{
			// YAML
			{
				"""
					plugins:
					  tree_layout:
					    is_enabled: false
					""",
				true
			},
			// JSON
			{
				"""
					{
						"plugins": {
							"tree_layout": {
								"is_enabled": false
							}
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(TreeLayoutPluginNotEnabledConfig))]
	public void LoadTreeLayoutPlugin_ConfigIsNotEnabled_PluginIsNotLoaded(string schema, bool isYaml, IContext ctx)
	{
		// Given a valid config with the tree layout plugin not enabled
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx, showErrorWindow: false);

		// Then the result is true, and the tree layout plugin is not set
		Assert.True(result);
		ctx.PluginManager.DidNotReceive().AddPlugin(Arg.Any<TreeLayoutPlugin>());
	}

	public static TheoryData<string, bool> InvalidTreeLayoutPluginConfig =>
		new()
		{
			// YAML, invalid random values
			{
				"""
					plugins:
					  tree_layout:
					    random_value: 123.5
					""",
				true
			},
			// JSON, invalid random values
			{
				"""
					{
						"plugins": {
							"tree_layout": {
								"random_value": 123.5
							}
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(InvalidTreeLayoutPluginConfig))]
	public void LoadTreeLayoutPlugin_InvalidConfig_PluginIsNotLoaded(string schema, bool isYaml, IContext ctx)
	{
		// Given a valid config with invalid tree layout plugin values
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx, showErrorWindow: false);

		// Then the result is false, and the tree layout plugin is not set
		Assert.True(result);
		ctx.PluginManager.DidNotReceive().AddPlugin(Arg.Any<TreeLayoutPlugin>());
	}
}
