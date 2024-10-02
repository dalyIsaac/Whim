using NSubstitute;
using Whim.SliceLayout;
using Whim.TestUtils;
using Xunit;

namespace Whim.Yaml.Tests;

public class YamlPluginLoader_SliceLayoutPluginTests
{
	public static TheoryData<string, bool> SliceLayoutConfig =>
		new()
		{
			// YAML, not explicitly enabled
			{
				"""
					plugins:
					  slice_layout: {}
					""",
				true
			},
			// JSON, not explicitly enabled
			{
				"""
					{
						"plugins": {
							"slice_layout": {}
						}
					}
					""",
				false
			},
			// YAML, explicitly enabled
			{
				"""
					plugins:
					  slice_layout:
					    is_enabled: true
					""",
				true
			},
			// JSON, explicitly enabled
			{
				"""
					{
						"plugins": {
							"slice_layout": {
								"is_enabled": true
							}
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(SliceLayoutConfig))]
	public void LoadSliceLayoutPlugin_ConfigIsSet_PluginIsLoaded(string schema, bool isYaml, IContext ctx)
	{
		// Given a valid config which loads the slice layout plugin
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and the slice layout plugin is set
		Assert.True(result);
		ctx.PluginManager.Received(1).AddPlugin(Arg.Any<SliceLayoutPlugin>());
	}

	public static TheoryData<string, bool> SliceLayoutPluginNotEnabledConfig =>
		new()
		{
			// YAML
			{
				"""
					plugins:
					  slice_layout:
					    is_enabled: false
					""",
				true
			},
			// JSON
			{
				"""
					{
						"plugins": {
							"slice_layout": {
								"is_enabled": false
							}
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(SliceLayoutPluginNotEnabledConfig))]
	public void LoadSliceLayoutPlugin_ConfigIsNotEnabled_PluginIsNotLoaded(string schema, bool isYaml, IContext ctx)
	{
		// Given a valid config with the slice layout plugin not enabled
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and the slice layout plugin is not set
		Assert.True(result);
		ctx.PluginManager.DidNotReceive().AddPlugin(Arg.Any<SliceLayoutPlugin>());
	}

	public static TheoryData<string, bool> InvalidSliceLayoutPluginConfig =>
		new()
		{
			// YAML, invalid random values
			{
				"""
					plugins:
					  slice_layout:
					    random_value: 123.5
					""",
				true
			},
			// JSON, invalid random values
			{
				"""
					{
						"plugins": {
							"slice_layout": {
								"random_value": 123.5
							}
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(InvalidSliceLayoutPluginConfig))]
	public void LoadSliceLayoutPlugin_InvalidConfig_PluginIsNotLoaded(string schema, bool isYaml, IContext ctx)
	{
		// Given a valid config with invalid slice layout plugin values
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is false, and the slice layout plugin is not set
		Assert.True(result);
		ctx.PluginManager.DidNotReceive().AddPlugin(Arg.Any<SliceLayoutPlugin>());
	}
}
