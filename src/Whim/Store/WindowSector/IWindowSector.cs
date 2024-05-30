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
	ImmutableDictionary<HWND, IWindow> Windows { get; }

	/// <summary>
	/// The windows which had their first location change event handled - see <see cref="IWindowManager.LocationRestoringFilterManager"/>.
	/// We maintain a set of the windows that have been handled so that we don't enter an infinite loop of location change events.
	/// </summary>
	ImmutableHashSet<HWND> HandledLocationRestoringWindows { get; }

	/// <summary>
	/// Whether a window is currently moving.
	/// </summary>
	bool IsMovingWindow { get; }

	/// <summary>
	/// Whether the user currently has the left mouse button down.
	/// Used for window movement.
	/// </summary>
	bool IsLeftMouseButtonDown { get; }

	/// <summary>
	/// The delay to wait when trying to restore windows from <see cref="IWindowManager.LocationRestoringFilterManager"/>.
	/// </summary>
	int WindowMovedDelay { get; }
}
