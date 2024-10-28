using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Yaml.Tests;

public class YamlLayoutEngineLoaderTests
{
	public static TheoryData<string, bool> NoLayoutEnginesConfig =>
		new()
		{
			{
				"""
					layout_engines:
					  entries: []
					""",
				true
			},
			{
				"""
					{
						"layout_engines": {
							"entries": []
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(NoLayoutEnginesConfig))]
	public void Load_NoLayoutEngines(string config, bool isYaml, IContext ctx)
	{
		// Given an invalid config with FocusLayoutEngine set
		ctx.PluginManager.LoadedPlugins.Returns([]);
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the layout engine
		bool result = YamlLoader.Load(ctx, showErrorWindow: false);

		// Then the layout engine is not loaded
		Assert.True(result);

		ILayoutEngine[]? engines = YamlLoaderTestUtils.GetLayoutEngines(ctx);
		Assert.Null(engines);
	}
}
