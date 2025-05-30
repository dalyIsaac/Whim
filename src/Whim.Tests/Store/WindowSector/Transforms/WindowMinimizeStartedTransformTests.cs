using System.Diagnostics.CodeAnalysis;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class WindowMinimizeStartedTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoWorkspaceForWindow(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
	{
		// Given
		WindowMinimizeStartedTransform sut = new(window);

		// When
		Result<Unit>? result = null;
		CustomAssert.DoesNotRaise<WindowMinimizeStartedEventArgs>(
			h => mutableRootSector.WindowSector.WindowMinimizeStarted += h,
			h => mutableRootSector.WindowSector.WindowMinimizeStarted -= h,
			() => result = ctx.Store.WhimDispatch(sut)
		);

		// Then
		Assert.False(result!.Value.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success(IContext ctx, MutableRootSector rootSector, IWindow window)
	{
		// Given the window is in a workspace
		Workspace workspace = CreateWorkspace(ctx);
		PopulateThreeWayMap(ctx, rootSector, CreateMonitor((HMONITOR)1), workspace, window);
		rootSector.MapSector.WindowWorkspaceMap = rootSector.MapSector.WindowWorkspaceMap.Add(
			window.Handle,
			workspace.Id
		);

		WindowMinimizeStartedTransform sut = new(window);

		// When
		Result<Unit>? result = null;
		Assert.RaisedEvent<WindowMinimizeStartedEventArgs> ev;

		ev = Assert.Raises<WindowMinimizeStartedEventArgs>(
			h => rootSector.WindowSector.WindowMinimizeStarted += h,
			h => rootSector.WindowSector.WindowMinimizeStarted -= h,
			() =>
			{
				CustomAssert.Layout(rootSector, () => result = ctx.Store.WhimDispatch(sut), [workspace.Id]);
			}
		);

		// Then
		Assert.True(result!.Value.IsSuccessful);
		Assert.Equal(window, ev.Arguments.Window);
		Assert.Contains(
			ctx.GetTransforms(),
			t => (t as MinimizeWindowStartTransform) == new MinimizeWindowStartTransform(workspace.Id, window.Handle)
		);
	}
}
