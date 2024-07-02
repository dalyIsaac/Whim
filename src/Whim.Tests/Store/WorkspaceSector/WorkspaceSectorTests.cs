using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Dispatching;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class WorkspaceSectorTests
{
	// TODO: DispatchEvents
	// TODO: GarbageCollect

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void DoLayout_NoMonitorFoundForWorkspace(IContext ctx, MutableRootSector root)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);

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
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = window2,
					Rectangle = new Rectangle<int>(100, 100, 100, 100),
					WindowSize = WindowSize.Minimized
				}
			});

		Workspace workspace = CreateWorkspace(ctx) with { LayoutEngines = ImmutableList.Create(engine) };
		PopulateMonitorWorkspaceMap(ctx, root, CreateMonitor((HMONITOR)1), workspace);

		ctx.NativeManager.DeferWindowPos().Returns(new DeferWindowPosHandle(ctx, internalCtx));

		// When
		root.WorkspaceSector.WorkspacesToLayout = root.WorkspaceSector.WorkspacesToLayout.Add(workspace.Id);
		CustomAssert.Layout(root, root.WorkspaceSector.DoLayout, new[] { workspace.Id });

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
}
