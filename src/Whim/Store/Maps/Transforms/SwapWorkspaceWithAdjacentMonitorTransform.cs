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

		Result<IMonitor> currentMonitorResult = ctx.Store.Pick(Pickers.GetMonitorForWorkspace(workspace));
		if (!currentMonitorResult.TryGet(out IMonitor currentMonitor))
		{
			return Result.FromException<Empty>(currentMonitorResult.Error!);
		}

		// Get the next monitor.
		IMonitor nextMonitor = Reverse
			? ctx.MonitorManager.GetPreviousMonitor(currentMonitor)
			: ctx.MonitorManager.GetNextMonitor(currentMonitor);

		if (currentMonitor.Equals(nextMonitor))
		{
			Logger.Debug($"Monitor {currentMonitor} is already the {(!Reverse ? "next" : "previous")} monitor");
			return Empty.Result;
		}

		// Get workspace on next monitor.
		Result<IWorkspace> nextWorkspaceResult = ctx.Store.Pick(Pickers.GetWorkspaceForMonitor(nextMonitor));
		if (!nextWorkspaceResult.TryGet(out IWorkspace nextWorkspace))
		{
			return Result.FromException<Empty>(nextWorkspaceResult.Error!);
		}

		ctx.Store.Dispatch(new ActivateWorkspaceTransform(nextWorkspace, currentMonitor));

		return Empty.Result;
	}
}
