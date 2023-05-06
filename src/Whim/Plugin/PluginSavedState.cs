using System.Text.Json;

namespace Whim;

internal class PluginSavedState
{
	/// <summary>
	/// The name of the plugin. See <see cref="IPlugin.Name"/>.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// The state of the plugin. It's up to the plugin to define, serialize, and deserialize this.
	/// </summary>
	JsonElement State { get; }
}

internal class PluginManagerSavedState
{
	/// <summary>
	/// The saved state of all plugins.
	/// </summary>
	PluginSavedState[] Plugins { get; }
}
