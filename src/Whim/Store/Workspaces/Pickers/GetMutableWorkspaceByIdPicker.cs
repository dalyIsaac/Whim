namespace Whim;

internal record GetMutableWorkspaceByIdPicker(uint Id) : Picker<IWorkspace?>
{
	internal override IWorkspace? Execute(IContext ctx, IInternalContext internalCtx)
	{
		return ctx.Store.WorkspaceSlice.MutableWorkspaces.Find(w => w.Id == Id);
	}
}
