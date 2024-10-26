using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.FloatingWindow.Tests;

internal static class ProxyFloatingLayoutEngineUtils
{
	/// <summary>
	/// Sets up UpdateInner.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="root"></param>
	/// <param name="monitor"></param>
	/// <param name="workspace"></param>
	/// <param name="window"></param>
	/// <param name="rect"></param>
	public static void SetupUpdate(
		IContext ctx,
		MutableRootSector root,
		IMonitor monitor,
		Workspace workspace,
		IWindow window,
		Rectangle<int> rect
	)
	{
		monitor.WorkingArea.Returns(new Rectangle<int>(0, 0, 100, 100));
		ctx.NativeManager.DwmGetWindowRectangle(window.Handle).Returns(rect);
		StoreTestUtils.PopulateThreeWayMap(ctx, root, monitor, workspace, window);
	}

	/// <summary>
	/// Sets up UpdateInner, if you're only setting it up once.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="root"></param>
	/// <returns></returns>
	public static IWindow SetupUpdateInner(IContext ctx, MutableRootSector root)
	{
		(IMonitor monitor, Workspace workspace, IWindow window) = Create(ctx);
		SetupUpdate(ctx, root, monitor, workspace, window, new Rectangle<int>(0, 0, 10, 10));
		return window;
	}

	public static (IMonitor, Workspace, IWindow) Create(IContext ctx)
	{
		IMonitor monitor = StoreTestUtils.CreateMonitor();
		Workspace workspace = StoreTestUtils.CreateWorkspace(ctx);
		IWindow window = StoreTestUtils.CreateWindow((HWND)1);
		return (monitor, workspace, window);
	}
}

public class ProxyFloatingLayoutEngine_AddWindowTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void AddNewWindow_PassToInner(IContext ctx, IFloatingWindowPlugin plugin, ILayoutEngine innerLayoutEngine)
	{
		// GIVEN a new window
		IWindow window = StoreTestUtils.CreateWindow((HWND)1);
		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);

		// WHEN adding the window
		ILayoutEngine result = sut.AddWindow(window);

		// THEN the inner layout engine should have the window.
		ProxyFloatingLayoutEngine proxy = Assert.IsType<ProxyFloatingLayoutEngine>(result);
		Assert.NotSame(proxy, sut);
		Assert.Empty(proxy.FloatingWindowRects);
		innerLayoutEngine.Received(1).AddWindow(window);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void AddNewWindow_FailUpdate(IContext ctx, IFloatingWindowPlugin plugin, ILayoutEngine innerLayoutEngine)
	{
		// GIVEN an existing window which is marked as floating in the plugin
		IWindow window = StoreTestUtils.CreateWindow((HWND)1);

		plugin.FloatingWindows.Returns(_ => new HashSet<HWND> { window.Handle });

		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);
		ProxyFloatingLayoutEngine proxy = (ProxyFloatingLayoutEngine)sut.AddWindow(window);

		// WHEN adding the window again
		ProxyFloatingLayoutEngine result = (ProxyFloatingLayoutEngine)proxy.AddWindow(window);

		// THEN the window should remain floating.
		Assert.NotSame(proxy, result);
		Assert.NotSame(sut, result);

		Assert.Empty(proxy.FloatingWindowRects);
		Assert.Empty(result.FloatingWindowRects);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void AddExistingWindow_Dock(
		IContext ctx,
		IFloatingWindowPlugin plugin,
		MutableRootSector root,
		ILayoutEngine innerLayoutEngine
	)
	{
		// GIVEN a floating window which we add to the engine
		IWindow window = ProxyFloatingLayoutEngineUtils.SetupUpdateInner(ctx, root);

		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);
		innerLayoutEngine.AddWindow(Arg.Any<IWindow>()).Returns(innerLayoutEngine);
		innerLayoutEngine.RemoveWindow(Arg.Any<IWindow>()).Returns(innerLayoutEngine);

		// (mark the window as floating)
		HashSet<HWND> floatingWindows = [window.Handle];
		plugin.FloatingWindows.Returns(_ => floatingWindows);

		ProxyFloatingLayoutEngine proxy = (ProxyFloatingLayoutEngine)sut.AddWindow(window);

		// WHEN adding the window again, but the window is no longer marked as floating
		floatingWindows.Clear();
		ProxyFloatingLayoutEngine result = (ProxyFloatingLayoutEngine)proxy.AddWindow(window);

		// THEN the window should be docked.
		Assert.NotSame(proxy, result);
		Assert.NotSame(sut, result);

		Assert.Empty(result.FloatingWindowRects);
		innerLayoutEngine.Received(1).AddWindow(window);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void AddExistingWindow_UpdatePosition(
		IContext ctx,
		MutableRootSector root,
		IFloatingWindowPlugin plugin,
		ILayoutEngine innerLayoutEngine
	)
	{
		// GIVEN an existing window which is marked as floating in the plugin
		// (mark as the window as floating)
		(IMonitor monitor, Workspace workspace, IWindow window) = ProxyFloatingLayoutEngineUtils.Create(ctx);
		plugin.FloatingWindows.Returns(_ => new HashSet<HWND> { window.Handle });
		ProxyFloatingLayoutEngineUtils.SetupUpdate(ctx, root, monitor, workspace, window, new());

		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);
		ProxyFloatingLayoutEngine proxy = (ProxyFloatingLayoutEngine)sut.AddWindow(window);

		// (update the window's position)
		Rectangle<int> rect = new(0, 0, 10, 10);
		ProxyFloatingLayoutEngineUtils.SetupUpdate(ctx, root, monitor, workspace, window, rect);

		// WHEN adding the window again
		ProxyFloatingLayoutEngine result = (ProxyFloatingLayoutEngine)proxy.AddWindow(window);

		// THEN the window should remain floating.
		Assert.NotSame(proxy, result);
		Assert.NotSame(sut, result);

		Assert.Single(proxy.FloatingWindowRects);
		Assert.Single(result.FloatingWindowRects);

		Assert.Equal(new Rectangle<double>(0, 0, 0.1, 0.1), result.FloatingWindowRects[window]);

		innerLayoutEngine.DidNotReceive().AddWindow(window);
	}
}

