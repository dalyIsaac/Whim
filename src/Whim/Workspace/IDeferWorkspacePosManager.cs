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
	/// <param name="workspace"></param>
	/// <param name="triggers"></param>
	/// <param name="windowStates">
	/// The window states dictionary to populate with the new window states.
	/// The old window states should be removed from this dictionary.
	/// </param>
	void DoLayout(IWorkspace workspace, WorkspaceManagerTriggers triggers, Dictionary<HWND, IWindowState> windowStates);
}
