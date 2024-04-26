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

	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx)
	{
		WorkspaceSlice slice = ctx.Store.WorkspaceSlice;

		for (int idx = 0; idx < slice.Workspaces.Count; idx++)
		{
			ImmutableWorkspace workspace = slice.Workspaces[idx];

			if (!ShouldRemove(workspace))
			{
				continue;
			}

			return WorkspaceUtils.Remove(ctx, idx);
		}

		return Result.FromException<Empty>(WorkspaceUtils.RemoveWorkspaceFailed());
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
