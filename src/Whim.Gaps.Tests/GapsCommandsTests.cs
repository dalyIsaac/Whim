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

			Plugin.SetupGet(p => p.Name).Returns("whim.gaps");
			Plugin.SetupGet(p => p.GapsConfig).Returns(Config);

			GapsCommands gapsCommands = new(Plugin.Object);
			Command = new PluginCommandsTestUtils(gapsCommands).GetCommand(commandId);
		}
	}

	[InlineData("whim.gaps.outer.increase", 1)]
	[InlineData("whim.gaps.outer.decrease", -1)]
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

	[InlineData("whim.gaps.inner.increase", 1)]
	[InlineData("whim.gaps.inner.decrease", -1)]
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
