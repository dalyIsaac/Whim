using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Xunit;

namespace Whim.Tests;

public class CommandItemsTests
{
	/// <summary>
	/// Add a command with no keybind.
	/// </summary>
	[Fact]
	public void AddCommand()
	{
		CommandItemContainer commandItems = new();

		Mock<ICommand> command = new();
		command.Setup(c => c.Id).Returns("command");

		commandItems.Add(command.Object);

		Assert.Equal(command.Object, commandItems.TryGetCommand(command.Object.Id));
	}

	/// <summary>
	/// Add a command with a keybind.
	/// </summary>
	[Fact]
	public void AddCommandWithKeybind()
	{
		CommandItemContainer commandItems = new();

		Mock<ICommand> command = new();
		command.Setup(c => c.Id).Returns("command");

		Keybind keybind = new(KeyModifiers.RWin, VIRTUAL_KEY.VK_F);
		commandItems.Add(command.Object, keybind);

		Assert.Equal(command.Object, commandItems.TryGetCommand(keybind));
		Assert.Equal(command.Object, commandItems.TryGetCommand(command.Object.Id));
	}

	/// <summary>
	/// Two commands with the same identifier.
	/// </summary>
	[Fact]
	public void AddTwoCommandsWithSameIdentifier()
	{
		CommandItemContainer commandItems = new();

		Mock<ICommand> command = new();
		command.Setup(c => c.Id).Returns("command");
		commandItems.Add(command.Object);

		Mock<ICommand> command2 = new();
		command2.Setup(c => c.Id).Returns("command");

		Assert.Throws<ArgumentException>(() => commandItems.Add(command2.Object));
	}

	/// <summary>
	/// Override an existing keybind with a new command.
	/// </summary>
	[Fact]
	public void OverrideKeybind()
	{
		CommandItemContainer commandItems = new();
		Keybind keybind = new(KeyModifiers.RWin, VIRTUAL_KEY.VK_F);

		// Set up the first command.
		Mock<ICommand> command = new();
		command.Setup(c => c.Id).Returns("command");

		commandItems.Add(command.Object, keybind);

		// Set up the second command.
		Mock<ICommand> command2 = new();
		command2.Setup(c => c.Id).Returns("command2");

		commandItems.Add(command2.Object, keybind);

		// Check that the first command is still there.
		Assert.Equal(command.Object, commandItems.TryGetCommand(command.Object.Id));

		// Check that the second command is now the one bound to the keybind.
		Assert.Equal(command2.Object, commandItems.TryGetCommand(keybind));
		Assert.Equal(command2.Object, commandItems.TryGetCommand(command2.Object.Id));
	}

	/// <summary>
	/// Set a keybind to a command which doesn't exist.
	/// </summary>
	[Fact]
	public void SetToNonExistentCommand()
	{
		CommandItemContainer commandItems = new();
		Keybind keybind = new(KeyModifiers.RWin, VIRTUAL_KEY.VK_F);

		Assert.False(commandItems.SetKeybind("command", keybind));
	}

	/// <summary>
	/// Set a keybind to an existing command.
	/// </summary>
	[Fact]
	public void SetToExistingCommand()
	{
		CommandItemContainer commandItems = new();
		Keybind keybind = new(KeyModifiers.RWin, VIRTUAL_KEY.VK_F);

		// Set up the first command.
		Mock<ICommand> command = new();
		command.Setup(c => c.Id).Returns("command");
		commandItems.Add(command.Object, keybind);

		// Set up the second command.
		Mock<ICommand> command2 = new();
		command2.Setup(c => c.Id).Returns("command2");
		commandItems.Add(command2.Object);

		// Set the keybind to the second command.
		commandItems.SetKeybind(command2.Object.Id, keybind);

		// Check that the first command is still there.
		Assert.Equal(command.Object, commandItems.TryGetCommand(command.Object.Id));

		// Check that the second command is now the one bound to the keybind.
		Assert.Equal(command2.Object, commandItems.TryGetCommand(keybind));
		Assert.Equal(keybind, commandItems.TryGetKeybind(command2.Object.Id));

		// Check that the first command is not bound to the keybind.
		Assert.Null(commandItems.TryGetKeybind(command.Object.Id));
	}

	/// <summary>
	/// Remove a keybind, but verify the command is still there.
	/// </summary>
	[Fact]
	public void RemoveKeybind()
	{
		CommandItemContainer commandItems = new();
		Keybind keybind = new(KeyModifiers.RWin, VIRTUAL_KEY.VK_F);

		// Set up the command.
		Mock<ICommand> command = new();
		command.Setup(c => c.Id).Returns("command");

		commandItems.Add(command.Object, keybind);

		// Remove the keybind.
		commandItems.RemoveKeybind(keybind);

		// Check that the command is still there.
		Assert.Equal(command.Object, commandItems.TryGetCommand(command.Object.Id));

		// Check that the keybind is no longer there.
		Assert.Null(commandItems.TryGetCommand(keybind));
	}

