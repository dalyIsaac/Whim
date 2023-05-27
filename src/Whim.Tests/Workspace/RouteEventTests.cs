using Moq;
using Xunit;

namespace Whim;

public class RouteEventTests
{
	[Fact]
	public void WindowAdded()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<IWorkspace> workspace = new();

		// When
		RouteEventArgs args = RouteEventArgs.WindowAdded(window.Object, workspace.Object);

		// Then
		Assert.Equal(window.Object, args.Window);
		Assert.Null(args.PreviousWorkspace);
		Assert.Equal(workspace.Object, args.CurrentWorkspace);
	}

	[Fact]
	public void WindowRemoved()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<IWorkspace> workspace = new();

		// When
		RouteEventArgs args = RouteEventArgs.WindowRemoved(window.Object, workspace.Object);

		// Then
		Assert.Equal(window.Object, args.Window);
		Assert.Equal(workspace.Object, args.PreviousWorkspace);
		Assert.Null(args.CurrentWorkspace);
	}

	[Fact]
	public void WindowMoved()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<IWorkspace> fromWorkspace = new();
		Mock<IWorkspace> toWorkspace = new();

		// When
		RouteEventArgs args = RouteEventArgs.WindowMoved(window.Object, fromWorkspace.Object, toWorkspace.Object);

		// Then
		Assert.Equal(window.Object, args.Window);
		Assert.Equal(fromWorkspace.Object, args.PreviousWorkspace);
		Assert.Equal(toWorkspace.Object, args.CurrentWorkspace);
	}
}
