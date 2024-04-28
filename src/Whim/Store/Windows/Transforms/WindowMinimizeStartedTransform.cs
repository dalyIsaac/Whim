using DotNext;

namespace Whim;

internal record WindowMinimizeStartedTransform(IWindow Window) : Transform
{
	internal override Result<Empty> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		UpdateMapSector(ctx, Window);

		mutableRootSector.Windows.QueueEvent(new WindowMinimizeStartedEventArgs() { Window = Window });

		return Empty.Result;
	}

	private static void UpdateMapSector(IContext ctx, IWindow window)
	{
		if (!ctx.Store.Pick(MapPickers.GetWorkspaceForWindow(window)).TryGet(out IWorkspace workspace))
		{
			Logger.Error($"Window {window} was not found in any workspace");
			return;
		}

		workspace.MinimizeWindowStart(window);
		workspace.DoLayout();
	}
}
