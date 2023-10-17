using System.Collections.Generic;
using Windows.Win32.Foundation;

namespace Whim;

internal interface IInternalWindowManager
{
	void OnWindowAdded(IWindow window);
	void OnWindowFocused(IWindow? window);
	void OnWindowRemoved(IWindow window);
	void OnWindowMinimizeStart(IWindow window);
	void OnWindowMinimizeEnd(IWindow window);

	/// <summary>
	/// Map of <see cref="HWND"/> to <see cref="IWindow"/> for easy <see cref="IWindow"/> lookup.
	/// </summary>
	IReadOnlyDictionary<HWND, IWindow> Windows { get; }

	/// <summary>
	/// The number of concurrent entries into the UI thread by <see cref="WindowManager.WindowsEventHook(Windows.Win32.UI.Accessibility.HWINEVENTHOOK, uint, Windows.Win32.Foundation.HWND, int, int, uint, uint)"/>.
	///
	/// When the count > 1, then the system is in a re-entrant state, and <see cref="IWorkspace.DoLayout"/>
	/// will be deferred until the count is less than or equal to 1.
	/// </summary>
	int EntriesCount { get; }
}
