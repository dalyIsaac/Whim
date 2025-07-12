using System.Text.Json;
using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Gaps.Tests;

public class GapsPluginTests
{
	private class Customization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			GapsConfig gapsConfig = fixture.Freeze<GapsConfig>();
			gapsConfig.InnerGap = 10;
			gapsConfig.OuterGap = 10;

			fixture.Inject(gapsConfig);
		}
	}

	[Theory, AutoSubstituteData<Customization>]
	public void UpdateOuterGap_IncreasesOuterGapByDelta(IContext context, GapsConfig gapsConfig)
	{
		// Given
		GapsPlugin plugin = new(context, gapsConfig);

		// When
		plugin.UpdateOuterGap(10);

		// Then
		Assert.Equal(20, gapsConfig.OuterGap);
		context.Store.Received(1).Dispatch(Arg.Any<LayoutAllActiveWorkspacesTransform>());
	}

	[Theory, AutoSubstituteData<Customization>]
	public void UpdateInnerGap_IncreasesInnerGapByDelta(IContext context, GapsConfig gapsConfig)
	{
		// Given
		GapsPlugin plugin = new(context, gapsConfig);

		// When
		plugin.UpdateInnerGap(10);

		// Then
		Assert.Equal(20, gapsConfig.InnerGap);
		context.Store.Received(1).Dispatch(Arg.Any<LayoutAllActiveWorkspacesTransform>());
	}

	[Theory, AutoSubstituteData<Customization>]
	public void UpdateOuterGap_WithNegativeDelta_DecreasesOuterGapByDelta(IContext context, GapsConfig gapsConfig)
	{
		// Given
		GapsPlugin plugin = new(context, gapsConfig);

		// When
		plugin.UpdateOuterGap(-10);

		// Then
		Assert.Equal(0, gapsConfig.OuterGap);
		context.Store.Received(1).Dispatch(Arg.Any<LayoutAllActiveWorkspacesTransform>());
	}

	[Theory, AutoSubstituteData<Customization>]
	public void UpdateInnerGap_WithNegativeDelta_DecreasesInnerGapByDelta(IContext context, GapsConfig gapsConfig)
	{
		// Given
		GapsPlugin plugin = new(context, gapsConfig);

		// When
		plugin.UpdateInnerGap(-10);

		// Then
		Assert.Equal(0, gapsConfig.InnerGap);
		context.Store.Received(1).Dispatch(Arg.Any<LayoutAllActiveWorkspacesTransform>());
	}

	[Theory, AutoSubstituteData<Customization>]
	public void PluginCommands(IContext context, GapsConfig gapsConfig)
	{
		// Given
		GapsPlugin plugin = new(context, gapsConfig);

		// When
		IPluginCommands pluginCommands = plugin.PluginCommands;

		// Then
		Assert.Equal(4, pluginCommands.Commands.Count());
	}

	[Theory, AutoSubstituteData<Customization>]
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
