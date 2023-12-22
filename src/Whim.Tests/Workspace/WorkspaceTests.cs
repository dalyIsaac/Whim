using System;
using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.Tests;

public class WorkspaceCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IInternalContext internalCtx = fixture.Freeze<IInternalContext>();

		// Assume windows are valid windows.
		internalCtx.CoreNativeManager.IsWindow(Arg.Any<HWND>()).Returns(true);

		// Assume windows are managed.
		internalCtx.WindowManager.HandleWindowMap.ContainsKey(Arg.Any<HWND>()).Returns(true);

		// Set up the triggers.
		WorkspaceManagerTriggers triggers =
			new()
			{
				ActiveLayoutEngineChanged = Substitute.For<Action<ActiveLayoutEngineChangedEventArgs>>(),
				WorkspaceLayoutStarted = Substitute.For<Action<WorkspaceEventArgs>>(),
				WorkspaceLayoutCompleted = Substitute.For<Action<WorkspaceEventArgs>>(),
				WorkspaceRenamed = Substitute.For<Action<WorkspaceRenamedEventArgs>>(),
			};
		fixture.Inject(triggers);
	}
}

[System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Reliability",
	"CA2000:Dispose objects before losing scope",
	Justification = "Unnecessary for tests"
)]
public class WorkspaceTests
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Style",
		"IDE0017:Simplify object initialization",
		Justification = "It's a test"
	)]
	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void Rename(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When
		workspace.Name = "Workspace2";

		// Then
		Assert.Equal("Workspace2", workspace.Name);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void TrySetLayoutEngine_CannotFindEngine(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When
		bool result = workspace.TrySetLayoutEngine("Layout2");

		// Then
		Assert.False(result);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void TrySetLayoutEngine_AlreadyActive(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine
	)
	{
		// Given
		ctx.WorkspaceManager.GetMonitorForWorkspace(Arg.Any<IWorkspace>()).Returns(null as IMonitor);
		layoutEngine.Name.Returns("Layout");

		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When
		bool result = workspace.TrySetLayoutEngine("Layout");

		// Then
		Assert.True(result);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void TrySetLayoutEngine_Success(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		ILayoutEngine layoutEngine2
	)
	{
		// Given
		layoutEngine2.Name.Returns("Layout2");

		Workspace workspace =
			new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine, layoutEngine2 });

		// When
		bool result = workspace.TrySetLayoutEngine("Layout2");

		// Then
		Assert.True(result);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void Constructor_FailWhenNoLayoutEngines(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers
	)
	{
		Assert.Throws<ArgumentException>(
			() => new Workspace(ctx, internalCtx, triggers, "Workspace", Array.Empty<ILayoutEngine>())
		);
	}

	#region DoLayout
	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void DoLayout_CannotFindMonitorForWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine
	)
	{
		// Given
		ctx.WorkspaceManager.GetMonitorForWorkspace(Arg.Any<IWorkspace>()).Returns(null as IMonitor);

		using Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When
		workspace.DoLayout();

		// Then
		layoutEngine.DidNotReceive().DoLayout(Arg.Any<IRectangle<int>>(), Arg.Any<IMonitor>());
		triggers.WorkspaceLayoutStarted.DidNotReceive().Invoke(Arg.Any<WorkspaceEventArgs>());
		triggers.WorkspaceLayoutCompleted.DidNotReceive().Invoke(Arg.Any<WorkspaceEventArgs>());
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void DoLayout_GarbageCollect_IsNotAWindow(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		using Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		internalCtx.CoreNativeManager.IsWindow(Arg.Any<HWND>()).Returns(false);

		// When
		workspace.AddWindow(window);

		// Then the window should have been removed, and the layout didn't start
		internalCtx.WindowManager.Received(1).OnWindowRemoved(window);
		ctx.WorkspaceManager.DidNotReceive().GetMonitorForWorkspace(Arg.Any<IWorkspace>());
		triggers.WorkspaceLayoutStarted.DidNotReceive().Invoke(Arg.Any<WorkspaceEventArgs>());
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void DoLayout_GarbageCollect_HandleIsNotManaged(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		using Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		internalCtx.CoreNativeManager.IsWindow(Arg.Any<HWND>()).Returns(true);
		internalCtx.WindowManager.HandleWindowMap.ContainsKey(Arg.Any<HWND>()).Returns(false);

		// When
		workspace.AddWindow(window);

		// Then the window should have been removed, and the layout didn't start
		internalCtx.WindowManager.Received(1).OnWindowRemoved(window);
		ctx.WorkspaceManager.DidNotReceive().GetMonitorForWorkspace(Arg.Any<IWorkspace>());
		triggers.WorkspaceLayoutStarted.DidNotReceive().Invoke(Arg.Any<WorkspaceEventArgs>());
	}
	#endregion

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void ContainsWindow_False(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When
		bool result = workspace.ContainsWindow(window);

		// Then
		Assert.False(result);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void ContainsWindow_True_NormalWindow(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		workspace.AddWindow(window);

		// When
		bool result = workspace.ContainsWindow(window);

		// Then
		Assert.True(result);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void ContainsWindow_True_MinimizedWindow(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		workspace.AddWindow(window);
		workspace.WindowMinimizeStart(window);

		// When
		bool result = workspace.ContainsWindow(window);

		// Then
		Assert.True(result);
	}

	#region WindowFocused
	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void WindowFocused_ContainsWindow(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given the window is in the workspace
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		workspace.AddWindow(window);

		// When
		workspace.WindowFocused(window);

		// Then
		Assert.Equal(window, workspace.LastFocusedWindow);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void WindowFocused_DoesNotContainWindow(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given the window is not in the workspace
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When
		workspace.WindowFocused(window);

		// Then
		Assert.Null(workspace.LastFocusedWindow);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void WindowFocused_WindowIsNull(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given the window is null
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When
		workspace.WindowFocused(window);
		workspace.WindowFocused(null);

		// Then
		Assert.Null(workspace.LastFocusedWindow);
	}
	#endregion

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void FocusFirstWindow(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When FocusFirstWindow is called
		workspace.FocusFirstWindow();

		// Then the LayoutEngine's GetFirstWindow method is called
		layoutEngine.Received(1).GetFirstWindow();
	}

	#region FocusLastFocusedWindow


	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void FocusLastFocusedWindow_LastFocusedWindow(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given the window is in the workspace
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		workspace.AddWindow(window);

		window.ClearReceivedCalls();

		// When FocusLastFocusedWindow is called
		workspace.FocusLastFocusedWindow();

		// Then the LayoutEngine's GetFirstWindow method is called
		layoutEngine.DidNotReceive().GetFirstWindow();
		window.Received(1).Focus();
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void FocusLastFocusedWindow_EmptyWorkspace_MonitorNotFound(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine
	)
	{
		// Given there are no windows in the workspace, and the workspace is not on a monitor
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		ctx.WorkspaceManager.GetMonitorForWorkspace(workspace).Returns((IMonitor?)null);

		// When FocusLastFocusedWindow is called
		workspace.FocusLastFocusedWindow();

		// Then
		internalCtx.MonitorManager.DidNotReceive().ActivateEmptyMonitor(Arg.Any<IMonitor>());
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void FocusLastFocusedWindow_EmptyWorkspace_MonitorFound(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IMonitor monitor
	)
	{
		// Given there are no windows in the workspace, and the workspace is on a monitor
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		ctx.WorkspaceManager.GetMonitorForWorkspace(workspace).Returns(monitor);

		// When FocusLastFocusedWindow is called
		workspace.FocusLastFocusedWindow();

		// Then
		internalCtx.MonitorManager.Received(1).ActivateEmptyMonitor(monitor);
	}
	#endregion

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void NextLayoutEngine(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		ILayoutEngine layoutEngine2
	)
	{
		// Given
		Workspace workspace =
			new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine, layoutEngine2 });

		// When NextLayoutEngine is called
		workspace.NextLayoutEngine();

		// Then the active layout engine is set to the next one
		Assert.True(Object.ReferenceEquals(layoutEngine2, workspace.ActiveLayoutEngine));
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void NextLayoutEngine_LastEngine(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		ILayoutEngine layoutEngine2
	)
	{
		// Given
		Workspace workspace =
			new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine, layoutEngine2 });

		// When NextLayoutEngine is called
		workspace.NextLayoutEngine();
		workspace.NextLayoutEngine();

		// Then the active layout engine is set to the first one
		Assert.True(Object.ReferenceEquals(layoutEngine, workspace.ActiveLayoutEngine));
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void PreviousLayoutEngine(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		ILayoutEngine layoutEngine2
	)
	{
		// Given
		Workspace workspace =
			new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine, layoutEngine2 });

		// When PreviousLayoutEngine is called
		workspace.PreviousLayoutEngine();

		// Then the active layout engine is set to the previous one
		Assert.True(Object.ReferenceEquals(layoutEngine2, workspace.ActiveLayoutEngine));
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void PreviousLayoutEngine_FirstEngine(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		ILayoutEngine layoutEngine2
	)
	{
		// Given
		Workspace workspace =
			new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine, layoutEngine2 });

		// When PreviousLayoutEngine is called
		workspace.PreviousLayoutEngine();
		workspace.PreviousLayoutEngine();

		// Then the active layout engine is set to the last one
		Assert.True(Object.ReferenceEquals(layoutEngine, workspace.ActiveLayoutEngine));
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void AddWindow_Fails_AlreadyIncludesWindow(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When AddWindow is called
		workspace.AddWindow(window);
		workspace.AddWindow(window);

		// Then the window is added to the layout engine
		layoutEngine.Received(1).AddWindow(window);
		ctx.WorkspaceManager.Received(1).GetMonitorForWorkspace(workspace);
		window.Received(2).Focus();
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void AddWindow_Success(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When AddWindow is called
		workspace.AddWindow(window);

		// Then the window is added to the layout engine
		layoutEngine.Received(1).AddWindow(window);
		ctx.WorkspaceManager.Received(1).GetMonitorForWorkspace(workspace);
		window.Received(1).Focus();
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void AddWindow_Success_WindowWasMinimized(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given the window has already been added but was since minimized
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		workspace.AddWindow(window);
		workspace.WindowMinimizeStart(window);

		// When AddWindow is called
		workspace.AddWindow(window);

		// Then the window is added to the layout engine
		layoutEngine.Received(1).AddWindow(window);
		ctx.WorkspaceManager.Received(3).GetMonitorForWorkspace(workspace);
		window.Received(2).Focus();
	}

	#region RemoveWindow
	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void RemoveWindow_Fails_AlreadyRemoved(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When RemoveWindow is called
		workspace.RemoveWindow(window);
		bool result = workspace.RemoveWindow(window);

		// Then the window is removed from the layout engine
		Assert.False(result);
		layoutEngine.DidNotReceive().RemoveWindow(window);
		internalCtx.CoreNativeManager.DidNotReceive().IsWindow(Arg.Any<HWND>());
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void RemoveWindow_Fails_DidNotRemoveFromLayoutEngine(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		workspace.AddWindow(window);

		internalCtx.CoreNativeManager.ClearReceivedCalls();

		ILayoutEngine givenEngine = workspace.ActiveLayoutEngine;
		givenEngine.RemoveWindow(window).Returns(givenEngine);

		// When RemoveWindow is called
		bool result = workspace.RemoveWindow(window);

		// Then the window is removed from the layout engine
		Assert.False(result);
		givenEngine.Received(1).RemoveWindow(window);
		internalCtx.CoreNativeManager.DidNotReceive().IsWindow(Arg.Any<HWND>());
		Assert.Null(workspace.LastFocusedWindow);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void RemoveWindow_Success_OneWindow(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given only one window is in the workspace
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		workspace.AddWindow(window);
		workspace.WindowFocused(window);

		ILayoutEngine givenEngine = workspace.ActiveLayoutEngine;
		ctx.WorkspaceManager.ClearReceivedCalls();
		internalCtx.CoreNativeManager.ClearReceivedCalls();

		// When RemoveWindow is called
		bool result = workspace.RemoveWindow(window);

		// Then the window is removed from the layout engine, and LastFocusedWindow is set to null
		Assert.True(result);
		givenEngine.Received(1).RemoveWindow(window);
		ctx.WorkspaceManager.Received(1).GetMonitorForWorkspace(workspace);
		Assert.Null(workspace.LastFocusedWindow);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void RemoveWindow_Success_MultipleWindows(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window,
		IWindow window2
	)
	{
		// Given multiple windows are in the workspace
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		workspace.AddWindow(window);
		workspace.AddWindow(window2);
		workspace.WindowFocused(window);

		ILayoutEngine givenEngine = workspace.ActiveLayoutEngine;
		ctx.WorkspaceManager.ClearReceivedCalls();
		internalCtx.CoreNativeManager.ClearReceivedCalls();

		// When RemoveWindow is called
		bool result = workspace.RemoveWindow(window);

		// Then the window is removed from the layout engine, and LastFocusedWindow is set to the next window
		Assert.True(result);
		givenEngine.Received(1).RemoveWindow(window);
		ctx.WorkspaceManager.Received(1).GetMonitorForWorkspace(workspace);
		Assert.Equal(window2, workspace.LastFocusedWindow);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void RemoveWindow_MinimizedWindow(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		window.IsMinimized.Returns(true);
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		workspace.AddWindow(window);
		workspace.WindowMinimizeStart(window);

		ctx.WorkspaceManager.ClearReceivedCalls();
		internalCtx.CoreNativeManager.ClearReceivedCalls();

		// When RemoveWindow is called
		bool result = workspace.RemoveWindow(window);

		// Then the window is not removed from the layout engine
		Assert.True(result);
		layoutEngine.DidNotReceive().RemoveWindow(window);
		ctx.WorkspaceManager.Received(1).GetMonitorForWorkspace(workspace);
	}
	#endregion

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void FocusWindowInDirection_Fails_DoesNotContainWindow(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When FocusWindowInDirection is called
		workspace.FocusWindowInDirection(Direction.Up, window);

		// Then the layout engine is not told to focus the window
		layoutEngine.DidNotReceive().FocusWindowInDirection(Direction.Up, window);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void FocusWindowInDirection_Fails_WindowIsMinimized(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		workspace.AddWindow(window);
		workspace.WindowMinimizeStart(window);

		// When FocusWindowInDirection is called
		workspace.FocusWindowInDirection(Direction.Up, window);

		// Then the layout engine is not told to focus the window
		layoutEngine.DidNotReceive().FocusWindowInDirection(Direction.Up, window);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void FocusWindowInDirection_Success(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		workspace.AddWindow(window);

		// When FocusWindowInDirection is called
		workspace.FocusWindowInDirection(Direction.Up, window);

		// Then the layout engine is told to focus the window
		workspace.ActiveLayoutEngine.Received(1).FocusWindowInDirection(Direction.Up, window);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void SwapWindowInDirection_Fails_WindowIsNull(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When SwapWindowInDirection is called
		workspace.SwapWindowInDirection(Direction.Up, null);

		// Then the layout engine is not told to swap the window
		layoutEngine.DidNotReceive().SwapWindowInDirection(Direction.Up, Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void SwapWindowInDirection_Fails_DoesNotContainWindow(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When SwapWindowInDirection is called
		workspace.SwapWindowInDirection(Direction.Up, window);

		// Then the layout engine is not told to swap the window
		layoutEngine.DidNotReceive().SwapWindowInDirection(Direction.Up, window);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void SwapWindowInDirection_Success(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		workspace.AddWindow(window);

		ILayoutEngine givenEngine = workspace.ActiveLayoutEngine;

		// When SwapWindowInDirection is called
		workspace.SwapWindowInDirection(Direction.Up, window);

		// Then the layout engine is told to swap the window
		givenEngine.Received(1).SwapWindowInDirection(Direction.Up, window);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void MoveWindowEdgesInDirection_Fails_WindowIsNull(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		IPoint<double> deltas = new Point<double>() { X = 0.3, Y = 0 };

		// When MoveWindowEdgesInDirection is called
		workspace.MoveWindowEdgesInDirection(Direction.Up, deltas, null);

		// Then the layout engine is not told to move the window
		layoutEngine.DidNotReceive().MoveWindowEdgesInDirection(Direction.Up, deltas, Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void MoveWindowEdgesInDirection_Fails_DoesNotContainWindow(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		IPoint<double> deltas = new Point<double>() { X = 0.3, Y = 0 };

		// When MoveWindowEdgesInDirection is called
		workspace.MoveWindowEdgesInDirection(Direction.Up, deltas, window);

		// Then the layout engine is not told to move the window
		layoutEngine.DidNotReceive().MoveWindowEdgesInDirection(Direction.Up, deltas, window);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void MoveWindowEdgesInDirection_Success(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		workspace.AddWindow(window);
		IPoint<double> deltas = new Point<double>() { X = 0.3, Y = 0 };
		ILayoutEngine givenEngine = workspace.ActiveLayoutEngine;

		// When MoveWindowEdgesInDirection is called
		workspace.MoveWindowEdgesInDirection(Direction.Up, deltas, window);

		// Then the layout engine is told to move the window
		givenEngine.Received(1).MoveWindowEdgesInDirection(Direction.Up, deltas, window);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void MoveWindowToPoint_Success_AddWindow(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		IPoint<double> point = new Point<double>() { X = 0.3, Y = 0.3 };
		window.ClearReceivedCalls();

		// When MoveWindowToPoint is called
		workspace.MoveWindowToPoint(window, point);

		// Then the layout engine is told to move the window
		layoutEngine.Received(1).MoveWindowToPoint(window, point);
		layoutEngine.DidNotReceive().RemoveWindow(window);
		window.Received(1).Focus();
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void MoveWindowToPoint_Success_WindowIsMinimized(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		IPoint<double> point = new Point<double>() { X = 0.3, Y = 0.3 };

		workspace.AddWindow(window);
		workspace.WindowMinimizeStart(window);

		ILayoutEngine givenEngine = workspace.ActiveLayoutEngine;
		window.ClearReceivedCalls();

		// When MoveWindowToPoint is called
		workspace.MoveWindowToPoint(window, point);

		// Then the layout engine is told to move the window
		givenEngine.Received(1).MoveWindowToPoint(window, point);
		givenEngine.DidNotReceive().RemoveWindow(window);
		window.Received(1).Focus();
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void MoveWindowToPoint_Success_WindowAlreadyExists(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		workspace.AddWindow(window);
		IPoint<double> point = new Point<double>() { X = 0.3, Y = 0.3 };

		ILayoutEngine givenEngine = workspace.ActiveLayoutEngine;
		window.ClearReceivedCalls();

		// When MoveWindowToPoint is called
		workspace.MoveWindowToPoint(window, point);

		// Then the layout engine is told to remove and add the window
		givenEngine.Received(1).MoveWindowToPoint(window, point);
		window.Received(1).Focus();
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void ToString_Success(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When ToString is called
		string result = workspace.ToString();

		// Then the result is as expected
		Assert.Equal("Workspace", result);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void Deactivate(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window,
		IWindow window2
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		workspace.AddWindow(window);
		workspace.AddWindow(window2);
		internalCtx.CoreNativeManager.ClearReceivedCalls();

		// When Deactivate is called
		workspace.Deactivate();

		// Then the windows are hidden and DoLayout is called
		internalCtx.CoreNativeManager.DidNotReceive().IsWindow(Arg.Any<HWND>());
		window.Received(1).Hide();
		window2.Received(1).Hide();
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void TryGetWindowState(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		ILayoutEngine resultingEngine,
		IWindow window
	)
	{
		// Given
		IWindowState expectedWindowState = new WindowState()
		{
			Rectangle = new Rectangle<int>(),
			Window = window,
			WindowSize = WindowSize.Normal
		};
		layoutEngine.AddWindow(window).Returns(resultingEngine);
		resultingEngine
			.DoLayout(Arg.Any<IRectangle<int>>(), Arg.Any<IMonitor>())
			.Returns(new IWindowState[] { expectedWindowState });

		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		workspace.AddWindow(window);

		// When TryGetWindowState is called
		IWindowState? result = workspace.TryGetWindowState(window);

		// Then the result is as expected
		Assert.NotNull(result);
		Assert.Equal(expectedWindowState, result);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void TryGetWindowState_MinimizedWindow(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When
		workspace.AddWindow(window);
		workspace.WindowMinimizeStart(window);
		IWindowState windowState = workspace.TryGetWindowState(window)!;

		// Then
		Assert.Equal(window, windowState.Window);
		Assert.Equal(0, windowState.Rectangle.X);
		Assert.Equal(0, windowState.Rectangle.Y);
		Assert.Equal(0, windowState.Rectangle.Width);
		Assert.Equal(0, windowState.Rectangle.Height);
		Assert.Equal(WindowSize.Minimized, windowState.WindowSize);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void Dispose(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		IWindow window,
		ILayoutEngine layoutEngine
	)
	{
		// Given
		ctx.WorkspaceManager.GetMonitorForWorkspace(Arg.Any<IWorkspace>()).Returns((IMonitor?)null);

		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		workspace.AddWindow(window);

		// When Dispose is called
		workspace.Dispose();

		// Then the window is minimized
		window.Received(1).ShowMinimized();
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void WindowMinimizeStart(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		workspace.AddWindow(window);

		ILayoutEngine givenEngine = workspace.ActiveLayoutEngine;

		// When WindowMinimizeStart is called
		workspace.WindowMinimizeStart(window);

		// Then
		givenEngine.Received(1).RemoveWindow(window);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void WindowMinimizeStart_Twice(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		workspace.AddWindow(window);

		workspace.ActiveLayoutEngine.RemoveWindow(window).Returns(workspace.ActiveLayoutEngine);

		// When WindowMinimizeStart is called
		workspace.WindowMinimizeStart(window);
		workspace.WindowMinimizeStart(window);

		// Then the window is only removed the first time
		workspace.ActiveLayoutEngine.Received(1).RemoveWindow(window);
	}

	#region WindowMinimizeEnd
	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void WindowMinimizeEnd(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		workspace.AddWindow(window);
		workspace.WindowMinimizeStart(window);

		ILayoutEngine givenEngine = workspace.ActiveLayoutEngine;

		// When WindowMinimizeEnd is called
		workspace.WindowMinimizeEnd(window);

		// Then
		givenEngine.Received(1).AddWindow(window);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void WindowMinimizeEnd_NotMinimized(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		workspace.AddWindow(window);

		ILayoutEngine givenEngine = workspace.ActiveLayoutEngine;

		// When WindowMinimizeEnd is called
		workspace.WindowMinimizeEnd(window);

		// Then
		givenEngine.DidNotReceive().AddWindow(window);
	}
	#endregion

	#region PerformCustomLayoutEngineAction
	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void PerformCustomLayoutEngineAction_NoChange(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine
	)
	{
		// Given
		layoutEngine.PerformCustomAction(Arg.Any<string>(), Arg.Any<string>()).Returns(layoutEngine);
		Workspace workspace = new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		string actionName = "Action";
		string args = "Args";

		layoutEngine.ClearReceivedCalls();

		// When PerformCustomLayoutEngineAction is called
		workspace.PerformCustomLayoutEngineAction(actionName, args);

		// Then the layout engine is not changed
		layoutEngine.Received(1).PerformCustomAction(actionName, args);
		layoutEngine.DidNotReceive().DoLayout(Arg.Any<IRectangle<int>>(), Arg.Any<IMonitor>());
		Assert.Equal(layoutEngine, workspace.ActiveLayoutEngine);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void PerformCustomLayoutEngineAction_ChangeInInactiveEngine(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		ILayoutEngine layoutEngine1,
		ILayoutEngine layoutEngine1Result
	)
	{
		// Given
		layoutEngine.PerformCustomAction(Arg.Any<string>(), Arg.Any<string>()).Returns(layoutEngine);
		layoutEngine1.PerformCustomAction(Arg.Any<string>(), Arg.Any<string>()).Returns(layoutEngine1Result);

		Workspace workspace =
			new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine, layoutEngine1 });

		string actionName = "Action";
		string args = "Args";

		layoutEngine.ClearReceivedCalls();

		// When PerformCustomLayoutEngineAction is called
		workspace.PerformCustomLayoutEngineAction(actionName, args);

		// Then the layout engine is changed
		layoutEngine.Received(1).PerformCustomAction(actionName, args);
		layoutEngine.DidNotReceive().DoLayout(Arg.Any<IRectangle<int>>(), Arg.Any<IMonitor>());

		layoutEngine1.Received(1).PerformCustomAction(actionName, args);
		layoutEngine1.DidNotReceive().DoLayout(Arg.Any<IRectangle<int>>(), Arg.Any<IMonitor>());
		layoutEngine1Result.DidNotReceive().DoLayout(Arg.Any<IRectangle<int>>(), Arg.Any<IMonitor>());

		Assert.Equal(layoutEngine, workspace.ActiveLayoutEngine);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void PerformCustomLayoutEngineAction_ChangeInActiveEngine(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		ILayoutEngine layoutEngineResult,
		ILayoutEngine layoutEngine1
	)
	{
		// Given
		layoutEngine.PerformCustomAction(Arg.Any<string>(), Arg.Any<string>()).Returns(layoutEngineResult);
		layoutEngine1.PerformCustomAction(Arg.Any<string>(), Arg.Any<string>()).Returns(layoutEngine1);

		Workspace workspace =
			new(ctx, internalCtx, triggers, "Workspace", new ILayoutEngine[] { layoutEngine, layoutEngine1 });

		string actionName = "Action";
		string args = "Args";

		layoutEngine.ClearReceivedCalls();

		// When PerformCustomLayoutEngineAction is called
		workspace.PerformCustomLayoutEngineAction(actionName, args);

		// Then the layout engine is changed
		layoutEngine.Received(1).PerformCustomAction(actionName, args);
		layoutEngine.DidNotReceive().DoLayout(Arg.Any<IRectangle<int>>(), Arg.Any<IMonitor>());

		layoutEngineResult.Received(1).DoLayout(Arg.Any<IRectangle<int>>(), Arg.Any<IMonitor>());

		layoutEngine1.Received(1).PerformCustomAction(actionName, args);
		layoutEngine1.DidNotReceive().DoLayout(Arg.Any<IRectangle<int>>(), Arg.Any<IMonitor>());

		Assert.Equal(layoutEngineResult, workspace.ActiveLayoutEngine);
	}
	#endregion
}
