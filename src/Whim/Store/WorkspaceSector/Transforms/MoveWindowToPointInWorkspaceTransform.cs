using System.Collections.Immutable;
using DotNext;
using Windows.Win32.Foundation;

namespace Whim;

internal record MoveWindowToPointInWorkspaceTransform(WorkspaceId WorkspaceId, HWND WindowHandle, IPoint<double> Point)
	: BaseWorkspaceWindowTransform(WorkspaceId, WindowHandle, DefaultToLastFocusedWindow: false)
{
	private protected override Result<ImmutableWorkspace> WindowOperation(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		ImmutableWorkspace workspace,
		IWindow window
	)
	{
		if (workspace.WindowHandles.Contains(window.Handle))
		{
			// The window is already in the workspace, so move it in just the active layout engine.
			return workspace with
			{
				LayoutEngines = workspace.LayoutEngines.SetItem(
					workspace.ActiveLayoutEngineIndex,
					workspace.LayoutEngines[workspace.ActiveLayoutEngineIndex].MoveWindowToPoint(window, Point)
				)
			};
		}

		// The window is new to the workspace, so add it to all layout engines
		ImmutableList<ILayoutEngine>.Builder newLayoutEngines = ImmutableList.CreateBuilder<ILayoutEngine>();
		foreach (ILayoutEngine layoutEngine in workspace.LayoutEngines)
		{
			newLayoutEngines.Add(layoutEngine.MoveWindowToPoint(window, Point));
		}

		return workspace with
		{
			WindowHandles = workspace.WindowHandles.Add(window.Handle),
			LayoutEngines = newLayoutEngines.ToImmutable()
		};
	}
}