	/// <summary>
	/// Remove a keybind, given the command.
	/// </summary>
	[Fact]
	public void RemoveKeybindGivenCommand()
	{
		CommandItemContainer commandItems = new();
		Keybind keybind = new(KeyModifiers.RWin, VIRTUAL_KEY.VK_F);

		// Set up the command.
		Mock<ICommand> command = new();
		command.Setup(c => c.Id).Returns("command");

		commandItems.Add(command.Object, keybind);

		// Remove the keybind.
		commandItems.RemoveKeybind(command.Object.Id);

		// Check that the command is still there.
		Assert.Equal(command.Object, commandItems.TryGetCommand(command.Object.Id));

		// Check that the keybind is no longer there.
		Assert.Null(commandItems.TryGetCommand(keybind));
	}

	/// <summary>
	/// Remove a command, and verify the keybind is no longer there.
	/// </summary>
	[Fact]
	public void RemoveBoundCommand()
	{
		CommandItemContainer commandItems = new();
		Keybind keybind = new(KeyModifiers.RWin, VIRTUAL_KEY.VK_F);

		// Set up the command.
		Mock<ICommand> command = new();
		command.Setup(c => c.Id).Returns("command");

		commandItems.Add(command.Object, keybind);

		// Remove the command.
		commandItems.RemoveCommand(command.Object.Id);

		// Check that the command is no longer there.
		Assert.Null(commandItems.TryGetCommand(command.Object.Id));

		// Check that the keybind is gone.
		Assert.Null(commandItems.TryGetCommand(keybind));
	}

	/// <summary>
	/// Remove an unbound command.
	/// </summary>
	[Fact]
	public void RemoveUnboundCommand()
	{
		CommandItemContainer commandItems = new();

		// Set up the command.
		Mock<ICommand> command = new();
		command.Setup(c => c.Id).Returns("command");

		// Remove the command.
		commandItems.RemoveCommand(command.Object.Id);

		// Check that the command is no longer there.
		Assert.Null(commandItems.TryGetCommand(command.Object.Id));
	}

	/// <summary>
	/// Clear all commands.
	/// </summary>
	[Fact]
	public void Clear()
	{
		CommandItemContainer commandItems = new();
		Keybind keybind = new(KeyModifiers.RWin, VIRTUAL_KEY.VK_F);

		// Set up the command.
		Mock<ICommand> command = new();
		command.Setup(c => c.Id).Returns("command");

		commandItems.Add(command.Object, keybind);

		// Clear all commands.
		commandItems.Clear();

		// Check that the command is no longer there.
		Assert.Null(commandItems.TryGetCommand(command.Object.Id));

		// Check that the keybind is gone.
		Assert.Null(commandItems.TryGetCommand(keybind));
	}

	/// <summary>
	/// Clear all keybinds.
	/// </summary>
	[Fact]
	public void ClearKeybinds()
	{
		CommandItemContainer commandItems = new();
		Keybind keybind = new(KeyModifiers.RWin, VIRTUAL_KEY.VK_F);

		// Set up the command.
		Mock<ICommand> command = new();
		command.Setup(c => c.Id).Returns("command");

		commandItems.Add(command.Object, keybind);

		// Clear all keybinds.
		commandItems.ClearKeybinds();

		// Check that the command is still there.
		Assert.Equal(command.Object, commandItems.TryGetCommand(command.Object.Id));

		// Check that the keybind is no longer there.
		Assert.Null(commandItems.TryGetCommand(keybind));
	}

	/// <summary>
	/// Enumerator.
	/// </summary>
	[Fact]
	public void Enumerator()
	{
		CommandItemContainer commandItems = new();

		// Set up the first command.
		Mock<ICommand> command = new();
		command.Setup(c => c.Id).Returns("command");

		Keybind keybind = new(KeyModifiers.RWin, VIRTUAL_KEY.VK_F);
		commandItems.Add(command.Object, keybind);

		// Set up the second command.
		Mock<ICommand> command2 = new();
		command2.Setup(c => c.Id).Returns("command2");
		commandItems.Add(command2.Object);

		// Set up the third command.
		Mock<ICommand> command3 = new();
		command3.Setup(c => c.Id).Returns("command3");
		commandItems.Add(command3.Object);

		// Set up the fourth command.
		Mock<ICommand> command4 = new();
		command4.Setup(c => c.Id).Returns("command4");

		Keybind keybind4 = new(KeyModifiers.RWin, VIRTUAL_KEY.VK_G);
		commandItems.Add(command4.Object, keybind4);

		// Iterate over the commands.
		List<CommandItem> commands = commandItems.ToList();

		// Check there's the right number of commands.
		Assert.Equal(4, commands.Count);

		// Check that each of the command-keybind pairs are in the list.
		Assert.Contains(new CommandItem() { Command = command.Object, Keybind = keybind }, commands);
		Assert.Contains(new CommandItem() { Command = command2.Object }, commands);
		Assert.Contains(new CommandItem() { Command = command3.Object }, commands);
		Assert.Contains(new CommandItem() { Command = command4.Object, Keybind = keybind4 }, commands);
	}
}
