using System.Collections.Immutable;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// The sector containing windows.
/// </summary>
public interface IWindowSector
{
	/// <summary>
	/// All the windows currently tracked by Whim.
	/// </summary>
	public ImmutableDictionary<HWND, IWindow> Windows { get; }
}
