namespace Whim;

/// <summary>
/// Queue focus for the provided <paramref name="WindowHandle"/>. If <paramref name="WindowHandle"/> is
/// <c>default</c>, focus the last focused window. If there is no last focused window, focus the monitor's desktop.
///
/// The focus operation will be performed after the transform sequence is executed.
/// </summary>
/// <param name="WindowHandle"></param>
/// <example>
/// <code>
/// context.Store.Dispatch(new FocusWindowTransform(windowHandle));
/// </code>
///
/// To focus the last focused window:
/// <code>
/// context.Store.Dispatch(new FocusWindowTransform());
/// </code>
/// </example>
public record FocusWindowTransform(HWND WindowHandle = default) : Transform<Unit>
{
	internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		HWND windowHandle = WindowHandle;

		if (windowHandle == default)
		{
			if (!ctx.Store.Pick(PickLastFocusedWindowHandle()).TryGet(out HWND lastFocusedWindowHandle))
			{
				Logger.Debug("No last focused window to focus, focusing desktop");
				HMONITOR activeMonitor = ctx.Store.Pick(PickActiveMonitor()).Handle;
				return ctx.Store.WhimDispatch(new FocusMonitorDesktopTransform(activeMonitor));
			}

			windowHandle = lastFocusedWindowHandle;
		}

		rootSector.WorkspaceSector.WindowHandleToFocus = windowHandle;
		return Unit.Result;
	}
}
