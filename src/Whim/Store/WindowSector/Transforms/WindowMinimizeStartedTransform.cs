namespace Whim;

internal record WindowMinimizeStartedTransform(IWindow Window) : Transform
{
	internal override Result<Unit> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		Result<IWorkspace> workspaceResult = ctx.Store.Pick(PickWorkspaceByWindow(Window.Handle));
		if (!workspaceResult.TryGet(out IWorkspace workspace))
		{
			return Result.FromError<Unit>(workspaceResult.Error!);
		}

		ctx.Store.Dispatch(new MinimizeWindowStartTransform(workspace.Id, Window.Handle));

		mutableRootSector.WindowSector.QueueEvent(new WindowMinimizeStartedEventArgs() { Window = Window });

		return Unit.Result;
	}
}
