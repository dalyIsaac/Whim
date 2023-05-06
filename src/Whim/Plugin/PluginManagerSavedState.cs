using System.Collections.Generic;
using System.Text.Json;

namespace Whim;

internal class PluginManagerSavedState
{
	/// <summary>
	/// The saved state of all plugins.
	/// </summary>
	public Dictionary<string, JsonElement> Plugins { get; set; } = new();
}
