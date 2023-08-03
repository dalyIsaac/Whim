using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Whim;

/// <summary>
/// Applies the user's config to the <see cref="IContext"/>.
/// </summary>
/// <param name="context">The <see cref="IContext"/> to operate on.</param>
public delegate void DoConfig(IContext context);

internal class ConfigLoader
{
	private readonly IFileManager _fileManager;
	private readonly string _configFilePath;

	private bool DoesConfigExist() => File.Exists(_configFilePath);

	/// <summary>
	/// Loads the Whim config from the Whim config file, if it exists.
	/// Otherwise, it will create a new Whim config file.
	/// </summary>
	/// <returns>The Whim config.</returns>
	private string LoadRawConfig() => File.ReadAllText(_configFilePath);

	public ConfigLoader(IFileManager fileManager)
	{
		_fileManager = fileManager;
		_configFilePath = _fileManager.GetWhimFileDir("whim.config.csx");
	}

	/// <summary>
	/// Read the given <paramref name="filename"/> from the assembly's resources and return it as a string.
	/// </summary>
	/// <param name="assembly"></param>
	/// <param name="filename"></param>
	/// <returns></returns>
	/// <exception cref="ConfigLoaderException"></exception>
	private static string ReadFile(Assembly assembly, string filename)
	{
		string templateName =
			assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(filename))
			?? throw new ConfigLoaderException($"Could not find file \"{filename}\" in assembly {assembly.FullName}");

		using Stream stream =
			assembly.GetManifestResourceStream(templateName)
			?? throw new ConfigLoaderException(
				$"Could not find manifest resource stream for \"{filename}\" in assembly {assembly.FullName}"
			);

		using StreamReader reader = new(stream);
		return reader.ReadToEnd();
	}

	/// <summary>
	/// Load the Whim template from the assembly's resources, and replace the WHIM_PATH.
	/// </summary>
	/// <returns>The Whim template.</returns>
	/// <exception cref="ConfigLoaderException"></exception>
	private static string ReadTemplateConfigFile(Assembly assembly)
	{
		// Load the Whim template from the assembly's resources.
		string template = ReadFile(assembly, "whim.config.csx");

		// Replace WHIM_PATH with the assembly's path.
		string? assemblyPath = Path.GetDirectoryName(assembly.Location);
		return assemblyPath == null
			? throw new ConfigLoaderException($"Could not find assembly path for assembly {assembly.FullName}")
			: template.Replace("WHIM_PATH", assemblyPath);
	}

	/// <summary>
	/// Creates a config based on the Whim template and saves it to the config file.
	/// This will throw if any null values are encountered.
	/// </summary>
	/// <exception cref="ConfigLoaderException"></exception>
	private void CreateConfig()
	{
		Assembly assembly =
			Assembly.GetAssembly(typeof(ConfigLoader))
			?? throw new ConfigLoaderException("Could not find assembly for ConfigLoader");

		string template = ReadTemplateConfigFile(assembly);

		// Save the files.
		File.WriteAllText(_configFilePath, template);
	}

	/// <summary>
	/// Acquires and evaluates the user's config.
	/// </summary>
	/// <returns>The <see cref="IContext"/>.</returns>
	/// <exception cref="ConfigLoaderException"></exception>
	internal DoConfig LoadConfig()
	{
		// Ensure the Whim directory exists.
		_fileManager.EnsureDirExists(_fileManager.WhimDir);

		// Acquire the Whim config.
		bool configExists = DoesConfigExist();
		if (!configExists)
		{
			CreateConfig();
		}

		string rawConfig = LoadRawConfig();

		// Evaluate the Whim config.
		ScriptOptions options = ScriptOptions.Default;
		Task<DoConfig> task = CSharpScript.EvaluateAsync<DoConfig>(rawConfig, options);
		return task.Result;
	}
}
