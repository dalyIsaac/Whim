using System.Collections.Immutable;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.FloatingWindow.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class FloatingUtilsTests
{
	[Theory, AutoSubstituteData]
	public void UpdateWindowRectangle_NoRectangle(IContext ctx)
	{
		// Given a window with no rectangle
		IWindow window = StoreTestUtils.CreateWindow((HWND)1);
		ctx.NativeManager.DwmGetWindowRectangle(window.Handle).Returns((IRectangle<int>?)null);

		// When we update the rectangle
		var result = FloatingUtils.UpdateWindowRectangle(
			ctx,
			ImmutableDictionary<IWindow, IRectangle<double>>.Empty,
			window
		);

		// Then the result should be null
		Assert.Null(result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	public void UpdateWindowRectangle_NoMonitorForWindow(IContext ctx)
	{
		// Given a window with a rectangle, but no monitor
		IWindow window = StoreTestUtils.CreateWindow((HWND)1);
		ctx.NativeManager.DwmGetWindowRectangle(window.Handle).Returns(new Rectangle<int>());

		// When we update the rectangle
		var result = FloatingUtils.UpdateWindowRectangle(
			ctx,
			ImmutableDictionary<IWindow, IRectangle<double>>.Empty,
			window
		);

		// Then the result should be null
		Assert.Null(result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void UpdateWindowRectangle_NoOldRectangle(IContext ctx, MutableRootSector root)
	{
		// Given a window with a rectangle and a monitor, but no old rectangle
		IWindow window = StoreTestUtils.CreateWindow((HWND)1);
		IMonitor monitor = StoreTestUtils.CreateMonitor();
		Workspace workspace = StoreTestUtils.CreateWorkspace();

		StoreTestUtils.PopulateThreeWayMap(root, monitor, workspace, window);

		ImmutableDictionary<IWindow, IRectangle<double>> dict = new Dictionary<IWindow, IRectangle<double>>()
		{
			{ StoreTestUtils.CreateWindow((HWND)123), new Rectangle<double>() },
		}.ToImmutableDictionary();

		// When we update the rectangle
		var result = FloatingUtils.UpdateWindowRectangle(ctx, dict, window);

		// Then the result should be the new dictionary
		Assert.NotEqual(dict, result);
		Assert.Equal(2, result!.Count);
		Assert.Contains(window, result.Keys);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void UpdateWindowRectangle_NoChange(IContext ctx, MutableRootSector root)
	{
		// Given a window with a rectangle and a monitor, but no change
		IWindow window = StoreTestUtils.CreateWindow((HWND)1);
		IMonitor monitor = StoreTestUtils.CreateMonitor();
		Workspace workspace = StoreTestUtils.CreateWorkspace();

		StoreTestUtils.PopulateThreeWayMap(root, monitor, workspace, window);

		ImmutableDictionary<IWindow, IRectangle<double>> dict = new Dictionary<IWindow, IRectangle<double>>()
		{
			{ window, monitor.WorkingArea.NormalizeRectangle(new Rectangle<int>()) },
		}.ToImmutableDictionary();

		// When we update the rectangle
		var result = FloatingUtils.UpdateWindowRectangle(ctx, dict, window);

		// Then the result should be the same dictionary
		Assert.Same(dict, result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void UpdateWindowRectangle_Change(IContext ctx, MutableRootSector root)
	{
		// Given a window with a rectangle and a monitor, and a change
		IWindow window = StoreTestUtils.CreateWindow((HWND)1);
		IMonitor monitor = StoreTestUtils.CreateMonitor();
		monitor.WorkingArea.Returns(new Rectangle<int>(0, 0, 1920, 1080));

		Workspace workspace = StoreTestUtils.CreateWorkspace();

		StoreTestUtils.PopulateThreeWayMap(root, monitor, workspace, window);

		ImmutableDictionary<IWindow, IRectangle<double>> dict = new Dictionary<IWindow, IRectangle<double>>()
		{
			{ window, monitor.WorkingArea.NormalizeRectangle(new Rectangle<int>(10, 10, 1920, 1080)) },
		}.ToImmutableDictionary();

		ctx.NativeManager.DwmGetWindowRectangle(window.Handle).Returns(new Rectangle<int>(0, 0, 192, 108));

		// When we update the rectangle
		var result = FloatingUtils.UpdateWindowRectangle(ctx, dict, window);

		// Then the result should be the new dictionary
		Assert.NotEqual(dict, result);
		Assert.Single(result!);
		Assert.Contains(window, result!.Keys);
		Assert.Equal(new Rectangle<double>(0, 0, 0.1, 0.1), result[window]);
	}
}
