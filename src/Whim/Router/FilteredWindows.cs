namespace Whim;

/// <summary>
/// Defaults for various <see cref="IFilterManager"/>s.
/// </summary>
public static class FilteredWindows
{
	/// <summary>
	/// Load the windows which should be ignored by Whim by default.
	/// </summary>
	/// <param name="filterManager"></param>
	public static void LoadWindowsIgnoredByWhim(IFilterManager filterManager) =>
		filterManager.AddProcessNameFilter("SearchUI.exe");

	/// <summary>
	/// Load the windows which try to set their own locations when the start up.
	/// See <see cref="IWindowManager.LocationRestoringFilterManager"/>
	/// </summary>
	/// <param name="filterManager"></param>
	public static void LoadLocationRestoringWindows(IFilterManager filterManager) =>
		filterManager.AddProcessNameFilter("firefox.exe").AddProcessNameFilter("gateway64.exe");
}
