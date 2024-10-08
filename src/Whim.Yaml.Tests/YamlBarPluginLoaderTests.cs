using FluentAssertions;
using NSubstitute;
using Whim.Bar;
using Whim.TestUtils;
using Whim.TreeLayout;
using Whim.TreeLayout.Bar;
using Xunit;

namespace Whim.Yaml.Tests;

public class YamlBarPluginLoaderTests
{
	private static BarPlugin GetBarPlugin(IContext ctx, int idx = 0)
	{
		BarPlugin plugin = (ctx.PluginManager.ReceivedCalls().ElementAt(idx).GetArguments()[0] as BarPlugin)!;
		plugin.Config.Initialize();
		return plugin;
	}

	public static TheoryData<string, bool> BarIsEnabledConfig =>
		new()
		{
			// YAML, is_enabled: true
			{
				"""
					plugins:
					  bar:
					    is_enabled: true
					""",
				// Is YAML
				true
			},
			// JSON, is_enabled: true
			{
				"""
					{
						"plugins": {
							"bar": {
								"is_enabled": true
							}
						}
					}
					""",
				// Is JSON
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(BarIsEnabledConfig))]
	public void LoadBarPlugin_IsEnabled(string schema, bool isYaml, IContext ctx)
	{
		// Given a valid config with the bar plugin
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and the bar plugin is loaded
		Assert.True(result);
		ctx.PluginManager.Received(1).AddPlugin(Arg.Is<BarPlugin>(p => p.Config.Height == 30));
	}

