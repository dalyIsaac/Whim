using AutoFixture;
using NSubstitute;
using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Gaps.Tests;

public class GapsCommandsCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IGapsPlugin plugin = fixture.Create<IGapsPlugin>();
		plugin.Name.Returns("whim.gaps");
		plugin.GapsConfig.Returns(new GapsConfig());
		fixture.Inject(plugin);
	}
}

public class GapsCommandsTests
{
	private static ICommand CreateSut(IGapsPlugin plugin, string id) =>
		new PluginCommandsTestUtils(new GapsCommands(plugin)).GetCommand(id);

public class GapsCommandsCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IGapsPlugin plugin = fixture.Create<IGapsPlugin>();
		plugin.Name.Returns("whim.gaps");
		plugin.GapsConfig.Returns(new GapsConfig());
		fixture.Inject(plugin);
	}
}

public class GapsCommandsTests
{
	private static ICommand CreateSut(IGapsPlugin plugin, string id) =>
		new PluginCommandsTestUtils(new GapsCommands(plugin)).GetCommand(id);

	[Theory]
	[InlineAutoSubstituteData<GapsCommandsCustomization>("whim.gaps.outer.increase", 1)]
	[InlineAutoSubstituteData<GapsCommandsCustomization>("whim.gaps.outer.decrease", -1)]
	public void OuterGapCommands(string commandId, int mul, IGapsPlugin plugin)
	[InlineAutoSubstituteData<GapsCommandsCustomization>("whim.gaps.outer.increase", 1)]
	[InlineAutoSubstituteData<GapsCommandsCustomization>("whim.gaps.outer.decrease", -1)]
	public void OuterGapCommands(string commandId, int mul, IGapsPlugin plugin)
	{
		// Given
		ICommand command = CreateSut(plugin, commandId);
		int expectedDelta = plugin.GapsConfig.DefaultOuterDelta * mul;
		ICommand command = CreateSut(plugin, commandId);
		int expectedDelta = plugin.GapsConfig.DefaultOuterDelta * mul;

		// When
		command.TryExecute();
		command.TryExecute();

		// Then
		plugin.Received(1).UpdateOuterGap(expectedDelta);
		plugin.Received(1).UpdateOuterGap(expectedDelta);
	}

	[Theory]
	[InlineAutoSubstituteData<GapsCommandsCustomization>("whim.gaps.inner.increase", 1)]
	[InlineAutoSubstituteData<GapsCommandsCustomization>("whim.gaps.inner.decrease", -1)]
	public void InnerGapCommands(string commandId, int mul, IGapsPlugin plugin)
	[InlineAutoSubstituteData<GapsCommandsCustomization>("whim.gaps.inner.increase", 1)]
	[InlineAutoSubstituteData<GapsCommandsCustomization>("whim.gaps.inner.decrease", -1)]
	public void InnerGapCommands(string commandId, int mul, IGapsPlugin plugin)
	{
		// Given
		ICommand command = CreateSut(plugin, commandId);
		int expectedDelta = plugin.GapsConfig.DefaultInnerDelta * mul;
		ICommand command = CreateSut(plugin, commandId);
		int expectedDelta = plugin.GapsConfig.DefaultInnerDelta * mul;

		// When
		command.TryExecute();
		command.TryExecute();

		// Then
		plugin.Received(1).UpdateInnerGap(expectedDelta);
		plugin.Received(1).UpdateInnerGap(expectedDelta);
	}
}
