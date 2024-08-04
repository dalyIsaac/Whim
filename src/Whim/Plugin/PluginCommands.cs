namespace Whim;

/// <inheritdoc />
public class PluginCommands : IPluginCommands
{
	private readonly List<ICommand> _commands = [];
	private readonly List<(string commandId, IKeybind keybind)> _keybinds = [];

	/// <inheritdoc />
	public string PluginName { get; init; }

	/// <inheritdoc />
	public IEnumerable<ICommand> Commands => _commands;

	/// <inheritdoc />
	public IEnumerable<(string commandId, IKeybind keybind)> Keybinds => _keybinds;

	/// <summary>
	/// Creates a new instance of the plugin commands.
	/// </summary>
	/// <param name="pluginName">The name of the plugin.</param>
	public PluginCommands(string pluginName)
	{
		PluginName = pluginName;
	}

	/// <summary>
	/// Add a command to the plugin.
	/// </summary>
	/// <param name="identifier">
	/// The identifier of the command. This shouldn't include the <see cref="PluginName"/>.
	/// </param>
	/// <param name="title">The title of the command.</param>
	/// <param name="callback">
	/// The callback to execute.
	/// This can include triggering a menu to be shown by Whim.CommandPalette,
	/// or to perform some other action.
	/// </param>
	/// <param name="condition">
	/// A condition to determine if the command should be visible, or able to be
	/// executed.
	/// If this is null, the command will always be accessible.
	/// </param>
	/// <param name="keybind">The keybind to associate with the command.</param>
	public PluginCommands Add(
		string identifier,
		string title,
		Action callback,
		Func<bool>? condition = null,
		IKeybind? keybind = null
	)
	{
		string fullId = $"{PluginName}.{identifier}";
		ICommand command = new Command(fullId, title, callback, condition);
		_commands.Add(command);

		if (keybind != null)
		{
			_keybinds.Add((fullId, keybind));
		}

		return this;
	}
}
