using System.Collections.Generic;
using DotNext;
using Windows.Win32.Foundation;

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
				: Result.FromException<IWindow>(StoreExceptions.WindowNotFound(handle));
}
