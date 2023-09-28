using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class RouteEventTests
{
	[Theory, AutoSubstituteData]
	public void WindowAdded(IWindow window, IWorkspace workspace)
	{
		// When
		RouteEventArgs args = RouteEventArgs.WindowAdded(window, workspace);

		// Then
		Assert.Equal(window, args.Window);
		Assert.Null(args.PreviousWorkspace);
		Assert.Equal(workspace, args.CurrentWorkspace);
	}

	[Theory, AutoSubstituteData]
	public void WindowRemoved(IWindow window, IWorkspace workspace)
	{
		// When
		RouteEventArgs args = RouteEventArgs.WindowRemoved(window, workspace);

		// Then
		Assert.Equal(window, args.Window);
		Assert.Equal(workspace, args.PreviousWorkspace);
		Assert.Null(args.CurrentWorkspace);
	}

	[Theory, AutoSubstituteData]
	public void WindowMoved(IWindow window, IWorkspace fromWorkspace, IWorkspace toWorkspace)
	{
		// When
		RouteEventArgs args = RouteEventArgs.WindowMoved(window, fromWorkspace, toWorkspace);

		// Then
		Assert.Equal(window, args.Window);
		Assert.Equal(fromWorkspace, args.PreviousWorkspace);
		Assert.Equal(toWorkspace, args.CurrentWorkspace);
	}
}
