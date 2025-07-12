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

public class ProxyFloatingLayoutEngine_DoLayoutTests
{
	[Theory, AutoSubstituteData<FloatingWindowCustomization>]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
	internal void DoLayout(
		IContext context,
		IFloatingWindowPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		MutableRootSector root
	)
	{
		// GIVEN windows have been added to the layout engine
		IWindow window1 = StoreTestUtils.CreateWindow((HWND)1);
		IWindow window2 = StoreTestUtils.CreateWindow((HWND)2);
		IWindow window3 = StoreTestUtils.CreateWindow((HWND)3);

		// (set up the inner layout engine)
		IMonitor monitor = StoreTestUtils.CreateMonitor();
		Workspace workspace = StoreTestUtils.CreateWorkspace(context);
		ProxyFloatingLayoutEngineUtils.SetupUpdate(
			context,
			root,
			monitor,
			workspace,
			window1,
			new Rectangle<int>(0, 0, 10, 10)
		);

		// (set up the layout for the third window in the inner layout engine)
		innerLayoutEngine.AddWindow(Arg.Any<IWindow>()).Returns(innerLayoutEngine);
		innerLayoutEngine.RemoveWindow(Arg.Any<IWindow>()).Returns(innerLayoutEngine);
		innerLayoutEngine
			.DoLayout(Arg.Any<IRectangle<int>>(), Arg.Any<IMonitor>())
			.Returns(_ =>
				[
					new WindowState
					{
						Window = window3,
						Rectangle = new Rectangle<int>(0, 0, 10, 10),
						WindowSize = WindowSize.Normal,
					},
				]
			);

		ProxyFloatingLayoutEngine sut = new(context, plugin, innerLayoutEngine);

		// (mark the first and second windows as floating)
		plugin.FloatingWindows.Returns(_ => (HashSet<HWND>)([window1.Handle, window2.Handle]));

		// (add the windows)
		sut = (ProxyFloatingLayoutEngine)sut.AddWindow(window1);
		sut = (ProxyFloatingLayoutEngine)sut.AddWindow(window2);
		sut = (ProxyFloatingLayoutEngine)sut.AddWindow(window3);

		// (minimize the second window)
		sut = (ProxyFloatingLayoutEngine)sut.MinimizeWindowStart(window2);

		// WHEN laying out the windows
		IWindowState[] result = [.. sut.DoLayout(new Rectangle<int>(0, 0, 100, 100), monitor)];

		// THEN the windows should be laid out
		Assert.Equal(3, result.Length);

		// (window 1 should be laid out by the inner layout engine)
		Assert.Equal(window1.Handle, result[0].Window.Handle);
		Assert.Equal(new Rectangle<int>(0, 0, 10, 10), result[0].Rectangle);

		// (window 2 should be laid out by the proxy, and minimized)
		Assert.Equal(window2.Handle, result[1].Window.Handle);
		Assert.Equal(new Rectangle<int>(0, 0, 10, 10), result[1].Rectangle);
		Assert.Equal(WindowSize.Minimized, result[1].WindowSize);

		// (window 3 should be laid out by the proxy)
		Assert.Equal(window3.Handle, result[2].Window.Handle);
		Assert.Equal(new Rectangle<int>(0, 0, 10, 10), result[2].Rectangle);
	}
}

