namespace Whim.Updater;

/// <summary>
/// The commands for the updater plugin.
/// </summary>
public class UpdaterCommands : PluginCommands
{
	private readonly IUpdaterPlugin _updaterPlugin;

	/// <summary>
	/// Creates a new instance of the updater commands.
	/// </summary>
	/// <param name="updaterPlugin"></param>
	public UpdaterCommands(IUpdaterPlugin updaterPlugin)
		: base(updaterPlugin.Name)
	{
		_updaterPlugin = updaterPlugin;

		Add(identifier: "check", title: "Check for updates", () => _updaterPlugin.CheckForUpdates());
	}
}
