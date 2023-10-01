using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class FocusWindowInDirectionTests
{
	[Theory, AutoSubstituteData]
	public void FocusWindowInDirection_RootIsNull(IWindow focusWindow)
	{
		// Given
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(null);

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity);

		// When
		engine.FocusWindowInDirection(Direction.Left, focusWindow);

		// Then
		focusWindow.DidNotReceive().Focus();
	}

	[Theory, AutoSubstituteData]
	public void FocusWindowInDirection_CannotFindWindow(IWindow window1, IWindow focusWindow)
	{
		// Given
		LayoutEngineWrapper wrapper = new();

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity).AddWindow(
			window1
		);

		// When
		engine.FocusWindowInDirection(Direction.Left, focusWindow);

		// Then
		window1.DidNotReceive().Focus();
		focusWindow.DidNotReceive().Focus();
	}

	[Theory, AutoSubstituteData]
	public void FocusWindowInDirection_CannotFindWindowInDirection(IWindow window1, IWindow window2)
	{
		// Given
		LayoutEngineWrapper wrapper = new();

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity)
			.AddWindow(window1)
			.AddWindow(window2);

		// When
		engine.FocusWindowInDirection(Direction.Left, window1);

		// Then
		window1.DidNotReceive().Focus();
		window2.DidNotReceive().Focus();
	}

	[Theory, AutoSubstituteData]
	public void FocusWindowInDirection_FocusRight(IWindow window1, IWindow window2)
	{
		// Given
		LayoutEngineWrapper wrapper = new();

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity)
			.AddWindow(window1)
			.AddWindow(window2);

		// When
		engine.FocusWindowInDirection(Direction.Right, window1);

		// Then
		window1.DidNotReceive().Focus();
		window2.Received().Focus();
	}
}
