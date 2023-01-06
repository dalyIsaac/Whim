namespace Whim;

/// <summary>
/// A command is a function with a unique identifier.
/// </summary>
public interface ICommand
{
	/// <summary>
	/// The unique identifier. For example, "whim.core.focus_right_window".
	/// </summary>
	public string Id { get; }

	/// <summary>
	/// The title of the command. For example, "Focus right window".
	/// </summary>
	public string Title { get; }

	/// <summary>
	/// Indicates whether the command can currently be executed.
	/// </summary>
	public bool CanExecute();

	/// <summary>
	/// Tries to execute the command. If the command is not executable (due to
	/// the condition not being met), false is returned.
	/// </summary>
	/// <returns>True if the command was executed, false otherwise.</returns>
	public bool TryExecute();
}
