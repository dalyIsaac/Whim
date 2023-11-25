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

	public FileManager(string[] args)
	{
		string? dir = GetDirFromArgs(args);
		if (dir != null)
		{
			WhimDir = dir;
		}
	}

	private static string? GetDirFromArgs(string[] args)
	{
		const string dirArg = "--dir";
		for (int i = 0; i < args.Length; i++)
		{
			string arg = args[i];
			if (!arg.StartsWith(dirArg))
			{
				continue;
			}

			if (arg.Length > dirArg.Length + 1)
			{
				return arg[(dirArg.Length + 1)..];
			}

			for (int j = i + 1; j < args.Length; j++)
			{
				string nextArg = args[j];
				if (string.IsNullOrEmpty(nextArg))
				{
					continue;
				}
				if (nextArg.StartsWith("--"))
				{
					return null;
				}

				return nextArg;
			}
		}

		return null;
	}

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

	/// <inheritdoc />
	public string ReadAllText(string filePath) => File.ReadAllText(filePath);

	/// <inheritdoc />
	public void WriteAllText(string filePath, string contents)
	{
		string? parentPath = Path.GetDirectoryName(filePath);
		if (parentPath != null)
		{
			EnsureDirExists(parentPath);
		}
		File.WriteAllText(filePath, contents);
	}
}
