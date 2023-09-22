using AutoFixture;
using NSubstitute;
using System.Text.Json;
using Whim.TestUtils;
using Xunit;

namespace Whim.Gaps.Test;

public class GapsPluginCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		GapsConfig gapsConfig = fixture.Freeze<GapsConfig>();
		gapsConfig.InnerGap = 10;
		gapsConfig.OuterGap = 10;

		fixture.Inject(gapsConfig);
	}
}

public class GapsPluginTests
{
	[Theory, AutoSubstituteData<GapsPluginCustomization>]
	public void UpdateOuterGap_IncreasesOuterGapByDelta(IContext context, GapsConfig gapsConfig)
	{
		// Given
		GapsPlugin plugin = new(context, gapsConfig);

		// When
		plugin.UpdateOuterGap(10);

		// Then
		Assert.Equal(20, gapsConfig.OuterGap);
		context.WorkspaceManager.Received(1).LayoutAllActiveWorkspaces();
	}

	[Theory, AutoSubstituteData<GapsPluginCustomization>]
	public void UpdateInnerGap_IncreasesInnerGapByDelta(IContext context, GapsConfig gapsConfig)
	{
		// Given
		GapsPlugin plugin = new(context, gapsConfig);

		// When
		plugin.UpdateInnerGap(10);

		// Then
		Assert.Equal(20, gapsConfig.InnerGap);
		context.WorkspaceManager.Received(1).LayoutAllActiveWorkspaces();
	}

	[Theory, AutoSubstituteData<GapsPluginCustomization>]
	public void UpdateOuterGap_WithNegativeDelta_DecreasesOuterGapByDelta(IContext context, GapsConfig gapsConfig)
	{
		// Given
		GapsPlugin plugin = new(context, gapsConfig);

		// When
		plugin.UpdateOuterGap(-10);

		// Then
		Assert.Equal(0, gapsConfig.OuterGap);
		context.WorkspaceManager.Received(1).LayoutAllActiveWorkspaces();
	}

	[Theory, AutoSubstituteData<GapsPluginCustomization>]
	public void UpdateInnerGap_WithNegativeDelta_DecreasesInnerGapByDelta(IContext context, GapsConfig gapsConfig)
	{
		// Given
		GapsPlugin plugin = new(context, gapsConfig);

		// When
		plugin.UpdateInnerGap(-10);

		// Then
		Assert.Equal(0, gapsConfig.InnerGap);
		context.WorkspaceManager.Received(1).LayoutAllActiveWorkspaces();
	}

	[Theory, AutoSubstituteData<GapsPluginCustomization>]
	public void PluginCommands(IContext context, GapsConfig gapsConfig)
	{
		// Given
		GapsPlugin plugin = new(context, gapsConfig);

		// When
		IPluginCommands pluginCommands = plugin.PluginCommands;

		// Then
		Assert.Equal(4, pluginCommands.Commands.Count());
	}

	[Theory, AutoSubstituteData<GapsPluginCustomization>]
	public void SaveState(IContext context, GapsConfig gapsConfig)
	{
		// Given
		GapsPlugin plugin = new(context, gapsConfig);

		// When
		JsonElement? state = plugin.SaveState();

		// Then
		Assert.Null(state);
	}
}
