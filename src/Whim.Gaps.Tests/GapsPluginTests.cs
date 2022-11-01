using Moq;
using Xunit;

namespace Whim.Gaps.Test;

public class GapsPluginTests
{
	private static (Mock<IConfigContext>, Mock<IWorkspaceManager>, GapsConfig) CreateMocks()
	{
		Mock<IConfigContext> configContext = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		GapsConfig gapsConfig = new() { InnerGap = 10, OuterGap = 10 };

		configContext.SetupGet(x => x.WorkspaceManager).Returns(workspaceManager.Object);

		return (configContext, workspaceManager, gapsConfig);
	}

	[Fact]
	public void UpdateOuterGap_IncreasesOuterGapByDelta()
	{
		// Arrange
		(Mock<IConfigContext> configContext, Mock<IWorkspaceManager> workspaceManager, GapsConfig gapsConfig) = CreateMocks();
		GapsPlugin plugin = new(configContext.Object, gapsConfig);

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
		(Mock<IConfigContext> configContext, Mock<IWorkspaceManager> workspaceManager, GapsConfig gapsConfig) = CreateMocks();
		GapsPlugin plugin = new(configContext.Object, gapsConfig);

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
		(Mock<IConfigContext> configContext, Mock<IWorkspaceManager> workspaceManager, GapsConfig gapsConfig) = CreateMocks();
		GapsPlugin plugin = new(configContext.Object, gapsConfig);

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
		(Mock<IConfigContext> configContext, Mock<IWorkspaceManager> workspaceManager, GapsConfig gapsConfig) = CreateMocks();
		GapsPlugin plugin = new(configContext.Object, gapsConfig);

		// Act
		plugin.UpdateInnerGap(-10);

		// Assert
		Assert.Equal(0, gapsConfig.InnerGap);
		workspaceManager.Verify(x => x.LayoutAllActiveWorkspaces(), Times.Once);
	}
}
