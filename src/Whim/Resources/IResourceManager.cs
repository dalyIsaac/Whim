using Microsoft.UI.Xaml;

namespace Whim;

/// <summary>
/// Interface to load package and user <see cref="ResourceDictionary"/>.
/// </summary>
public interface IResourceManager
{
	/// <summary>
	/// Path to default dictionary relative to package root. Omit the leading "/" when setting.
	/// </summary>
	string DefaultDict { get; set; }

	/// <summary>
	/// Initializes the <see cref="ResourceManager"/>.
	/// </summary>
	void Initialize();

	/// <summary>
	/// Add new dictionary from the package.
	/// <param name="filePath">Path to dictionary relative to the package root.</param>
	/// </summary>
	void SetAppDictionary(string filePath);

	/// <summary>
	/// Add  new dictionary from the users file system.
	/// <param name="filePath">Absolute path to dictionary in the local filesystem.</param>
	/// </summary>
	void SetUserDictionary(string filePath);
}
