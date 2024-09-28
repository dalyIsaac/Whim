using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Yaml.Tests;

public class YamlLoader_LoadRoutersTests
{
	public static TheoryData<string, bool> RouterConfig =>
		new()
		{
			{
				"""
					routers:
					  entries:
					  - type: window_class
					    value: class1
					    workspace_name: workspace1
					  - type: process_file_name
					    value: process1
					    workspace_name: workspace2
					  - type: title
					    value: title1
					    workspace_name: workspace3
					  - type: title_regex
					    value: titleMatch1
					    workspace_name: workspace4
					""",
				true
			},
			{
				"""
					{
						"routers": {
							"entries": [
								{
									"type": "window_class",
									"value": "class1",
									"workspace_name": "workspace1"
								},
								{
									"type": "process_file_name",
									"value": "process1",
									"workspace_name": "workspace2"
								},
								{
									"type": "title",
									"value": "title1",
									"workspace_name": "workspace3"
								},
								{
									"type": "title_regex",
									"value": "titleMatch1",
									"workspace_name": "workspace4"
								}
							]
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(RouterConfig))]
	public void Load_Routers(string config, bool isYaml, IContext ctx)
	{
		// Given a valid config with routers set
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the routers are updated
		Assert.True(result);
		ctx.RouterManager.Received(1).AddWindowClassRoute("class1", "workspace1");
		ctx.RouterManager.Received(1).AddProcessFileNameRoute("process1", "workspace2");
		ctx.RouterManager.Received(1).AddTitleRoute("title1", "workspace3");
		ctx.RouterManager.Received(1).AddTitleMatchRoute("titleMatch1", "workspace4");
	}

	public static TheoryData<string, bool> InvalidRouterConfig =>
		new()
		{
			{
				"""
					routers:
					  entries:
					  - type: invalid
					    value: class1
					    workspace_name: workspace1
					""",
				true
			},
			{
				"""
					{
						"routers": {
							"entries": [
								{
									"type": "invalid",
									"value": "class1",
									"workspace_name": "workspace1"
								}
							]
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(InvalidRouterConfig))]
	public void Load_InvalidRouters(string config, bool isYaml, IContext ctx)
	{
		// Given an invalid config with routers set
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the routers are not updated
		Assert.True(result);
		ctx.RouterManager.DidNotReceive().AddWindowClassRoute(Arg.Any<string>(), Arg.Any<string>());
		ctx.RouterManager.DidNotReceive().AddProcessFileNameRoute(Arg.Any<string>(), Arg.Any<string>());
		ctx.RouterManager.DidNotReceive().AddTitleRoute(Arg.Any<string>(), Arg.Any<string>());
		ctx.RouterManager.DidNotReceive().AddTitleMatchRoute(Arg.Any<string>(), Arg.Any<string>());
	}

	[Theory]
	[InlineAutoSubstituteData<YamlLoaderCustomization>(
		"route_to_launched_workspace",
		RouterOptions.RouteToLaunchedWorkspace
	)]
	[InlineAutoSubstituteData<YamlLoaderCustomization>(
		"route_to_active_workspace",
		RouterOptions.RouteToActiveWorkspace
	)]
	[InlineAutoSubstituteData<YamlLoaderCustomization>(
		"route_to_last_tracked_active_workspace",
		RouterOptions.RouteToLastTrackedActiveWorkspace
	)]
	public void Load_RouterBehavior(string routerOption, RouterOptions expected, IContext ctx)
	{
		// Given a valid config with a router option set
		string config = "routers:\n" + "  routing_behavior: " + routerOption + "\n";
		ctx.FileManager.FileExists(Arg.Is<string>(s => s.EndsWith("yaml"))).Returns(true);
		ctx.FileManager.ReadAllText(Arg.Any<string>()).Returns(config);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the router option is updated
		Assert.True(result);
		ctx.RouterManager.Received(1).RouterOptions = expected;
	}

	[Theory, AutoSubstituteData<YamlLoaderCustomization>]
	public void Load_InvalidRouterBehavior(IContext ctx)
	{
		// Given an invalid config with a router option set
		string config = """
			routers:
			  routing_behavior: route_to_bogus_workspace
			""";
		ctx.FileManager.FileExists(Arg.Is<string>(s => s.EndsWith("yaml"))).Returns(true);
		ctx.FileManager.ReadAllText(Arg.Any<string>()).Returns(config);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the router option is not updated
		Assert.True(result);
		ctx.RouterManager.DidNotReceive().RouterOptions = Arg.Any<RouterOptions>();
	}
}
