using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Yaml.Tests;

public class YamlLoader_UpdateRoutersTests
{
	public static TheoryData<string, bool> RouterConfig =>
		new()
		{
			{
				"""
					routers:
					  - type: windowClass
					    value: "class1"
					    workspace: "workspace1"
					  - type: processFileName
					    value: "process1"
					    workspace: "workspace2"
					  - type: title
					    value: "title1"
					    workspace: "workspace3"
					  - type: titleMatch
					    value: "titleMatch1"
					    workspace: "workspace4"
					""",
				true
			},
			{
				"""
					{
						"routers": [
							{
								"type": "windowClass",
								"value": "class1",
								"workspace": "workspace1"
							},
							{
								"type": "processFileName",
								"value": "process1",
								"workspace": "workspace2"
							},
							{
								"type": "title",
								"value": "title1",
								"workspace": "workspace3"
							},
							{
								"type": "titleMatch",
								"value": "titleMatch1",
								"workspace": "workspace4"
							}
						]
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(RouterConfig))]
	public void Load_Routers(string config, bool isYaml, IContext ctx)
	{
		// Given a valid config with routers set
		ctx.FileManager.FileExists(Arg.Is<string>(s => s.EndsWith(isYaml ? "yaml" : "json"))).Returns(true);
		ctx.FileManager.ReadAllText(Arg.Any<string>()).Returns(config);

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
					  - type: invalid
					    value: "class1"
					    workspace: "workspace1"
					""",
				true
			},
			{
				"""
					{
						"routers": [
							{
								"type": "invalid",
								"value": "class1",
								"workspace": "workspace1"
							}
						]
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(InvalidRouterConfig))]
	public void Load_InvalidRouters(string config, bool isYaml, IContext ctx)
	{
		// Given an invalid config with routers set
		ctx.FileManager.FileExists(Arg.Is<string>(s => s.EndsWith(isYaml ? "yaml" : "json"))).Returns(true);
		ctx.FileManager.ReadAllText(Arg.Any<string>()).Returns(config);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the routers are not updated
		Assert.True(result);
		ctx.RouterManager.DidNotReceive().AddWindowClassRoute(Arg.Any<string>(), Arg.Any<string>());
		ctx.RouterManager.DidNotReceive().AddProcessFileNameRoute(Arg.Any<string>(), Arg.Any<string>());
		ctx.RouterManager.DidNotReceive().AddTitleRoute(Arg.Any<string>(), Arg.Any<string>());
		ctx.RouterManager.DidNotReceive().AddTitleMatchRoute(Arg.Any<string>(), Arg.Any<string>());
	}
}
