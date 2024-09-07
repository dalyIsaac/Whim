namespace Whim.Yaml;

/// <summary>
/// Parses JSON or YAML configuration files.
/// </summary>
public static class JsonParser
{
	private const string JsonConfigFileName = "whim.config.json";
	private const string YamlConfigFileName = "whim.config.yaml";

	/// <summary>
	/// Loads and applies the declarative configuration from a JSON or YAML file.
	/// </summary>
	/// <param name="ctx">The <see cref="IContext"/> to operate on.</param>
	public static void Load(IContext ctx)
	{
		Schema? schema = ParseJson(ctx);
		if (schema == null)
		{
			return;
		}

		UpdateKeybinds(ctx, schema);
	}

	private static Schema? ParseJson(IContext ctx)
	{
		string jsonConfigPath = ctx.FileManager.GetWhimFileDir(JsonConfigFileName);
		string yamlConfigPath = ctx.FileManager.GetWhimFileDir(YamlConfigFileName);

		if (ctx.FileManager.FileExists(jsonConfigPath))
		{
			string json = ctx.FileManager.ReadAllText(jsonConfigPath);
			return ParseJson(json);
		}

		if (ctx.FileManager.FileExists(yamlConfigPath))
		{
			string yaml = ctx.FileManager.ReadAllText(yamlConfigPath);
			return ParseYaml(yaml);
		}

		return null;
	}

	private static void UpdateKeybinds(IContext ctx, Schema schema)
	{
		IKeybindManager keybindManager = ctx.KeybindManager;
		keybindManager.Clear();

		foreach (KeyValuePair<string, Keybind> keybind in schema.Keybinds)
		{
			keybindManager.SetKeybind(keybind.Key, keybind.Value);
		}
	}
}
