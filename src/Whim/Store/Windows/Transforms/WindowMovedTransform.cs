using System.Threading.Tasks;
using DotNext;

namespace Whim;

internal record WindowMovedTransform(IWindow Window) : Transform
{
	internal override Result<Empty> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		WindowSector sector = mutableRootSector.Windows;

		if (!sector.IsMovingWindow)
		{
			if (
				Window.ProcessFileName == null
				|| sector.HandledLocationRestoringWindows.Contains(Window)
				|| !ctx.WindowManager.LocationRestoringFilterManager.ShouldBeIgnored(Window)
			)
			{
				// Ignore the window moving event.
				return Empty.Result;
			}

			// The window's application tried to restore its position.
			// Wait, then restore the position.
			ctx.NativeManager.TryEnqueue(async () =>
			{
				await Task.Delay(sector.WindowMovedDelay).ConfigureAwait(true);
				if (ctx.Store.Pick(Pickers.GetWorkspaceForWindow(Window)).TryGet(out IWorkspace workspace))
				{
					sector.HandledLocationRestoringWindows.Add(Window);
					workspace.DoLayout();
				}
			});
		}

		IPoint<int>? cursorPoint = null;
		if (sector.IsLeftMouseButtonDown && internalCtx.CoreNativeManager.GetCursorPos(out IPoint<int> point))
		{
			cursorPoint = point;
		}

		sector.QueueEvent(
			new WindowMovedEventArgs()
			{
				Window = Window,
				CursorDraggedPoint = cursorPoint,
				MovedEdges = WindowUtils.GetMovedEdges(ctx, Window)?.MovedEdges
			}
		);

		return Empty.Result;
	}
}
