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
		window.ProcessFileName.Returns("Test.exe");
		window.Title.Returns("Test");
	}
}

public class RouterManagerTests
{
	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	public void AddWindowClassRouteString(IContext ctx, IWindow window)
	{
		// Given
		RouterManager routerManager = new(ctx);

		// When
		routerManager.AddWindowClassRoute("Test", "Test");

		// Then
		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	public void AddWindowClassRoute(IContext ctx, IWindow window, IWorkspace workspace)
	{
		// Given
		RouterManager routerManager = new(ctx);

		// When
		routerManager.AddWindowClassRoute("Test", workspace);

		// Then
		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	public void AddProcessNameRouteString(IContext ctx, IWindow window)
	{
		// Given
		RouterManager routerManager = new(ctx);

		// When
		routerManager.AddProcessNameRoute("Test", "Test");

		// Then
		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	public void AddProcessNameRoute(IContext ctx, IWindow window, IWorkspace workspace)
	{
		// Given
		RouterManager routerManager = new(ctx);

		// When
		routerManager.AddProcessNameRoute("Test", workspace);

		// Then
		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	public void AddProcessFileNameRouteString(IContext ctx, IWindow window)
	{
		// Given
		RouterManager routerManager = new(ctx);

		// When
		routerManager.AddProcessFileNameRoute("Test.exe", "Test");

		// Then
		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	public void AddProcessFileNameRouteString_ProcessFileNameIsNull(IContext ctx, IWindow window)
	{
		// Given
		RouterManager routerManager = new(ctx);
		routerManager.AddProcessFileNameRoute("Test.exe", "Test");

		// When
		window.ProcessFileName.Returns((string?)null);

		// Then
		Assert.Null(routerManager.RouteWindow(window));
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	public void AddProcessFileNameRoute(IContext ctx, IWindow window, IWorkspace workspace)
	{
		// Given
		RouterManager routerManager = new(ctx);

		// When
		routerManager.AddProcessFileNameRoute("Test.exe", workspace);

		// Then
		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	public void AddProcessFileNameRoute_ProcessFileNameIsNull(IContext ctx, IWindow window, IWorkspace workspace)
	{
		// Given
		RouterManager routerManager = new(ctx);
		routerManager.AddProcessFileNameRoute("Test.exe", workspace);

		// When
		window.ProcessFileName.Returns((string?)null);

		// Then
		Assert.Null(routerManager.RouteWindow(window));
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	public void AddTitleRouteString(IContext ctx, IWindow window)
	{
		// Given
		RouterManager routerManager = new(ctx);

		// When
		routerManager.AddTitleRoute("Test", "Test");

		// Then
		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	public void AddTitleRoute(IContext ctx, IWindow window, IWorkspace workspace)
	{
		// Given
		RouterManager routerManager = new(ctx);

		// When
		routerManager.AddTitleRoute("Test", workspace);

		// Then
		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	public void AddTitleMatchRouteString(IContext ctx, IWindow window)
	{
		// Given
		RouterManager routerManager = new(ctx);

		// When
		routerManager.AddTitleMatchRoute("Test", "Test");

		// Then
		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	public void AddTitleMatchRoute(IContext ctx, IWindow window, IWorkspace workspace)
	{
		// Given
		RouterManager routerManager = new(ctx);

		// When
		routerManager.AddTitleMatchRoute("Test", workspace);

		// Then
		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	public void Clear(IContext ctx, IWindow window)
	{
		// Given
		RouterManager routerManager = new(ctx);
		routerManager.AddWindowClassRoute("Test", "Test");

		// When
		routerManager.Clear();

		// Then
		Assert.Null(routerManager.RouteWindow(window));
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	public void CustomRouter(IContext ctx, IWindow window, IWorkspace workspace)
	{
		// Given
		RouterManager routerManager = new(ctx);

		routerManager.Add((w) => w.WindowClass == "Not Test" ? Substitute.For<IWorkspace>() : null);

		// When
		routerManager.Add((w) => w.WindowClass == "Test" ? workspace : null);

		// Then
		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}
}
