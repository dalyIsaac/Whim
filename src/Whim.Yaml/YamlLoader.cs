using Corvus.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Yaml2JsonNode;
using YamlDotNet.RepresentationModel;

namespace Whim.Yaml;

/// <summary>
/// Parses JSON or YAML configuration files.
/// </summary>
public static class YamlLoader
{
	private const string JsonConfigFileName = "whim.config.json";
	private const string YamlConfigFileName = "whim.config.yaml";

	/// <summary>
	/// Loads and applies the declarative configuration from a JSON or YAML file.
	/// </summary>
	/// <param name="ctx">The <see cref="IContext"/> to operate on.</param>
	public static void Load(IContext ctx)
	{
		if (Parse(ctx) is not Schema schema)
		{
			return;
		}

		UpdateKeybinds(ctx, schema);
	}

	private static Schema? Parse(IContext ctx)
	{
		string jsonConfigPath = ctx.FileManager.GetWhimFileDir(JsonConfigFileName);
		string yamlConfigPath = ctx.FileManager.GetWhimFileDir(YamlConfigFileName);

		if (ctx.FileManager.FileExists(jsonConfigPath))
		{
			string json = ctx.FileManager.ReadAllText(jsonConfigPath);
			return Schema.Parse(json);
		}

		if (ctx.FileManager.FileExists(yamlConfigPath))
		{
			string yaml = ctx.FileManager.ReadAllText(yamlConfigPath);
			YamlStream stream = [];
			stream.Load(new StringReader(yaml));

			if (stream.ToJsonNode().First() is not JsonNode root)
			{
				return null;
			}

			JsonElement element = JsonSerializer.Deserialize<JsonElement>(root);
			return Schema.FromJson(element);
		}

		Logger.Debug("No configuration file found.");
		return null;
	}

	private static void UpdateKeybinds(IContext ctx, Schema schema)
	{
		IKeybindManager keybindManager = ctx.KeybindManager;
		keybindManager.Clear();

		foreach (Schema.RequiredCommandAndKeybind pair in schema.Keybinds)
		{
			if (!pair.Keybind.IsValid())
			{
				Logger.Error($"Invalid keybind: {pair.Keybind}");
				continue;
			}

			if (!pair.Command.IsValid())
			{
				Logger.Error($"Invalid command: {pair.Command}");
				continue;
			}

			if (Keybind.FromString((string)pair.Keybind) is not Keybind keybind)
			{
				Logger.Error($"Invalid keybind: {pair.Keybind}");
				continue;
			}

			keybindManager.SetKeybind((string)pair.Command, keybind);
		}
	}
}
