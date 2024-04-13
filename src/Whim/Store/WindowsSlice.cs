using System.Collections.Immutable;
using Windows.Win32.Foundation;

namespace Whim;

public class WindowsSlice
{
	public ImmutableDictionary<HWND, IWindow> Windows { get; internal set; } = ImmutableDictionary<HWND, IWindow>.Empty;
}
