namespace Whim;

public static partial class Pickers
{
	/// <summary>
	/// Get all the <see cref="IWindow"/>s tracked by Whim.
	/// </summary>
	/// <returns></returns>
	public static PurePicker<IEnumerable<IWindow>> PickAllWindows() =>
		static (rootSector) => rootSector.WindowSector.Windows.Values;

	/// <summary>
	/// Try to get a <see cref="IWindow"/> by its <see cref="HWND"/> handle.
	/// </summary>
	/// <param name="handle"></param>
	/// <returns></returns>
	public static PurePicker<Result<IWindow>> PickWindowByHandle(HWND handle) =>
		(rootSector) =>
			rootSector.WindowSector.Windows.TryGetValue(handle, out IWindow? w)
				? Result.FromValue(w)
				: Result.FromError<IWindow>(StoreExceptions.WindowNotFound(handle));

	/// <summary>
	/// Returns whether a window was open when Whim started.
	/// </summary>
	/// <param name="handle"></param>
	/// <returns></returns>
	public static PurePicker<bool> PickIsStartupWindow(HWND handle) =>
		(rootSector) => rootSector.WindowSector.StartupWindows.Contains(handle);
}
