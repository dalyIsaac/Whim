using DotNext;
using NSubstitute;
using Whim.FloatingWindow;
using Whim.Gaps;
using Whim.TestUtils;
using Xunit;

namespace Whim.Yaml.Tests;

public class YamlLoader_LoadFloatingWindowPluginTests
{
	public static TheoryData<string, bool> FloatingWindowConfig =>
		new()
		{
			{
				// YAML, all values set
				"""
					plugins:
					  floating_window:
					    is_enabled: true
					""",
				true
			},
			{
				// JSON, all values set
				"""
					{
						"plugins": {
							"floating_window": {
								"is_enabled": true
							}
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(FloatingWindowConfig))]
	public void LoadFloatingWindowPlugin(string config, bool isYaml, IContext ctx)
	{
		// Given a floating window plugin configuration
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and the floating window plugin is added
		Assert.True(result);
		ctx.PluginManager.Received().AddPlugin(Arg.Any<FloatingWindowPlugin>());
	}

	public static TheoryData<string, bool> DisabledFloatingWindowConfig =>
		new()
		{
			// YAML, is_enabled set to false
			{
				"""
					plugins:
					  floating_window:
					    is_enabled: false
					""",
				true
			},
			// JSON, is_enabled set to false
			{
				"""
					{
						"plugins": {
							"floating_window": {
								"is_enabled": false
							}
						}
					}
					""",
				false
			},
			// YAML, plugin not listed
			{
				"""
					plugins:
					  focus_indicator:
					    is_enabled: true
					""",
				true
			},
			// JSON, plugin not listed
			{
				"""
					{
						"plugins": {
							"focus_indicator": {
								"is_enabled": true
							}
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(DisabledFloatingWindowConfig))]
	public void LoadDisabledFloatingWindowConfig(string config, bool isYaml, IContext ctx)
	{
		// Given a disabled floating window plugin configuration
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and the floating window plugin is not added
		Assert.True(result);
		ctx.PluginManager.DidNotReceive().AddPlugin(Arg.Any<FloatingWindowPlugin>());
	}

	public static TheoryData<string, bool> FloatingWrapsGapsConfig =>
		new()
		{
			// YAML
			{
				"""
					layout_engines:
					  entries:
					    - type: focus
					plugins:
					  floating_window:
					    is_enabled: true
					  gaps:
					    is_enabled: true
					""",
				true
			},
			// JSON
			{
				"""
					{
						"layout_engines": {
							"entries": [
								{
									"type": "focus"
								}
							]
						},
						"plugins": {
							"floating_window": {
								"is_enabled": true
							},
							"gaps": {
								"is_enabled": true
							}
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(FloatingWrapsGapsConfig))]
	public void LoadFloatingWrapsGapsConfig(string config, bool isYaml, IContext ctx)
	{
		// Given a floating window and gap plugin configuration
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the config
		YamlLoader.Load(ctx);

		// Then the floating layout plugin is loaded first, followed by the gaps plugin
		var calls = ctx.PluginManager.ReceivedCalls().ToArray();
		Assert.Equal(2, calls.Length);
		Assert.IsType<FloatingWindowPlugin>(calls[0].GetArguments()[0]);
		Assert.IsType<GapsPlugin>(calls[1].GetArguments()[0]);
	}
}
