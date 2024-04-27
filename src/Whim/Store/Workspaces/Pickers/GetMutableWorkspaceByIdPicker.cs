using System;

namespace Whim;

internal record GetMutableWorkspaceByIdPicker(Guid Id) : Picker<IWorkspace?>
{
	internal override IWorkspace? Execute(
		IContext ctx,
		IInternalContext internalCtx,
		IRootSector rootSelector
	)
	{
		return rootSelector.Workspaces.MutableWorkspaces.Find(w => w.Id == Id);
	}
}
