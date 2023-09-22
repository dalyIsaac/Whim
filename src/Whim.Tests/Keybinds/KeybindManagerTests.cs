using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Xunit;

namespace Whim.Tests;

public class KeybindManagerTests
{
	[Theory, AutoSubstituteData]
	public void Add_DoesNotContainKeybind(IContext context, ICommand command)
	{
		// Given
		IKeybindManager keybindManager = new KeybindManager(context);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);

		context.CommandManager.TryGetCommand("command").Returns(command);

		// When
		keybindManager.Add("command", keybind);

		// Then
		ICommand[] allCommands = keybindManager.GetCommands(keybind);
		Assert.Single(allCommands);
		Assert.Equal(command, allCommands[0]);
	}

	[Theory, AutoSubstituteData]
	public void Add_ContainsKeybind(IContext context, ICommand command, ICommand command2)
	{
		// Given
		IKeybindManager keybindManager = new KeybindManager(context);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);

		context.CommandManager.TryGetCommand("command").Returns(command);
		context.CommandManager.TryGetCommand("command2").Returns(command2);

		// When
		keybindManager.Add("command", keybind);
		keybindManager.Add("command2", keybind);

		// Then
		ICommand[] allCommands = keybindManager.GetCommands(keybind);
		Assert.Equal(2, allCommands.Length);
		Assert.Equal(command, allCommands[0]);
		Assert.Equal(command2, allCommands[1]);
	}

	[Theory, AutoSubstituteData]
	public void GetCommands_DoesNotContainKeybind(IContext context)
	{
		// Given
		IKeybindManager keybindManager = new KeybindManager(context);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);

		// When
		ICommand[] allCommands = keybindManager.GetCommands(keybind);

		// Then
		Assert.Empty(allCommands);
	}

	[Theory, AutoSubstituteData]
	public void GetCommands_DoesNotContainCommand(IContext context)
	{
		// Given
		IKeybindManager keybindManager = new KeybindManager(context);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);

		context.CommandManager.TryGetCommand("command2").Returns((ICommand?)null);

		// When
		keybindManager.Add("command2", keybind);
		ICommand[] allCommands = keybindManager.GetCommands(keybind);

		// Then
		Assert.Empty(allCommands);
	}

	[Theory, AutoSubstituteData]
	public void GetCommands_Success(IContext context, ICommand command, ICommand command2)
	{
		// Given
		IKeybindManager keybindManager = new KeybindManager(context);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);

		context.CommandManager.TryGetCommand("command").Returns(command);
		context.CommandManager.TryGetCommand("command2").Returns(command2);

		// When
		keybindManager.Add("command", keybind);
		keybindManager.Add("command2", keybind);

		// Then
		ICommand[] allCommands = keybindManager.GetCommands(keybind);
		Assert.Equal(2, allCommands.Length);
		Assert.Equal(command, allCommands[0]);
		Assert.Equal(command2, allCommands[1]);
	}

	[Theory, AutoSubstituteData]
	public void TryGet_DoesNotContainCommand(IContext context)
	{
		// Given
		IKeybindManager keybindManager = new KeybindManager(context);

		// When
		IKeybind? keybind = keybindManager.TryGetKeybind("command");

		// Then
		Assert.Null(keybind);
	}

	[Theory, AutoSubstituteData]
	public void TryGet_ContainsCommand(IContext context)
	{
		// Given
		IKeybindManager keybindManager = new KeybindManager(context);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);

		// When
		keybindManager.Add("command", keybind);
		IKeybind? result = keybindManager.TryGetKeybind("command");

		// Then
		Assert.Equal(keybind, result);
	}

	[Theory, AutoSubstituteData]
	public void Remove_DoesNotContainCommand(IContext context)
	{
		// Given
		IKeybindManager keybindManager = new KeybindManager(context);

		// When
		bool result = keybindManager.Remove("command");

		// Then
		Assert.False(result);
	}

	[Theory, AutoSubstituteData]
	public void Remove_ContainsCommand(IContext context)
	{
		// Given
		IKeybindManager keybindManager = new KeybindManager(context);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);

		// When
		keybindManager.Add("command", keybind);
		bool result = keybindManager.Remove("command");

		// Then
		Assert.True(result);
		Assert.Empty(keybindManager.GetCommands(keybind));
	}
}
