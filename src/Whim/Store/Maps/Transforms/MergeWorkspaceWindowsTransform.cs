using DotNext;

namespace Whim;

/// <summary>
/// Merges the windows of the given <paramref name="Source"/> into the given <paramref name="Target"/>.
/// </summary>
/// <param name="Source">The workspace to remove.</param>
/// <param name="Target">The workspace to merge the windows into.</param>
public record MergeWorkspaceWindowsTransform(IWorkspace Source, IWorkspace Target) : Transform
{
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		MapSector sector = rootSector.Maps;

		// Remove the workspace from the monitor map.
		IMonitor? monitor = ctx.Store.Pick(MapPickers.GetMonitorForWorkspace(Source)).OrDefault();
		if (monitor != null)
		{
			sector.MonitorWorkspaceMap = sector.MonitorWorkspaceMap.SetItem(monitor, Target);
		}

		// Remap windows to the first workspace which isn't active.
		foreach (IWindow window in Source.Windows)
		{
			sector.WindowWorkspaceMap = sector.WindowWorkspaceMap.SetItem(window, Target);
		}

		foreach (IWindow window in Source.Windows)
		{
			Target.AddWindow(window);
		}

		return Empty.Result;
	}
}
