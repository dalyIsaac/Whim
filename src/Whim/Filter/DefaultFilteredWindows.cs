using System.IO;

namespace Whim;

/// <summary>
/// Defaults for various <see cref="IFilterManager"/>s.
/// </summary>
public static class DefaultFilteredWindows
{
	/// <summary>
	/// Load the windows which should be ignored by Whim by default.
	/// </summary>
	/// <param name="filterManager"></param>
	public static void LoadWindowsIgnoredByWhim(IFilterManager filterManager)
	{
		DefaultFilteredWindowsKomorebi.LoadWindowsIgnoredByKomorebi(filterManager);

		// Whim's own UI windows (the bar, command palette, focus indicator, layout preview, the
		// exception dialog, etc.) are all hosted in this same process. They rely on a Title-based
		// filter that's set partway through their constructor, which can race with Whim discovering
		// the new window - e.g. during a monitor change, a freshly (re)created bar window can be
		// observed by WindowAddedTransform before its Title is set to "Whim Bar", so the title
		// filter silently fails to match and the window gets tiled like a normal app window.
		// Filtering by this process's own executable name has no such race: the process' file name
		// is available from the OS as soon as the window handle exists, and nothing this process
		// creates should ever be treated as a manageable application window.
		if (Environment.ProcessPath is string processPath)
		{
			filterManager.AddProcessFileNameFilter(Path.GetFileName(processPath));
		}
	}
}
