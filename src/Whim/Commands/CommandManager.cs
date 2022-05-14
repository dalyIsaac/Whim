using System;

namespace Whim;

public class CommandManager : ICommandManager
{
	private readonly ICommandItems _commandItems;
	private readonly IKeybindManager _keybindManager;
	private bool disposedValue;

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
	public bool Bind(string identifier, IKeybind keybind) => _commandItems.Bind(identifier, keybind);
	public void Clear() => _commandItems.Clear();
	public void ClearKeybinds() => _commandItems.ClearKeybinds();
	public bool Remove(IKeybind keybind) => _commandItems.Remove(keybind);
	public bool Remove(string identifier) => _commandItems.Remove(identifier);
	public ICommand? TryGetCommand(string identifier) => _commandItems?.TryGetCommand(identifier);
	public ICommand? TryGetCommand(IKeybind keybind) => _commandItems?.TryGetCommand(keybind);
	public IKeybind? TryGetKeybind(string identifier) => _commandItems?.TryGetKeybind(identifier);

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_keybindManager.Dispose();
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
