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
	IReadOnlyDictionary<HWND, IWindow> HandleWindowMap { get; }
}
