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
	/// The windows which were open when Whim started.
	/// </summary>
	ImmutableHashSet<HWND> StartupWindows { get; }

	/// <summary>
	/// Whether a window is currently moving.
	/// </summary>
	bool IsMovingWindow { get; }

	/// <summary>
	/// Whether the user currently has the left mouse button down.
	/// Used for window movement.
	/// </summary>
	bool IsLeftMouseButtonDown { get; }
}
