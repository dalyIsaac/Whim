using System.Collections.Generic;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// Sets the position of all the windows in a workspace at once.
/// </summary>
internal interface IDeferWorkspacePosManager
{
	Dictionary<HWND, IWindowState>? DoLayout(IWorkspace workspace, WorkspaceManagerTriggers triggers);
}
