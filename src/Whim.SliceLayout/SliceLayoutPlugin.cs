using System.Text.Json;

namespace Whim.SliceLayout;

public class SliceLayoutPlugin : ISliceLayoutPlugin
{
	public string Name => "whim.leader_stack_layout";

	// TODO
	public IPluginCommands PluginCommands => new PluginCommands(Name);

	public WindowInsertionType WindowInsertionType { get; set; }

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
