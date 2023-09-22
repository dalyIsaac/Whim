using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Gaps.Tests;

public class GapsCommandsTests
{
	private static IGapsPlugin CreatePlugin()
	{
		IGapsPlugin plugin = Substitute.For<IGapsPlugin>();
		plugin.Name.Returns("whim.gaps");
		plugin.GapsConfig.Returns(new GapsConfig());
		return plugin;
	}

	private static ICommand CreateSut(IGapsPlugin plugin, string id) =>
		new PluginCommandsTestUtils(new GapsCommands(plugin)).GetCommand(id);

	[Theory]
	[InlineAutoSubstituteData("whim.gaps.outer.increase", 1)]
	[InlineAutoSubstituteData("whim.gaps.outer.decrease", -1)]
	public void OuterGapCommands(string commandId, int mul)
	{
		// Given
		IGapsPlugin plugin = CreatePlugin();
		ICommand command = CreateSut(plugin, commandId);
		int expectedDelta = plugin.GapsConfig.DefaultOuterDelta * mul;

		// When
		command.TryExecute();

		// Then
		plugin.Received(1).UpdateOuterGap(expectedDelta);
	}

	[Theory]
	[InlineAutoSubstituteData("whim.gaps.inner.increase", 1)]
	[InlineAutoSubstituteData("whim.gaps.inner.decrease", -1)]
	public void InnerGapCommands(string commandId, int mul)
	{
		// Given
		IGapsPlugin plugin = CreatePlugin();
		ICommand command = CreateSut(plugin, commandId);
		int expectedDelta = plugin.GapsConfig.DefaultInnerDelta * mul;

		// When
		command.TryExecute();

		// Then
		plugin.Received(1).UpdateInnerGap(expectedDelta);
	}
}
