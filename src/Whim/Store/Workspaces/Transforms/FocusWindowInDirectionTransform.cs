using DotNext;

namespace Whim;

/// <summary>
/// Focus the <paramref name="Window"/> in the provided <paramref name="Workspace"/>
/// in the provided <paramref name="Direction"/>.
///
/// Returns whether the active layout engine changed.
/// </summary>
/// <param name="Workspace"></param>
/// <param name="Window"></param>
/// <param name="Direction"></param>
public record FocusWindowInDirectionTransform(ImmutableWorkspace Workspace, IWindow? Window, Direction Direction)
	: Transform<bool>
{
	internal override Result<bool> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		WorkspaceSector sector = mutableRootSector.Workspaces;

		int idx = sector.Workspaces.IndexOf(Workspace);
		if (idx == -1)
		{
			return Result.FromException<bool>(WorkspaceUtils.WorkspaceDoesNotExist());
		}

		Result<IWindow> result = WorkspaceUtils.GetValidWorkspaceWindow(Workspace, Window);
		if (!result.TryGet(out IWindow validWindow))
		{
			return Result.FromException<bool>(result.Error!);
		}

		ILayoutEngine layoutEngine = Workspace.LayoutEngines[Workspace.ActiveLayoutEngineIndex];
		ILayoutEngine newLayoutEngine = layoutEngine.FocusWindowInDirection(Direction, validWindow);

		if (newLayoutEngine == layoutEngine)
		{
			Logger.Debug("Window already in focus");
			return Result.FromValue(false);
		}

		ImmutableWorkspace workspace = Workspace with
		{
			LayoutEngines = Workspace.LayoutEngines.SetItem(Workspace.ActiveLayoutEngineIndex, newLayoutEngine)
		};

		sector.Workspaces = sector.Workspaces.SetItem(idx, workspace);
		sector.WorkspacesToLayout.Add(Workspace.Id);

		return Result.FromValue(true);
	}
}
