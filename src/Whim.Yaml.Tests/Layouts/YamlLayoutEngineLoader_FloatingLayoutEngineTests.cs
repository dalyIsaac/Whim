using FluentAssertions;
using NSubstitute;
using Whim.FloatingWindow;
using Whim.TestUtils;
using Xunit;

namespace Whim.Yaml.Tests;

public class YamlLoader_LoadFloatingLayoutEngineTests
{
	public static TheoryData<string, bool> FloatingLayoutEngineConfig =>
		new()
		{
			{
				"""
					layout_engines:
					  entries:
					    - type: floating
					""",
				true
			},
			{
				"""
					{
						"layout_engines": {
							"entries": [
								{
									"type": "floating"
								}
							]
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(FloatingLayoutEngineConfig))]
	public void Load_FloatingLayoutEngine(string config, bool isYaml, IContext ctx)
	{
		// Given a valid config with FloatingLayoutEngine set
		ctx.PluginManager.LoadedPlugins.Returns([]);
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the layout engine
		bool result = YamlLoader.Load(ctx, showErrorWindow: false);

		// Then the layout engine is loaded
		Assert.True(result);

		ILayoutEngine[] engines = YamlLoaderTestUtils.GetLayoutEngines(ctx)!;
		Assert.Single(engines);
		engines[0].Should().BeEquivalentTo(new FloatingLayoutEngine(ctx, engines[0].Identity));
	}
}
