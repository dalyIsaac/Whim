using System;
using DotNext;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// Set the last focused window in the workspace with given <paramref name="WorkspaceId"/>.
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="WindowHandle"></param>
internal record SetLastFocusedWindowTransform(Guid WorkspaceId, HWND WindowHandle)
	: BaseWorkspaceWindowTransform(WorkspaceId, WindowHandle, false)
{
	private protected override Result<ImmutableWorkspace> WindowOperation(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		ImmutableWorkspace workspace,
		IWindow window
	) =>
		workspace.LastFocusedWindowHandle == window.Handle
			? workspace
			: workspace with
			{
				LastFocusedWindowHandle = window.Handle
			};
}