using System;
using Windows.Win32.Foundation;

namespace Whim;

internal class WindowUtils
{
	[Obsolete("Temporary until the Butler is replaced")]
	public static IWindow? GetWindow(MutableRootSector rootSector, HWND handle) =>
		rootSector.WindowSector.Windows.TryGetValue(handle, out IWindow? w) ? w : null;
}
