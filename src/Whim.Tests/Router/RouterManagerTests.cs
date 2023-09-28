using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class RouterManagerCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IContext ctx = fixture.Freeze<IContext>();

		IWorkspace workspace = fixture.Freeze<IWorkspace>();
		workspace.Name.Returns("Test");

		ctx.WorkspaceManager.TryGet("Test").Returns(workspace);

		IWindow window = fixture.Freeze<IWindow>();
		window.WindowClass.Returns("Test");
		window.ProcessName.Returns("Test");
		window.Title.Returns("Test");
	}
}

public class RouterManagerTests
{
	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	public void AddWindowClassRouteString(IContext ctx, IWindow window)
	{
		RouterManager routerManager = new(ctx);
		routerManager.AddWindowClassRoute("Test", "Test");

		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	public void AddWindowClassRoute(IContext ctx, IWindow window, IWorkspace workspace)
	{
		RouterManager routerManager = new(ctx);
		routerManager.AddWindowClassRoute("Test", workspace);

		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	public void AddProcessNameRouteString(IContext ctx, IWindow window)
	{
		RouterManager routerManager = new(ctx);
		routerManager.AddProcessNameRoute("Test", "Test");

		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	public void AddProcessNameRoute(IContext ctx, IWindow window, IWorkspace workspace)
	{
		RouterManager routerManager = new(ctx);
		routerManager.AddProcessNameRoute("Test", workspace);

		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	public void AddTitleRouteString(IContext ctx, IWindow window)
	{
		RouterManager routerManager = new(ctx);
		routerManager.AddTitleRoute("Test", "Test");

		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	public void AddTitleRoute(IContext ctx, IWindow window, IWorkspace workspace)
	{
		RouterManager routerManager = new(ctx);
		routerManager.AddTitleRoute("Test", workspace);

		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	public void AddTitleMatchRouteString(IContext ctx, IWindow window)
	{
		RouterManager routerManager = new(ctx);
		routerManager.AddTitleMatchRoute("Test", "Test");

		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	public void AddTitleMatchRoute(IContext ctx, IWindow window, IWorkspace workspace)
	{
		RouterManager routerManager = new(ctx);
		routerManager.AddTitleMatchRoute("Test", workspace);

		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	public void Clear(IContext ctx, IWindow window)
	{
		RouterManager routerManager = new(ctx);
		routerManager.AddWindowClassRoute("Test", "Test");

		routerManager.Clear();

		Assert.Null(routerManager.RouteWindow(window));
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	public void CustomRouter(IContext ctx, IWindow window, IWorkspace workspace)
	{
		RouterManager routerManager = new(ctx);

		routerManager.Add((w) => w.WindowClass == "Not Test" ? Substitute.For<IWorkspace>() : null);
		routerManager.Add((w) => w.WindowClass == "Test" ? workspace : null);

		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}
}
