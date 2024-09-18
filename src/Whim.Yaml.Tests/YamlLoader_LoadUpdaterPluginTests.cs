using NSubstitute;
using Whim.TestUtils;
using Whim.Updater;
using Xunit;

namespace Whim.Yaml.Tests;

public class YamlLoader_LoadUpdaterPluginTests
{
	public static TheoryData<string, bool, UpdateFrequency, ReleaseChannel> UpdaterPluginConfig =>
		new()
		{
			// YAML, all values set
			{
				"""
					plugins:
					  updater:
					    is_enabled: true
					    update_frequency: daily
					    release_channel: alpha
					""",
				true,
				UpdateFrequency.Daily,
				ReleaseChannel.Alpha
			},
			// JSON, all values set
			{
				"""
					{
						"plugins": {
							"updater": {
								"is_enabled": true,
								"update_frequency": "daily",
								"release_channel": "alpha"
							}
						}
					}
					""",
				false,
				UpdateFrequency.Daily,
				ReleaseChannel.Alpha
			},
			// YAML, no values set
			{
				"""
					plugins:
					  updater: {}
					""",
				true,
				UpdateFrequency.Weekly,
				ReleaseChannel.Alpha
			},
			// JSON, no values set
			{
				"""
					{
						"plugins": {
							"updater": {}
						}
					}
					""",
				false,
				UpdateFrequency.Weekly,
				ReleaseChannel.Alpha
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(UpdaterPluginConfig))]
	public void LoadUpdaterPlugin_ConfigIsSet_PluginIsLoaded(
		string schema,
		bool isYaml,
		UpdateFrequency updateFrequency,
		ReleaseChannel releaseChannel,
		IContext ctx
	)
	{
		// Given a valid config with the updater plugin
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the updater plugin is loaded
		Assert.True(result);
		ctx.PluginManager.Received()
			.AddPlugin(
				Arg.Is<UpdaterPlugin>(p =>
					p.Config.UpdateFrequency == updateFrequency && p.Config.ReleaseChannel == releaseChannel
				)
			);
	}

	public static TheoryData<string, bool> UpdaterPluginNotEnabledConfig =>
		new()
		{
			// YAML
			{
				"""
					plugins:
					  updater:
					    is_enabled: false
					""",
				true
			},
			// JSON
			{
				"""
					{
						"plugins": {
							"updater": {
								"is_enabled": false
							}
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(UpdaterPluginNotEnabledConfig))]
	public void LoadUpdaterPlugin_ConfigNotEnabled_PluginIsNotLoaded(string schema, bool isYaml, IContext ctx)
	{
		// Given a valid config with the updater plugin not enabled
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the updater plugin is not loaded
		Assert.True(result);
		ctx.PluginManager.DidNotReceive().AddPlugin(Arg.Any<UpdaterPlugin>());
	}
}
