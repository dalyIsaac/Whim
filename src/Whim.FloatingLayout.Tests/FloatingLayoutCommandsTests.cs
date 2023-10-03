using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.FloatingLayout.Tests;

public class FloatingLayoutCommandsCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IFloatingLayoutPlugin plugin = fixture.Freeze<IFloatingLayoutPlugin>();
		plugin.Name.Returns("whim.floating_layout");
		fixture.Inject(plugin);
	}
}

public class FloatingLayoutCommandsTests
{
	private static ICommand CreateSut(IFloatingLayoutPlugin plugin, string id) =>
		new PluginCommandsTestUtils(new FloatingLayoutCommands(plugin)).GetCommand(id);

	[Theory, AutoSubstituteData<FloatingLayoutCommandsCustomization>]
	public void ToggleWindowFloatingCommand(IFloatingLayoutPlugin plugin)
	{
		// Given
		ICommand command = CreateSut(plugin, "whim.floating_layout.toggle_window_floating");

		// When
		command.TryExecute();

		// Then
		plugin.Received(1).ToggleWindowFloating(null);
	}

	[Theory, AutoSubstituteData<FloatingLayoutCommandsCustomization>]
	public void MarkWindowAsFloatingCommand(IFloatingLayoutPlugin plugin)
	{
		// Given
		ICommand command = CreateSut(plugin, "whim.floating_layout.mark_window_as_floating");

		// When
		command.TryExecute();

		// Then
		plugin.Received(1).MarkWindowAsFloating(null);
	}

	[Theory, AutoSubstituteData<FloatingLayoutCommandsCustomization>]
	public void MarkWindowAsDockedCommand(IFloatingLayoutPlugin plugin)
	{
		// Given
		ICommand command = CreateSut(plugin, "whim.floating_layout.mark_window_as_docked");

		// When
		command.TryExecute();

		// Then
		plugin.Received(1).MarkWindowAsDocked(null);
	}
}
