using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.SliceLayout.Tests;

public class SliceLayoutCommandsCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		ISliceLayoutPlugin plugin = fixture.Freeze<ISliceLayoutPlugin>();
		plugin.Name.Returns("whim.slicelayout");
	}
}

public class SliceLayoutCommandsTests
{
	private static ICommand CreateSut(ISliceLayoutPlugin plugin, string id) =>
		new PluginCommandsTestUtils(new SliceLayoutCommands(plugin)).GetCommand(id);

	[InlineAutoSubstituteData<SliceLayoutCommandsCustomization>(
		"whim.slicelayout.set_insertion_type.swap",
		WindowInsertionType.Swap
	)]
	[InlineAutoSubstituteData<SliceLayoutCommandsCustomization>(
		"whim.slicelayout.set_insertion_type.rotate",
		WindowInsertionType.Rotate
	)]
	[Theory]
	public void SetInsertionTypeCommands(
		string commandId,
		WindowInsertionType windowInsertionType,
		ISliceLayoutPlugin plugin
	)
	{
		// Given
		ICommand command = CreateSut(plugin, commandId);

		// When
		command.TryExecute();

		// Then
		plugin.Received(1).WindowInsertionType = windowInsertionType;
	}

	[Theory]
	[AutoSubstituteData<SliceLayoutCommandsCustomization>]
	public void PromoteWindowInStack(ISliceLayoutPlugin plugin)
	{
		// Given
		ICommand command = CreateSut(plugin, "whim.slicelayout.stack.promote");

		// When
		command.TryExecute();

		// Then
		plugin.Received(1).PromoteWindowInStack();
	}

	[Theory]
	[AutoSubstituteData<SliceLayoutCommandsCustomization>]
	public void DemoteWindowInStack(ISliceLayoutPlugin plugin)
	{
		// Given
		ICommand command = CreateSut(plugin, "whim.slicelayout.stack.demote");

		// When
		command.TryExecute();

		// Then
		plugin.Received(1).DemoteWindowInStack();
	}
}
