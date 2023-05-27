using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Whim.Tests;

public class CommandManagerTests
{
	[Fact]
	public void AddPluginCommand_Success()
	{
		// Given
		Mock<ICommand> command = new();
		command.SetupGet(c => c.Id).Returns("command");
		CommandManager commandManager = new();

		// When
		commandManager.AddPluginCommand(command.Object);

		// Then
		Assert.Contains(command.Object, commandManager);
		Assert.Equal(command.Object, commandManager.TryGetCommand(command.Object.Id));
		Assert.Single(commandManager);
	}

	[Fact]
	public void AddPluginCommand_AlreadyContainsCommand()
	{
		// Given
		Mock<ICommand> command = new();
		command.SetupGet(c => c.Id).Returns("command");
		CommandManager commandManager = new();

		// When
		commandManager.AddPluginCommand(command.Object);
		Assert.Throws<InvalidOperationException>(() => commandManager.AddPluginCommand(command.Object));

		// Then
		Assert.Contains(command.Object, commandManager);
		Assert.Equal(command.Object, commandManager.TryGetCommand(command.Object.Id));
		Assert.Single(commandManager);
	}

	[Fact]
	public void Add()
	{
		// Given
		CommandManager commandManager = new();

		// When
		commandManager.Add("command", "title", () => { });

		// Then
		ICommand? command = commandManager.TryGetCommand("whim.custom.command");
		Assert.NotNull(command);
		Assert.Equal("whim.custom.command", command!.Id);
		Assert.Equal("title", command.Title);
	}

	[Fact]
	public void GetEnumerator()
	{
		// Given
		Mock<ICommand> command = new();
		command.SetupGet(c => c.Id).Returns("command");
		CommandManager commandManager = new();

		// When
		commandManager.AddPluginCommand(command.Object);

		// Then
		List<ICommand> allCommands = commandManager.ToList();
		Assert.Single(allCommands);
		Assert.Equal(command.Object, allCommands[0]);
	}
}
