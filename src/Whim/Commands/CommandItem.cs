namespace Whim;

/// <summary>
/// A command and associated keybind.
/// </summary>
public record CommandItem
{
	/// <summary>
	/// The command to execute.
	/// </summary>
	public required ICommand Command { get; init; }

	/// <summary>
	/// The keybind to execute the command.
	/// </summary>
	public IKeybind? Keybind { get; init; }

	/// <inheritdoc />
	public override int GetHashCode() => Command.GetHashCode();

	/// <inheritdoc />
	public void Deconstruct(out ICommand command, out IKeybind? keybind) => (command, keybind) = (Command, Keybind);
}
