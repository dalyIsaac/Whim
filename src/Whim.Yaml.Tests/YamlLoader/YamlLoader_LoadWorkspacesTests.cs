using System.Collections.Immutable;
using FluentAssertions;
using Whim.SliceLayout;
using Whim.TestUtils;
using Xunit;

namespace Whim.Yaml.Tests;

public class YamlLoader_LoadWorkspacesTests
{
	public static TheoryData<string, bool> WorkspacesConfig =>
		new()
		{
			{
				"""
					workspaces:
					  entries:
					    - name: workspace1
					    - name: workspace2
					    - name: workspace3
					      layout_engines:
					        entries:
					          - type: FocusLayoutEngine
					          - type: SliceLayoutEngine
					            variant:
					              type: column
					""",
				true
			},
			{
				"""
					{
						"workspaces": {
							"entries": [
								{
									"name": "workspace1"
								},
								{
									"name": "workspace2"
								},
								{
									"name": "workspace3",
									"layout_engines": {
										"entries": [
											{
												"type": "FocusLayoutEngine"
											},
											{
												"type": "SliceLayoutEngine",
												"variant": {
													"type": "column"
												}
											}
										]
									}
								}
							]
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(WorkspacesConfig))]
	public void Load_Workspaces(string config, bool isYaml, IContext ctx, ISliceLayoutPlugin plugin)
	{
		// Given a valid config with workspaces set
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the workspaces
		bool result = YamlLoader.Load(ctx);

		// Then the workspaces are loaded
		Assert.True(result);

		IWorkspace[] workspaces = YamlLoaderTestUtils.GetWorkspaces(ctx)!;
		Assert.Equal(3, workspaces.Length);

		// ...with the expected names...
#pragma warning disable CS0618 // Type or member is obsolete
		Assert.Equal("workspace1", workspaces[0].Name);
		Assert.Equal("workspace2", workspaces[1].Name);
		Assert.Equal("workspace3", workspaces[2].Name);
#pragma warning restore CS0618 // Type or member is obsolete

		// ...and the expected layout engines.
		Assert.Empty(workspaces[0].LayoutEngines);
		Assert.Empty(workspaces[1].LayoutEngines);

		var engines = workspaces[2].LayoutEngines;
		ImmutableList<ILayoutEngine> expectedLayoutEngines =
		[
			new FocusLayoutEngine(engines[0].Identity),
			SliceLayouts.CreateColumnLayout(ctx, plugin, engines[1].Identity),
		];

		expectedLayoutEngines.Should().BeEquivalentTo(engines);
	}

	public static TheoryData<string, bool> NoWorkspacesConfig =>
		new()
		{
			{
				"""
					workspaces:
					  entries: []
					""",
				true
			},
			{
				"""
					{
						"workspaces": {
							"entries": []
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(NoWorkspacesConfig))]
	public void Load_NoWorkspaces(string config, bool isYaml, IContext ctx)
	{
		// Given a valid config with no workspaces set
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the workspaces
		bool result = YamlLoader.Load(ctx);

		// Then the workspaces are loaded
		Assert.True(result);

		IWorkspace[] workspaces = YamlLoaderTestUtils.GetWorkspaces(ctx)!;
		Assert.Empty(workspaces);
	}
}
