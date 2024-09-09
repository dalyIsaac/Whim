using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Yaml.Tests;

public class YamlLoader_UpdateFiltersTests
{
	public static TheoryData<string, bool> FilterConfig =>
		new()
		{
			{
				"""
					filters:
					  entries:
					  - filter_type: window_class
					    value: class1
					  - filter_type: process_file_name
					    value: process1
					  - filter_type: title
					    value: title1
					  - filter_type: title_regex
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
									"filter_type": "window_class",
									"value": "class1"
								},
								{
									"filter_type": "process_file_name",
									"value": "process1"
								},
								{
									"filter_type": "title",
									"value": "title1"
								},
								{
									"filter_type": "title_regex",
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
		ctx.FileManager.FileExists(Arg.Is<string>(s => s.EndsWith(isYaml ? "yaml" : "json"))).Returns(true);
		ctx.FileManager.ReadAllText(Arg.Any<string>()).Returns(config);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

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
					  - filter_type: invalid
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
									"filter_type": "invalid",
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
		ctx.FileManager.FileExists(Arg.Is<string>(s => s.EndsWith(isYaml ? "yaml" : "json"))).Returns(true);
		ctx.FileManager.ReadAllText(Arg.Any<string>()).Returns(config);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the filters should not be updated
		Assert.True(result);
		ctx.FilterManager.DidNotReceive().AddWindowClassFilter(Arg.Any<string>());
		ctx.FilterManager.DidNotReceive().AddProcessFileNameFilter(Arg.Any<string>());
		ctx.FilterManager.DidNotReceive().AddTitleFilter(Arg.Any<string>());
		ctx.FilterManager.DidNotReceive().AddTitleMatchFilter(Arg.Any<string>());
	}
}
