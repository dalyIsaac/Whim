using System.Text.Json;
using System.Text.Json.Nodes;
using Corvus.Json;
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
	/// <returns>
	/// <see langword="true"/> if the configuration was parsed successfully; otherwise, <see langword="false"/>.
	/// </returns>
	public static bool Load(IContext ctx)
	{
		if (Parse(ctx) is not Schema schema)
		{
			return false;
		}

		UpdateKeybinds(ctx, schema);
		UpdateFilters(ctx, schema);
		UpdateRouters(ctx, schema);
		return true;
	}

	private static Schema? Parse(IContext ctx)
	{
		string yamlConfigPath = ctx.FileManager.GetWhimFileDir(YamlConfigFileName);
		if (ctx.FileManager.FileExists(yamlConfigPath))
		{
			string yaml = ctx.FileManager.ReadAllText(yamlConfigPath);
			YamlStream stream = [];
			stream.Load(new StringReader(yaml));

			if (stream.ToJsonNode().FirstOrDefault() is not JsonNode root)
			{
				return null;
			}

			JsonElement element = JsonSerializer.Deserialize<JsonElement>(root);
			return Schema.FromJson(element);
		}

		string jsonConfigPath = ctx.FileManager.GetWhimFileDir(JsonConfigFileName);
		if (ctx.FileManager.FileExists(jsonConfigPath))
		{
			string json = ctx.FileManager.ReadAllText(jsonConfigPath);
			return Schema.Parse(json);
		}

		Logger.Debug("No configuration file found.");
		return null;
	}

	private static void UpdateKeybinds(IContext ctx, Schema schema)
	{
		if (!schema.Keybinds.IsValid())
		{
			return;
		}

		if (schema.Keybinds.UnifyKeyModifiers.TryGetBoolean(out bool unifyKeyModifiers))
		{
			ctx.KeybindManager.UnifyKeyModifiers = unifyKeyModifiers;
		}

		if (schema.Keybinds.Entries.AsOptional() is not { } entries)
		{
			return;
		}

		foreach (var pair in entries)
		{
			if (Keybind.FromString((string)pair.Keybind) is not Keybind keybind)
			{
				Logger.Error($"Invalid keybind: {pair.Keybind}");
				continue;
			}

			ctx.KeybindManager.SetKeybind((string)pair.Command, keybind);
		}
	}

	private static void UpdateFilters(IContext ctx, Schema schema)
	{
		if (!schema.Filters.IsValid())
		{
			return;
		}

		foreach (var filter in schema.Filters)
		{
			switch ((string)filter.Type)
			{
				case "windowClass":
					ctx.FilterManager.AddWindowClassFilter((string)filter.Value);
					break;
				case "processFileName":
					ctx.FilterManager.AddProcessFileNameFilter((string)filter.Value);
					break;
				case "title":
					ctx.FilterManager.AddTitleFilter((string)filter.Value);
					break;
				case "titleMatch":
					ctx.FilterManager.AddTitleMatchFilter((string)filter.Value);
					break;
				default:
					Logger.Error($"Invalid filter type: {filter.Type}");
					break;
			}
		}
	}

	private static void UpdateRouters(IContext ctx, Schema schema)
	{
		if (!schema.Routers.IsValid())
		{
			return;
		}

		foreach (var router in schema.Routers)
		{
			switch ((string)router.Type)
			{
				case "windowClass":
					ctx.RouterManager.AddWindowClassRoute((string)router.Value, (string)router.Workspace);
					break;
				case "processFileName":
					ctx.RouterManager.AddProcessFileNameRoute((string)router.Value, (string)router.Workspace);
					break;
				case "title":
					ctx.RouterManager.AddTitleRoute((string)router.Value, (string)router.Workspace);
					break;
				case "titleMatch":
					ctx.RouterManager.AddTitleMatchRoute((string)router.Value, (string)router.Workspace);
					break;
				default:
					Logger.Error($"Invalid router type: {router.Type}");
					break;
			}
		}
	}
}
