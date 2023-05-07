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
	/// Gets a file path in the Whim directory.
	/// </summary>
	/// <param name="fileName">The file name.</param>
	/// <returns>The file path.</returns>
	string GetWhimFileDir(string fileName);
}
