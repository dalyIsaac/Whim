namespace Whim;

/// <inheritdoc />
/// <summary>
/// Initializes a new instance of the <see cref="Command"/> class.
/// </summary>
/// <param name="identifier">The identifier of the command.</param>
/// <param name="title">The title of the command.</param>
/// <param name="callback">
/// The callback to execute.
/// This can include triggering a menu to be shown by Whim.CommandPalette,
/// or to perform some other action.
/// </param>
/// <param name="condition">
/// A condition to determine if the command should be visible, or able to be
/// executed.
///
/// When this evaluates to false, the <paramref name="callback"/> will not be executed.
///
/// If this is null, the command will always be accessible.
/// </param>
public class Command(string identifier, string title, Action callback, Func<bool>? condition = null) : ICommand
{
	private readonly Action _callback = callback;
	private readonly Func<bool>? _condition = condition;

	/// <inheritdoc />
	public string Id { get; } = identifier;

	/// <inheritdoc />
	public string Title { get; } = title;

	/// <inheritdoc />
	public bool CanExecute()
	{
		return _condition?.Invoke() ?? true;
	}

	/// <inheritdoc />
	public bool TryExecute()
	{
		Logger.Debug($"Trying to execute command {Id}");

		if (CanExecute())
		{
			_callback();
			return true;
		}

		Logger.Debug($"Command {Id} is not executable right now");
		return false;
	}

	/// <inheritdoc />
	public override string ToString() => $"{Id} ({Title})";
}
