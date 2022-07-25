using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Whim;

/// <summary>
/// Applies the user's config to the <see cref="IConfigContext"/>.
/// </summary>
/// <param name="configContext">The <see cref="IConfigContext"/> to operate on.</param>
public delegate void DoConfig(IConfigContext configContext);

internal static class ConfigLoader
{
	private static readonly string ConfigFilePath = FileHelper.GetWhimFileDir("whim.config.csx");

	private static bool DoesConfigExist() => File.Exists(ConfigFilePath);

	/// <summary>
	/// Loads the Whim config from the Whim config file, if it exists.
	/// Otherwise, it will create a new Whim config file.
	/// </summary>
	/// <returns>The Whim config.</returns>
	private static string LoadRawConfig() => File.ReadAllText(ConfigFilePath);

	/// <summary>
	/// Read the given <paramref name="filename"/> from the assembly's resources and return it as a string.
	/// </summary>
	/// <param name="assembly"></param>
	/// <param name="filename"></param>
	/// <returns></returns>
	/// <exception cref="ConfigLoaderException"></exception>
	private static string ReadFile(this Assembly assembly, string filename)
	{
		string? templateName = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(filename));
		if (templateName == null)
		{
			throw new ConfigLoaderException($"Could not find file \"{filename}\" in assembly {assembly.FullName}");
		}

		using Stream? stream = assembly.GetManifestResourceStream(templateName);
		if (stream == null)
		{
			throw new ConfigLoaderException($"Could not find manifest resource stream for \"{filename}\" in assembly {assembly.FullName}");
		}

		using StreamReader reader = new(stream);
		return reader.ReadToEnd();
	}

	/// <summary>
	/// Load the Whim template from the assembly's resources, and replace the WHIM_PATH.
	/// </summary>
	/// <returns>The Whim template.</returns>
	/// <exception cref="ConfigLoaderException"></exception>
	private static string ReadTemplateConfigFile(this Assembly assembly)
	{
		// Load the Whim template from the assembly's resources.
		string template = assembly.ReadFile("whim.config.csx");

		// Replace WHIM_PATH with the assembly's path.
		string? assemblyPath = Path.GetDirectoryName(assembly.Location);
		if (assemblyPath == null)
		{
			throw new ConfigLoaderException($"Could not find assembly path for assembly {assembly.FullName}");
		}

		return template.Replace("WHIM_PATH", assemblyPath);
	}

	/// <summary>
	/// Creates a config based on the Whim template and saves it to the config file.
	/// This will throw if any null values are encountered.
	/// </summary>
	/// <exception cref="ConfigLoaderException"></exception>
	private static void CreateConfig()
	{
		Assembly? assembly = Assembly.GetAssembly(typeof(ConfigLoader));
		if (assembly is null)
		{
			throw new ConfigLoaderException("Could not find assembly for ConfigLoader");
		}

		string template = assembly.ReadTemplateConfigFile();
		string omnisharpJson = assembly.ReadFile("omnisharp.json");

		// Save the files.
		File.WriteAllText(ConfigFilePath, template);
		File.WriteAllText(FileHelper.GetWhimFileDir("omnisharp.json"), omnisharpJson);
	}

	/// <summary>
	/// Acquires and evaluates the user's <see cref="IConfigContext"/>.
	/// </summary>
	/// <returns>The <see cref="IConfigContext"/>.</returns>
	/// <exception cref="ConfigLoaderException"></exception>
	internal static DoConfig LoadConfigContext()
	{
		// Ensure the Whim directory exists.
		FileHelper.EnsureWhimDirExists();

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