public class ProxyFloatingLayoutEngine_RemoveWindowTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void RemoveWindow_NotFloating(
		IContext ctx,
		IFloatingWindowPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		ILayoutEngine resultLayoutEngine,
		MutableRootSector root
	)
	{
		// GIVEN a window which is not floating
		IWindow window = ProxyFloatingLayoutEngineUtils.SetupUpdateInner(ctx, root);

		innerLayoutEngine.AddWindow(window).Returns(resultLayoutEngine);
		resultLayoutEngine.RemoveWindow(window).Returns(innerLayoutEngine);

		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);
		ProxyFloatingLayoutEngine proxy = (ProxyFloatingLayoutEngine)sut.AddWindow(window);

		// WHEN removing the window
		ILayoutEngine result = proxy.RemoveWindow(window);

		// THEN the window should be removed from the inner layout engine.
		Assert.NotSame(proxy, result);
		innerLayoutEngine.Received(1).AddWindow(window);
		resultLayoutEngine.Received(1).RemoveWindow(window);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void RemoveWindow_Floating(
		IContext ctx,
		IFloatingWindowPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		MutableRootSector root
	)
	{
		// GIVEN a window which is floating
		IWindow window = ProxyFloatingLayoutEngineUtils.SetupUpdateInner(ctx, root);

		// (mark the window as floating)
		plugin.FloatingWindows.Returns(_ => new HashSet<HWND> { window.Handle });

		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);
		ProxyFloatingLayoutEngine proxy = (ProxyFloatingLayoutEngine)sut.AddWindow(window);

		// WHEN removing the window
		ProxyFloatingLayoutEngine result = (ProxyFloatingLayoutEngine)proxy.RemoveWindow(window);

		// THEN the window should be removed from the proxy and the inner layout engine.
		Assert.NotSame(proxy, result);

		Assert.NotEmpty(proxy.FloatingWindowRects);
		Assert.Empty(result.FloatingWindowRects);

		innerLayoutEngine.DidNotReceive().AddWindow(window);
		innerLayoutEngine.Received(1).RemoveWindow(window);
	}
}

