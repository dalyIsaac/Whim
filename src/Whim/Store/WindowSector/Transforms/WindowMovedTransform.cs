using System.Threading.Tasks;
using DotNext;

namespace Whim;

internal record WindowMovedTransform(IWindow Window) : Transform
{
	internal override Result<Unit> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		WindowSector windowSector = mutableRootSector.WindowSector;

		if (!windowSector.IsMovingWindow)
		{
			if (
				Window.ProcessFileName == null
				|| windowSector.HandledLocationRestoringWindows.Contains(Window.Handle)
				|| !ctx.WindowManager.LocationRestoringFilterManager.ShouldBeIgnored(Window)
			)
			{
				// Ignore the window moving event.
				return Unit.Result;
			}

			// The window's application tried to restore its position.
			// Wait, then restore the position.
			ctx.NativeManager.TryEnqueue(async () =>
			{
				await Task.Delay(windowSector.WindowMovedDelay).ConfigureAwait(true);
				if (ctx.Butler.Pantry.GetWorkspaceForWindow(Window) is IWorkspace workspace)
				{
					windowSector.HandledLocationRestoringWindows.Add(Window.Handle);
					workspace.DoLayout();
				}
			});
		}

		IPoint<int>? cursorPoint = null;
		if (windowSector.IsLeftMouseButtonDown && internalCtx.CoreNativeManager.GetCursorPos(out IPoint<int> point))
		{
			cursorPoint = point;
		}

		windowSector.QueueEvent(
			new WindowMovedEventArgs()
			{
				Window = Window,
				CursorDraggedPoint = cursorPoint,
				MovedEdges = WindowUtils.GetMovedEdges(ctx, Window)?.MovedEdges
			}
		);

		return Unit.Result;
	}
}
