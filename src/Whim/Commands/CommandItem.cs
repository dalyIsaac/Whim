namespace Whim;

/// <summary>
/// A command and associated keybind.
/// </summary>
public class CommandItem
{
	/// <summary>
	/// The command to execute.
	/// </summary>
	public ICommand Command { get; }

	/// <summary>
	/// The keybind to execute the command.
	/// </summary>
	public IKeybind? Keybind { get; }

	/// <summary>
	/// Creates a new match from the given command and keybind.
	/// </summary>
	/// <param name="command"></param>
	/// <param name="keybind"></param>
	public CommandItem(ICommand command, IKeybind? keybind = null)
	{
		Command = command;
		Keybind = keybind;
	}

	/// <inheritdoc />
	public override int GetHashCode() => Command.GetHashCode();

	/// <inheritdoc />
	public void Deconstruct(out ICommand command, out IKeybind? keybind) => (command, keybind) = (Command, Keybind);
}
