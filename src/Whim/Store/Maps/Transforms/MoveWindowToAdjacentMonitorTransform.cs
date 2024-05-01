using DotNext;

namespace Whim;

/// <summary>
/// Moves the given <paramref name="Window"/> to the next (or previous) monitor.
/// </summary>
/// <param name="Window">
/// The window to move. If <see langword="null"/>, this will default to the focused/active window.
/// </param>
/// <param name="Reverse">
/// When <see langword="true"/>, moves to the previous monitor, otherwise moves to the next monitor. Defaults to <see langword="false" />.
/// </param>
public record MoveWindowToAdjacentMonitorTransform(IWindow? Window = null, bool Reverse = false) : Transform
{
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		IMonitor activeMonitor = ctx.Store.Pick(new GetActiveMonitorPicker());
		Result<IMonitor> targetMonitorResult = ctx.Store.Pick(new GetAdjacentMonitorPicker(activeMonitor));

		if (!targetMonitorResult.TryGet(out IMonitor? targetMonitor))
		{
			return Result.FromException<Empty>(targetMonitorResult.Error!);
		}

		return ctx.Store.Dispatch(new MoveWindowToMonitorTransform(targetMonitor, Window));
	}
}
