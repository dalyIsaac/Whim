using System;
using System.IO;

namespace Whim;

/// <inheritdoc />
internal class FileManager : IFileManager
{
	/// <inheritdoc />
	public string WhimDir { get; } =
		Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".whim");

	/// <inheritdoc />
	public string SavedStateDir => Path.Combine(WhimDir, "state");

	/// <inheritdoc />
	public void EnsureDirExists(string dir)
	{
		if (!Directory.Exists(dir))
		{
			Directory.CreateDirectory(dir);
		}
	}

	/// <inheritdoc />
	public bool FileExists(string filePath) => File.Exists(filePath);

	/// <inheritdoc />
	public string GetWhimFileDir(string fileName) => Path.Combine(WhimDir, fileName);

	/// <inheritdoc />
	public Stream OpenRead(string filePath) => File.OpenRead(filePath);
}
