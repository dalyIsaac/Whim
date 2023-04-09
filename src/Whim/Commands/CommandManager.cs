using System;
using System.Collections;
using System.Collections.Generic;

namespace Whim;

/// <inheritdoc />
internal class CommandManager : ICommandManager
{
	private readonly IContext _context;
	private readonly ICoreNativeManager _coreNativeManager;
	private readonly ICommandItemContainer _commandItems;
	private readonly KeybindHook _keybindHook;
	private bool _disposedValue;

	/// <summary>
	/// Initializes a new instance of the <see cref="CommandManager"/> class.
	/// </summary>
	public CommandManager(IContext context, ICoreNativeManager coreNativeManager)
	{
		_context = context;
		_coreNativeManager = coreNativeManager;
		_commandItems = new CommandItemContainer();
		_keybindHook = new KeybindHook(_coreNativeManager, _commandItems);
	}

	/// <inheritdoc />
	public void Initialize()
	{
		_keybindHook.Initialize();
		LoadCommands(new CoreCommands(_context));
	}

	/// <inheritdoc />
	public void Add(ICommand command, IKeybind? keybind = null) => _commandItems.Add(command, keybind);

	/// <inheritdoc />
	public bool SetKeybind(string commandIdentifier, IKeybind keybind) =>
		_commandItems.SetKeybind(commandIdentifier, keybind);

	/// <inheritdoc />
	public void Clear() => _commandItems.Clear();

	/// <inheritdoc />
	public void ClearKeybinds() => _commandItems.ClearKeybinds();

	/// <inheritdoc />
	public bool RemoveKeybind(IKeybind keybind) => _commandItems.RemoveKeybind(keybind);

	/// <inheritdoc />
	public bool RemoveKeybind(string commandIdentifier) => _commandItems.RemoveKeybind(commandIdentifier);

	/// <inheritdoc />
	public bool RemoveCommand(string commandIdentifier) => _commandItems.RemoveCommand(commandIdentifier);

	/// <inheritdoc />
	public bool RemoveCommand(ICommand command) => _commandItems.RemoveCommand(command);

	/// <inheritdoc />
	public ICommand? TryGetCommand(string commandIdentifier) => _commandItems?.TryGetCommand(commandIdentifier);

	/// <inheritdoc />
	public ICommand? TryGetCommand(IKeybind keybind) => _commandItems?.TryGetCommand(keybind);

	/// <inheritdoc />
	public IKeybind? TryGetKeybind(string commandIdentifier) => _commandItems?.TryGetKeybind(commandIdentifier);

	/// <inheritdoc />
	public IKeybind? TryGetKeybind(ICommand command) => _commandItems?.TryGetKeybind(command);

	/// <inheritdoc />
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

	/// <inheritdoc />
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc />
	public IEnumerator<CommandItem> GetEnumerator() => _commandItems.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => _commandItems.GetEnumerator();

	/// <inheritdoc />
	public void LoadCommands(IEnumerable<CommandItem> commands)
	{
		foreach ((ICommand command, IKeybind? keybind) in commands)
		{
			Add(command, keybind);
		}
	}
}
