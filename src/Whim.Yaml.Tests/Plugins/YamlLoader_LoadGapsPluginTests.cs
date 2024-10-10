using NSubstitute;
using Whim.Gaps;
using Whim.TestUtils;
using Xunit;

namespace Whim.Yaml.Tests;

public class YamlLoader_LoadGapsPluginTests
{
	private const int _defaultOuterGap = 0;
	private const int _defaultInnerGap = 10;
	private const int _defaultOuterDelta = 2;
	private const int _defaultInnerDelta = 2;

	public static TheoryData<string, bool, int, int, int, int> GapsConfig =>
		new()
		{
			// YAML, all values set
			{
				"""
					plugins:
					  gaps:
					    is_enabled: true
					    outer_gap: 1
					    inner_gap: 2
					    default_outer_delta: 3
					    default_inner_delta: 4
					""",
				true,
				1,
				2,
				3,
				4
			},
			// JSON, all values set
			{
				"""
					{
						"plugins": {
							"gaps": {
								"is_enabled": true,
								"outer_gap": 1,
								"inner_gap": 2,
								"default_outer_delta": 3,
								"default_inner_delta": 4
							}
						}
					}
					""",
				false,
				1,
				2,
				3,
				4
			},
			// YAML, no values set
			{
				"""
					plugins:
					  gaps: {}
					""",
				true,
				_defaultOuterGap,
				_defaultInnerGap,
				_defaultOuterDelta,
				_defaultInnerDelta
			},
			// JSON, no values set
			{
				"""
					{
						"plugins": {
							"gaps": {}
						}
					}
					""",
				false,
				_defaultOuterGap,
				_defaultInnerGap,
				_defaultOuterDelta,
				_defaultInnerDelta
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(GapsConfig))]
	public void Load_GapsPlugin(
		string config,
		bool isYaml,
		int outerGap,
		int innerGap,
		int defaultOuterDelta,
		int defaultInnerDelta,
		IContext ctx
	)
	{
		// Given a valid config with the gaps plugin
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and the gaps plugin is set
		Assert.True(result);
		ctx.PluginManager.Received(1)
			.AddPlugin(
				Arg.Is<GapsPlugin>(p =>
					p.GapsConfig.OuterGap == outerGap
					&& p.GapsConfig.InnerGap == innerGap
					&& p.GapsConfig.DefaultOuterDelta == defaultOuterDelta
					&& p.GapsConfig.DefaultInnerDelta == defaultInnerDelta
				)
			);
	}

	public static TheoryData<string, bool> GapsPluginNotEnabledConfig =>
		new()
		{
			// YAML
			{
				"""
					plugins:
					  gaps:
					    is_enabled: false
					""",
				true
			},
			// JSON
			{
				"""
					{
						"plugins": {
							"gaps": {
								"is_enabled": false
							}
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(GapsPluginNotEnabledConfig))]
	public void Load_GapsPlugin_NotEnabled(string config, bool isYaml, IContext ctx)
	{
		// Given a valid config with the gaps plugin not enabled
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and the gaps plugin is not set
		Assert.True(result);
		ctx.PluginManager.DidNotReceive().AddPlugin(Arg.Any<GapsPlugin>());
	}

	public static TheoryData<string, bool> InvalidGapsPluginConfig =>
		new()
		{
			// YAML, invalid outer gap
			{
				"""
					plugins:
					  gaps:
					    outer_gap: 123.5
					""",
				true
			},
			// JSON, invalid outer gap
			{
				"""
					{
						"plugins": {
							"gaps": {
								"outer_gap": 123.5
							}
						}
					}
					""",
				false
			},
			// YAML, invalid inner gap
			{
				"""
					plugins:
					  gaps:
					    inner_gap: 123.5
					""",
				true
			},
			// JSON, invalid inner gap
			{
				"""
					{
						"plugins": {
							"gaps": {
								"inner_gap": 123.5
							}
						}
					}
					""",
				false
			},
			// YAML, invalid default outer delta
			{
				"""
					plugins:
					  gaps:
					    default_outer_delta: 123.5
					""",
				true
			},
			// JSON, invalid default outer delta
			{
				"""
					{
						"plugins": {
							"gaps": {
								"default_outer_delta": 123.5
							}
						}
					}
					""",
				false
			},
			// YAML, invalid default inner delta
			{
				"""
					plugins:
					  gaps:
					    default_inner_delta: 123.5
					""",
				true
			},
			// JSON, invalid default inner delta
			{
				"""
					{
						"plugins": {
							"gaps": {
								"default_inner_delta": 123.5
							}
						}
					}
					""",
				false
			},
			// YAML, invalid is_enabled
			{
				"""
					plugins:
					  gaps:
					    is_enabled: 1
					""",
				true
			},
			// JSON, invalid is_enabled
			{
				"""
					{
						"plugins": {
							"gaps": {
								"is_enabled": 1
							}
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(InvalidGapsPluginConfig))]
	public void Load_GapsPlugin_InvalidConfig(string config, bool isYaml, IContext ctx)
	{
		// Given an invalid config with the gaps plugin
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is false, and the gaps plugin is not set
		Assert.True(result);
		ctx.PluginManager.DidNotReceive().AddPlugin(Arg.Any<GapsPlugin>());
	}
}
