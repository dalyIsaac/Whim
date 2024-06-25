using System.Diagnostics.CodeAnalysis;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class RouteEventTests
{
	[Theory, AutoSubstituteData]
	public void WindowAdded(IContext ctx, IWindow window)
	{
		// When
		IWorkspace workspace = StoreTestUtils.CreateWorkspace(ctx);
		RouteEventArgs args = RouteEventArgs.WindowAdded(window, workspace);

		// Then
		Assert.Equal(window, args.Window);
		Assert.Null(args.PreviousWorkspace);
		Assert.Equal(workspace, args.CurrentWorkspace);
	}

	[Theory, AutoSubstituteData]
	public void WindowRemoved(IContext ctx, IWindow window)
	{
		// When
		IWorkspace workspace = StoreTestUtils.CreateWorkspace(ctx);
		RouteEventArgs args = RouteEventArgs.WindowRemoved(window, workspace);

		// Then
		Assert.Equal(window, args.Window);
		Assert.Equal(workspace, args.PreviousWorkspace);
		Assert.Null(args.CurrentWorkspace);
	}

	[Theory, AutoSubstituteData]
	public void WindowMoved(IContext ctx, IWindow window)
	{
		// When
		IWorkspace fromWorkspace = StoreTestUtils.CreateWorkspace(ctx);
		IWorkspace toWorkspace = StoreTestUtils.CreateWorkspace(ctx);
		RouteEventArgs args = RouteEventArgs.WindowMoved(window, fromWorkspace, toWorkspace);

		// Then
		Assert.Equal(window, args.Window);
		Assert.Equal(fromWorkspace, args.PreviousWorkspace);
		Assert.Equal(toWorkspace, args.CurrentWorkspace);
	}

	[Theory, AutoSubstituteData]
	public void Equals_Null(IContext ctx, IWindow window)
	{
		// Given
		IWorkspace workspace = StoreTestUtils.CreateWorkspace(ctx);
		RouteEventArgs a = RouteEventArgs.WindowAdded(window, workspace);

		// Then
		Assert.False(a.Equals(null));
	}

	[Theory, AutoSubstituteData]
	public void Equals_DifferentType(IContext ctx, IWindow window)
	{
		// Given
		IWorkspace workspace = StoreTestUtils.CreateWorkspace(ctx);
		RouteEventArgs a = RouteEventArgs.WindowAdded(window, workspace);

		// Then
		Assert.False(a.Equals(true));
	}

	[Theory, AutoSubstituteData]
	public void Equals_DifferentWindow(IContext ctx, IWindow aWindow, IWindow bWindow)
	{
		// Given
		IWorkspace workspace = StoreTestUtils.CreateWorkspace(ctx);
		RouteEventArgs a = RouteEventArgs.WindowAdded(aWindow, workspace);
		RouteEventArgs b = RouteEventArgs.WindowAdded(bWindow, workspace);

		// Then
		Assert.False(a.Equals(b));
	}

	[Theory, AutoSubstituteData]
	public void Equals_DifferentPreviousWorkspace(IContext ctx, IWindow Window)
	{
		// Given
		IWorkspace aWorkspace = StoreTestUtils.CreateWorkspace(ctx);
		IWorkspace bWorkspace = StoreTestUtils.CreateWorkspace(ctx);
		RouteEventArgs a = RouteEventArgs.WindowRemoved(Window, aWorkspace);
		RouteEventArgs b = RouteEventArgs.WindowRemoved(Window, bWorkspace);

		// Then
		Assert.False(a.Equals(b));
	}

	[Theory, AutoSubstituteData]
	public void Equals_DifferentNextWorkspace(IContext ctx, IWindow Window)
	{
		// Given
		IWorkspace aWorkspace = StoreTestUtils.CreateWorkspace(ctx);
		IWorkspace bWorkspace = StoreTestUtils.CreateWorkspace(ctx);
		RouteEventArgs a = RouteEventArgs.WindowAdded(Window, aWorkspace);
		RouteEventArgs b = RouteEventArgs.WindowAdded(Window, bWorkspace);

		// Then
		Assert.False(a.Equals(b));
	}

	[Theory, AutoSubstituteData]
	public void Equals_Sucess(IContext ctx, IWindow Window)
	{
		// Given
		IWorkspace fromWorkspace = StoreTestUtils.CreateWorkspace(ctx);
		IWorkspace toWorkspace = StoreTestUtils.CreateWorkspace(ctx);
		RouteEventArgs a = RouteEventArgs.WindowMoved(Window, fromWorkspace, toWorkspace);
		RouteEventArgs b = RouteEventArgs.WindowMoved(Window, fromWorkspace, toWorkspace);

		// Then
		Assert.True(a.Equals(b));
	}

	[Theory, AutoSubstituteData]
	public void GetHashCode_Success(IContext ctx, IWindow Window)
	{
		// Given
		IWorkspace currentWorkspace = StoreTestUtils.CreateWorkspace(ctx);
		IWorkspace previousWorkspace = StoreTestUtils.CreateWorkspace(ctx);
		RouteEventArgs a = RouteEventArgs.WindowMoved(Window, currentWorkspace, previousWorkspace);

		// Then
		Assert.Equal(HashCode.Combine(Window, currentWorkspace, previousWorkspace), a.GetHashCode());
	}
}
