using Moq;
using Xunit;

namespace Whim.ImmutableTreeLayout.Tests;

public class FocusWindowInDirectionTests
{
	[Fact]
	public void FocusWindowInDirection_RootIsNull()
	{
		// Given
		Mock<IWindow> focusWindow = new();
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(null);

		IImmutableLayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object);

		// When
		engine.FocusWindowInDirection(Direction.Left, focusWindow.Object);

		// Then
		focusWindow.Verify(x => x.Focus(), Times.Never);
	}

	[Fact]
	public void FocusWindowInDirection_CannotFindWindow()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> focusWindow = new();

		LayoutEngineWrapper wrapper = new();

		IImmutableLayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object).Add(
			window1.Object
		);

		// When
		engine.FocusWindowInDirection(Direction.Left, focusWindow.Object);

		// Then
		window1.Verify(x => x.Focus(), Times.Never);
		focusWindow.Verify(x => x.Focus(), Times.Never);
	}

	[Fact]
	public void FocusWindowInDirection_CannotFindWindowInDirection()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();

		LayoutEngineWrapper wrapper = new();

		IImmutableLayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
			.Add(window1.Object)
			.Add(window2.Object);

		// When
		engine.FocusWindowInDirection(Direction.Left, window1.Object);

		// Then
		window1.Verify(x => x.Focus(), Times.Never);
		window2.Verify(x => x.Focus(), Times.Never);
	}

	[Fact]
	public void FocusWindowInDirection_FocusRight()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();

		LayoutEngineWrapper wrapper = new();

		IImmutableLayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
			.Add(window1.Object)
			.Add(window2.Object);

		// When
		engine.FocusWindowInDirection(Direction.Right, window1.Object);

		// Then
		window1.Verify(x => x.Focus(), Times.Never);
		window2.Verify(x => x.Focus(), Times.Once);
	}
}