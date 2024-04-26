using System;
using DotNext;

namespace Whim;

/// <summary>
/// Set the name of the workspace with the provided <paramref name="Id"/> to <paramref name="Name"/>.
/// </summary>
/// <param name="Id">
/// The id of the workspace to set the name of.
/// </param>
/// <param name="Name">
/// The new name of the workspace.
/// </param>
public record SetWorkspaceNameTransform(Guid Id, string Name) : Transform
{
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx)
	{
		WorkspaceSlice slice = ctx.Store.WorkspaceSlice;

		for (int idx = 0; idx < slice.Workspaces.Count; idx++)
		{
			ImmutableWorkspace workspace = slice.Workspaces[idx];

			if (workspace.Id != Id)
			{
				continue;
			}

			ImmutableWorkspace newWorkspace = workspace with { Name = Name };
			slice.Workspaces = slice.Workspaces.SetItem(idx, newWorkspace);
			slice.QueueEvent(
				new WorkspaceRenamedEventArgs() { PreviousName = workspace.Name, Workspace = newWorkspace }
			);
			return Empty.Result;
		}

		return Result.FromException<Empty>(new WhimException($"Workspace with id {Id} not found"));
	}
}
