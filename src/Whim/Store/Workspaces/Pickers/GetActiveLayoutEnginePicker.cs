namespace Whim;

/// <summary>
/// Get the active layout engine in the provided workspace.
/// </summary>
/// <param name="Workspace">The workspace to get the active layout engine for.</param>
public record GetActiveLayoutEnginePicker(ImmutableWorkspace Workspace) : Picker<ILayoutEngine>
{
	internal override ILayoutEngine Execute(IContext ctx, IInternalContext internalCtx)
	{
		int layoutEngineIdx = Workspace.ActiveLayoutEngineIndex;
		return Workspace.LayoutEngines[layoutEngineIdx];
	}
}
