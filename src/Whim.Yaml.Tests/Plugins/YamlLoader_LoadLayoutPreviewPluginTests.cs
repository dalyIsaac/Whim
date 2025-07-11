using NSubstitute;
using Whim.LayoutPreview;
using Whim.TestUtils;
using Xunit;

namespace Whim.Yaml.Tests;

public class YamlLoader_LoadLayoutPreviewPluginTests
{
	public static TheoryData<string, bool, bool> LayoutPreviewConfig =>
		new()
		{
			// YAML, enabled'
			{
				"""
					plugins:
					  layout_preview:
					    is_enabled: true
					""",
				true,
				true
			},
			// JSON, enabled
			{
				"""
					{
						"plugins": {
							"layout_preview": {
								"is_enabled": true
							}
						}
					}
					""",
				false,
				true
			},
			// YAML, no values set
			{
				"""
					plugins:
					  layout_preview: {}
					""",
				true,
				false
			},
			// JSON, no values set
			{
				"""
						{
							"plugins": {
								"layout_preview": {}
							}
						}
					""",
				false,
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(LayoutPreviewConfig))]
	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Usage",
		"xUnit1026:Theory methods should use all of their parameters",
		Justification = "Theory data is shared with other tests"
	)]
	public void LoadLayoutPreviewPlugin(string yaml, bool isYaml, bool _isEnabled, IContext ctx)
	{
		// Given a a YAML config with the layout preview plugin
		YamlLoaderTestUtils.SetupFileConfig(ctx, yaml, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx, showErrorWindow: false);

		// Then the result is true, and the layout preview plugin is set
		Assert.True(result);
		ctx.PluginManager.Received().AddPlugin(Arg.Any<LayoutPreviewPlugin>());
	}

	public static TheoryData<string, bool> DisabledLayoutPreviewConfig =>
		new()
		{
			// YAML
			{
				"""
					plugins:
					  layout_preview:
					    is_enabled: false
					""",
				true
			},
			// JSON
			{
				"""
					{
						"plugins": {
							"layout_preview": {
								"is_enabled": false
							}
						}
					}
					""",
				false
			},
			// YAML, invalid
			{
				"""
					plugins:
					  layout_preview:
					    bob: true
					""",
				true
			},
			// JSON, invalid
			{
				"""
					{
						"plugins": {
							"layout_preview": {
								"bob": true
							}
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(DisabledLayoutPreviewConfig))]
	public void LoadLayoutPreviewPlugin_ConfigIsNotEnabled_PluginIsNotLoaded(string schema, bool isYaml, IContext ctx)
	{
		// Given a valid config with the layout preview plugin not enabled
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx, showErrorWindow: false);

		// Then the result is true, and the layout preview plugin is not set
		Assert.True(result);
		ctx.PluginManager.DidNotReceive().AddPlugin(Arg.Any<LayoutPreviewPlugin>());
	}
}
