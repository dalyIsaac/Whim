using Moq;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Xunit;

namespace Whim.Tests;

public class KeybindManagerTests
{
	private class Wrapper
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<ICommandManager> CommandManager { get; } = new();

		public Wrapper()
		{
			Context.SetupGet(context => context.CommandManager).Returns(CommandManager.Object);
		}
	}

	[Fact]
	public void Add_DoesNotContainKeybind()
	{
		// Given
		Wrapper wrapper = new();
		IKeybindManager keybindManager = new KeybindManager(wrapper.Context.Object);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);
		ICommand command = new Mock<ICommand>().Object;

		wrapper.CommandManager.Setup(commandManager => commandManager.TryGetCommand("command")).Returns(command);

		// When
		keybindManager.Add("command", keybind);

		// Then
		ICommand[] allCommands = keybindManager.GetCommands(keybind);
		Assert.Single(allCommands);
		Assert.Equal(command, allCommands[0]);
	}

	[Fact]
	public void Add_ContainsKeybind()
	{
		// Given
		Wrapper wrapper = new();
		IKeybindManager keybindManager = new KeybindManager(wrapper.Context.Object);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);
		ICommand command = new Mock<ICommand>().Object;
		ICommand command2 = new Mock<ICommand>().Object;

		wrapper.CommandManager.Setup(commandManager => commandManager.TryGetCommand("command")).Returns(command);
		wrapper.CommandManager.Setup(commandManager => commandManager.TryGetCommand("command2")).Returns(command2);

		// When
		keybindManager.Add("command", keybind);
		keybindManager.Add("command2", keybind);

		// Then
		ICommand[] allCommands = keybindManager.GetCommands(keybind);
		Assert.Equal(2, allCommands.Length);
		Assert.Equal(command, allCommands[0]);
		Assert.Equal(command2, allCommands[1]);
	}

	[Fact]
	public void GetCommands_DoesNotContainKeybind()
	{
		// Given
		Wrapper wrapper = new();
		IKeybindManager keybindManager = new KeybindManager(wrapper.Context.Object);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);

		// When
		ICommand[] allCommands = keybindManager.GetCommands(keybind);

		// Then
		Assert.Empty(allCommands);
	}

	[Fact]
	public void GetCommands_DoesNotContainCommand()
	{
		// Given
		Wrapper wrapper = new();
		IKeybindManager keybindManager = new KeybindManager(wrapper.Context.Object);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);
		ICommand command = new Mock<ICommand>().Object;

		wrapper.CommandManager.Setup(commandManager => commandManager.TryGetCommand("command")).Returns(command);

		// When
		keybindManager.Add("command2", keybind);
		ICommand[] allCommands = keybindManager.GetCommands(keybind);

		// Then
		Assert.Empty(allCommands);
	}

	[Fact]
	public void GetCommands_Success()
	{
		// Given
		Wrapper wrapper = new();
		IKeybindManager keybindManager = new KeybindManager(wrapper.Context.Object);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);
		ICommand command = new Mock<ICommand>().Object;
		ICommand command2 = new Mock<ICommand>().Object;

		wrapper.CommandManager.Setup(commandManager => commandManager.TryGetCommand("command")).Returns(command);
		wrapper.CommandManager.Setup(commandManager => commandManager.TryGetCommand("command2")).Returns(command2);

		// When
		keybindManager.Add("command", keybind);
		keybindManager.Add("command2", keybind);

		// Then
		ICommand[] allCommands = keybindManager.GetCommands(keybind);
		Assert.Equal(2, allCommands.Length);
		Assert.Equal(command, allCommands[0]);
		Assert.Equal(command2, allCommands[1]);
	}

	[Fact]
	public void TryGet_DoesNotContainCommand()
	{
		// Given
		Wrapper wrapper = new();
		IKeybindManager keybindManager = new KeybindManager(wrapper.Context.Object);

		// When
		IKeybind? keybind = keybindManager.TryGetKeybind("command");

		// Then
		Assert.Null(keybind);
	}

	[Fact]
	public void TryGet_ContainsCommand()
	{
		// Given
		Wrapper wrapper = new();
		IKeybindManager keybindManager = new KeybindManager(wrapper.Context.Object);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);

		// When
		keybindManager.Add("command", keybind);
		IKeybind? result = keybindManager.TryGetKeybind("command");

		// Then
		Assert.Equal(keybind, result);
	}

	[Fact]
	public void Remove_DoesNotContainCommand()
	{
		// Given
		Wrapper wrapper = new();
		IKeybindManager keybindManager = new KeybindManager(wrapper.Context.Object);

		// When
		bool result = keybindManager.Remove("command");

		// Then
		Assert.False(result);
	}

	[Fact]
	public void Remove_ContainsCommand()
	{
		// Given
		Wrapper wrapper = new();
		IKeybindManager keybindManager = new KeybindManager(wrapper.Context.Object);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);

		// When
		keybindManager.Add("command", keybind);
		bool result = keybindManager.Remove("command");

		// Then
		Assert.True(result);
		Assert.Empty(keybindManager.GetCommands(keybind));
	}
}
