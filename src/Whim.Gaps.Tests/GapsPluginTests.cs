using Moq;
using Xunit;

namespace Whim.Gaps.Test;

public class GapsPluginTests
{
	private static (Mock<IContext>, Mock<IWorkspaceManager>, GapsConfig) CreateMocks()
	{
		Mock<IContext> context = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		GapsConfig gapsConfig = new() { InnerGap = 10, OuterGap = 10 };

		context.SetupGet(x => x.WorkspaceManager).Returns(workspaceManager.Object);

		return (context, workspaceManager, gapsConfig);
	}

	[Fact]
	public void UpdateOuterGap_IncreasesOuterGapByDelta()
	{
		// Arrange
		(Mock<IContext> context, Mock<IWorkspaceManager> workspaceManager, GapsConfig gapsConfig) = CreateMocks();
		GapsPlugin plugin = new(context.Object, gapsConfig);

		// Act
		plugin.UpdateOuterGap(10);

		// Assert
		Assert.Equal(20, gapsConfig.OuterGap);
		workspaceManager.Verify(x => x.LayoutAllActiveWorkspaces(), Times.Once);
	}

	[Fact]
	public void UpdateInnerGap_IncreasesInnerGapByDelta()
	{
		// Arrange
		(Mock<IContext> context, Mock<IWorkspaceManager> workspaceManager, GapsConfig gapsConfig) = CreateMocks();
		GapsPlugin plugin = new(context.Object, gapsConfig);

		// Act
		plugin.UpdateInnerGap(10);

		// Assert
		Assert.Equal(20, gapsConfig.InnerGap);
		workspaceManager.Verify(x => x.LayoutAllActiveWorkspaces(), Times.Once);
	}

	[Fact]
	public void UpdateOuterGap_WithNegativeDelta_DecreasesOuterGapByDelta()
	{
		// Arrange
		(Mock<IContext> context, Mock<IWorkspaceManager> workspaceManager, GapsConfig gapsConfig) = CreateMocks();
		GapsPlugin plugin = new(context.Object, gapsConfig);

		// Act
		plugin.UpdateOuterGap(-10);

		// Assert
		Assert.Equal(0, gapsConfig.OuterGap);
		workspaceManager.Verify(x => x.LayoutAllActiveWorkspaces(), Times.Once);
	}

	[Fact]
	public void UpdateInnerGap_WithNegativeDelta_DecreasesInnerGapByDelta()
	{
		// Arrange
		(Mock<IContext> context, Mock<IWorkspaceManager> workspaceManager, GapsConfig gapsConfig) = CreateMocks();
		GapsPlugin plugin = new(context.Object, gapsConfig);

		// Act
		plugin.UpdateInnerGap(-10);

		// Assert
		Assert.Equal(0, gapsConfig.InnerGap);
		workspaceManager.Verify(x => x.LayoutAllActiveWorkspaces(), Times.Once);
	}
}
