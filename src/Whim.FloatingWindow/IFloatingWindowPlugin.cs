using System.Collections.Generic;
using Windows.Win32.Foundation;

namespace Whim.FloatingWindow;

/// <summary>
/// FloatingWindowPlugin lets windows escape the layout engine and be free-floating.
/// </summary>
public interface IFloatingWindowPlugin : IPlugin
{
	/// <summary>
	/// The floating windows.
	/// </summary>
	IReadOnlySet<HWND> FloatingWindows { get; }

	/// <summary>
	/// Mark the given <paramref name="window"/> as a floating window
	/// </summary>
	/// <param name="window"></param>
	void MarkWindowAsFloating(IWindow? window = null);

	/// <summary>
	/// Mark the given <paramref name="window"/> as a docked window
	/// </summary>
	/// <param name="window"></param>
	void MarkWindowAsDocked(IWindow? window = null);

	/// <summary>
	/// Toggle the floating state of the given <paramref name="window"/>.
	/// </summary>
	/// <param name="window"></param>
	void ToggleWindowFloating(IWindow? window = null);
}
