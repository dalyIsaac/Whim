using System.Diagnostics.CodeAnalysis;
using FluentAssertions;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class LegacyWorkspaceTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Name_Get(IContext ctx, MutableRootSector root)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx) with
		{
			BackingName = "Bob",
		};
		AddWorkspaceToManager(ctx, root, workspace);

		// When
		string name = workspace.Name;

		// Then
		Assert.Equal("Bob", name);
	}

	[Theory, AutoSubstituteData]
	internal void Name_Set(IContext ctx)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);

		// When
		workspace.Name = "Bob";

		// Then
		ctx.Store.Received(1).Dispatch(new SetWorkspaceNameTransform(workspace.Id, "Bob"));
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void LastFocusedWindow(IContext ctx, MutableRootSector root)
	{
		// Given
		IWindow window = CreateWindow((HWND)123);
		AddWindowToSector(root, window);

		Workspace workspace = CreateWorkspace(ctx) with { LastFocusedWindowHandle = window.Handle };
		AddWorkspaceToManager(ctx, root, workspace);

		// When
		IWindow? result = workspace.LastFocusedWindow;

		// Then
		Assert.Same(result, window);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ActiveLayoutEngine(IContext ctx, MutableRootSector root, ILayoutEngine engine)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx) with
		{
			LayoutEngines = [engine],
		};
		AddWorkspaceToManager(ctx, root, workspace);

		// When
		ILayoutEngine result = workspace.ActiveLayoutEngine;

		// Then
		Assert.Same(result, engine);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Windows(IContext ctx, MutableRootSector root)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);

		IWindow window1 = CreateWindow((HWND)1);
		IWindow window2 = CreateWindow((HWND)2);

		workspace = PopulateWindowWorkspaceMap(ctx, root, window1, workspace);
		workspace = PopulateWindowWorkspaceMap(ctx, root, window2, workspace);

		// When
		IEnumerable<IWindow> result = workspace.Windows;

		// Then
		result.Should().BeEquivalentTo(new[] { window1, window2 });
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowFocused(IContext ctx, MutableRootSector root)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);
		IWindow window = CreateWindow((HWND)1);

		workspace = PopulateWindowWorkspaceMap(ctx, root, window, workspace);

		// When
		workspace.WindowFocused(window);

		// Then
		Assert.Equal(window.Handle, root.WorkspaceSector.Workspaces[workspace.Id].LastFocusedWindowHandle);
	}

	[Theory, AutoSubstituteData]
	internal void MinimizeWindowStart(IContext ctx)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);
		IWindow window = CreateWindow((HWND)1);

		// When
		workspace.MinimizeWindowStart(window);

		// Then
		ctx.Store.Received(1).Dispatch(new MinimizeWindowStartTransform(workspace.Id, window.Handle));
	}

	[Theory, AutoSubstituteData]
	internal void MinimizeWindowEnd(IContext ctx)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);
		IWindow window = CreateWindow((HWND)1);

		// When
		workspace.MinimizeWindowEnd(window);

		// Then
		ctx.Store.Received(1).Dispatch(new MinimizeWindowEndTransform(workspace.Id, window.Handle));
	}

	[Theory, AutoSubstituteData]
	internal void FocusLastFocusedWindow(IContext ctx)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);

		// When
		workspace.FocusLastFocusedWindow();

		// Then
		ctx.Store.Received(1).Dispatch(new FocusWorkspaceTransform(workspace.Id));
	}

	[Theory, AutoSubstituteData]
	internal void TrySetLayoutEngineFromIndex(IContext ctx)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);

		// When
		workspace.TrySetLayoutEngineFromIndex(1);

		// Then
		ctx.Store.Received(1).Dispatch(Arg.Any<ActivateLayoutEngineTransform>());
	}

	[Theory, AutoSubstituteData]
	internal void CycleLayoutEngine(IContext ctx)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);

		// When
		workspace.CycleLayoutEngine();

		// Then
		ctx.Store.Received(1).Dispatch(new CycleLayoutEngineTransform(workspace.Id));
	}

	[Theory, AutoSubstituteData]
	internal void ActivatePreviouslyActiveLayoutEngine(IContext ctx)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);

		// When
		workspace.ActivatePreviouslyActiveLayoutEngine();

		// Then
		ctx.Store.Received(1).Dispatch(Arg.Any<ActivateLayoutEngineTransform>());
	}

	[Theory, AutoSubstituteData]
	internal void TrySetLayoutEngineFromName(IContext ctx)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);

		// When
		workspace.TrySetLayoutEngineFromName("test");

		// Then
		ctx.Store.Received(1).Dispatch(Arg.Any<ActivateLayoutEngineTransform>());
	}

	[Theory, AutoSubstituteData]
	internal void AddWindow(IContext ctx)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);
		IWindow window = CreateWindow((HWND)1);

		// When
		workspace.AddWindow(window);

		// Then
		ctx.Store.Received(1).Dispatch(new AddWindowToWorkspaceTransform(workspace.Id, window));
	}

	[Theory, AutoSubstituteData]
	internal void RemoveWindow(IContext ctx)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);
		IWindow window = CreateWindow((HWND)1);

		// When
		workspace.RemoveWindow(window);

		// Then
		ctx.Store.Received(1).Dispatch(new RemoveWindowFromWorkspaceTransform(workspace.Id, window));
	}

	[Theory, AutoSubstituteData]
	internal void FocusWindowInDirection(IContext ctx)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);
		IWindow window = CreateWindow((HWND)1);

		// When
		workspace.FocusWindowInDirection(Direction.Left, window);

		// Then
		ctx.Store.Received(1)
			.Dispatch(new FocusWindowInDirectionTransform(workspace.Id, Direction.Left, window.Handle));
	}

	[Theory, AutoSubstituteData]
	internal void SwapWindowInDirection(IContext ctx)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);
		IWindow window = CreateWindow((HWND)1);

		// When
		workspace.SwapWindowInDirection(Direction.Left, window);

		// Then
		ctx.Store.Received(1)
			.Dispatch(new SwapWindowInDirectionTransform(workspace.Id, Direction.Left, window.Handle));
	}

	[Theory, AutoSubstituteData]
	internal void MoveWindowEdgesInDirection(IContext ctx)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);
		IWindow window = CreateWindow((HWND)1);

		// When
		workspace.MoveWindowEdgesInDirection(Direction.Left, new Point<double>(), window);

		// Then
		ctx.Store.Received(1)
			.Dispatch(
				new MoveWindowEdgesInDirectionWorkspaceTransform(
					workspace.Id,
					Direction.Left,
					new Point<double>(),
					window.Handle
				)
			);
	}

	[Theory, AutoSubstituteData]
	internal void MoveWindowToPoint(IContext ctx)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);
		IWindow window = CreateWindow((HWND)1);

		// When
		workspace.MoveWindowToPoint(window, new Point<double>());

		// Then
		ctx.Store.Received(1)
			.Dispatch(new MoveWindowToPointInWorkspaceTransform(workspace.Id, window.Handle, new Point<double>()));
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ToString_Override(IContext ctx, MutableRootSector root)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx) with
		{
			BackingName = "Legacy",
		};
		AddWorkspacesToManager(ctx, root, workspace);

		// When
		string result = workspace.ToString();

		// Then
		Assert.Equal("Legacy", result);
	}

	[Theory, AutoSubstituteData]
	internal void Deactivate(IContext ctx)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);

		// When
		workspace.Deactivate();

		// Then
		ctx.Store.Received(1).Dispatch(new DeactivateWorkspaceTransform(workspace.Id));
	}

	[Theory, AutoSubstituteData]
	internal void TryGetWindowState_WindowNotFound(IContext ctx)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);
		IWindow window = CreateWindow((HWND)1);
		ctx.Store.Pick(Arg.Any<PurePicker<Result<WindowPosition>>>())
			.Returns(
				Result.FromException<WindowPosition>(
					StoreExceptions.WindowNotFoundInWorkspace(window.Handle, workspace.Id)
				)
			);

		// When
		IWindowState? result = workspace.TryGetWindowState(window);

		// Then
		Assert.Null(result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void TryGetWindowState_WindowFound(IContext ctx, MutableRootSector root)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);
		IWindow window = CreateWindow((HWND)1);

		workspace = PopulateWindowWorkspaceMap(ctx, root, window, workspace);

		// When
		IWindowState? result = workspace.TryGetWindowState(window);

		// Then
		Assert.NotNull(result);
	}

	[Theory, AutoSubstituteData]
	internal void DoLayout(IContext ctx)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);

		// When
		workspace.DoLayout();

		// Then
		ctx.Store.Received(1).Dispatch(new DoWorkspaceLayoutTransform(workspace.Id));
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ContainsWindow(IContext ctx, MutableRootSector root)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);
		IWindow window = CreateWindow((HWND)1);

		PopulateWindowWorkspaceMap(ctx, root, window, workspace);

		// When
		bool result = workspace.ContainsWindow(window);

		// Then
		Assert.True(result);
	}

	[Theory]
	[InlineAutoSubstituteData(true)]
	[InlineAutoSubstituteData(false)]
	internal void PerformCustomLayoutEngineAction(bool isChanged, IContext ctx, IWindow window)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);
		LayoutEngineCustomAction action = new() { Name = "Test", Window = window };

		ctx.Store.Dispatch(new LayoutEngineCustomActionTransform(workspace.Id, action)).Returns(isChanged);

		// When
		bool result = workspace.PerformCustomLayoutEngineAction(action);

		// Then
		Assert.Equal(isChanged, result);
	}

	[Theory]
	[InlineAutoSubstituteData(true)]
	[InlineAutoSubstituteData(false)]
	internal void PerformCustomLayoutEngineAction_Payload(bool isChanged, IContext ctx, IWindow window)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);
		LayoutEngineCustomAction<IWindow> action =
			new()
			{
				Name = "Test",
				Window = window,
				Payload = window,
			};

		ctx.Store.Dispatch(new LayoutEngineCustomActionWithPayloadTransform<IWindow>(workspace.Id, action))
			.Returns(isChanged);

		// When
		bool result = workspace.PerformCustomLayoutEngineAction(action);

		// Then
		Assert.Equal(isChanged, result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Dispose_WorkspaceNotActive(IContext ctx, MutableRootSector root)
	{
		// Given
		IWindow window1 = CreateWindow((HWND)1);
		IWindow window2 = CreateWindow((HWND)2);

		Workspace workspace = CreateWorkspace(ctx);

		workspace = PopulateWindowWorkspaceMap(ctx, root, window1, workspace);
		workspace = PopulateWindowWorkspaceMap(ctx, root, window2, workspace);

		// When
		workspace.Dispose();

		// Then
		window1.Received().ShowMinimized();
		window2.Received().ShowMinimized();
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Dispose_WorkspaceActive(IContext ctx, MutableRootSector root)
	{
		// Given
		IMonitor monitor = CreateMonitor((HMONITOR)1);

		IWindow window1 = CreateWindow((HWND)1);
		IWindow window2 = CreateWindow((HWND)2);

		Workspace workspace = CreateWorkspace(ctx);

		workspace = PopulateThreeWayMap(ctx, root, monitor, workspace, window1);
		workspace = PopulateWindowWorkspaceMap(ctx, root, window2, workspace);

		// When
		workspace.Dispose();

		// Then
		window1.DidNotReceive().ShowMinimized();
		window2.DidNotReceive().ShowMinimized();
	}
}
