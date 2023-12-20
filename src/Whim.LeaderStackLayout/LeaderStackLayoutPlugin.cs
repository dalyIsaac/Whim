using System.Text.Json;

namespace Whim.LeaderStackLayout;

public class LeaderStackLayoutPlugin : ILeaderStackLayoutPlugin
{
	public string Name => "whim.leader_stack_layout";

	// TODO
	public IPluginCommands PluginCommands => new PluginCommands(Name);

	public void PreInitialize()
	{
		// TODO
	}

	public void PostInitialize()
	{
		// TODO
	}

	public void LoadState(JsonElement state)
	{
		// TODO
	}

	public JsonElement? SaveState()
	{
		// TODO
		return null;
	}
}
