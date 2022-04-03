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
	/// </summary>
	internal static void CreateConfig()
	{
		// Load the Whim template from the assembly's resources.
		Assembly? assembly = Assembly.GetAssembly(typeof(Program));
		string templateName = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("whim.config.template.csx"));

		using Stream stream = assembly.GetManifestResourceStream(templateName);
		using StreamReader reader = new(stream);
		string template = reader.ReadToEnd();

		// Replace WHIM_PATH with the assembly's path.
		string assemblyPath = Path.GetDirectoryName(assembly.Location);
		template = template.Replace("WHIM_PATH", assemblyPath);

		// Save the Whim config to the Whim config file.
		File.WriteAllText(configFilePath, template);
	}
}
