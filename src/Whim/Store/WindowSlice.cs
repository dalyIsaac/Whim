using System.Collections.Immutable;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// The slice containing windows.
/// </summary>
public class WindowSlice
{
	/// <summary>
	/// All the windows currently tracked by Whim.
	/// </summary>
	public ImmutableDictionary<HWND, IWindow> Windows { get; internal set; } = ImmutableDictionary<HWND, IWindow>.Empty;
}