	public static TheoryData<string, bool> BarIsNotEnabledConfig =>
		new()
		{
			// YAML, is_enabled: false
			{
				"""
					plugins:
					  bar:
					    is_enabled: false
					""",
				true
			},
			// JSON, is_enabled: false
			{
				"""
					{
						"plugins": {
							"bar": {
								"is_enabled": false
							}
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(BarIsNotEnabledConfig))]
	public void LoadBarPlugin_IsNotEnabled(string schema, bool isYaml, IContext ctx)
	{
		// Given a valid config with the bar plugin that is not enabled
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and the bar plugin is not loaded
		Assert.True(result);
		ctx.PluginManager.DidNotReceive().AddPlugin(Arg.Any<BarPlugin>());
	}

	public static TheoryData<string, bool> BarPluginIsNotValidConfig =>
		new()
		{
			// YAML
			{
				"""
					plugins:
					  bar:
					    height: "not an int"
					""",
				true
			},
			// JSON
			{
				"""
					{
						"plugins": {
							"bar": {
								"height": "not an int"
							}
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(BarPluginIsNotValidConfig))]
	public void LoadBarPlugin_ConfigIsNotValid_PluginIsNotLoaded(string schema, bool isYaml, IContext ctx)
	{
		// Given a config with the bar plugin that is not valid
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is false, and the bar plugin is not loaded
		Assert.True(result);
		ctx.PluginManager.DidNotReceive().AddPlugin(Arg.Any<BarPlugin>());
	}

	public static TheoryData<string, bool> BarPluginHeightConfig =>
		new()
		{
			// YAML
			{
				"""
					plugins:
					  bar:
					    height: 42
					""",
				true
			},
			// JSON
			{
				"""
					{
						"plugins": {
							"bar": {
								"height": 42
							}
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(BarPluginHeightConfig))]
	public void LoadBarPlugin_ConfigHasHeight(string schema, bool isYaml, IContext ctx)
	{
		// Given a valid config with the bar plugin and a height
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and the bar plugin has the height
		Assert.True(result);
		ctx.PluginManager.Received(1).AddPlugin(Arg.Is<BarPlugin>(p => p.Config.Height == 42));
	}

	public static TheoryData<string, bool> ActiveLayoutBarWidgetConfig =>
		new()
		{
			// YAML
			{
				"""
					plugins:
					  bar:
					    left_components:
					      entries:
					        - type: ActiveLayoutWidget
					""",
				true
			},
			// JSON
			{
				"""
					{
						"plugins": {
							"bar": {
								"left_components": {
									"entries": [
										{
											"type": "ActiveLayoutWidget"
										}
									]
								}
							}
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(ActiveLayoutBarWidgetConfig))]
	public void LoadBarPlugin_ConfigHasActiveLayoutWidget(string schema, bool isYaml, IContext ctx)
	{
		// Given a valid config with the bar plugin and an active layout widget
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and the bar plugin has the active layout widget
		Assert.True(result);
		ctx.PluginManager.Received(1).AddPlugin(Arg.Is<BarPlugin>(p => p.Config.LeftComponents.Count == 1));

		ActiveLayoutComponent activeLayoutComponent = (ActiveLayoutComponent)GetBarPlugin(ctx).Config.LeftComponents[0];
		Assert.Equal(new ActiveLayoutComponent(), activeLayoutComponent);
	}

	public static TheoryData<string, bool> BatteryBarWidgetConfig =>
		new()
		{
			// YAML
			{
				"""
					plugins:
					  bar:
					    left_components:
					      entries:
					        - type: BatteryWidget
					""",
				true
			},
			// JSON
			{
				"""
					{
						"plugins": {
							"bar": {
								"left_components": {
									"entries": [
										{
											"type": "BatteryWidget"
										}
									]
								}
							}
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(BatteryBarWidgetConfig))]
	public void LoadBarPlugin_ConfigHasBatteryWidget(string schema, bool isYaml, IContext ctx)
	{
		// Given a valid config with the bar plugin and a battery widget
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and the bar plugin has the battery widget
		Assert.True(result);
		ctx.PluginManager.Received(1).AddPlugin(Arg.Is<BarPlugin>(p => p.Config.LeftComponents.Count == 1));

		BatteryComponent batteryComponent = (BatteryComponent)GetBarPlugin(ctx).Config.LeftComponents[0];
		Assert.Equal(new BatteryComponent(), batteryComponent);
	}

	public static TheoryData<string, bool, int, string> DateAndTimeBarWidgetConfig =>
		new()
		{
			// YAML, 1000ms, full date
			{
				"""
					plugins:
					  bar:
					    left_components:
					      entries:
					        - type: DateTimeWidget
					          format: "yyyy-MM-dd HH:mm:ss"
					""",
				true,
				1000,
				"yyyy-MM-dd HH:mm:ss"
			},
			// JSON, 1000ms, full date
			{
				"""
					{
						"plugins": {
							"bar": {
								"left_components": {
									"entries": [
										{
											"type": "DateTimeWidget",
											"format": "yyyy-MM-dd HH:mm:ss"
										}
									]
								}
							}
						}
					}
					""",
				false,
				1000,
				"yyyy-MM-dd HH:mm:ss"
			},
			// YAML, 500ms, short date
			{
				"""
					plugins:
					  bar:
					    left_components:
					      entries:
					        - type: DateTimeWidget
					          interval: 500
					          format: "yyyy-MM-dd"
					""",
				true,
				500,
				"yyyy-MM-dd"
			},
			// JSON, 500ms, short date
			{
				"""
					{
						"plugins": {
							"bar": {
								"left_components": {
									"entries": [
										{
											"type": "DateTimeWidget",
											"interval": 500,
											"format": "yyyy-MM-dd"
										}
									]
								}
							}
						}
					}
					""",
				false,
				500,
				"yyyy-MM-dd"
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(DateAndTimeBarWidgetConfig))]
	public void LoadBarPlugin_ConfigHasDateTimeBarWidget(
		string schema,
		bool isYaml,
		int interval,
		string format,
		IContext ctx
	)
	{
		// Given a valid config with the bar plugin and a date and time widget
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and the bar plugin has the date and time widget
		Assert.True(result);
		ctx.PluginManager.Received(1).AddPlugin(Arg.Is<BarPlugin>(p => p.Config.LeftComponents.Count == 1));

		DateTimeComponent dateTimeComponent = (DateTimeComponent)GetBarPlugin(ctx).Config.LeftComponents[0];
		Assert.Equal(new DateTimeComponent(interval, format), dateTimeComponent);
	}

	public static TheoryData<string, bool, bool> FocusedWindowBarWidgetConfig =>
		new()
		{
			// YAML, show full name
			{
				"""
					plugins:
					  bar:
					    left_components:
					      entries:
					        - type: FocusedWindowWidget
					          shorten_title: false
					""",
				true,
				false
			},
			// JSON, show full name
			{
				"""
					{
						"plugins": {
							"bar": {
								"left_components": {
									"entries": [
										{
											"type": "FocusedWindowWidget",
											"shorten_title": false
										}
									]
								}
							}
						}
					}
					""",
				false,
				false
			},
			// YAML, shorten title
			{
				"""
					plugins:
					  bar:
					    left_components:
					      entries:
					        - type: FocusedWindowWidget
					          shorten_title: true
					""",
				true,
				true
			},
			// JSON, shorten title
			{
				"""
					{
						"plugins": {
							"bar": {
								"left_components": {
									"entries": [
										{
											"type": "FocusedWindowWidget",
											"shorten_title": true
										}
									]
								}
							}
						}
					}
					""",
				false,
				true
			},
			// YAML, show full name (default)
			{
				"""
					plugins:
					  bar:
					    left_components:
					      entries:
					        - type: FocusedWindowWidget
					""",
				true,
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(FocusedWindowBarWidgetConfig))]
	public void LoadBarPlugin_ConfigHasFocusedWindowWidget(string schema, bool isYaml, bool shortenTitle, IContext ctx)
	{
		// Given a valid config with the bar plugin and a focused window widget
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and the bar plugin has the focused window widget
		Assert.True(result);
		ctx.PluginManager.Received(1).AddPlugin(Arg.Is<BarPlugin>(p => p.Config.LeftComponents.Count == 1));

		FocusedWindowComponent focusedWindowComponent = (FocusedWindowComponent)
			GetBarPlugin(ctx).Config.LeftComponents[0];
		Assert.IsType<FocusedWindowComponent>(focusedWindowComponent);

		// Verify the title type.
		string longTitle = "Window title | Very Long Application Name";
		string shortTitle = "Window title";
		string expectedTitle = shortenTitle ? shortTitle : longTitle;

		var focusedWindow = Substitute.For<IWindow>();
		focusedWindow.Title.Returns(longTitle);

		Assert.Equal(expectedTitle, focusedWindowComponent.GetTitle!(focusedWindow));
	}

	public static TheoryData<string, bool> WorkspaceBarWidgetConfig =>
		new()
		{
			// YAML
			{
				"""
					plugins:
					  bar:
					    left_components:
					      entries:
					        - type: WorkspaceWidget
					""",
				true
			},
			// JSON
			{
				"""
					{
						"plugins": {
							"bar": {
								"left_components": {
									"entries": [
										{
											"type": "WorkspaceWidget"
										}
									]
								}
							}
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(WorkspaceBarWidgetConfig))]
	public void LoadBarPlugin_ConfigHasWorkspaceWidget(string schema, bool isYaml, IContext ctx)
	{
		// Given a valid config with the bar plugin and a workspace widget
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and the bar plugin has the workspace widget
		Assert.True(result);
		ctx.PluginManager.Received(1).AddPlugin(Arg.Is<BarPlugin>(p => p.Config.LeftComponents.Count == 1));

		WorkspaceComponent workspaceComponent = (WorkspaceComponent)GetBarPlugin(ctx).Config.LeftComponents[0];
		Assert.Equal(new WorkspaceComponent(), workspaceComponent);
	}

	public static TheoryData<string, bool> TreeLayoutEngineBarWidgetConfig =>
		new()
		{
			// YAML
			{
				"""
					plugins:
					  bar:
					    left_components:
					      entries:
					        - type: TreeLayoutWidget
					""",
				true
			},
			// JSON
			{
				"""
					{
						"plugins": {
							"bar": {
								"left_components": {
									"entries": [
										{
											"type": "TreeLayoutWidget"
										}
									]
								}
							}
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(TreeLayoutEngineBarWidgetConfig))]
	public void LoadBarPlugin_ConfigHasTreeLayoutWidget(string schema, bool isYaml, IContext ctx)
	{
		// Given a valid config with the bar plugin and a tree layout engine widget
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and the bar plugin has the tree layout engine widget
		Assert.True(result);
		ctx.PluginManager.Received(1).AddPlugin(Arg.Is<BarPlugin>(p => p.Config.LeftComponents.Count == 1));

		TreeLayoutComponent treeLayoutEngineComponent = (TreeLayoutComponent)
			GetBarPlugin(ctx, 4).Config.LeftComponents[0];

		new TreeLayoutComponent(new TreeLayoutPlugin(ctx)).Should().BeEquivalentTo(treeLayoutEngineComponent);
	}

	public static TheoryData<string, bool> MultipleBarComponentsConfig =>
		new()
		{
			// YAML
			{
				"""
					plugins:
					  bar:
					    left_components:
					      entries:
					        - type: ActiveLayoutWidget
					    center_components:
					      entries:
					        - type: DateTimeWidget
					          format: "yyyy-MM-dd HH:mm:ss"
					    right_components:
					      entries:
					        - type: FocusedWindowWidget
					        - type: BatteryWidget
					""",
				true
			},
			// JSON
			{
				"""
					{
						"plugins": {
							"bar": {
								"left_components": {
									"entries": [
										{
											"type": "ActiveLayoutWidget"
										}
									]
								},
								"center_components": {
									"entries": [
										{
											"type": "DateTimeWidget",
											"format": "yyyy-MM-dd HH:mm:ss"
										}
									]
								},
								"right_components": {
									"entries": [
										{
											"type": "FocusedWindowWidget"
										},
										{
											"type": "BatteryWidget"
										}
									]
								}
							}
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(MultipleBarComponentsConfig))]
	public void LoadBarPlugin_ConfigHasMultipleComponents(string schema, bool isYaml, IContext ctx)
	{
		// Given a valid config with the bar plugin and multiple components
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and the bar plugin has the multiple components
		Assert.True(result);
		ctx.PluginManager.Received(1).AddPlugin(Arg.Any<BarPlugin>());

		BarPlugin plugin = GetBarPlugin(ctx);

		Assert.Single(plugin.Config.LeftComponents);
		Assert.Single(plugin.Config.CenterComponents);
		Assert.Equal(2, plugin.Config.RightComponents.Count);

		ActiveLayoutComponent activeLayoutComponent = (ActiveLayoutComponent)plugin.Config.LeftComponents[0];
		Assert.Equal(new ActiveLayoutComponent(), activeLayoutComponent);

		DateTimeComponent dateTimeComponent = (DateTimeComponent)plugin.Config.CenterComponents[0];
		Assert.Equal(new DateTimeComponent(1000, "yyyy-MM-dd HH:mm:ss"), dateTimeComponent);

		FocusedWindowComponent focusedWindowComponent = (FocusedWindowComponent)plugin.Config.RightComponents[0];
		Assert.Equal(new FocusedWindowComponent(FocusedWindowWidget.GetTitle), focusedWindowComponent);

		BatteryComponent batteryComponent = (BatteryComponent)plugin.Config.RightComponents[1];
		Assert.Equal(new BatteryComponent(), batteryComponent);
	}
}
