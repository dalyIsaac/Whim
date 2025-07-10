using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class FocusWindowInDirectionTests
{
	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void FocusWindowInDirection_RootIsNull(
		IContext ctx,
		MutableRootSector root,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		Workspace workspace,
		IWindow focusWindow
	)
	{
		// Given
		TreeCustomization.SetAsLastFocusedWindow(ctx, root, workspace, null);

		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity);

		// When
		engine.FocusWindowInDirection(Direction.Left, focusWindow);

		// Then
		focusWindow.DidNotReceive().Focus();
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void FocusWindowInDirection_CannotFindWindow(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window1,
		IWindow focusWindow
	)
	{
		// Given
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window1);

		// When
		engine.FocusWindowInDirection(Direction.Left, focusWindow);

		// Then
		window1.DidNotReceive().Focus();
		focusWindow.DidNotReceive().Focus();
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void FocusWindowInDirection_CannotFindWindowInDirection(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window1,
		IWindow window2
	)
	{
		// Given
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window1).AddWindow(window2);

		// When
		engine.FocusWindowInDirection(Direction.Left, window1);

		// Then
		window1.DidNotReceive().Focus();
		window2.DidNotReceive().Focus();
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void FocusWindowInDirection_FocusRight(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window1,
		IWindow window2
	)
	{
		// Given
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window1).AddWindow(window2);

		// When
		engine.FocusWindowInDirection(Direction.Right, window1);

		// Then
		window1.DidNotReceive().Focus();
		window2.Received().Focus();
	}
}
