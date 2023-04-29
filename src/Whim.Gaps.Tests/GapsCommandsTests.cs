using Moq;
using Whim.TestUtilities;
using Xunit;

namespace Whim.Gaps.Tests;

public class GapsCommandsTests
{
	private class Wrapper
	{
		public Mock<IGapsPlugin> Plugin { get; }
		public GapsConfig Config { get; }
		public ICommand Command { get; }

		public Wrapper(string commandId)
		{
			Plugin = new();
			Config = new();

			Plugin.SetupGet(p => p.GapsConfig).Returns(Config);

			GapsCommands gapsCommands = new(Plugin.Object);
			Command = new PluginCommandsTestUtils(gapsCommands).GetCommand(commandId);
		}
	}

	[InlineData("whim.gaps.increase_outer_gap", 1)]
	[InlineData("whim.gaps.decrease_outer_gap", -1)]
	[Theory]
	public void OuterGapCommands(string commandId, int mul)
	{
		// Given
		Wrapper wrapper = new(commandId);
		int expectedDelta = wrapper.Config.DefaultOuterDelta * mul;

		// When
		wrapper.Command.TryExecute();

		// Then
		wrapper.Plugin.Verify(p => p.UpdateOuterGap(expectedDelta), Times.Once);
	}

	[InlineData("whim.gaps.increase_inner_gap", 1)]
	[InlineData("whim.gaps.decrease_inner_gap", -1)]
	[Theory]
	public void InnerGapCommands(string commandId, int mul)
	{
		// Given
		Wrapper wrapper = new(commandId);
		int expectedDelta = wrapper.Config.DefaultInnerDelta * mul;

		// When
		wrapper.Command.TryExecute();

		// Then
		wrapper.Plugin.Verify(p => p.UpdateInnerGap(expectedDelta), Times.Once);
	}
}
