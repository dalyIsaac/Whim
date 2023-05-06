using System;
using System.IO;

namespace Whim;

/// <summary>
/// Utility file methods.
/// </summary>
public static class FileHelper
{
	private static void EnsureDirExists(string dirName)
	{
		if (!Directory.Exists(dirName))
		{
			Directory.CreateDirectory(dirName);
		}
	}

	/// <summary>
	/// Gets the Whim path: <c>~/.whim</c>
	/// </summary>
	/// <returns></returns>
	public static string GetWhimDir() =>
		Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".whim");

	/// <summary>
	/// Ensures that the Whim directory exists. If it does not, it is created.
	/// </summary>
	public static void EnsureWhimDirExists() => EnsureDirExists(GetWhimDir());

	/// <summary>
	/// Gets a file path in the Whim directory.
	/// </summary>
	/// <param name="fileName">The file name.</param>
	/// <returns>The file path.</returns>
	public static string GetWhimFileDir(string fileName) => Path.Combine(GetWhimDir(), fileName);

	/// <summary>
	/// Gets the path to the saved state directory.
	/// </summary>
	/// <returns></returns>
	public static string GetSavedStateDir() => Path.Combine(GetWhimDir(), "state");

	/// <summary>
	/// Ensures that the saved state directory exists. If it does not, it is created.
	/// </summary>
	public static void EnsureSavedStateDirExists() => EnsureDirExists(GetSavedStateDir());

	/// <summary>
	/// Gets the path to the JSON file of the saved plugins state.
	/// </summary>
	/// <returns></returns>
	public static string GetSavedPluginsStatePath() => Path.Combine(GetSavedStateDir(), "plugins.json");
}
