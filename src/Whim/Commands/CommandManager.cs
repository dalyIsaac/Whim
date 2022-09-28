using System;
using System.Collections;
using System.Collections.Generic;

namespace Whim;

internal class CommandManager : ICommandManager
{
	private readonly ICommandItems _commandItems;
	private readonly KeybindHook _keybindHook;
	private bool _disposedValue;

	public CommandManager()
	{
		_commandItems = new CommandItems();
		_keybindHook = new KeybindHook(_commandItems);
	}

	public void Initialize()
	{
		_keybindHook.Initialize();
	}

	public void Add(ICommand command, IKeybind? keybind = null) => _commandItems.Add(command, keybind);
	public bool Add(string commandIdentifier, IKeybind keybind) => _commandItems.Add(commandIdentifier, keybind);
	public void Clear() => _commandItems.Clear();
	public void ClearKeybinds() => _commandItems.ClearKeybinds();
	public bool RemoveKeybind(IKeybind keybind) => _commandItems.RemoveKeybind(keybind);
	public bool RemoveKeybind(string commandIdentifier) => _commandItems.RemoveKeybind(commandIdentifier);
	public bool RemoveCommand(string commandIdentifier) => _commandItems.RemoveCommand(commandIdentifier);
	public bool RemoveCommand(ICommand command) => _commandItems.RemoveCommand(command);
	public ICommand? TryGetCommand(string commandIdentifier) => _commandItems?.TryGetCommand(commandIdentifier);
	public ICommand? TryGetCommand(IKeybind keybind) => _commandItems?.TryGetCommand(keybind);
	public IKeybind? TryGetKeybind(string commandIdentifier) => _commandItems?.TryGetKeybind(commandIdentifier);
	public IKeybind? TryGetKeybind(ICommand command) => _commandItems?.TryGetKeybind(command);

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				Logger.Debug("Disposing command manager");

				// dispose managed state (managed objects)
				_keybindHook.Dispose();
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public IEnumerator<(ICommand, IKeybind?)> GetEnumerator() => _commandItems.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => _commandItems.GetEnumerator();

	public void LoadCommands(IEnumerable<(ICommand, IKeybind?)> commands)
	{
		foreach ((ICommand command, IKeybind? keybind) in commands)
		{
			Add(command, keybind);
		}
	}
}
