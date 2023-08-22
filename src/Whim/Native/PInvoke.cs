using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Windows.Win32;

// Bespoke PInvoke signatures.

internal partial class PInvoke
{
	// Differs from the provided signature in that it has an `out nint`, instead of `out Windows.System.DispatcherQueueController`.
	/// <inheritdoc cref="CreateDispatcherQueueController(System.WinRT.DispatcherQueueOptions, out Windows.System.DispatcherQueueController)"/>
	[DllImport("CoreMessaging.dll", ExactSpelling = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	internal static extern HRESULT CreateDispatcherQueueController(
		System.WinRT.DispatcherQueueOptions options,
		out nint dispatcherQueueController
	);

	// For some reason, cswin32 wasn't generating this outside of Visual Studio.
	/// <inheritdoc cref="GetClassLongPtr(HWND, GET_CLASS_LONG_INDEX)"/>
	[DllImport("USER32.dll", ExactSpelling = true, EntryPoint = "GetClassLongPtrW", SetLastError = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	internal static extern nuint GetClassLongPtr(HWND hWnd, GET_CLASS_LONG_INDEX nIndex);
}
