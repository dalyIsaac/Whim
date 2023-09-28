using AutoFixture;
using NSubstitute;
using System;
using System.Threading;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.Tests;

public class WorkspaceCustomization : ICustomization
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Reliability",
		"CA2000:Dispose objects before losing scope",
		Justification = "Unnecessary for tests"
	)]
	public void Customize(IFixture fixture)
	{
		IInternalContext internalContext = fixture.Freeze<IInternalContext>();

		// Mock the readonly property LayoutLock in InternalContext.
		ReaderWriterLockSlim layoutLock = new();
		internalContext.LayoutLock.Returns(layoutLock);

		// Run DoLayout synchronously for tests.
		WorkspaceTestUtils.SetupDoLayout(internalContext);

		// Assume windows are valid windows.
		internalContext.CoreNativeManager.IsWindow(Arg.Any<HWND>()).Returns(true);

		// Assume windows are managed.
		internalContext.WindowManager.Windows.ContainsKey(Arg.Any<HWND>()).Returns(true);

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
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When
		workspace.Name = "Workspace2";

		// Then
		Assert.Equal("Workspace2", workspace.Name);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void TrySetLayoutEngine_CannotFindEngine(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When
		bool result = await workspace.TrySetLayoutEngine("Layout2");

		// Then
		Assert.False(result);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void TrySetLayoutEngine_AlreadyActive(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine
	)
	{
		// Given
		context.WorkspaceManager.GetMonitorForWorkspace(Arg.Any<IWorkspace>()).Returns(null as IMonitor);
		layoutEngine.Name.Returns("Layout");

		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When
		bool result = await workspace.TrySetLayoutEngine("Layout");

		// Then
		Assert.True(result);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void TrySetLayoutEngine_Success(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		ILayoutEngine layoutEngine2
	)
	{
		// Given
		layoutEngine2.Name.Returns("Layout2");

		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine, layoutEngine2 });

		// When
		bool result = await workspace.TrySetLayoutEngine("Layout2");

		// Then
		Assert.True(result);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void Constructor_FailWhenNoLayoutEngines(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers
	)
	{
		Assert.Throws<ArgumentException>(
			() => new Workspace(context, internalContext, triggers, "Workspace", Array.Empty<ILayoutEngine>())
		);
	}

	#region DoLayout
	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void DoLayout_CannotFindMonitorForWorkspace(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine
	)
	{
		// Given
		context.WorkspaceManager.GetMonitorForWorkspace(Arg.Any<IWorkspace>()).Returns(null as IMonitor);

		using Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When
		await workspace.DoLayout();

		// Then
		layoutEngine.DidNotReceive().DoLayout(Arg.Any<ILocation<int>>(), Arg.Any<IMonitor>());
		triggers.WorkspaceLayoutStarted.DidNotReceive().Invoke(Arg.Any<WorkspaceEventArgs>());
		triggers.WorkspaceLayoutCompleted.DidNotReceive().Invoke(Arg.Any<WorkspaceEventArgs>());
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void DoLayout_MinimizedWindow(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given

		using Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When
		await workspace.AddWindow(window);
		triggers.WorkspaceLayoutStarted.ClearReceivedCalls();
		triggers.WorkspaceLayoutCompleted.ClearReceivedCalls();
		await workspace.WindowMinimizeStart(window);

		// Then
		context.NativeManager.DidNotReceive().ShowWindowNoActivate(Arg.Any<HWND>());
		window.Received(1).ShowMinimized();
		triggers.WorkspaceLayoutStarted.Received(1).Invoke(Arg.Any<WorkspaceEventArgs>());
		triggers.WorkspaceLayoutCompleted.Received(1).Invoke(Arg.Any<WorkspaceEventArgs>());
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void DoLayout_GarbageCollect_IsNotAWindow(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		using Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		internalContext.CoreNativeManager.IsWindow(Arg.Any<HWND>()).Returns(false);

		// When
		await workspace.AddWindow(window);

		// Then the window should have been removed, and the layout didn't start
		internalContext.WindowManager.Received(1).OnWindowRemoved(window);
		context.WorkspaceManager.DidNotReceive().GetMonitorForWorkspace(Arg.Any<IWorkspace>());
		triggers.WorkspaceLayoutStarted.DidNotReceive().Invoke(Arg.Any<WorkspaceEventArgs>());
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void DoLayout_GarbageCollect_HandleIsNotManaged(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		using Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		internalContext.CoreNativeManager.IsWindow(Arg.Any<HWND>()).Returns(true);
		internalContext.WindowManager.Windows.ContainsKey(Arg.Any<HWND>()).Returns(false);

		// When
		await workspace.AddWindow(window);

		// Then the window should have been removed, and the layout didn't start
		internalContext.WindowManager.Received(1).OnWindowRemoved(window);
		context.WorkspaceManager.DidNotReceive().GetMonitorForWorkspace(Arg.Any<IWorkspace>());
		triggers.WorkspaceLayoutStarted.DidNotReceive().Invoke(Arg.Any<WorkspaceEventArgs>());
	}
	#endregion

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void ContainsWindow_False(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When
		bool result = workspace.ContainsWindow(window);

		// Then
		Assert.False(result);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void ContainsWindow_True_NormalWindow(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		await workspace.AddWindow(window);

		// When
		bool result = workspace.ContainsWindow(window);

		// Then
		Assert.True(result);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void ContainsWindow_True_MinimizedWindow(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		await workspace.AddWindow(window);
		await workspace.WindowMinimizeStart(window);

		// When
		bool result = workspace.ContainsWindow(window);

		// Then
		Assert.True(result);
	}

	#region WindowFocused
	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void WindowFocused_ContainsWindow(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given the window is in the workspace
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		await workspace.AddWindow(window);

		// When
		workspace.WindowFocused(window);

		// Then
		Assert.Equal(window, workspace.LastFocusedWindow);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void WindowFocused_DoesNotContainWindow(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given the window is not in the workspace
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When
		workspace.WindowFocused(window);

		// Then
		Assert.Null(workspace.LastFocusedWindow);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void WindowFocused_WindowIsNull(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given the window is null
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When
		workspace.WindowFocused(window);
		workspace.WindowFocused(null);

		// Then
		Assert.Null(workspace.LastFocusedWindow);
	}
	#endregion

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void FocusFirstWindow(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When FocusFirstWindow is called
		workspace.FocusFirstWindow();

		// Then the LayoutEngine's GetFirstWindow method is called
		layoutEngine.Received(1).GetFirstWindow();
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void NextLayoutEngine(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		ILayoutEngine layoutEngine2
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine, layoutEngine2 });

		// When NextLayoutEngine is called
		await workspace.NextLayoutEngine();

		// Then the active layout engine is set to the next one
		Assert.True(Object.ReferenceEquals(layoutEngine2, workspace.ActiveLayoutEngine));
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void NextLayoutEngine_LastEngine(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		ILayoutEngine layoutEngine2
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine, layoutEngine2 });

		// When NextLayoutEngine is called
		await workspace.NextLayoutEngine();
		await workspace.NextLayoutEngine();

		// Then the active layout engine is set to the first one
		Assert.True(Object.ReferenceEquals(layoutEngine, workspace.ActiveLayoutEngine));
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void PreviousLayoutEngine(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		ILayoutEngine layoutEngine2
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine, layoutEngine2 });

		// When PreviousLayoutEngine is called
		await workspace.PreviousLayoutEngine();

		// Then the active layout engine is set to the previous one
		Assert.True(Object.ReferenceEquals(layoutEngine2, workspace.ActiveLayoutEngine));
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void PreviousLayoutEngine_FirstEngine(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		ILayoutEngine layoutEngine2
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine, layoutEngine2 });

		// When PreviousLayoutEngine is called
		await workspace.PreviousLayoutEngine();
		await workspace.PreviousLayoutEngine();

		// Then the active layout engine is set to the last one
		Assert.True(Object.ReferenceEquals(layoutEngine, workspace.ActiveLayoutEngine));
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void AddWindow_Fails_AlreadyIncludesWindow(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When AddWindow is called
		await workspace.AddWindow(window);
		await workspace.AddWindow(window);

		// Then the window is added to the layout engine
		layoutEngine.Received(1).AddWindow(window);
		context.WorkspaceManager.Received(1).GetMonitorForWorkspace(workspace);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void AddWindow_Success(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When AddWindow is called
		await workspace.AddWindow(window);

		// Then the window is added to the layout engine
		layoutEngine.Received(1).AddWindow(window);
		context.WorkspaceManager.Received(1).GetMonitorForWorkspace(workspace);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void RemoveWindow_Fails_AlreadyRemoved(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When RemoveWindow is called
		await workspace.RemoveWindow(window);
		bool result = await workspace.RemoveWindow(window);

		// Then the window is removed from the layout engine
		Assert.False(result);
		layoutEngine.DidNotReceive().RemoveWindow(window);
		internalContext.CoreNativeManager.DidNotReceive().IsWindow(Arg.Any<HWND>());
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void RemoveWindow_Fails_DidNotRemoveFromLayoutEngine(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		await workspace.AddWindow(window);

		internalContext.CoreNativeManager.ClearReceivedCalls();

		ILayoutEngine givenEngine = workspace.ActiveLayoutEngine;
		givenEngine.RemoveWindow(window).Returns(givenEngine);

		// When RemoveWindow is called
		bool result = await workspace.RemoveWindow(window);

		// Then the window is removed from the layout engine
		Assert.False(result);
		givenEngine.Received(1).RemoveWindow(window);
		internalContext.CoreNativeManager.DidNotReceive().IsWindow(Arg.Any<HWND>());
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void RemoveWindow_Success(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		await workspace.AddWindow(window);
		workspace.WindowFocused(window);

		ILayoutEngine givenEngine = workspace.ActiveLayoutEngine;
		context.WorkspaceManager.ClearReceivedCalls();
		internalContext.CoreNativeManager.ClearReceivedCalls();

		// When RemoveWindow is called
		bool result = await workspace.RemoveWindow(window);

		// Then the window is removed from the layout engine
		Assert.True(result);
		givenEngine.Received(1).RemoveWindow(window);
		context.WorkspaceManager.Received(1).GetMonitorForWorkspace(workspace);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void RemoveWindow_MinimizedWindow(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		window.IsMinimized.Returns(true);
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		await workspace.AddWindow(window);
		await workspace.WindowMinimizeStart(window);

		context.WorkspaceManager.ClearReceivedCalls();
		internalContext.CoreNativeManager.ClearReceivedCalls();

		// When RemoveWindow is called
		bool result = await workspace.RemoveWindow(window);

		// Then the window is not removed from the layout engine
		Assert.True(result);
		layoutEngine.DidNotReceive().RemoveWindow(window);
		context.WorkspaceManager.Received(1).GetMonitorForWorkspace(workspace);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void FocusWindowInDirection_Fails_DoesNotContainWindow(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When FocusWindowInDirection is called
		workspace.FocusWindowInDirection(Direction.Up, window);

		// Then the layout engine is not told to focus the window
		layoutEngine.DidNotReceive().FocusWindowInDirection(Direction.Up, window);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void FocusWindowInDirection_Fails_WindowIsMinimized(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		await workspace.AddWindow(window);
		await workspace.WindowMinimizeStart(window);

		// When FocusWindowInDirection is called
		workspace.FocusWindowInDirection(Direction.Up, window);

		// Then the layout engine is not told to focus the window
		layoutEngine.DidNotReceive().FocusWindowInDirection(Direction.Up, window);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void FocusWindowInDirection_Success(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		await workspace.AddWindow(window);

		// When FocusWindowInDirection is called
		workspace.FocusWindowInDirection(Direction.Up, window);

		// Then the layout engine is told to focus the window
		workspace.ActiveLayoutEngine.Received(1).FocusWindowInDirection(Direction.Up, window);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void SwapWindowInDirection_Fails_WindowIsNull(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When SwapWindowInDirection is called
		await workspace.SwapWindowInDirection(Direction.Up, null);

		// Then the layout engine is not told to swap the window
		layoutEngine.DidNotReceive().SwapWindowInDirection(Direction.Up, Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void SwapWindowInDirection_Fails_DoesNotContainWindow(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When SwapWindowInDirection is called
		await workspace.SwapWindowInDirection(Direction.Up, window);

		// Then the layout engine is not told to swap the window
		layoutEngine.DidNotReceive().SwapWindowInDirection(Direction.Up, window);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void SwapWindowInDirection_Success(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		await workspace.AddWindow(window);

		ILayoutEngine givenEngine = workspace.ActiveLayoutEngine;

		// When SwapWindowInDirection is called
		await workspace.SwapWindowInDirection(Direction.Up, window);

		// Then the layout engine is told to swap the window
		givenEngine.Received(1).SwapWindowInDirection(Direction.Up, window);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void MoveWindowEdgesInDirection_Fails_WindowIsNull(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		IPoint<double> deltas = new Point<double>() { X = 0.3, Y = 0 };

		// When MoveWindowEdgesInDirection is called
		await workspace.MoveWindowEdgesInDirection(Direction.Up, deltas, null);

		// Then the layout engine is not told to move the window
		layoutEngine.DidNotReceive().MoveWindowEdgesInDirection(Direction.Up, deltas, Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void MoveWindowEdgesInDirection_Fails_DoesNotContainWindow(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		IPoint<double> deltas = new Point<double>() { X = 0.3, Y = 0 };

		// When MoveWindowEdgesInDirection is called
		await workspace.MoveWindowEdgesInDirection(Direction.Up, deltas, window);

		// Then the layout engine is not told to move the window
		layoutEngine.DidNotReceive().MoveWindowEdgesInDirection(Direction.Up, deltas, window);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void MoveWindowEdgesInDirection_Success(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		await workspace.AddWindow(window);
		IPoint<double> deltas = new Point<double>() { X = 0.3, Y = 0 };
		ILayoutEngine givenEngine = workspace.ActiveLayoutEngine;

		// When MoveWindowEdgesInDirection is called
		await workspace.MoveWindowEdgesInDirection(Direction.Up, deltas, window);

		// Then the layout engine is told to move the window
		givenEngine.Received(1).MoveWindowEdgesInDirection(Direction.Up, deltas, window);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void MoveWindowToPoint_Success_AddWindow(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		IPoint<double> point = new Point<double>() { X = 0.3, Y = 0.3 };

		// When MoveWindowToPoint is called
		await workspace.MoveWindowToPoint(window, point);

		// Then the layout engine is told to move the window
		layoutEngine.Received(1).MoveWindowToPoint(window, point);
		layoutEngine.DidNotReceive().RemoveWindow(window);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void MoveWindowToPoint_Success_WindowIsMinimized(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		IPoint<double> point = new Point<double>() { X = 0.3, Y = 0.3 };

		await workspace.AddWindow(window);
		await workspace.WindowMinimizeStart(window);

		ILayoutEngine givenEngine = workspace.ActiveLayoutEngine;

		// When MoveWindowToPoint is called
		await workspace.MoveWindowToPoint(window, point);

		// Then the layout engine is told to move the window
		givenEngine.Received(1).MoveWindowToPoint(window, point);
		givenEngine.DidNotReceive().RemoveWindow(window);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void MoveWindowToPoint_Success_WindowAlreadyExists(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		await workspace.AddWindow(window);
		IPoint<double> point = new Point<double>() { X = 0.3, Y = 0.3 };

		ILayoutEngine givenEngine = workspace.ActiveLayoutEngine;

		// When MoveWindowToPoint is called
		await workspace.MoveWindowToPoint(window, point);

		// Then the layout engine is told to remove and add the window
		givenEngine.Received(1).MoveWindowToPoint(window, point);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void ToString_Success(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When ToString is called
		string result = workspace.ToString();

		// Then the result is as expected
		Assert.Equal("Workspace", result);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void Deactivate(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window,
		IWindow window2
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		await workspace.AddWindow(window);
		await workspace.AddWindow(window2);
		internalContext.CoreNativeManager.ClearReceivedCalls();

		// When Deactivate is called
		workspace.Deactivate();

		// Then the windows are hidden and DoLayout is called
		internalContext.CoreNativeManager.DidNotReceive().IsWindow(Arg.Any<HWND>());
		window.Received(1).Hide();
		window2.Received(1).Hide();
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void TryGetWindowLocation(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		ILayoutEngine resultingEngine,
		IWindow window
	)
	{
		// Given
		layoutEngine.AddWindow(window).Returns(resultingEngine);
		resultingEngine
			.DoLayout(Arg.Any<ILocation<int>>(), Arg.Any<IMonitor>())
			.Returns(
				new WindowState[]
				{
					new()
					{
						Location = new Location<int>(),
						Window = window,
						WindowSize = WindowSize.Normal
					}
				}
			);

		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		await workspace.AddWindow(window);

		// When TryGetWindowLocation is called
		IWindowState? result = workspace.TryGetWindowLocation(window);

		// Then the result is as expected
		Assert.NotNull(result);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void TryGetWindowLocation_MinimizedWindow(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });

		// When
		await workspace.AddWindow(window);
		await workspace.WindowMinimizeStart(window);
		IWindowState windowState = workspace.TryGetWindowLocation(window)!;

		// Then
		Assert.Equal(window, windowState.Window);
		Assert.Equal(0, windowState.Location.X);
		Assert.Equal(0, windowState.Location.Y);
		Assert.Equal(0, windowState.Location.Width);
		Assert.Equal(0, windowState.Location.Height);
		Assert.Equal(WindowSize.Minimized, windowState.WindowSize);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void Dispose(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		IWindow window,
		ILayoutEngine layoutEngine
	)
	{
		// Given
		context.WorkspaceManager.GetMonitorForWorkspace(Arg.Any<IWorkspace>()).Returns((IMonitor?)null);

		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		await workspace.AddWindow(window);

		// When Dispose is called
		workspace.Dispose();

		// Then the window is minimized
		window.Received(1).ShowMinimized();
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void WindowMinimizeStart(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		await workspace.AddWindow(window);

		ILayoutEngine givenEngine = workspace.ActiveLayoutEngine;

		// When WindowMinimizeStart is called
		await workspace.WindowMinimizeStart(window);

		// Then
		givenEngine.Received(1).RemoveWindow(window);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void WindowMinimizeStart_Twice(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		await workspace.AddWindow(window);

		workspace.ActiveLayoutEngine.RemoveWindow(window).Returns(workspace.ActiveLayoutEngine);

		// When WindowMinimizeStart is called
		await workspace.WindowMinimizeStart(window);
		await workspace.WindowMinimizeStart(window);

		// Then the window is only removed the first time
		workspace.ActiveLayoutEngine.Received(1).RemoveWindow(window);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void WindowMinimizeEnd(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		await workspace.AddWindow(window);
		await workspace.WindowMinimizeStart(window);

		ILayoutEngine givenEngine = workspace.ActiveLayoutEngine;

		// When WindowMinimizeEnd is called
		await workspace.WindowMinimizeEnd(window);

		// Then
		givenEngine.Received(1).AddWindow(window);
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal async void WindowMinimizeEnd_NotMinimized(
		IContext context,
		IInternalContext internalContext,
		WorkspaceManagerTriggers triggers,
		ILayoutEngine layoutEngine,
		IWindow window
	)
	{
		// Given
		Workspace workspace =
			new(context, internalContext, triggers, "Workspace", new ILayoutEngine[] { layoutEngine });
		await workspace.AddWindow(window);

		ILayoutEngine givenEngine = workspace.ActiveLayoutEngine;

		// When WindowMinimizeEnd is called
		await workspace.WindowMinimizeEnd(window);

		// Then
		givenEngine.DidNotReceive().AddWindow(window);
	}
}
