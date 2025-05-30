namespace Whim;

internal record WindowMinimizeEndedTransform(IWindow Window) : WhimTransform
{
	internal override WhimResult<Unit> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		Result<IWorkspace> workspaceResult = ctx.Store.Pick(PickWorkspaceByWindow(Window.Handle));
		if (!workspaceResult.TryGet(out IWorkspace workspace))
		{
			return Result.FromException<Unit>(workspaceResult.Error!);
		}

		ctx.Store.WhimDispatch(new MinimizeWindowEndTransform(workspace.Id, Window.Handle));
		ctx.Store.WhimDispatch(new DoWorkspaceLayoutTransform(workspace.Id));

		mutableRootSector.WindowSector.QueueEvent(new WindowMinimizeEndedEventArgs() { Window = Window });

		return Unit.Result;
	}
}
