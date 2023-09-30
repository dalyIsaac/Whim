using System.IO;

namespace Whim;

/// <summary>
/// Manages files and directories for Whim.
/// </summary>
public interface IFileManager
{
	/// <summary>
	/// The path to the Whim directory.
	/// </summary>
	string WhimDir { get; }

	/// <summary>
	/// The path to the saved state directory.
	/// </summary>
	string SavedStateDir { get; }

	/// <summary>
	/// Ensures the given <paramref name="dir"/> exists. If it does not, it is created.
	/// </summary>
	/// <param name="dir">The directory to ensure exists.</param>
	void EnsureDirExists(string dir);

	/// <summary>
	/// Checks whether the given <paramref name="filePath"/> exists.
	/// </summary>
	/// <param name="filePath">The file path to check.</param>
	/// <returns>Whether the file exists.</returns>
	bool FileExists(string filePath);

	/// <summary>
	/// Gets a file path in the Whim directory.
	/// </summary>
	/// <param name="fileName">The file name.</param>
	/// <returns>The file path.</returns>
	string GetWhimFileDir(string fileName);

	/// <summary>
	/// Opens an existing file for reading.
	/// </summary>
	/// <param name="filePath">The file path.</param>
	/// <returns>The file stream.</returns>
	Stream OpenRead(string filePath);

	/// <summary>
	/// Opens a text file, reads all the text in the file, and then closes the file.
	/// </summary>
	/// <param name="filePath">The file path.</param>
	/// <returns>A string containing all the text in the file.</returns>
	string ReadAllText(string filePath);

	/// <summary>
	/// WRites the given <paramref name="contents"/> to the given <paramref name="filePath"/>.
	/// </summary>
	/// <param name="filePath">The file path.</param>
	/// <param name="contents">The contents to write.</param>
	void WriteAllText(string filePath, string contents);
}
