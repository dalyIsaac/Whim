using DotNext;

namespace Whim;

/// <summary>
/// Get the last focused window in the provided workspace.
/// </summary>
/// <param name="Workspace">The workspace to get the last focused window for.</param>
public record GetLastFocusedWindowPicker(ImmutableWorkspace Workspace) : Picker<Result<IWindow?>>
{
	internal override Result<IWindow?> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		IRootSector rootSelector
	) => Result.FromValue(Workspace.LastFocusedWindow);
}
