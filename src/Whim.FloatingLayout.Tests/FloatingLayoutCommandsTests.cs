using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.FloatingLayout.Tests;

public class FloatingLayoutCommandsDataAttribute : AutoDataAttribute
{
	public FloatingLayoutCommandsDataAttribute()
		: base(CreateFixture) { }

	private static IFixture CreateFixture()
	{
		IFixture fixture = new Fixture();
		fixture.Customize(new AutoNSubstituteCustomization());
		fixture.Customize(new FloatingLayoutCommandsCustomization());
		return fixture;
	}
}

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

	[Theory, FloatingLayoutCommandsData]
	public void ToggleWindowFloatingCommand(IFloatingLayoutPlugin plugin)
	{
		// Given
		ICommand command = CreateSut(plugin, "whim.floating_layout.toggle_window_floating");

		// When
		command.TryExecute();

		// Then
		plugin.Received(1).ToggleWindowFloating(null);
	}

	[Theory, FloatingLayoutCommandsData]
	public void MarkWindowAsFloatingCommand(IFloatingLayoutPlugin plugin)
	{
		// Given
		ICommand command = CreateSut(plugin, "whim.floating_layout.mark_window_as_floating");

		// When
		command.TryExecute();

		// Then
		plugin.Received(1).MarkWindowAsFloating(null);
	}

	[Theory, FloatingLayoutCommandsData]
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
