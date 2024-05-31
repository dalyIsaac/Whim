using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// Some extensions which allows easy operations on <see cref="HWND"/> without requiring
/// an <see cref="IWindow"/>.
/// </summary>
internal static class HWNDExtensions
{
	public static bool IsFocused(this HWND hwnd, IInternalContext internalCtx) =>
		internalCtx.CoreNativeManager.GetForegroundWindow() == hwnd;

	public static bool IsMinimized(this HWND hwnd, IInternalContext internalCtx) =>
		internalCtx.CoreNativeManager.IsWindowMinimized(hwnd);

	public static bool IsMaximized(this HWND hwnd, IInternalContext internalCtx) =>
		internalCtx.CoreNativeManager.IsWindowMaximized(hwnd);
}
