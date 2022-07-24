namespace Whim;

/// <summary>
/// A plugin for Whim.
/// </summary>
public interface IPlugin
{
	/// <summary>
	/// <b>This method is to be called by the plugin manager.</b>
	/// Initializes the plugin before the <see cref="IConfigContext"/> has been initialized.
	/// Put things like event listeners here or adding proxy layout engines
	/// (see <see cref="IWorkspaceManager.AddProxyLayoutEngine(ProxyLayoutEngine)"/>).
	/// </summary>
	public void PreInitialize();

	/// <summary>
	/// <b>This method is to be called by the plugin manager.</b>
	/// Initializes the plugin after the rest of the <see cref="IConfigContext"/> has been initialized.
	/// Put things which rely on the rest of the config context here.
	/// </summary>
	public void PostInitialize();

	/// <summary>
	/// Get the default commands for this plugin, and their associated keybinds.
	/// It's up to the user to register the commands themselves using
	/// <see cref="ICommandManager.LoadCommands"/>.
	/// </summary>
	public (ICommand, IKeybind?)[] GetCommands();
}
