using System.Threading.Tasks;
using DotNext;

namespace Whim;

internal record WindowMovedTransform(IWindow Window) : Transform
{
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx)
	{
		WindowSlice slice = ctx.Store.WindowSlice;

		if (!slice.IsMovingWindow)
		{
			if (
				Window.ProcessFileName == null
				|| slice.HandledLocationRestoringWindows.Contains(Window)
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
				await Task.Delay(slice.WindowMovedDelay).ConfigureAwait(true);
				if (ctx.Butler.Pantry.GetWorkspaceForWindow(Window) is IWorkspace workspace)
				{
					slice.HandledLocationRestoringWindows.Add(Window);
					workspace.DoLayout();
				}
			});
		}

		IPoint<int>? cursorPoint = null;
		if (slice.IsLeftMouseButtonDown && internalCtx.CoreNativeManager.GetCursorPos(out IPoint<int> point))
		{
			cursorPoint = point;
		}

		slice.QueueEvent(
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
