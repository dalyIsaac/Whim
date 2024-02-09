using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.Tests;

public class ButlerChoresTests
{
	[Theory, AutoSubstituteData]
	internal void FocusMonitorDesktop(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IMonitor monitor
	)
	{
		// Given
		ButlerChores chores = new(ctx, internalCtx, triggers);

		// When
		chores.FocusMonitorDesktop(monitor);

		// Then
		internalCtx.CoreNativeManager.Received(1).GetDesktopWindow();
		internalCtx.CoreNativeManager.Received(1).SetForegroundWindow(Arg.Any<HWND>());
		internalCtx.WindowManager.Received(1).OnWindowFocused(null);
		internalCtx.MonitorManager.Received(1).ActivateEmptyMonitor(monitor);
	}
}
