using FluentAssertions;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Yaml.Tests;

public class YamlLoader_LoadFocusLayoutEngineTests
{
	public static TheoryData<string, bool, bool> FocusLayoutEngineConfig =>
		new()
		{
			{
				"""
					layout_engines:
					  entries:
					    - type: focus
					""",
				false,
				true
			},
			{
				"""
					{
						"layout_engines": {
							"entries": [
								{
									"type": "focus"
								}
							]
						}
					}
					""",
				false,
				false
			},
			{
				"""
					layout_engines:
					  entries:
					    - type: focus
					      maximize: true
					""",
				true,
				true
			},
			{
				"""
					{
						"layout_engines": {
							"entries": [
								{
									"type": "focus",
									"maximize": true
								}
							]
						}
					}
					""",
				true,
				false
			},
			{
				"""
					layout_engines:
					  entries:
					    - type: focus
					      maximize: false
					""",
				false,
				true
			},
			{
				"""
					{
						"layout_engines": {
							"entries": [
								{
									"type": "focus",
									"maximize": false
								}
							]
						}
					}
					""",
				false,
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(FocusLayoutEngineConfig))]
	public void Load_FocusLayoutEngine(string config, bool maximize, bool isYaml, IContext ctx)
	{
		// Given a valid config with FocusLayoutEngine set
		ctx.PluginManager.LoadedPlugins.Returns([]);
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the layout engine
		bool result = YamlLoader.Load(ctx);

		// Then the layout engine is loaded
		Assert.True(result);

		ILayoutEngine[] engines = YamlLoaderTestUtils.GetLayoutEngines(ctx)!;
		Assert.Single(engines);
		engines[0].Should().BeEquivalentTo(new FocusLayoutEngine(engines[0].Identity, maximize));
	}

	public static TheoryData<string, bool> InvalidFocusLayoutEngineConfig =>
		new()
		{
			{
				"""
					layout_engines:
					  entries:
					    - type: focus
					      maximize: "bob"
					""",
				true
			},
			{
				"""
					{
						"layout_engines": {
							"entries": [
								{
									"type": "focus",
									"maximize": "bob"
								}
							]
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(InvalidFocusLayoutEngineConfig))]
	public void Load_InvalidFocusLayoutEngine(string config, bool isYaml, IContext ctx)
	{
		// Given an invalid config with FocusLayoutEngine set
		ctx.PluginManager.LoadedPlugins.Returns([]);
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the layout engine
		bool result = YamlLoader.Load(ctx);

		// Then the layout engine is not loaded
		Assert.True(result);

		ILayoutEngine[]? engines = YamlLoaderTestUtils.GetLayoutEngines(ctx);
		Assert.Null(engines);
	}
}
