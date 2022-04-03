using System;
using System.IO;

namespace Whim;

/// <summary>
/// Utility file methods.
/// </summary>
public static class FileHelper
{
	/// <summary>
	/// Gets the Whim path: <c>~/.whim</c>
	/// </summary>
	/// <returns></returns>
	public static string GetWhimDir() => Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
			".whim"
		);

	public static void EnsureWhimDirExists()
	{
		if (!Directory.Exists(GetWhimDir()))
		{
			Directory.CreateDirectory(GetWhimDir());
		}
	}

	/// <summary>
	/// Gets a file path in the Whim directory.
	/// </summary>
	/// <param name="fileName">The file name.</param>
	/// <returns>The file path.</returns>
	public static string GetWhimFileDir(string fileName) => Path.Combine(
			GetWhimDir(),
			fileName
		);
}
