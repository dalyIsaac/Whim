using System.Collections.Immutable;
using FluentAssertions;
using NSubstitute;
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
					          - type: focus
					          - type: slice
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
												"type": "focus"
											},
											{
												"type": "slice",
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
		bool result = YamlLoader.Load(ctx, showErrorWindow: false);

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
			{
				"""
					workspaces:
					""",
				true
			},
			{
				"""
					{
						"workspaces": {}
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
		bool result = YamlLoader.Load(ctx, showErrorWindow: false);

		// Then the workspaces are loaded
		Assert.True(result);

		IWorkspace[] workspaces = YamlLoaderTestUtils.GetWorkspaces(ctx)!;
		Assert.Empty(workspaces);
	}

	public static TheoryData<string, bool> WorkspacesMonitorConfig =>
		new()
		{
			{
				"""
					workspaces:
					  entries:
					    - name: workspace1
					      monitors: [0, 1]
					    - name: workspace2
					      monitors: [2]
					    - name: workspace3
					      monitors: []
					""",
				true
			},
			{
				"""
					{
						"workspaces": {
							"entries": [
								{
									"name": "workspace1",
									"monitors": [0, 1]
								},
								{
									"name": "workspace2",
									"monitors": [2]
								},
								{
									"name": "workspace3",
									"monitors": []
								}
							]
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(WorkspacesMonitorConfig))]
	internal void Load_WorkspacesWithMonitors(string config, bool isYaml, IContext ctx)
	{
		// Given a valid config with workspaces and monitor indices
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the workspaces
		bool result = YamlLoader.Load(ctx, showErrorWindow: false);

		// Then the workspaces and monitor indices are loaded correctly
		Assert.True(result);

		// Verify the expected transforms were executed
		var received = ctx.Store.ReceivedCalls().ToArray();
		Assert.Equal(3, received.Length);

		// Verify the monitor indices.
		AddWorkspaceTransform[] transforms = received
			.Select(c => c.GetArguments()[0])
			.OfType<AddWorkspaceTransform>()
			.ToArray();

		transforms[0].MonitorIndices.Should().BeEquivalentTo([0, 1]);
		transforms[1].MonitorIndices.Should().BeEquivalentTo([2]);
		transforms[2].MonitorIndices.Should().BeEmpty();
	}
}
