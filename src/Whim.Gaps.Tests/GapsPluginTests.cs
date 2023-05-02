using Moq;
using Xunit;

namespace Whim.Gaps.Test;

public class GapsPluginTests
{
	private class Wrapper
	{
		public Mock<IContext> Context { get; }
		public Mock<IWorkspaceManager> WorkspaceManager { get; }
		public GapsConfig GapsConfig { get; }

		public Wrapper()
		{
			Context = new();
			WorkspaceManager = new();
			GapsConfig = new() { InnerGap = 10, OuterGap = 10 };

			Context.SetupGet(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);
		}
	}

	[Fact]
	public void UpdateOuterGap_IncreasesOuterGapByDelta()
	{
		// Given
		Wrapper wrapper = new();
		GapsPlugin plugin = new(wrapper.Context.Object, wrapper.GapsConfig);

		// When
		plugin.UpdateOuterGap(10);

		// Then
		Assert.Equal(20, wrapper.GapsConfig.OuterGap);
		wrapper.WorkspaceManager.Verify(x => x.LayoutAllActiveWorkspaces(), Times.Once);
	}

	[Fact]
	public void UpdateInnerGap_IncreasesInnerGapByDelta()
	{
		// Given
		Wrapper wrapper = new();
		GapsPlugin plugin = new(wrapper.Context.Object, wrapper.GapsConfig);

		// When
		plugin.UpdateInnerGap(10);

		// Then
		Assert.Equal(20, wrapper.GapsConfig.InnerGap);
		wrapper.WorkspaceManager.Verify(x => x.LayoutAllActiveWorkspaces(), Times.Once);
	}

	[Fact]
	public void UpdateOuterGap_WithNegativeDelta_DecreasesOuterGapByDelta()
	{
		// Given
		Wrapper wrapper = new();
		GapsPlugin plugin = new(wrapper.Context.Object, wrapper.GapsConfig);

		// When
		plugin.UpdateOuterGap(-10);

		// Then
		Assert.Equal(0, wrapper.GapsConfig.OuterGap);
		wrapper.WorkspaceManager.Verify(x => x.LayoutAllActiveWorkspaces(), Times.Once);
	}

	[Fact]
	public void UpdateInnerGap_WithNegativeDelta_DecreasesInnerGapByDelta()
	{
		// Given
		Wrapper wrapper = new();
		GapsPlugin plugin = new(wrapper.Context.Object, wrapper.GapsConfig);

		// When
		plugin.UpdateInnerGap(-10);

		// Then
		Assert.Equal(0, wrapper.GapsConfig.InnerGap);
		wrapper.WorkspaceManager.Verify(x => x.LayoutAllActiveWorkspaces(), Times.Once);
	}

	[Fact]
	public void PluginCommands()
	{
		// Given
		Wrapper wrapper = new();
		GapsPlugin plugin = new(wrapper.Context.Object, wrapper.GapsConfig);

		// When
		IPluginCommands pluginCommands = plugin.PluginCommands;

		// Then
		Assert.Equal(4, pluginCommands.Commands.Count());
	}
}