public class ProxyFloatingLayoutEngine_MinimizeWindowStartTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MinimizeWindowStart_MinimizeFloatingWindow(
		IContext ctx,
		IFloatingWindowPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		MutableRootSector root
	)
	{
		// GIVEN a window which is floating
		IWindow window = ProxyFloatingLayoutEngineUtils.SetupUpdateInner(ctx, root);

		// (set up the inner layout engine)

		// (mark the window as floating)
		plugin.FloatingWindows.Returns(_ => new HashSet<HWND> { window.Handle });

		// (set up the sut)
		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);
		ProxyFloatingLayoutEngine proxy = (ProxyFloatingLayoutEngine)sut.AddWindow(window);

		// WHEN minimizing the window
		ProxyFloatingLayoutEngine result = (ProxyFloatingLayoutEngine)proxy.MinimizeWindowStart(window);

		// THEN the window should be minimized in the proxy.
		Assert.NotSame(sut, proxy);
		Assert.NotSame(proxy, result);

		Assert.Single(proxy.FloatingWindowRects);
		Assert.Empty(proxy.MinimizedWindowRects);

		Assert.Empty(result.FloatingWindowRects);
		Assert.Single(result.MinimizedWindowRects);

		innerLayoutEngine.DidNotReceive().MinimizeWindowStart(window);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MinimizeWindowStart_WindowAlreadyMinimized(
		IContext ctx,
		IFloatingWindowPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		MutableRootSector root
	)
	{
		// GIVEN a window which is floating
		IWindow window = ProxyFloatingLayoutEngineUtils.SetupUpdateInner(ctx, root);

		// (set up the inner layout engine)

		// (mark the window as floating)
		plugin.FloatingWindows.Returns(_ => new HashSet<HWND> { window.Handle });

		// (set up the sut)
		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);
		ProxyFloatingLayoutEngine proxy = (ProxyFloatingLayoutEngine)sut.AddWindow(window);

		// (minimize the window)
		proxy = (ProxyFloatingLayoutEngine)proxy.MinimizeWindowStart(window);

		// WHEN minimizing the window again
		ProxyFloatingLayoutEngine result = (ProxyFloatingLayoutEngine)proxy.MinimizeWindowStart(window);

		// THEN the window should remain minimized.
		Assert.NotSame(sut, proxy);
		Assert.Same(proxy, result);

		Assert.Empty(result.FloatingWindowRects);
		Assert.Single(result.MinimizedWindowRects);

		innerLayoutEngine.DidNotReceive().MinimizeWindowStart(window);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MinimizeWindowStart_MinimizeInInnerLayoutEngine(
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

		// WHEN minimizing the window
		ProxyFloatingLayoutEngine result = (ProxyFloatingLayoutEngine)proxy.MinimizeWindowStart(window);

		// THEN the window should be minimized in the inner layout engine.
		Assert.NotSame(proxy, result);

		Assert.Empty(result.FloatingWindowRects);
		Assert.Empty(result.MinimizedWindowRects);

		innerLayoutEngine.Received(1).MinimizeWindowStart(window);
	}
}

public class ProxyFloatingLayoutEngine_MinimizeWindowEndTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MinimizeWindowEnd_WindowIsNotMinimized(
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

		// WHEN ending the minimization
		ProxyFloatingLayoutEngine result = (ProxyFloatingLayoutEngine)proxy.MinimizeWindowEnd(window);

		// THEN the window should not be minimized.
		Assert.Same(proxy, result);

		Assert.Single(result.FloatingWindowRects);
		Assert.Empty(result.MinimizedWindowRects);

		innerLayoutEngine.DidNotReceive().MinimizeWindowEnd(window);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MinimizeWindowEnd_FloatingWindow(
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

		// (minimize the window)
		proxy = (ProxyFloatingLayoutEngine)proxy.MinimizeWindowStart(window);

		// WHEN ending the minimization
		ProxyFloatingLayoutEngine result = (ProxyFloatingLayoutEngine)proxy.MinimizeWindowEnd(window);

		// THEN the window should no longer be minimized.
		Assert.NotSame(sut, proxy);
		Assert.NotSame(proxy, result);

		Assert.Single(result.FloatingWindowRects);
		Assert.Empty(result.MinimizedWindowRects);

		innerLayoutEngine.DidNotReceive().MinimizeWindowEnd(window);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MinimizeWindowEnd_InnerLayoutEngine(
		IContext ctx,
		IFloatingWindowPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		ILayoutEngine resultLayoutEngine,
		MutableRootSector root
	)
	{
		// GIVEN a window which is not floating
		IWindow window = ProxyFloatingLayoutEngineUtils.SetupUpdateInner(ctx, root);

		// (set up the inner layout engine)
		innerLayoutEngine.AddWindow(Arg.Any<IWindow>()).Returns(innerLayoutEngine);
		innerLayoutEngine.RemoveWindow(Arg.Any<IWindow>()).Returns(innerLayoutEngine);
		innerLayoutEngine.MinimizeWindowStart(Arg.Any<IWindow>()).Returns(resultLayoutEngine);

		// (set up the sut)
		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);
		ProxyFloatingLayoutEngine proxy = (ProxyFloatingLayoutEngine)sut.AddWindow(window);

		// (minimize the window)
		proxy = (ProxyFloatingLayoutEngine)proxy.MinimizeWindowStart(window);

		// WHEN ending the minimization
		ProxyFloatingLayoutEngine result = (ProxyFloatingLayoutEngine)proxy.MinimizeWindowEnd(window);

		// THEN the window should no longer be minimized.
		Assert.NotSame(proxy, result);

		Assert.Empty(result.FloatingWindowRects);
		Assert.Empty(result.MinimizedWindowRects);

		resultLayoutEngine.Received(1).MinimizeWindowEnd(window);
	}
}

