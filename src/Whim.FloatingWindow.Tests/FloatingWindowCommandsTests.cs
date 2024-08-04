using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.FloatingWindow.Tests;

public class FloatingWindowCommandsCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IFloatingWindowPlugin plugin = fixture.Freeze<IFloatingWindowPlugin>();
		plugin.Name.Returns("whim.floating_window");
		fixture.Inject(plugin);
	}
}

public class FloatingWindowCommandsTests
{
	private static ICommand CreateSut(IFloatingWindowPlugin plugin, string id) =>
		new PluginCommandsTestUtils(new FloatingWindowCommands(plugin)).GetCommand(id);

	[Theory, AutoSubstituteData<FloatingWindowCommandsCustomization>]
	public void ToggleWindowFloatingCommand(IFloatingWindowPlugin plugin)
	{
		// Given
		ICommand command = CreateSut(plugin, "whim.floating_window.toggle_window_floating");

		// When
		command.TryExecute();

		// Then
		plugin.Received(1).ToggleWindowFloating(null);
	}

	[Theory, AutoSubstituteData<FloatingWindowCommandsCustomization>]
	public void MarkWindowAsFloatingCommand(IFloatingWindowPlugin plugin)
	{
		// Given
		ICommand command = CreateSut(plugin, "whim.floating_window.mark_window_as_floating");

		// When
		command.TryExecute();

		// Then
		plugin.Received(1).MarkWindowAsFloating(null);
	}

	[Theory, AutoSubstituteData<FloatingWindowCommandsCustomization>]
	public void MarkWindowAsDockedCommand(IFloatingWindowPlugin plugin)
	{
		// Given
		ICommand command = CreateSut(plugin, "whim.floating_window.mark_window_as_docked");

		// When
		command.TryExecute();

		// Then
		plugin.Received(1).MarkWindowAsDocked(null);
	}
}
