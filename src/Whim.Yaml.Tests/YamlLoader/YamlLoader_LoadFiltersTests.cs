using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Yaml.Tests;

public class YamlLoader_LoadFiltersTests
{
	public static TheoryData<string, bool> FilterConfig =>
		new()
		{
			{
				"""
					filters:
					  entries:
					  - type: window_class
					    value: class1
					  - type: process_file_name
					    value: process1
					  - type: title
					    value: title1
					  - type: title_regex
					    value: titleMatch1
					""",
				true
			},
			{
				"""
					{
						"filters": {
							"entries": [
								{
									"type": "window_class",
									"value": "class1"
								},
								{
									"type": "process_file_name",
									"value": "process1"
								},
								{
									"type": "title",
									"value": "title1"
								},
								{
									"type": "title_regex",
									"value": "titleMatch1"
								}
							]
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(FilterConfig))]
	public void Load_Filters(string config, bool isYaml, IContext ctx)
	{
		// Given a valid config with filters set
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx, showErrorWindow: false);

		// Then the filters should be updated
		Assert.True(result);
		ctx.FilterManager.Received(1).AddWindowClassFilter("class1");
		ctx.FilterManager.Received(1).AddProcessFileNameFilter("process1");
		ctx.FilterManager.Received(1).AddTitleFilter("title1");
		ctx.FilterManager.Received(1).AddTitleMatchFilter("titleMatch1");
	}

	public static TheoryData<string, bool> InvalidFilterConfig =>
		new()
		{
			{
				"""
					filters:
					  entries:
					  - type: invalid
					    value: class1
					""",
				true
			},
			{
				"""
					filters:
					  entries:
					  - type: windowClass
					    value: false
					""",
				true
			},
			{
				"""
					{
						"filters": {
							"entries": [
								{
									"type": "invalid",
									"value": "class1"
								}
							]
						}
					}
					""",
				false
			},
			{
				// Not technically invalid, but no filters are set
				"""
					{
						"filters": {}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(InvalidFilterConfig))]
	public void Load_InvalidFilters(string config, bool isYaml, IContext ctx)
	{
		// Given an invalid config with filters set
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx, showErrorWindow: false);

		// Then the filters should not be updated
		Assert.True(result);
		ctx.FilterManager.DidNotReceive().AddWindowClassFilter(Arg.Any<string>());
		ctx.FilterManager.DidNotReceive().AddProcessFileNameFilter(Arg.Any<string>());
		ctx.FilterManager.DidNotReceive().AddTitleFilter(Arg.Any<string>());
		ctx.FilterManager.DidNotReceive().AddTitleMatchFilter(Arg.Any<string>());
	}
}
