using System;
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

	[Theory, AutoSubstituteData]
	public void Equals_Null(IWindow window, IWorkspace workspace)
	{
		// Given
		RouteEventArgs a = RouteEventArgs.WindowAdded(window, workspace);

		// Then
#pragma warning disable CA1508 // Avoid dead conditional code
		Assert.False(a.Equals(null));
#pragma warning restore CA1508 // Avoid dead conditional code
	}

	[Theory, AutoSubstituteData]
	public void Equals_DifferentType(IWindow window, IWorkspace workspace)
	{
		// Given
		RouteEventArgs a = RouteEventArgs.WindowAdded(window, workspace);

		// Then
		Assert.False(a.Equals(true));
	}

	[Theory, AutoSubstituteData]
	public void Equals_DifferentWindow(IWindow aWindow, IWorkspace workspace, IWindow bWindow)
	{
		// Given
		RouteEventArgs a = RouteEventArgs.WindowAdded(aWindow, workspace);
		RouteEventArgs b = RouteEventArgs.WindowAdded(bWindow, workspace);

		// Then
		Assert.False(a.Equals(b));
	}

	[Theory, AutoSubstituteData]
	public void Equals_DifferentPreviousWorkspace(IWindow Window, IWorkspace aWorkspace, IWorkspace bWorkspace)
	{
		// Given
		RouteEventArgs a = RouteEventArgs.WindowRemoved(Window, aWorkspace);
		RouteEventArgs b = RouteEventArgs.WindowRemoved(Window, bWorkspace);

		// Then
		Assert.False(a.Equals(b));
	}

	[Theory, AutoSubstituteData]
	public void Equals_DifferentNextWorkspace(IWindow Window, IWorkspace aWorkspace, IWorkspace bWorkspace)
	{
		// Given
		RouteEventArgs a = RouteEventArgs.WindowAdded(Window, aWorkspace);
		RouteEventArgs b = RouteEventArgs.WindowAdded(Window, bWorkspace);

		// Then
		Assert.False(a.Equals(b));
	}

	[Theory, AutoSubstituteData]
	public void Equals_Sucess(IWindow Window, IWorkspace fromWorkspace, IWorkspace toWorkspace)
	{
		// Given
		RouteEventArgs a = RouteEventArgs.WindowMoved(Window, fromWorkspace, toWorkspace);
		RouteEventArgs b = RouteEventArgs.WindowMoved(Window, fromWorkspace, toWorkspace);

		// Then
		Assert.True(a.Equals(b));
	}

	[Theory, AutoSubstituteData]
	public void GetHashCode_Success(IWindow Window, IWorkspace currentWorkspace, IWorkspace previousWorkspace)
	{
		// Given
		RouteEventArgs a = RouteEventArgs.WindowMoved(Window, currentWorkspace, previousWorkspace);

		// Then
		Assert.Equal(HashCode.Combine(Window, currentWorkspace, previousWorkspace), a.GetHashCode());
	}
}
