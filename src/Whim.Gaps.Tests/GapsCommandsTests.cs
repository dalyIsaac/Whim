using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Gaps.Tests;

public class GapsCommandsTests
{
	private static ICommand CreateSut(IGapsPlugin plugin, string id) =>
		new PluginCommandsTestUtils(new GapsCommands(plugin)).GetCommand(id);

	private static IGapsPlugin CreatePlugin()
	{
		IGapsPlugin plugin = CreatePlugin();
		plugin.Name.Returns("whim.gaps");
		return plugin;
	}

	[Theory, AutoSubstituteData]
	[InlineAutoSubstituteData("whim.gaps.outer.increase", 1)]
	[InlineAutoSubstituteData("whim.gaps.outer.decrease", -1)]
	public void OuterGapCommands(string commandId, int mul, GapsConfig gapsConfig)
	{
		// Given
		IGapsPlugin plugin = CreatePlugin();
		ICommand command = CreateSut(plugin, commandId);
		int expectedDelta = gapsConfig.DefaultOuterDelta * mul;

		// When
		command.TryExecute();

		// Then
		plugin.Received(1).UpdateOuterGap(expectedDelta);
	}

	[Theory, AutoSubstituteData]
	[InlineAutoSubstituteData("whim.gaps.inner.increase", 1)]
	[InlineAutoSubstituteData("whim.gaps.inner.decrease", -1)]
	public void InnerGapCommands(string commandId, int mul, GapsConfig gapsConfig)
	{
		// Given
		IGapsPlugin plugin = CreatePlugin();
		ICommand command = CreateSut(plugin, commandId);
		int expectedDelta = gapsConfig.DefaultInnerDelta * mul;

		// When
		command.TryExecute();

		// Then
		plugin.Received(1).UpdateInnerGap(expectedDelta);
	}
}
