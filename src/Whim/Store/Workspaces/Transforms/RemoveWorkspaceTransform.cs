using System;
using DotNext;

namespace Whim;

/// <summary>
/// Remove a workspace.
/// </summary>
public abstract record BaseRemoveWorkspaceTransform() : Transform
{
	/// <summary>
	/// Determines if the provided <paramref name="workspace"/> should be removed.
	/// </summary>
	/// <param name="workspace"></param>
	/// <returns></returns>
	public abstract bool ShouldRemove(ImmutableWorkspace workspace);

	internal override Result<Empty> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		WorkspaceSector sector = mutableRootSector.Workspaces;

		if (sector.Workspaces.Count - 1 < mutableRootSector.Monitors.Monitors.Length)
		{
			return Result.FromException<Empty>(new WhimException("There must be a workspace for each monitor"));
		}

		int idx = sector.Workspaces.FindIndex(ShouldRemove);
		if (idx == -1)
		{
			return Result.FromException<Empty>(WorkspaceUtils.WorkspaceDoesNotExist());
		}

		// Remove the workspace
		ImmutableWorkspace oldWorkspace = sector.Workspaces[idx];
		sector.Workspaces = sector.Workspaces.RemoveAt(idx);

		IWorkspace oldMutableWorkspace = sector.MutableWorkspaces[idx];
		sector.MutableWorkspaces = sector.MutableWorkspaces.RemoveAt(idx);

		// Queue events
		ctx.Butler.MergeWorkspaceWindows(oldMutableWorkspace, sector.MutableWorkspaces[^1]);
		ctx.Butler.Activate(sector.MutableWorkspaces[^1]);

		sector.QueueEvent(new WorkspaceRemovedEventArgs() { Workspace = oldWorkspace });
		sector.WorkspacesToLayout.Remove(oldWorkspace.Id);

		return Empty.Result;
	}
}

/// <summary>
/// Removes the first workspace which matches the <paramref name="Id"/>.
/// </summary>
/// <param name="Id"></param>
public record RemoveWorkspaceByIdTransform(Guid Id) : BaseRemoveWorkspaceTransform()
{
	/// <inheritdoc />
	public override bool ShouldRemove(ImmutableWorkspace workspace) => workspace.Id == Id;
}

/// <summary>
/// Removes the first workspace which matches the <paramref name="Name"/>.
/// </summary>
/// <param name="Name"></param>
public record RemoveWorkspaceByNameTransform(string Name) : BaseRemoveWorkspaceTransform()
{
	/// <inheritdoc />
	public override bool ShouldRemove(ImmutableWorkspace workspace) => workspace.Name == Name;
}

/// <summary>
/// Removes the provided <paramref name="Workspace"/>.
/// </summary>
/// <param name="Workspace"></param>
public record RemoveWorkspaceTransform(ImmutableWorkspace Workspace) : BaseRemoveWorkspaceTransform()
{
	/// <inheritdoc />
	public override bool ShouldRemove(ImmutableWorkspace workspace) => workspace == Workspace;
}
