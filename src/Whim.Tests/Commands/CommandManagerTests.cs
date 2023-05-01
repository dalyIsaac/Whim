using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Whim.Tests;

public class CommandManagerTests
{
	[Fact]
	public void Add_Success()
	{
		// Given
		Mock<ICommand> command = new();
		command.SetupGet(c => c.Id).Returns("command");
		ICommandManager commandManager = new CommandManager();

		// When
		commandManager.Add(command.Object);

		// Then
		Assert.True(commandManager.Contains(command.Object));
		Assert.Equal(command.Object, commandManager.TryGetCommand(command.Object.Id));
		Assert.Single(commandManager);
	}

	[Fact]
	public void Add_AlreadyContainsCommand()
	{
		// Given
		Mock<ICommand> command = new();
		command.SetupGet(c => c.Id).Returns("command");
		ICommandManager commandManager = new CommandManager();

		// When
		commandManager.Add(command.Object);
		Assert.Throws<InvalidOperationException>(() => commandManager.Add(command.Object));

		// Then
		Assert.True(commandManager.Contains(command.Object));
		Assert.Equal(command.Object, commandManager.TryGetCommand(command.Object.Id));
		Assert.Single(commandManager);
	}

	[Fact]
	public void Clear()
	{
		// Given
		Mock<ICommand> command = new();
		command.SetupGet(c => c.Id).Returns("command");
		ICommandManager commandManager = new CommandManager();

		// When
		commandManager.Add(command.Object);
		commandManager.Clear();

		// Then
		Assert.False(commandManager.Contains(command.Object));
		Assert.Null(commandManager.TryGetCommand(command.Object.Id));
		Assert.Empty(commandManager);
	}

	[Fact]
	public void Contains()
	{
		// Given
		Mock<ICommand> command = new();
		command.SetupGet(c => c.Id).Returns("command");

		Mock<ICommand> notAddedCommand = new();
		notAddedCommand.SetupGet(c => c.Id).Returns("notAddedCommand");

		ICommandManager commandManager = new CommandManager();

		// When
		commandManager.Add(command.Object);

		// Then
		Assert.True(commandManager.Contains(command.Object));
		Assert.False(commandManager.Contains(notAddedCommand.Object));
	}

	[Fact]
	public void CopyTo()
	{
		// Given
		Mock<ICommand> command = new();
		command.SetupGet(c => c.Id).Returns("command");
		ICommandManager commandManager = new CommandManager();

		// When
		commandManager.Add(command.Object);
		ICommand[] commands = new ICommand[1];
		commandManager.CopyTo(commands, 0);

		// Then
		Assert.Equal(command.Object, commands[0]);
	}

	[Fact]
	public void GetEnumerator()
	{
		// Given
		Mock<ICommand> command = new();
		command.SetupGet(c => c.Id).Returns("command");
		ICommandManager commandManager = new CommandManager();

		// When
		commandManager.Add(command.Object);

		// Then
		List<ICommand> allCommands = commandManager.ToList();
		Assert.Single(allCommands);
		Assert.Equal(command.Object, allCommands[0]);
	}

	[Fact]
	public void Remove_DoesNotContainCommand()
	{
		// Given
		Mock<ICommand> command = new();
		command.SetupGet(c => c.Id).Returns("command");
		ICommandManager commandManager = new CommandManager();

		// When
		bool result = commandManager.Remove(command.Object);

		// Then
		Assert.False(result);
	}

	[Fact]
	public void Remove_ContainsCommand()
	{
		// Given
		Mock<ICommand> command = new();
		command.SetupGet(c => c.Id).Returns("command");
		ICommandManager commandManager = new CommandManager();

		// When
		commandManager.Add(command.Object);
		bool result = commandManager.Remove(command.Object);

		// Then
		Assert.True(result);
		Assert.False(commandManager.Contains(command.Object));
		Assert.Null(commandManager.TryGetCommand(command.Object.Id));
		Assert.Empty(commandManager);
	}
}
