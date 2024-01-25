using System.Collections.Generic;
using Windows.Win32.Foundation;

namespace Whim;

internal interface IDeferWorkspacePosManager
{
	Dictionary<HWND, IWindowState>? DoLayout(IWorkspace workspace, WorkspaceManagerTriggers triggers);
}
