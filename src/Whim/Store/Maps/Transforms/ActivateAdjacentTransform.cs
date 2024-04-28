using DotNext;

namespace Whim;

/// <summary>
/// Activates the next (or previous) workspace in the given monitor.
/// </summary>
/// <param name="Monitor">
/// The monitor to activate the next workspace in. Defaults to <see cref="IMonitorManager.ActiveMonitor"/>.
/// </param>
/// <param name="Reverse">
/// When <see langword="true"/>, gets the previous monitor, otherwise gets the next monitor. Defaults to <see langword="false" />.
/// </param>
/// <param name="SkipActive">
/// When <see langword="true"/>, skips all workspaces that are active on any other monitor. Defaults to <see langword="false"/>.
/// </param>
public record ActivateAdjacentTransform(IMonitor? Monitor = null, bool Reverse = false, bool SkipActive = false)
	: Transform
{
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		IMonitor targetMonitor = Monitor ?? ctx.MonitorManager.ActiveMonitor;

		Result<IWorkspace> currentWorkspaceResult = ctx.Store.Pick(MapPickers.GetWorkspaceForMonitor(targetMonitor));
		if (!currentWorkspaceResult.TryGet(out IWorkspace currentWorkspace))
		{
			return Result.FromException<Empty>(currentWorkspaceResult.Error!);
		}

		Result<IWorkspace> nextWorkspaceResult = ctx.Store.Pick(
			MapPickers.GetAdjacentWorkspace(currentWorkspace, Reverse, SkipActive)
		);
		if (!nextWorkspaceResult.TryGet(out IWorkspace? nextWorkspace))
		{
			return Result.FromException<Empty>(nextWorkspaceResult.Error!);
		}

		return ctx.Store.Dispatch(new ActivateWorkspaceTransform(nextWorkspace, targetMonitor));
	}
}
