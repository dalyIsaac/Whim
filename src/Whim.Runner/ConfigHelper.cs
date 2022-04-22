using System.IO;
using System.Linq;
using System.Reflection;

namespace Whim.Runner;

internal static class ConfigHelper
{
	private static readonly string configFilePath = FileHelper.GetWhimFileDir("whim.config.csx");

	internal static bool DoesConfigExist() => File.Exists(configFilePath);

	/// <summary>
	/// Loads the Whim config from the Whim config file, if it exists.
	/// Otherwise, it will create a new Whim config file.
	/// </summary>
	/// <returns>The Whim config.</returns>
	internal static string LoadConfig() => File.ReadAllText(configFilePath);

	/// <summary>
	/// Read the given <paramref name="filename"/> from the assembly's resources and return it as a string.
	/// </summary>
	/// <param name="assembly"></param>
	/// <param name="filename"></param>
	/// <returns></returns>
	/// <exception cref="RunnerException"></exception>
	private static string ReadFile(this Assembly assembly, string filename)
	{
		string? templateName = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(filename));
		if (templateName == null)
		{
			throw new RunnerException($"Could not find file \"{filename}\" in assembly {assembly.FullName}");
		}

		using Stream? stream = assembly.GetManifestResourceStream(templateName);
		if (stream == null)
		{
			throw new RunnerException($"Could not find manifest resource stream for \"{filename}\" in assembly {assembly.FullName}");
		}

		using StreamReader reader = new(stream);
		return reader.ReadToEnd();
	}

	/// <summary>
	/// Load the Whim template from the assembly's resources, and replace the WHIM_PATH.
	/// </summary>
	/// <returns>The Whim template.</returns>
	private static string ReadConfigFile(this Assembly assembly)
	{
		// Load the Whim template from the assembly's resources.
		string template = assembly.ReadFile("whim.config.template.csx");

		// Replace WHIM_PATH with the assembly's path.
		string? assemblyPath = Path.GetDirectoryName(assembly.Location);
		if (assemblyPath == null)
		{
			throw new RunnerException($"Could not find assembly path for assembly {assembly.FullName}");
		}

		return template.Replace("WHIM_PATH", assemblyPath);
	}

	/// <summary>
	/// Creates a config based on the Whim template and saves it to the config file.
	/// This will throw if any null values are encountered.
	/// </summary>
	internal static void CreateConfig()
	{
		// Load the assembly.
		Assembly? assembly = Assembly.GetAssembly(typeof(Program));
		if (assembly == null)
		{
			throw new RunnerException($"Could not find assembly for {nameof(Program)}");
		}

		string template = assembly.ReadConfigFile();
		string omnisharpJson = assembly.ReadFile("omnisharp.json");

		// Save the files.
		File.WriteAllText(configFilePath, template);
		File.WriteAllText(FileHelper.GetWhimFileDir("omnisharp.json"), omnisharpJson);
	}
}
