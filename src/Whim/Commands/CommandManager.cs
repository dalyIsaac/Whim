using System;
using System.Collections;
using System.Collections.Generic;

namespace Whim;

internal class CommandManager : ICommandManager
{
	private readonly ICommandItems _commandItems;
	private readonly IKeybindManager _keybindManager;
	private bool _disposedValue;

	public CommandManager()
	{
		_commandItems = new CommandItems();
		_keybindManager = new KeybindManager(_commandItems);
	}

	public void Initialize()
	{
		_keybindManager.Initialize();
	}

	public void Add(ICommand command, IKeybind? keybind = null) => _commandItems.Add(command, keybind);
	public bool SetKeybind(string identifier, IKeybind keybind) => _commandItems.SetKeybind(identifier, keybind);
	public void Clear() => _commandItems.Clear();
	public void ClearKeybinds() => _commandItems.ClearKeybinds();
	public bool RemoveKeybind(IKeybind keybind) => _commandItems.RemoveKeybind(keybind);
	public bool RemoveKeybind(string identifier) => _commandItems.RemoveKeybind(identifier);
	public bool Remove(string identifier) => _commandItems.Remove(identifier);
	public ICommand? TryGetCommand(string identifier) => _commandItems?.TryGetCommand(identifier);
	public ICommand? TryGetCommand(IKeybind keybind) => _commandItems?.TryGetCommand(keybind);
	public IKeybind? TryGetKeybind(string identifier) => _commandItems?.TryGetKeybind(identifier);

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				Logger.Debug("Disposing command manager");

				// dispose managed state (managed objects)
				_keybindManager.Dispose();
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
