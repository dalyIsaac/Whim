using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Whim.App;

internal static class ConfigHelper
{
	private readonly static string configFilePath = FileHelper.GetWhimFileDir("whim.config.csx");

	internal static bool DoesConfigExist() => File.Exists(configFilePath);

	/// <summary>
	/// Loads the Whim config from the Whim config file, if it exists.
	/// Otherwise, it will create a new Whim config file.
	/// </summary>
	/// <returns>The Whim config.</returns>
	internal static string LoadConfig()
	{
		return File.ReadAllText(configFilePath);
	}

	/// <summary>
	/// Creates a config based on the Whim template and saves it to the config file.
	/// This will throw if any null values are encountered.
	/// </summary>
	internal static void CreateConfig()
	{
		// Load the Whim template from the assembly's resources.
		Assembly? assembly = Assembly.GetAssembly(typeof(Program));
		if (assembly == null)
		{
			throw new Exception($"Could not find assembly for {nameof(Program)}");
		}

		string? templateName = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("whim.config.template.csx"));
		if (templateName == null)
		{
			throw new Exception($"Could not find Whim template in assembly {assembly.FullName}");
		}

		using Stream? stream = assembly.GetManifestResourceStream(templateName);
		if (stream == null)
		{
			throw new Exception($"Could not find manifest resource stream for Whim template in assembly {assembly.FullName}");
		}

		using StreamReader reader = new(stream);
		string template = reader.ReadToEnd();

		// Replace WHIM_PATH with the assembly's path.
		string? assemblyPath = Path.GetDirectoryName(assembly.Location);
		if (assemblyPath == null)
		{
			throw new Exception($"Could not find assembly path for assembly {assembly.FullName}");
		}

		template = template.Replace("WHIM_PATH", assemblyPath);

		// Save the Whim config to the Whim config file.
		File.WriteAllText(configFilePath, template);
	}
}
