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

		YamlPluginLoader.LoadPlugins(ctx, schema);
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
			Logger.Debug("Keybinds plugin is not valid.");
			return;
		}

		if (schema.Keybinds.UnifyKeyModifiers.TryGetBoolean(out bool unifyKeyModifiers))
		{
			ctx.KeybindManager.UnifyKeyModifiers = unifyKeyModifiers;
		}

		if (schema.Keybinds.Entries.AsOptional() is not { } entries)
		{
			Logger.Debug("No keybinds found.");
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
			Logger.Debug("Filters plugin is not valid.");
			return;
		}

		if (schema.Filters.Entries.AsOptional() is not { } entries)
		{
			Logger.Debug("No filters found.");
			return;
		}

		foreach (var filter in entries)
		{
			string value = (string)filter.Value;

			switch ((string)filter.Type)
			{
				case "window_class":
					ctx.FilterManager.AddWindowClassFilter(value);
					break;
				case "process_file_name":
					ctx.FilterManager.AddProcessFileNameFilter(value);
					break;
				case "title":
					ctx.FilterManager.AddTitleFilter(value);
					break;
				case "title_regex":
					ctx.FilterManager.AddTitleMatchFilter(value);
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
			Logger.Debug("Routers plugin is not valid.");
			return;
		}

		if (
			schema.Routers.RoutingBehavior.TryGetString(out string? routingBehavior)
			&& Enum.TryParse(routingBehavior?.SnakeToPascal(), out RouterOptions routerOptions)
		)
		{
			ctx.RouterManager.RouterOptions = routerOptions;
		}

		if (schema.Routers.Entries.AsOptional() is not { } entries)
		{
			return;
		}

		foreach (var router in entries)
		{
			string value = (string)router.Value;
			string workspaceName = (string)router.WorkspaceName;

			switch ((string)router.Type)
			{
				case "window_class":
					ctx.RouterManager.AddWindowClassRoute(value, workspaceName);
					break;
				case "process_file_name":
					ctx.RouterManager.AddProcessFileNameRoute(value, workspaceName);
					break;
				case "title":
					ctx.RouterManager.AddTitleRoute(value, workspaceName);
					break;
				case "title_regex":
					ctx.RouterManager.AddTitleMatchRoute(value, workspaceName);
					break;
				default:
					Logger.Error($"Invalid router type: {router.Type}");
					break;
			}
		}
	}

	private static void UpdateLayoutEngines(IContext cts, Schema schema)
	{

		if (!schema.LayoutEngines.IsValid())
		{
			Logger.Debug("LayoutEngines plugin is not valid.");
			return;
		}

		if (schema.LayoutEngines.AsOptional() is not { } entries)
		{
			Logger.Debug("No layout engines found.");
			return;
		}

		foreach (var engine in entries)
		{
			return engine.Match(
				(in Schema.RequiredType.)
			);
		}
	}
}
