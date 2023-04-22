using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestFocusWindowInDirection
{
	[Fact]
	public void IllegalWindow()
	{
		TestTreeEngineMocks testTreeEngine = new();

		// Try to focus a window that doesn't exist.
		Mock<IWindow> illegalWindow = new();
		testTreeEngine.Engine.FocusWindowInDirection(Direction.Left, illegalWindow.Object);

		// Verify that the window was not focused.
		illegalWindow.Verify(w => w.Focus(), Times.Never);
	}

	[Fact]
	public void NoAdjacentWindow()
	{
		TestTreeEngineMocks testTreeEngine = new();

		// Set the currently focused window.
		testTreeEngine.ActiveWorkspace
			.Setup(w => w.LastFocusedWindow)
			.Returns(testTreeEngine.RightBottomWindow.Object);

		// Try to focus to the left of the left window.
		testTreeEngine.Engine.FocusWindowInDirection(Direction.Left, testTreeEngine.LeftWindow.Object);

		// Verify that the window was not focused.
		testTreeEngine.LeftWindow.Verify(w => w.Focus(), Times.Never);
	}

	[Fact]
	public void FocusLeftWindow()
	{
		TestTreeEngineMocks testTreeEngine = new();

		// Set the currently focused window.
		testTreeEngine.ActiveWorkspace
			.Setup(w => w.LastFocusedWindow)
			.Returns(testTreeEngine.RightBottomWindow.Object);

		// Try to focus to the left of the right window.
		testTreeEngine.Engine.FocusWindowInDirection(Direction.Left, testTreeEngine.RightBottomWindow.Object);

		// Verify that the window was focused.
		testTreeEngine.LeftWindow.Verify(w => w.Focus(), Times.Once);
	}
}
