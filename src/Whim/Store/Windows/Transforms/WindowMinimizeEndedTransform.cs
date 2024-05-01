using DotNext;

namespace Whim;

internal record WindowMinimizeEndedTransform(IWindow Window) : Transform
{
	internal override Result<Empty> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		UpdateMapSector(ctx, Window);
		mutableRootSector.Windows.QueueEvent(new WindowMinimizeEndedEventArgs() { Window = Window });

		return Empty.Result;
	}

	private static void UpdateMapSector(IContext ctx, IWindow window)
	{
		if (!ctx.Store.Pick(Pickers.GetWorkspaceForWindow(window)).TryGet(out IWorkspace workspace))
		{
			Logger.Error($"Window {window} was not found in any workspace");
			return;
		}

		workspace.MinimizeWindowEnd(window);
		workspace.DoLayout();
	}
}
