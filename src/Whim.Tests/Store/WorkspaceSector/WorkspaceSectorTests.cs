using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Dispatching;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class WorkspaceSectorTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void DoLayout_NoMonitorFoundForWorkspace(IContext ctx, MutableRootSector root)
	{
		// Given
		Workspace workspace = CreateWorkspace();

		// When
		root.WorkspaceSector.WorkspacesToLayout = root.WorkspaceSector.WorkspacesToLayout.Add(workspace.Id);
		root.WorkspaceSector.DoLayout();

		// Then
		ctx.NativeManager.DidNotReceive().TryEnqueue(Arg.Any<DispatcherQueueHandler>());
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void DoLayout_Success(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector root,
		ILayoutEngine engine
	)
	{
		// Given
		IWindow window1 = CreateWindow((HWND)1);
		IWindow window2 = CreateWindow((HWND)2);

		engine
			.DoLayout(Arg.Any<Rectangle<int>>(), Arg.Any<IMonitor>())
			.Returns(_ => new List<IWindowState>()
			{
				new WindowState()
				{
					Window = window1,
					Rectangle = new Rectangle<int>(0, 0, 100, 100),
					WindowSize = WindowSize.Normal,
				},
				new WindowState()
				{
					Window = window2,
					Rectangle = new Rectangle<int>(100, 100, 100, 100),
					WindowSize = WindowSize.Minimized,
				},
			});

		Workspace workspace = CreateWorkspace() with { LayoutEngines = [engine] };
		PopulateMonitorWorkspaceMap(root, CreateMonitor((HMONITOR)1), workspace);

		ctx.NativeManager.DeferWindowPos().Returns(new DeferWindowPosHandle(ctx, internalCtx));

		// When
		root.WorkspaceSector.WorkspacesToLayout = root.WorkspaceSector.WorkspacesToLayout.Add(workspace.Id);
		CustomAssert.Layout(root, root.WorkspaceSector.DoLayout, [workspace.Id]);

		// Then
		ctx.NativeManager.Received(2).TryEnqueue(Arg.Any<DispatcherQueueHandler>());

		// And
		Workspace resultWorkspace = root.WorkspaceSector.Workspaces[workspace.Id];
		Assert.Equal(2, resultWorkspace.WindowPositions.Count);

		WindowPosition position1 = resultWorkspace.WindowPositions[window1.Handle];
		Assert.Equal(new Rectangle<int>(0, 0, 100, 100), position1.LastWindowRectangle);
		Assert.Equal(WindowSize.Normal, position1.WindowSize);

		WindowPosition position2 = resultWorkspace.WindowPositions[window2.Handle];
		Assert.Equal(new Rectangle<int>(100, 100, 100, 100), position2.LastWindowRectangle);
		Assert.Equal(WindowSize.Minimized, position2.WindowSize);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void DoLayout_GarbageCollect(IInternalContext internalCtx, MutableRootSector root)
	{
		// Given one of the windows is not a valid window.
		IWindow validWindow = CreateWindow((HWND)1);
		IWindow invalidWindow = CreateWindow((HWND)2);

		internalCtx.CoreNativeManager.IsWindow(invalidWindow.Handle).Returns(false);

		Workspace workspace = CreateWorkspace();
		workspace = PopulateWindowWorkspaceMap(root, validWindow, workspace);
		workspace = PopulateWindowWorkspaceMap(root, invalidWindow, workspace);
		PopulateMonitorWorkspaceMap(root, CreateMonitor((HMONITOR)1), workspace);

		WorkspaceSector sut = root.WorkspaceSector;
		sut.WorkspacesToLayout = sut.WorkspacesToLayout.Add(workspace.Id);

		// When we do the layout
		CustomAssert.Layout(root, root.WorkspaceSector.DoLayout, [workspace.Id]);

		// Then the invalid window should be removed from the workspace.
		Assert.DoesNotContain(invalidWindow.Handle, sut.Workspaces[workspace.Id].WindowPositions.Keys);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void DoLayout_FocusWindow(MutableRootSector root, List<object> transforms)
	{
		// Given
		IWindow window = CreateWindow((HWND)1);

		Workspace workspace = CreateWorkspace();
		workspace = PopulateWindowWorkspaceMap(root, window, workspace);

		PopulateMonitorWorkspaceMap(root, CreateMonitor((HMONITOR)1), workspace);

		WorkspaceSector sut = root.WorkspaceSector;
		sut.WorkspacesToLayout = sut.WorkspacesToLayout.Add(workspace.Id);
		sut.WindowHandleToFocus = window.Handle;

		// When we do the layout
		CustomAssert.Layout(root, root.WorkspaceSector.DoLayout, [workspace.Id]);

		// Then the window should be focused
		window.Received().Focus();
		Assert.Contains(transforms, t => t.Equals(new WindowFocusedTransform(window)));
		Assert.Contains(transforms, t => t.Equals(new MinimizeWindowEndTransform(workspace.Id, window.Handle)));

		Assert.Equal(default, sut.WindowHandleToFocus);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void DoLayout_FocusHandle(IInternalContext internalCtx, MutableRootSector root, List<object> transforms)
	{
		// Given
		HWND handle = (HWND)1;

		WorkspaceSector sut = root.WorkspaceSector;
		sut.WindowHandleToFocus = handle;

		// When we do the layout
		root.WorkspaceSector.DoLayout();

		// Then the window should be focused
		internalCtx.CoreNativeManager.Received().SetForegroundWindow(handle);

		Assert.Equal(default, sut.WindowHandleToFocus);
		Assert.Contains(transforms, t => t.Equals(new WindowFocusedTransform(null)));
		Assert.DoesNotContain(transforms, t => t is MinimizeWindowEndTransform);
	}
}
