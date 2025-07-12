using System.Diagnostics.CodeAnalysis;
using AutoFixture;

namespace Whim.Tests;

public class RouterManagerCustomization : StoreCustomization
{
	public IRouterManager? RouterManager { get; private set; }

	[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
	protected override void PostCustomize(IFixture fixture)
	{
		// System under test
		RouterManager = new RouterManager(_ctx);
		fixture.Inject(RouterManager);

		// Setup workspace
		Workspace workspace = CreateWorkspace(_ctx) with
		{
			Name = "Test",
		};
		fixture.Inject(workspace);
		AddWorkspaceToManager(_ctx, _store._root.MutableRootSector, workspace);

		// Setup window
		IWindow window = fixture.Freeze<IWindow>();
		window.WindowClass.Returns("Test");
		window.ProcessFileName.Returns("Test.exe");
		window.Title.Returns("Test");
	}
}

public class RouterManagerTests
{
	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	internal void AddWindowClassRouteString(RouterManager routerManager, IWindow window)
	{
		// Given

		// When
		routerManager.AddWindowClassRoute("Test", "Test");

		// Then
		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	internal void AddWindowClassRoute(RouterManager routerManager, IWindow window, Workspace workspace)
	{
		// Given

		// When
		routerManager.AddWindowClassRoute("Test", workspace);

		// Then
		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	internal void AddProcessFileNameRouteString(RouterManager routerManager, IWindow window)
	{
		// Given

		// When
		routerManager.AddProcessFileNameRoute("Test.exe", "Test");

		// Then
		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	internal void AddProcessFileNameRouteString_ProcessFileNameIsNull(RouterManager routerManager, IWindow window)
	{
		// Given
		routerManager.AddProcessFileNameRoute("Test.exe", "Test");

		// When
		window.ProcessFileName.Returns((string?)null);

		// Then
		Assert.Null(routerManager.RouteWindow(window));
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	internal void AddProcessFileNameRoute(RouterManager routerManager, IWindow window, Workspace workspace)
	{
		// Given

		// When
		routerManager.AddProcessFileNameRoute("Test.exe", workspace);

		// Then
		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	internal void AddProcessFileNameRoute_ProcessFileNameIsNull(
		RouterManager routerManager,
		IWindow window,
		Workspace workspace
	)
	{
		// Given
		routerManager.AddProcessFileNameRoute("Test.exe", workspace);

		// When
		window.ProcessFileName.Returns((string?)null);

		// Then
		Assert.Null(routerManager.RouteWindow(window));
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	internal void AddTitleRouteString(RouterManager routerManager, IWindow window)
	{
		// Given

		// When
		routerManager.AddTitleRoute("Test", "Test");

		// Then
		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	internal void AddTitleRoute(RouterManager routerManager, IWindow window, Workspace workspace)
	{
		// Given

		// When
		routerManager.AddTitleRoute("Test", workspace);

		// Then
		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	internal void AddTitleMatchRouteString(RouterManager routerManager, IWindow window)
	{
		// Given

		// When
		routerManager.AddTitleMatchRoute("Test", "Test");

		// Then
		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	internal void AddTitleMatchRoute(RouterManager routerManager, IWindow window, Workspace workspace)
	{
		// Given

		// When
		routerManager.AddTitleMatchRoute("Test", workspace);

		// Then
		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	internal void Clear(RouterManager routerManager, IWindow window)
	{
		// Given
		routerManager.AddWindowClassRoute("Test", "Test");

		// When
		routerManager.Clear();

		// Then
		Assert.Null(routerManager.RouteWindow(window));
	}

	[Theory, AutoSubstituteData<RouterManagerCustomization>]
	internal void CustomRouter(RouterManager routerManager, IWindow window, Workspace workspace)
	{
		// Given
		routerManager.Add((w) => w.WindowClass == "Not Test" ? Substitute.For<IWorkspace>() : null);

		// When
		routerManager.Add((w) => w.WindowClass == "Test" ? workspace : null);

		// Then
		Assert.Equal("Test", routerManager.RouteWindow(window)?.Name);
	}
}
