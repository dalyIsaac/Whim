using System.Text;
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
	private static ErrorWindow? _errorWindow;

	/// <summary>
	/// Loads and applies the declarative configuration from a JSON or YAML file.
	/// </summary>
	/// <param name="ctx">The <see cref="IContext"/> to operate on.</param>
	/// <param name="showErrorWindow">Whether to show an error window if the configuration is invalid.</param>
	/// <returns>
	/// <see langword="true"/> if the configuration was parsed successfully; otherwise, <see langword="false"/>.
	/// </returns>
	public static bool Load(IContext ctx, bool showErrorWindow = true)
	{
		if (Parse(ctx) is not Schema schema)
		{
			return false;
		}

		ValidateConfig(ctx, schema, showErrorWindow);

		UpdateWorkspaces(ctx, schema);

		UpdateKeybinds(ctx, schema);
		UpdateFilters(ctx, schema);
		UpdateRouters(ctx, schema);
		UpdateStyles(ctx, schema, showErrorWindow);

		YamlPluginLoader.LoadPlugins(ctx, schema);
		YamlLayoutEngineLoader.UpdateLayoutEngines(ctx, schema);

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

	private static void ValidateConfig(IContext ctx, Schema schema, bool showErrorWindow)
	{
		ValidationContext result = schema.Validate(ValidationContext.ValidContext, ValidationLevel.Detailed);
		if (result.IsValid)
		{
			return;
		}

		StringBuilder sb = new();
		int idx = 0;
		foreach (ValidationResult error in result.Results)
		{
			if (error.Valid)
			{
				continue;
			}

			sb.AppendFormat("Error {0}:\n", idx + 1);
			sb.AppendLine(error.Message);
			sb.AppendFormat(
				"Violated {0}\n",
				error.Location.HasValue ? error.Location.Value.ValidationLocation.ToString() : "unknown schema"
			);
			idx += 1;
		}
		string errors = sb.ToString();

		Logger.Error("Configuration file is not valid.");
		Logger.Error(errors);

		if (showErrorWindow)
		{
			ShowError(ctx, errors);
		}
	}

	private static void ShowError(IContext ctx, string errors)
	{
		if (_errorWindow == null)
		{
			_errorWindow = new(ctx, errors);
			_errorWindow.Activate();
		}
		else
		{
			_errorWindow.AppendMessage(errors);
		}
	}

	private static void UpdateWorkspaces(IContext ctx, Schema schema)
	{
		if (schema.Workspaces is not { } workspaces || !workspaces.IsValid())
		{
			Logger.Debug("Workspaces config is not valid.");
			return;
		}

		if (workspaces.Entries is not { } entries)
		{
			Logger.Debug("No workspaces found.");
			return;
		}

		foreach (var workspace in entries)
		{
			string workspaceName = (string)workspace.Name;

			CreateLeafLayoutEngine[]? engines = null;
			List<int>? monitorIndices = null;

			if (workspace.LayoutEngines?.Entries is { } definedEngines)
			{
				engines = YamlLayoutEngineLoader.GetCreateLeafLayoutEngines(ctx, [.. definedEngines]);
			}

			if (workspace.Monitors is { } definedMonitorIndices)
			{
				monitorIndices = [];
				foreach (var monitorIndex in definedMonitorIndices)
				{
					monitorIndices.Add((int)monitorIndex);
				}
			}

			ctx.Store.Dispatch(new AddWorkspaceTransform(workspaceName, engines, MonitorIndices: monitorIndices));
		}
	}

	private static void UpdateKeybinds(IContext ctx, Schema schema)
	{
		if (schema.Keybinds is not { } keybinds || !keybinds.IsValid())
		{
			Logger.Debug("Keybinds config is not valid.");
			return;
		}

		if (keybinds.UnifyKeyModifiers?.TryGetBoolean(out bool unifyKeyModifiers) == true)
		{
			ctx.KeybindManager.UnifyKeyModifiers = unifyKeyModifiers;
		}

		if (keybinds.Entries is not { } entries)
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
		if (schema.Filters is not { } filters || !filters.IsValid())
		{
			Logger.Debug("Filters config is not valid.");
			return;
		}

		if (filters.Entries is not { } entries)
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
		if (schema.Routers is not { } routers || !routers.IsValid())
		{
			Logger.Debug("Routers cohfig is not valid.");
			return;
		}

		if (
			routers.RoutingBehavior?.TryGetString(out string? routingBehavior) == true
			&& routingBehavior != null
			&& Enum.TryParse(routingBehavior.SnakeToPascal(), out RouterOptions routerOptions)
		)
		{
			ctx.RouterManager.RouterOptions = routerOptions;
		}

		if (routers.Entries is not { } entries)
		{
			Logger.Debug("No routers found.");
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

	private static void UpdateStyles(IContext ctx, Schema schema, bool showErrorWindow)
	{
		if (schema.Styles is not { } styles || !styles.IsValid())
		{
			Logger.Debug("Styles config is not valid.");
			return;
		}

		if (styles.UserDictionaries is not { } userDictionaries)
		{
			Logger.Debug("No styles found.");
			return;
		}

		foreach (var userDictionary in userDictionaries)
		{
			if (GetUserDictionaryPath(ctx, (string)userDictionary, showErrorWindow) is not string filePath)
			{
				continue;
			}

			ctx.ResourceManager.AddUserDictionary(filePath);
		}
	}

	private static string? GetUserDictionaryPath(IContext ctx, string filePath, bool showErrorWindow)
	{
		if (ctx.FileManager.FileExists(filePath))
		{
			return filePath;
		}

		string relativePath = Path.Combine(ctx.FileManager.WhimDir, filePath);
		if (!ctx.FileManager.FileExists(relativePath))
		{
			string error = $"User dictionary not found: {filePath}";
			Logger.Error(error);

			if (showErrorWindow)
			{
				ShowError(ctx, error);
			}
			return null;
		}

		return relativePath;
	}
}