public class ProxyFloatingLayoutEngine_MoveWindowToPointTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MoveWindowToPoint_AddToInner(
		IContext ctx,
		IFloatingWindowPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		MutableRootSector root
	)
	{
		// GIVEN a window which is not floating
		IWindow window = ProxyFloatingLayoutEngineUtils.SetupUpdateInner(ctx, root);

		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);

		// WHEN moving the window
		sut.MoveWindowToPoint(window, new Rectangle<double>(0, 0, 0.1, 0.1));

		// THEN the window should be moved in the inner layout engine.
		innerLayoutEngine.Received(1).MoveWindowToPoint(window, new Rectangle<double>(0, 0, 0.1, 0.1));
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MoveWindowToPoint_AddFloatingWindow(
		IContext ctx,
		IFloatingWindowPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		MutableRootSector root
	)
	{
		// GIVEN a window which is floating
		IWindow window = ProxyFloatingLayoutEngineUtils.SetupUpdateInner(ctx, root);

		// (mark the window as floating)
		plugin.FloatingWindows.Returns(_ => new HashSet<HWND> { window.Handle });

		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);

		// WHEN moving the window
		ProxyFloatingLayoutEngine result = (ProxyFloatingLayoutEngine)
			sut.MoveWindowToPoint(window, new Rectangle<double>(0, 0, 0.1, 0.1));

		// THEN the window should be moved in the proxy.
		Assert.NotSame(sut, result);
		Assert.Single(result.FloatingWindowRects);
		Assert.Equal(new Rectangle<double>(0, 0, 0.1, 0.1), result.FloatingWindowRects[window]);

		innerLayoutEngine.DidNotReceive().MoveWindowToPoint(window, Arg.Any<Rectangle<double>>());
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MoveWindowToPoint_DockFloatingWindow(
		IContext ctx,
		IFloatingWindowPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		MutableRootSector root
	)
	{
		// GIVEN a window which is floating
		IWindow window = ProxyFloatingLayoutEngineUtils.SetupUpdateInner(ctx, root);

		// (mark the window as floating)
		HashSet<HWND> floatingWindows = [window.Handle];
		plugin.FloatingWindows.Returns(_ => floatingWindows);

		// (set up the inner layout engine)
		innerLayoutEngine.AddWindow(Arg.Any<IWindow>()).Returns(innerLayoutEngine);
		innerLayoutEngine.RemoveWindow(Arg.Any<IWindow>()).Returns(innerLayoutEngine);

		// (set up the sut)
		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);
		ProxyFloatingLayoutEngine proxy = (ProxyFloatingLayoutEngine)
			sut.MoveWindowToPoint(window, new Point<double>(0, 0));

		// WHEN moving the window
		// (mark the window as docked)
		floatingWindows.Clear();
		ProxyFloatingLayoutEngine result = (ProxyFloatingLayoutEngine)
			proxy.MoveWindowToPoint(window, new Rectangle<double>(0, 0, 0.1, 0.1));

		// THEN the window should be docked.
		Assert.NotSame(proxy, result);
		Assert.NotSame(sut, result);

		Assert.Empty(result.FloatingWindowRects);
		innerLayoutEngine.Received(1).MoveWindowToPoint(window, new Rectangle<double>(0, 0, 0.1, 0.1));
	}
}

public class ProxyFloatingLayoutEngine_MoveWindowEdgesInDirectionTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MoveWindowEdgesInDirection_MoveInnerLayoutEngine(
		IContext ctx,
		IFloatingWindowPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		MutableRootSector root
	)
	{
		// GIVEN a window which is not floating
		IWindow window = ProxyFloatingLayoutEngineUtils.SetupUpdateInner(ctx, root);

		// (set up the inner layout engine)
		innerLayoutEngine.AddWindow(Arg.Any<IWindow>()).Returns(innerLayoutEngine);
		innerLayoutEngine.RemoveWindow(Arg.Any<IWindow>()).Returns(innerLayoutEngine);

		// (set up the sut)
		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);
		ProxyFloatingLayoutEngine proxy = (ProxyFloatingLayoutEngine)sut.AddWindow(window);

		// WHEN moving the window
		ILayoutEngine result = proxy.MoveWindowEdgesInDirection(Direction.Up, new Point<double>(0, 0), window);

		// THEN the window should be moved in the inner layout engine.
		Assert.NotSame(proxy, result);
		innerLayoutEngine.Received(1).MoveWindowEdgesInDirection(Direction.Up, new Point<double>(0, 0), window);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MoveWindowEdgesInDirection_MoveFloatingWindow(
		IContext ctx,
		IFloatingWindowPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		MutableRootSector root
	)
	{
		// GIVEN a window which is floating
		IWindow window = ProxyFloatingLayoutEngineUtils.SetupUpdateInner(ctx, root);

		// (mark the window as floating)
		plugin.FloatingWindows.Returns(_ => new HashSet<HWND> { window.Handle });

		// (set up the sut)
		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);
		ProxyFloatingLayoutEngine proxy = (ProxyFloatingLayoutEngine)sut.AddWindow(window);

		ctx.NativeManager.DwmGetWindowRectangle(window.Handle).Returns(new Rectangle<int>(0, 1, 2, 3));

		// WHEN moving the window
		ProxyFloatingLayoutEngine result = (ProxyFloatingLayoutEngine)
			proxy.MoveWindowEdgesInDirection(Direction.Up, new Point<double>(1, 1), window);

		// THEN the window should be moved in the proxy (we don't yet support window edges being moved)
		Assert.NotSame(proxy, result);
		Assert.Single(result.FloatingWindowRects);

		innerLayoutEngine
			.DidNotReceive()
			.MoveWindowEdgesInDirection(Arg.Any<Direction>(), Arg.Any<IPoint<double>>(), Arg.Any<IWindow>());
	}
}
