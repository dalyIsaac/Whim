using System.Text.Json;

namespace Whim.SliceLayout;

public class SliceLayoutPlugin : ISliceLayoutPlugin
{
	public string Name => "whim.leader_stack_layout";

	// TODO: Bar extension issue
	public IPluginCommands PluginCommands => new SliceLayoutCommands(this);

	public WindowInsertionType WindowInsertionType { get; set; }

	public void PreInitialize() { }

	public void PostInitialize() { }

	public void LoadState(JsonElement state) { }

	public JsonElement? SaveState() => null;
}
