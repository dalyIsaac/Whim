using System;
using DotNext;

namespace Whim;

/// <summary>
/// Base transform for a window operation in a given workspace, where the window exists in the
/// workspace.
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="Window"></param>
/// <param name="DefaultToLastFocusedWindow">
/// If <paramref name="Window"/> is <c>null</c>, try to use the last focused window.
/// </param>
/// <param name="SkipDoLayout">
/// If <c>true</c>, do not perform a workspace layout.
/// </param>
public abstract record BaseWorkspaceWindowTransform(
	Guid WorkspaceId,
	IWindow? Window,
	bool DefaultToLastFocusedWindow,
	bool SkipDoLayout = false
) : BaseWorkspaceTransform(WorkspaceId, SkipDoLayout)
{
	private protected override Result<ImmutableWorkspace> WorkspaceOperation(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceSector workspaceSector,
		ImmutableWorkspace workspace
	)
	{
		Result<IWindow> result = WorkspaceUtils.GetValidWorkspaceWindow(workspace, Window, DefaultToLastFocusedWindow);
		if (!result.TryGet(out IWindow validWindow))
		{
			return Result.FromException<ImmutableWorkspace>(result.Error!);
		}

		return WindowOperation(ctx, internalCtx, workspaceSector, workspace, validWindow);
	}

	/// <summary>
	/// The operation to execute.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="internalCtx"></param>
	/// <param name="workspaceSector"></param>
	/// <param name="workspace"></param>
	/// <param name="window"></param>
	/// <returns>
	/// The updated workspace.
	/// </returns>
	private protected abstract Result<ImmutableWorkspace> WindowOperation(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceSector workspaceSector,
		ImmutableWorkspace workspace,
		IWindow window
	);
}
