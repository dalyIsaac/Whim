using System.Collections.Generic;
using Windows.Win32.Foundation;

namespace Whim.FloatingWindow;

public enum WindowFloatingState
{
	PossiblyRemoved,
	Floating,
}

internal interface IInternalFloatingWindowPlugin : IFloatingWindowPlugin
{
	Dictionary<HWND, WindowFloatingState> WindowFloatingStates { get; }

	void MarkWindowAsPossiblyRemoved(HWND hwnd);

	bool IsWindowPossiblyRemoved(HWND hwnd);
}