public class ProxyFloatingLayoutEngine_ContainsWindowTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ContainsWindow_InnerLayoutEngine(
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
		innerLayoutEngine.ContainsWindow(window).Returns(true);

		// (set up the sut)
		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);
		ProxyFloatingLayoutEngine proxy = (ProxyFloatingLayoutEngine)sut.AddWindow(window);

		// WHEN checking if the window is contained
		bool result = proxy.ContainsWindow(window);

		// THEN the window should be contained in the inner layout engine.
		Assert.True(result);
		innerLayoutEngine.Received(1).ContainsWindow(window);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ContainsWindow_MinimizedFloatingWindow(
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

		// (minimize the window)
		proxy = (ProxyFloatingLayoutEngine)proxy.MinimizeWindowStart(window);

		// WHEN checking if the window is contained
		bool result = proxy.ContainsWindow(window);

		// THEN the window should be contained in the proxy.
		Assert.True(result);

		innerLayoutEngine.DidNotReceive().ContainsWindow(window);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ContainsWindow_FloatingWindow(
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

		// WHEN checking if the window is contained
		bool result = proxy.ContainsWindow(window);

		// THEN the window should be contained in the proxy.
		Assert.True(result);

		innerLayoutEngine.DidNotReceive().ContainsWindow(window);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ContainsWindow_NotContained(
		IContext ctx,
		IFloatingWindowPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		MutableRootSector root
	)
	{
		// GIVEN a window which is not floating
		IWindow window = ProxyFloatingLayoutEngineUtils.SetupUpdateInner(ctx, root);

		// (set up the inner layout engine)
		innerLayoutEngine.ContainsWindow(window).Returns(false);

		// (set up the sut)
		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);

		// WHEN checking if the window is contained
		bool result = sut.ContainsWindow(window);

		// THEN the window should not be contained in the inner layout engine.
		Assert.False(result);
		innerLayoutEngine.Received(1).ContainsWindow(window);
	}
}

public class ProxyFloatingLayoutEngine_FocusWindowInDirectionTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void FocusWindowInDirection_InnerLayoutEngine(
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

		// WHEN focusing the window
		ILayoutEngine result = proxy.FocusWindowInDirection(Direction.Up, window);

		// THEN the window should be focused in the inner layout engine.
		Assert.NotSame(proxy, result);
		innerLayoutEngine.Received(1).FocusWindowInDirection(Direction.Up, window);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void FocusWindowInDirection_FloatingWindow(
		IContext ctx,
		IFloatingWindowPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		MutableRootSector root
	)
	{
		// GIVEN a window which is floating
		IWindow window1 = ProxyFloatingLayoutEngineUtils.SetupUpdateInner(ctx, root);
		IWindow window2 = StoreTestUtils.CreateWindow((HWND)2);

		// (set up the inner layout engine)
		innerLayoutEngine.GetFirstWindow().Returns((IWindow?)null);
		innerLayoutEngine.RemoveWindow(Arg.Any<IWindow>()).Returns(innerLayoutEngine);

		// (mark the window as floating)
		plugin.FloatingWindows.Returns(_ => new HashSet<HWND> { window1.Handle, window2.Handle });

		// (set up the sut)
		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);
		ProxyFloatingLayoutEngine proxy = (ProxyFloatingLayoutEngine)sut.AddWindow(window1);
		ProxyFloatingLayoutEngine proxy2 = (ProxyFloatingLayoutEngine)proxy.AddWindow(window2);

		// WHEN focusing the window
		ILayoutEngine result = proxy.FocusWindowInDirection(Direction.Up, window2);

		// THEN the window should be focused in the proxy.
		Assert.NotSame(sut, proxy);
		Assert.NotSame(proxy, proxy2);
		Assert.NotSame(proxy2, result);

		innerLayoutEngine.DidNotReceive().FocusWindowInDirection(Direction.Up, window1);
		window1.Received(1).Focus();
	}
}

