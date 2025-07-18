namespace Whim;

/// <summary>
/// Swap the workspace with <paramref name="WorkspaceId"/> with the adjacent monitor.
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="Reverse">
/// When <see langword="true"/>, swaps workspace with the previous monitor, otherwise with the next. Defaults to <see langword="false" />.
/// </param>
public record SwapWorkspaceWithAdjacentMonitorTransform(WorkspaceId WorkspaceId = default, bool Reverse = false)
	: Transform
{
	internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Get the current monitor.
		WorkspaceId workspaceId = WorkspaceId.OrActiveWorkspace(ctx);

		Result<IMonitor> currentMonitorResult = ctx.Store.Pick(PickMonitorByWorkspace(workspaceId));
		if (!currentMonitorResult.TryGet(out IMonitor currentMonitor))
		{
			return Result.FromError<Unit>(currentMonitorResult.Error!);
		}

		// Get the next monitor.
		Result<IMonitor> nextMonitorResult = Reverse
			? ctx.Store.Pick(PickAdjacentMonitor(currentMonitor.Handle, reverse: true))
			: ctx.Store.Pick(PickAdjacentMonitor(currentMonitor.Handle, reverse: false));

		if (!nextMonitorResult.TryGet(out IMonitor nextMonitor))
		{
			return Result.FromError<Unit>(nextMonitorResult.Error!);
		}

		if (currentMonitor.Equals(nextMonitor))
		{
			Logger.Debug($"Monitor {currentMonitor} is already the {(!Reverse ? "next" : "previous")} monitor");
			return Unit.Result;
		}

		// Get workspace on next monitor.
		Result<IWorkspace> nextWorkspaceResult = ctx.Store.Pick(PickWorkspaceByMonitor(nextMonitor.Handle));
		if (!nextWorkspaceResult.TryGet(out IWorkspace nextWorkspace))
		{
			return Result.FromError<Unit>(nextWorkspaceResult.Error!);
		}

		return ctx.Store.Dispatch(new ActivateWorkspaceTransform(nextWorkspace.Id, currentMonitor.Handle));
	}
}
