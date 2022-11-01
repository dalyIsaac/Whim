using Moq;
using Xunit;

namespace Whim.Gaps.Tests;

public class GapsCommandsTests
{
	private static (Mock<IGapsPlugin>, GapsConfig) CreateMocks()
	{
		Mock<IGapsPlugin> plugin = new();
		GapsConfig config = new();

		plugin.SetupGet(p => p.GapsConfig).Returns(config);

		return (plugin, config);
	}

	[Fact]
	public void IncreaseOuterGapCommand()
	{
		(Mock<IGapsPlugin> plugin, GapsConfig config) = CreateMocks();
		GapsCommands commands = new(plugin.Object);

		commands.IncreaseOuterGapCommand.Command.TryExecute();

		plugin.Verify(p => p.UpdateOuterGap(config.DefaultOuterDelta), Times.Once);
	}

	[Fact]
	public void DecreaseOuterGapCommand()
	{
		(Mock<IGapsPlugin> plugin, GapsConfig config) = CreateMocks();
		GapsCommands commands = new(plugin.Object);

		commands.DecreaseOuterGapCommand.Command.TryExecute();

		plugin.Verify(p => p.UpdateOuterGap(-config.DefaultOuterDelta), Times.Once);
	}

	[Fact]
	public void IncreaseInnerGapCommand()
	{
		(Mock<IGapsPlugin> plugin, GapsConfig config) = CreateMocks();
		GapsCommands commands = new(plugin.Object);

		commands.IncreaseInnerGapCommand.Command.TryExecute();

		plugin.Verify(p => p.UpdateInnerGap(config.DefaultInnerDelta), Times.Once);
	}

	[Fact]
	public void DecreaseInnerGapCommand()
	{
		(Mock<IGapsPlugin> plugin, GapsConfig config) = CreateMocks();
		GapsCommands commands = new(plugin.Object);

		commands.DecreaseInnerGapCommand.Command.TryExecute();

		plugin.Verify(p => p.UpdateInnerGap(-config.DefaultInnerDelta), Times.Once);
	}
}
