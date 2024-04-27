using DotNext;

namespace Whim;

/// <summary>
/// Base transform for a window operation in a given workspace.
/// </summary>
/// <param name="Workspace"></param>
/// <param name="Window"></param>
/// <param name="DefaultToLastFocusedWindow">
/// If <paramref name="Window"/> is <c>null</c>, try to use the last focused window.
/// </param>
public abstract record BaseWorkspaceWindowTransform(
	ImmutableWorkspace Workspace,
	IWindow? Window,
	bool DefaultToLastFocusedWindow
) : Transform<bool>
{
	/// <summary>
	/// The operation to execute.
	/// </summary>
	/// <param name="window">
	/// A window in the workspace.
	/// </param>
	/// <returns>
	/// The updated workspace.
	/// </returns>
	protected abstract ImmutableWorkspace Operation(IWindow window);

	internal override Result<bool> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		WorkspaceSector sector = rootSector.Workspaces;

		int idx = sector.Workspaces.IndexOf(Workspace);
		if (idx == -1)
		{
			return Result.FromException<bool>(StoreExceptions.WorkspaceNotFound());
		}

		Result<IWindow> result = WorkspaceUtils.GetValidWorkspaceWindow(Workspace, Window, DefaultToLastFocusedWindow);
		if (!result.TryGet(out IWindow validWindow))
		{
			return Result.FromException<bool>(result.Error!);
		}

		ImmutableWorkspace newWorkspace = Operation(validWindow);

		if (newWorkspace == Workspace)
		{
			return Result.FromValue(false);
		}

		sector.Workspaces = sector.Workspaces.SetItem(idx, newWorkspace);
		sector.WorkspacesToLayout.Add(Workspace.Id);

		return Result.FromValue(true);
	}
}
