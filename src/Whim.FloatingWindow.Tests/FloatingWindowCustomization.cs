using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Whim.FloatingWindow.Tests;

public class FloatingWindowCustomization : ICustomization
{
	[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
	public void Customize(IFixture fixture)
	{
		IContext ctx = fixture.Freeze<IContext>();
		IInternalContext internalCtx = fixture.Freeze<IInternalContext>();

		Store store = new(ctx, internalCtx);
		ctx.Store.Returns(store);

		IWindow window1 = StoreTestUtils.CreateWindow((HWND)1);
		IWindow window2 = StoreTestUtils.CreateWindow((HWND)2);
		IWindow window3 = StoreTestUtils.CreateWindow((HWND)3);
		Workspace workspace = StoreTestUtils.CreateWorkspace();

		IMonitor monitor = StoreTestUtils.CreateMonitor((HMONITOR)123);
		monitor.WorkingArea.Returns(new Rectangle<int>() { Width = 1000, Height = 1000 });

		StoreTestUtils.SetupMonitorAtPoint(internalCtx, store._root.MutableRootSector, monitor);

		StoreTestUtils.PopulateThreeWayMap(store._root.MutableRootSector, monitor, workspace, window1);
		StoreTestUtils.PopulateThreeWayMap(store._root.MutableRootSector, monitor, workspace, window2);
		StoreTestUtils.PopulateThreeWayMap(store._root.MutableRootSector, monitor, workspace, window3);

		ctx.NativeManager.DwmGetWindowRectangle(Arg.Any<HWND>())
			.Returns(new Rectangle<int>() { Width = 100, Height = 100 });

		fixture.Inject(monitor);
		fixture.Inject(store._root);
		fixture.Inject(store._root.MutableRootSector);
		fixture.Inject(workspace);

		// Only inject the first window. Other windows can be retrieved from the WindowSector.
		fixture.Inject(window1);
	}
}