public class ProxyFloatingLayoutEngine_SwapWindowInDirectionTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void SwapWindowInDirection_InnerLayoutEngine(
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

		// WHEN swapping the window
		ILayoutEngine result = proxy.SwapWindowInDirection(Direction.Up, window);

		// THEN the window should be swapped in the inner layout engine.
		Assert.NotSame(proxy, result);
		innerLayoutEngine.Received(1).SwapWindowInDirection(Direction.Up, window);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void SwapWindowInDirection_FloatingWindow(
		IContext ctx,
		IFloatingWindowPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		MutableRootSector root
	)
	{
		// GIVEN a window which is floating
		IWindow window1 = ProxyFloatingLayoutEngineUtils.SetupUpdateInner(ctx, root);
		IWindow window2 = StoreTestUtils.CreateWindow((HWND)2);

		// (mark the window as floating)
		plugin.FloatingWindows.Returns(_ => new HashSet<HWND> { window1.Handle, window2.Handle });

		// (set up the sut)
		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);
		ProxyFloatingLayoutEngine proxy = (ProxyFloatingLayoutEngine)sut.AddWindow(window1);
		ProxyFloatingLayoutEngine proxy2 = (ProxyFloatingLayoutEngine)proxy.AddWindow(window2);

		// WHEN swapping the window
		ILayoutEngine result = proxy.SwapWindowInDirection(Direction.Up, window2);

		// THEN the window should be swapped in the proxy.
		Assert.NotSame(sut, proxy);
		Assert.NotSame(proxy, proxy2);
		Assert.Same(proxy, result);

		innerLayoutEngine.DidNotReceive().SwapWindowInDirection(Direction.Up, window1);
	}
}

public class ProxyFloatingLayoutEngine_GetFirstWindowTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void GetFirstWindow_InnerLayoutEngine(
		IContext ctx,
		IFloatingWindowPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		MutableRootSector root
	)
	{
		// GIVEN a window which is not floating
		IWindow window = ProxyFloatingLayoutEngineUtils.SetupUpdateInner(ctx, root);

		// (set up the inner layout engine)
		innerLayoutEngine.GetFirstWindow().Returns(window);

		// (set up the sut)
		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);

		// WHEN getting the first window
		IWindow? result = sut.GetFirstWindow();

		// THEN the first window should be returned from the inner layout engine.
		Assert.Same(window, result);
		innerLayoutEngine.Received(1).GetFirstWindow();
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void GetFirstWindow_FloatingWindow(
		IContext ctx,
		IFloatingWindowPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		MutableRootSector root
	)
	{
		// GIVEN a window which is floating
		IWindow window = ProxyFloatingLayoutEngineUtils.SetupUpdateInner(ctx, root);

		// (set up the inner layout engine)
		innerLayoutEngine.RemoveWindow(Arg.Any<IWindow>()).Returns(innerLayoutEngine);
		innerLayoutEngine.GetFirstWindow().Returns((IWindow?)null);

		// (mark the window as floating)
		plugin.FloatingWindows.Returns(_ => new HashSet<HWND> { window.Handle });

		// (set up the sut)
		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);
		ProxyFloatingLayoutEngine proxy = (ProxyFloatingLayoutEngine)sut.AddWindow(window);

		// WHEN getting the first window
		IWindow? result = proxy.GetFirstWindow();

		// THEN the first window should be returned from the proxy.
		Assert.Same(window, result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void GetFirstWindow_MinimizedFloatingWindow(
		IContext ctx,
		IFloatingWindowPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		MutableRootSector root
	)
	{
		// GIVEN a window which is floating
		IWindow window = ProxyFloatingLayoutEngineUtils.SetupUpdateInner(ctx, root);

		// (set up the inner layout engine)
		innerLayoutEngine.RemoveWindow(Arg.Any<IWindow>()).Returns(innerLayoutEngine);
		innerLayoutEngine.GetFirstWindow().Returns((IWindow?)null);

		// (mark the window as floating)
		plugin.FloatingWindows.Returns(_ => new HashSet<HWND> { window.Handle });

		// (set up the sut)
		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);
		ProxyFloatingLayoutEngine proxy = (ProxyFloatingLayoutEngine)sut.AddWindow(window);

		// (minimize the window)
		proxy = (ProxyFloatingLayoutEngine)proxy.MinimizeWindowStart(window);

		// WHEN getting the first window
		IWindow? result = proxy.GetFirstWindow();

		// THEN the first window should be returned from the proxy.
		Assert.Same(window, result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void GetFirstWindow_NoWindows(IContext ctx, IFloatingWindowPlugin plugin, ILayoutEngine innerLayoutEngine)
	{
		// GIVEN no windows
		innerLayoutEngine.GetFirstWindow().Returns((IWindow?)null);

		// (set up the sut)
		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);

		// WHEN getting the first window
		IWindow? result = sut.GetFirstWindow();

		// THEN null should be returned.
		Assert.Null(result);
		innerLayoutEngine.Received(1).GetFirstWindow();
	}
}
