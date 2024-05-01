using DotNext;

namespace Whim;

/// <summary>
/// Swap the given <paramref name="Workspace"/> with the adjacent monitor.
/// </summary>
/// <param name="Workspace"></param>
/// <param name="Reverse">
/// When <see langword="true"/>, swaps workspace with the previous monitor, otherwise with the next. Defaults to <see langword="false" />.
/// </param>
public record SwapWorkspaceWithAdjacentMonitorTransform(IWorkspace? Workspace = null, bool Reverse = false) : Transform
{
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Get the current monitor.
		IWorkspace? workspace = Workspace ?? ctx.WorkspaceManager.ActiveWorkspace;
		IMonitor? currentMonitor = ctx.Butler.Pantry.GetMonitorForWorkspace(workspace);

		if (currentMonitor == null)
		{
			return Result.FromException<Empty>(
				new WhimException($"Workspace {workspace} was not found in any monitor")
			);
		}

		// Get the next monitor.
		IMonitor nextMonitor = Reverse
			? ctx.MonitorManager.GetPreviousMonitor(currentMonitor)
			: ctx.MonitorManager.GetNextMonitor(currentMonitor);

		if (currentMonitor.Equals(nextMonitor))
		{
			return Result.FromException<Empty>(
				new WhimException($"Monitor {currentMonitor} is already the {(!Reverse ? "next" : "previous")} monitor")
			);
		}

		// Get workspace on next monitor.
		IWorkspace? nextWorkspace = ctx.Butler.Pantry.GetWorkspaceForMonitor(nextMonitor);
		if (nextWorkspace == null)
		{
			return Result.FromException<Empty>(
				new WhimException($"Monitor {nextMonitor} was not found to correspond to any workspace")
			);
		}

		ctx.Store.Dispatch(new ActivateWorkspaceTransform(nextWorkspace, currentMonitor));

		return Empty.Result;
	}
}
