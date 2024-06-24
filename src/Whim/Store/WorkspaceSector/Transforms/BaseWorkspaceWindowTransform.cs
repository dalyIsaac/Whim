using System;
using DotNext;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// Base transform for a window operation in a given workspace, where the window exists in the
/// workspace.
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="WindowHandle">
/// The handle of the window to operate on.
/// </param>
/// <param name="DefaultToLastFocusedWindow">
/// If <paramref name="WindowHandle"/> is <c>null</c>, try to use the last focused window.
/// </param>
/// <param name="IsWindowRequiredInWorkspace">
/// When <see langword="true"/>, the window must be in the workspace.
/// </param>
/// <param name="SkipDoLayout">
/// If <c>true</c>, do not perform a workspace layout.
/// </param>
public abstract record BaseWorkspaceWindowTransform(
	Guid WorkspaceId,
	HWND WindowHandle,
	bool DefaultToLastFocusedWindow,
	bool IsWindowRequiredInWorkspace,
	bool SkipDoLayout
) : BaseWorkspaceTransform(WorkspaceId, SkipDoLayout)
{
	private protected override Result<Workspace> WorkspaceOperation(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		Workspace workspace
	)
	{
		Result<IWindow> result = WorkspaceUtils.GetValidWorkspaceWindow(
			ctx,
			workspace,
			WindowHandle,
			DefaultToLastFocusedWindow,
			IsWindowRequiredInWorkspace
		);
		if (!result.TryGet(out IWindow validWindow))
		{
			return Result.FromException<Workspace>(result.Error!);
		}

		return WindowOperation(ctx, internalCtx, rootSector, workspace, validWindow);
	}

	/// <summary>
	/// The operation to execute.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="internalCtx"></param>
	/// <param name="rootSector"></param>
	/// <param name="workspace"></param>
	/// <param name="window"></param>
	/// <returns>
	/// The updated workspace.
	/// </returns>
	private protected abstract Result<Workspace> WindowOperation(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		Workspace workspace,
		IWindow window
	);
}
