using DotNext;

namespace Whim;

internal record WindowMinimizeStartedTransform(IWindow Window) : Transform
{
	internal override Result<Unit> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		Result<IWorkspace> workspaceResult = ctx.Store.Pick(Pickers.PickWorkspaceByWindow(Window.Handle));
		if (!workspaceResult.TryGet(out IWorkspace workspace))
		{
			return Result.FromException<Unit>(workspaceResult.Error!);
		}

		workspace.MinimizeWindowStart(Window);
		workspace.DoLayout();

		mutableRootSector.WindowSector.QueueEvent(new WindowMinimizeStartedEventArgs() { Window = Window });

		return Unit.Result;
	}
}
