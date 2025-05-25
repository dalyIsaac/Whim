namespace Whim;

internal record WindowMinimizeEndedTransform(IWindow Window) : Transform
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
			return new Result<Unit>(workspaceResult.Error!);
		}

		ctx.Store.Dispatch(new MinimizeWindowEndTransform(workspace.Id, Window.Handle));
		ctx.Store.Dispatch(new DoWorkspaceLayoutTransform(workspace.Id));

		mutableRootSector.WindowSector.QueueEvent(new WindowMinimizeEndedEventArgs() { Window = Window });

		return Unit.Result;
	}
}
