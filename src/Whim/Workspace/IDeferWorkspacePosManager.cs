using System.Collections.Generic;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// Sets the position of all the windows in a workspace at once.
/// </summary>
internal interface IDeferWorkspacePosManager
{
	/// <summary>
	/// Sets the position of all the windows in a workspace at once.
	/// </summary>
	/// <param name="workspaceSector"></param>
	/// <param name="workspace"></param>
	/// <param name="windowStates">
	/// The window states dictionary to populate with the new window states.
	/// The old window states should be removed from this dictionary.
	/// </param>
	/// <returns>
	/// Whether a layout was performed.
	/// </returns>
	bool DoLayout(
		WorkspaceSector workspaceSector,
		ImmutableWorkspace workspace,
		Dictionary<HWND, IWindowState> windowStates
	);
}
