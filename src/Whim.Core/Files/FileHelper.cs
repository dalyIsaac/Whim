using System;
using System.IO;

namespace Whim.Core;

/// <summary>
/// Utility file methods.
/// </summary>
public static class FileHelper
{
	/// <summary>
	/// Gets the Whim path: <c>~/.whim</c>
	/// </summary>
	/// <returns></returns>
	public static string GetUserWhimPath() => Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
			".whim"
		);
}
