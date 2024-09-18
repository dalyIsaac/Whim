using NSubstitute;
using Whim.FocusIndicator;
using Whim.TestUtils;
using Xunit;

namespace Whim.Yaml.Tests;

public class YamlLoader_LoadFocusIndicatorPluginTests
{
	private const int _defaultBorderSize = 4;
	private const bool _defaultIsFadeEnabled = false;
	private const int _defaultFadeTimeout = 10;

	public static TheoryData<string, bool, int, bool, int> FocusIndicatorConfig =>
		new()
		{
			// YAML, all values set
			{
				"""
					plugins:
					  focus_indicator:
					    is_enabled: true
					    border_size: 1
					    is_fade_enabled: false
					    fade_timeout: 2
					""",
				true,
				1,
				false,
				2
			},
			// JSON, all values set
			{
				"""
					{
						"plugins": {
							"focus_indicator": {
								"is_enabled": true,
								"border_size": 1,
								"is_fade_enabled": false,
								"fade_timeout": 2
							}
						}
					}
					""",
				false,
				1,
				false,
				2
			},
			// YAML, no values set
			{
				"""
					plugins:
					  focus_indicator: {}
					""",
				true,
				_defaultBorderSize,
				_defaultIsFadeEnabled,
				_defaultFadeTimeout
			},
			// JSON, no values set
			{
				"""
					{
						"plugins": {
							"focus_indicator": {}
						}
					}
					""",
				false,
				_defaultBorderSize,
				_defaultIsFadeEnabled,
				_defaultFadeTimeout
			},
			// YAML, border_size set to 0
			{
				"""
					plugins:
					  focus_indicator:
					    border_size: 0
					""",
				true,
				0,
				_defaultIsFadeEnabled,
				_defaultFadeTimeout
			},
			// JSON, border_size set to 0
			{
				"""
					{
						"plugins": {
							"focus_indicator": {
								"border_size": 0
							}
						}
					}
					""",
				false,
				0,
				_defaultIsFadeEnabled,
				_defaultFadeTimeout
			},
			// YAML, is_fade_enabled set to true
			{
				"""
					plugins:
					  focus_indicator:
					    is_fade_enabled: true
					""",
				true,
				_defaultBorderSize,
				true,
				_defaultFadeTimeout
			},
			// JSON, is_fade_enabled set to true
			{
				"""
					{
						"plugins": {
							"focus_indicator": {
								"is_fade_enabled": true
							}
						}
					}
					""",
				false,
				_defaultBorderSize,
				true,
				_defaultFadeTimeout
			},
			// YAML, fade_timeout set to 0
			{
				"""
					plugins:
					  focus_indicator:
					    fade_timeout: 0
					""",
				true,
				_defaultBorderSize,
				_defaultIsFadeEnabled,
				0
			},
			// JSON, fade_timeout set to 0
			{
				"""
					{
						"plugins": {
							"focus_indicator": {
								"fade_timeout": 0
							}
						}
					}
					""",
				false,
				_defaultBorderSize,
				_defaultIsFadeEnabled,
				0
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(FocusIndicatorConfig))]
	public void LoadFocusIndicatorConfig(
		string config,
		bool isYaml,
		int borderSize,
		bool isFadeEnabled,
		int fadeTimeout,
		IContext ctx
	)
	{
		// Given a focus indicator configuration
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the focus indicator configuration is loaded
		Assert.True(result);
		ctx.PluginManager.Received(1)
			.AddPlugin(
				Arg.Is<FocusIndicatorPlugin>(p =>
					p.Config.BorderSize == borderSize
					&& p.Config.FadeEnabled == isFadeEnabled
					&& p.Config.FadeTimeout == TimeSpan.FromSeconds(fadeTimeout)
				)
			);
	}

	public static TheoryData<string, bool> DisabledFocusIndicatorConfig =>
		new()
		{
			// YAML, is_enabled set to false
			{
				"""
					plugins:
					  focus_indicator:
					    is_enabled: false
					""",
				true
			},
			// JSON, is_enabled set to false
			{
				"""
					{
						"plugins": {
							"focus_indicator": {
								"is_enabled": false
							}
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(DisabledFocusIndicatorConfig))]
	public void LoadDisabledFocusIndicatorConfig(string config, bool isYaml, IContext ctx)
	{
		// Given a disabled focus indicator configuration
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the focus indicator configuration is not loaded
		Assert.True(result);
		ctx.PluginManager.DidNotReceive().AddPlugin(Arg.Any<FocusIndicatorPlugin>());
	}
}
