using NSubstitute;
using Whim.Bar;
using Whim.TestUtils;
using Xunit;

namespace Whim.Yaml.Tests;

public class YamlBarPluginLoaderTests
{
	public static TheoryData<string, bool, bool> BarIsEnabledConfig =>
		new()
		{
			// YAML, is_enabled: true
			{
				"""
					plugins:
					  bar:
					    is_enabled: true
					""",
				// Is YAML
				true,
				// Is enabled
				true
			},
			// JSON, is_enabled: true
			{
				"""
					{
						"plugins": {
							"bar": {
								"is_enabled": true
							}
						}
					}
					""",
				// Is JSON
				false,
				// Is enabled
				true
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(BarIsEnabledConfig))]
	public void LoadBarPlugin_IsEnabled(string schema, bool isYaml, IContext ctx)
	{
		// Given a valid config with the bar plugin
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and the bar plugin is loaded
		Assert.True(result);
		ctx.PluginManager.Received(1).AddPlugin(Arg.Is<BarPlugin>(p => p.Config.Height == 30));
	}

	public static TheoryData<string, bool> BarIsNotEnabledConfig =>
		new()
		{
			// YAML, is_enabled: false
			{
				"""
					plugins:
					  bar:
					    is_enabled: false
					""",
				true
			},
			// JSON, is_enabled: false
			{
				"""
					{
						"plugins": {
							"bar": {
								"is_enabled": false
							}
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(BarIsNotEnabledConfig))]
	public void LoadBarPlugin_IsNotEnabled(string schema, bool isYaml, IContext ctx)
	{
		// Given a valid config with the bar plugin that is not enabled
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and the bar plugin is not loaded
		Assert.True(result);
		ctx.PluginManager.DidNotReceive().AddPlugin(Arg.Any<BarPlugin>());
	}

	public static TheoryData<string, bool> BarPluginIsNotValidConfig =>
		new()
		{
			// YAML
			{
				"""
					plugins:
					  bar:
					    height: "not an int"
					""",
				true
			},
			// JSON
			{
				"""
					{
						"plugins": {
							"bar": {
								"height": "not an int"
							}
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(BarPluginIsNotValidConfig))]
	public void LoadBarPlugin_ConfigIsNotValid_PluginIsNotLoaded(string schema, bool isYaml, IContext ctx)
	{
		// Given a config with the bar plugin that is not valid
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is false, and the bar plugin is not loaded
		Assert.False(result);
		ctx.PluginManager.DidNotReceive().AddPlugin(Arg.Any<BarPlugin>());
	}

	public static TheoryData<string, bool> BarPluginHeightConfig =>
		new()
		{
			// YAML
			{
				"""
					plugins:
					  bar:
					    height: 42
					""",
				true
			},
			// JSON
			{
				"""
					{
						"plugins": {
							"bar": {
								"height": 42
							}
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(BarPluginHeightConfig))]
	public void LoadBarPlugin_ConfigHasHeight_PluginHasHeight(string schema, bool isYaml, IContext ctx)
	{
		// Given a valid config with the bar plugin and a height
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and the bar plugin has the height
		Assert.True(result);
		ctx.PluginManager.Received(1).AddPlugin(Arg.Is<BarPlugin>(p => p.Config.Height == 42));
	}
}
