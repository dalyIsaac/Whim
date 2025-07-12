namespace Whim.Tests;

public class WindowMinimizeEndedTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoWorkspaceForWindow(IContext ctx, MutableRootSector rootSector, IWindow window)
	{
		// Given
		WindowMinimizeEndedTransform sut = new(window);

		// When
		Result<Unit>? result = null;
		CustomAssert.DoesNotRaise<WindowMinimizeEndedEventArgs>(
			h => rootSector.WindowSector.WindowMinimizeEnded += h,
			h => rootSector.WindowSector.WindowMinimizeEnded -= h,
			() => result = ctx.Store.Dispatch(sut)
		);

		// Then
		Assert.False(result!.Value.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success(IContext ctx, MutableRootSector rootSector, IWindow window)
	{
		// Given the window is in a workspace
		Workspace workspace = CreateWorkspace();
		PopulateThreeWayMap(rootSector, CreateMonitor((HMONITOR)1), workspace, window);

		WindowMinimizeEndedTransform sut = new(window);

		// When
		Result<Unit>? result = null;
		Assert.RaisedEvent<WindowMinimizeEndedEventArgs> ev;

		ev = Assert.Raises<WindowMinimizeEndedEventArgs>(
			h => rootSector.WindowSector.WindowMinimizeEnded += h,
			h => rootSector.WindowSector.WindowMinimizeEnded -= h,
			() =>
			{
				CustomAssert.Layout(rootSector, () => result = ctx.Store.Dispatch(sut), [workspace.Id]);
			}
		);

		// Then
		Assert.True(result!.Value.IsSuccessful);
		Assert.Equal(window, ev.Arguments.Window);

		Assert.Contains(
			ctx.GetTransforms(),
			t => (t as MinimizeWindowEndTransform) == new MinimizeWindowEndTransform(workspace.Id, window.Handle)
		);
	}
}
