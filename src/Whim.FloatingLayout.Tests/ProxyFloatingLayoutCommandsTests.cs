using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.FloatingLayout.Tests;

public class ProxyFloatingLayoutCommandsCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IProxyFloatingLayoutPlugin plugin = fixture.Freeze<IProxyFloatingLayoutPlugin>();
		plugin.Name.Returns("whim.floating_layout");
		fixture.Inject(plugin);
	}
}

public class ProxyFloatingLayoutCommandsTests
{
	private static ICommand CreateSut(IProxyFloatingLayoutPlugin plugin, string id) =>
		new PluginCommandsTestUtils(new ProxyFloatingLayoutCommands(plugin)).GetCommand(id);

	[Theory, AutoSubstituteData<ProxyFloatingLayoutCommandsCustomization>]
	public void ToggleWindowFloatingCommand(IProxyFloatingLayoutPlugin plugin)
	{
		// Given
		ICommand command = CreateSut(plugin, "whim.floating_layout.toggle_window_floating");

		// When
		command.TryExecute();

		// Then
		plugin.Received(1).ToggleWindowFloating(null);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutCommandsCustomization>]
	public void MarkWindowAsFloatingCommand(IProxyFloatingLayoutPlugin plugin)
	{
		// Given
		ICommand command = CreateSut(plugin, "whim.floating_layout.mark_window_as_floating");

		// When
		command.TryExecute();

		// Then
		plugin.Received(1).MarkWindowAsFloating(null);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutCommandsCustomization>]
	public void MarkWindowAsDockedCommand(IProxyFloatingLayoutPlugin plugin)
	{
		// Given
		ICommand command = CreateSut(plugin, "whim.floating_layout.mark_window_as_docked");

		// When
		command.TryExecute();

		// Then
		plugin.Received(1).MarkWindowAsDocked(null);
	}
}
