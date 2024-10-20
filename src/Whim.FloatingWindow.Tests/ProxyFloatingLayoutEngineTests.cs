using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.FloatingWindow.Tests;

internal static class ProxyFloatingLayoutEngineUtils
{
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
}

public class ProxyFloatingLayoutEngine_AddWindowTests
{
	private static (IMonitor, Workspace, IWindow) Create(IContext ctx)
	{
		IMonitor monitor = StoreTestUtils.CreateMonitor();
		Workspace workspace = StoreTestUtils.CreateWorkspace(ctx);
		IWindow window = StoreTestUtils.CreateWindow((HWND)1);
		return (monitor, workspace, window);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void AddNewWindow_PassToInner(IContext ctx, IFloatingWindowPlugin plugin, ILayoutEngine innerLayoutEngine)
	{
		// Given a new window
		IWindow window = StoreTestUtils.CreateWindow((HWND)1);
		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);

		// When adding the window
		ILayoutEngine result = sut.AddWindow(window);

		// Then the inner layout engine should have the window
		ProxyFloatingLayoutEngine proxy = Assert.IsType<ProxyFloatingLayoutEngine>(result);
		Assert.NotSame(proxy, sut);
		Assert.Empty(proxy.FloatingWindowRects);
		innerLayoutEngine.Received(1).AddWindow(window);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void AddNewWindow_FailUpdate(IContext ctx, IFloatingWindowPlugin plugin, ILayoutEngine innerLayoutEngine)
	{
		// Given an existing window which is marked as floating in the plugin
		(IMonitor monitor, Workspace workspace, IWindow window) = Create(ctx);
		plugin.FloatingWindows.Returns(_ => new HashSet<HWND> { window.Handle });

		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);
		ProxyFloatingLayoutEngine proxy = (sut.AddWindow(window) as ProxyFloatingLayoutEngine)!;

		Rectangle<int> rect = new(0, 0, 10, 10);

		// When adding the window again
		ProxyFloatingLayoutEngine result = (proxy.AddWindow(window) as ProxyFloatingLayoutEngine)!;

		// Then the window should remain floating
		Assert.NotSame(proxy, result);
		Assert.NotSame(sut, result);

		Assert.Empty(proxy.FloatingWindowRects);
		Assert.Empty(result.FloatingWindowRects);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void AddExistingWindow_Dock(IContext ctx, IFloatingWindowPlugin plugin, ILayoutEngine innerLayoutEngine)
	{
		// Given an existing window
		IWindow window = StoreTestUtils.CreateWindow((HWND)1);
		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);
		ProxyFloatingLayoutEngine proxy = (sut.AddWindow(window) as ProxyFloatingLayoutEngine)!;

		// When adding the window again
		ILayoutEngine result = proxy.AddWindow(window);

		// Then the window should be docked
		Assert.NotSame(proxy, result);
		Assert.NotSame(sut, result);

		Assert.Empty(proxy.FloatingWindowRects);
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
		// Given an existing window which is marked as floating in the plugin
		(IMonitor monitor, Workspace workspace, IWindow window) = Create(ctx);
		plugin.FloatingWindows.Returns(_ => new HashSet<HWND> { window.Handle });
		ProxyFloatingLayoutEngineUtils.SetupUpdate(ctx, root, monitor, workspace, window, new());

		ProxyFloatingLayoutEngine sut = new(ctx, plugin, innerLayoutEngine);
		ProxyFloatingLayoutEngine proxy = (sut.AddWindow(window) as ProxyFloatingLayoutEngine)!;

		Rectangle<int> rect = new(0, 0, 10, 10);

		// When adding the window again
		ProxyFloatingLayoutEngineUtils.SetupUpdate(ctx, root, monitor, workspace, window, rect);
		ProxyFloatingLayoutEngine result = (proxy.AddWindow(window) as ProxyFloatingLayoutEngine)!;

		// Then the window should remain floating
		Assert.NotSame(proxy, result);
		Assert.NotSame(sut, result);

		Assert.Single(proxy.FloatingWindowRects);
		Assert.Single(result.FloatingWindowRects);

		Assert.Equal(new Rectangle<double>(0, 0, 0.1, 0.1), result.FloatingWindowRects[window]);

		innerLayoutEngine.DidNotReceive().AddWindow(window);
	}
}
