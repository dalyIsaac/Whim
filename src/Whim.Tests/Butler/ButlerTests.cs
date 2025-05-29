using System.Diagnostics.CodeAnalysis;
using NSubstitute.ReceivedExtensions;

namespace Whim.Tests;

[SuppressMessage("Usage", "NS5000:Received check.")]
[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class ButlerTests
{
	[Theory, AutoSubstituteData]
	internal void TriggerWindowRouted(IContext ctx, IWindow window)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);
		Butler sut = new(ctx);

		// When the event is triggered, then an event is raised
		sut.Initialize();
		Assert.Raises<RouteEventArgs>(
			h => sut.WindowRouted += h,
			h => sut.WindowRouted -= h,
			() =>
				ctx.Store.MapEvents.WindowRouted += Raise.Event<EventHandler<RouteEventArgs>>(
					ctx.Store.MapEvents,
					RouteEventArgs.WindowAdded(window, workspace)
				)
		);

		// Then when the disposable is disposed, then the event is unsubscribed
		sut.Dispose();
		ctx.Store.MapEvents.Received(1).WindowRouted -= Arg.Any<EventHandler<RouteEventArgs>>();
	}

	[Theory, AutoSubstituteData]
	internal void TriggerMonitorWorkspaceChanged(IContext ctx, IMonitor monitor)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);
		Butler sut = new(ctx);

		// When the event is triggered, then an event is raised
		sut.Initialize();
		Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => sut.MonitorWorkspaceChanged += h,
			h => sut.MonitorWorkspaceChanged -= h,
			() =>
				ctx.Store.MapEvents.MonitorWorkspaceChanged += Raise.Event<
					EventHandler<MonitorWorkspaceChangedEventArgs>
				>(
					ctx.Store.MapEvents,
					new MonitorWorkspaceChangedEventArgs() { CurrentWorkspace = workspace, Monitor = monitor }
				)
		);

		// Then when the disposable is disposed, then the event is unsubscribed
		sut.Dispose();
		ctx.Store.MapEvents.Received(1).MonitorWorkspaceChanged -= Arg.Any<
			EventHandler<MonitorWorkspaceChangedEventArgs>
		>();
	}

	[Theory, AutoSubstituteData]
	internal void Activate(IContext ctx)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);
		Butler sut = new(ctx);

		// When
		sut.Activate(workspace);

		// Then
		ctx.Store.Received(1).WhimDispatch(new ActivateWorkspaceTransform(workspace.Id, default));
	}

	[Theory, AutoSubstituteData]
	internal void ActivateAdjacent(IContext ctx, IMonitor monitor)
	{
		// Given
		Butler sut = new(ctx);

		// When
		sut.ActivateAdjacent(monitor);

		// Then
		ctx.Store.Received(1).WhimDispatch(new ActivateAdjacentWorkspaceTransform(monitor.Handle, false, false));
	}

	[Theory, AutoSubstituteData]
	internal void LayoutAllActiveWorkspaces(IContext ctx)
	{
		// Given
		Butler sut = new(ctx);

		// When
		sut.LayoutAllActiveWorkspaces();

		// Then
		ctx.Store.Received(1).WhimDispatch(new LayoutAllActiveWorkspacesTransform());
	}

	[Theory, AutoSubstituteData]
	internal void FocusMonitorDesktop(IContext ctx, IMonitor monitor)
	{
		// Given
		Butler sut = new(ctx);

		// When
		sut.FocusMonitorDesktop(monitor);

		// Then
		ctx.Store.Received(1).WhimDispatch(new FocusMonitorDesktopTransform(monitor.Handle));
	}

	[Theory, AutoSubstituteData]
	internal void MoveWindowEdgesInDirection(IContext ctx, IWindow window)
	{
		// Given
		Butler sut = new(ctx);

		// When
		sut.MoveWindowEdgesInDirection(Direction.Down, new Point<int>(10, 10), window);

		// Then
		ctx.Store.Received(1)
			.WhimDispatch(
				new MoveWindowEdgesInDirectionTransform(Direction.Down, new Point<int>(10, 10), window.Handle)
			);
	}

	[Theory, AutoSubstituteData]
	internal void MoveWindowToAdjacentWorkspace(IContext ctx, IWindow window)
	{
		// Given
		Butler sut = new(ctx);

		// When
		sut.MoveWindowToAdjacentWorkspace(window);

		// Then
		ctx.Store.Received(1).WhimDispatch(new MoveWindowToAdjacentWorkspaceTransform(window.Handle, false, false));
	}

	[Theory, AutoSubstituteData]
	internal void MoveWindowToPoint(IContext ctx, IWindow window)
	{
		// Given
		Butler sut = new(ctx);

		// When
		sut.MoveWindowToPoint(window, new Point<int>(10, 10));

		// Then
		ctx.Store.Received(1).Dispatch(new MoveWindowToPointTransform(window.Handle, new Point<int>(10, 10)));
	}

	[Theory, AutoSubstituteData]
	internal void MoveWindowToMonitor(IContext ctx, IMonitor monitor, IWindow window)
	{
		// Given
		Butler sut = new(ctx);

		// When
		sut.MoveWindowToMonitor(monitor, window);

		// Then
		ctx.Store.Received(1).Dispatch(new MoveWindowToMonitorTransform(monitor.Handle, window.Handle));
	}

	[Theory, AutoSubstituteData]
	internal void MoveWindowToPreviousMonitor(IContext ctx, IWindow window)
	{
		// Given
		Butler sut = new(ctx);

		// When
		sut.MoveWindowToPreviousMonitor(window);

		// Then
		ctx.Store.Received(1).WhimDispatch(new MoveWindowToAdjacentMonitorTransform(window.Handle, Reverse: true));
	}

	[Theory, AutoSubstituteData]
	internal void MoveWindowToNextMonitor(IContext ctx, IWindow window)
	{
		// Given
		Butler sut = new(ctx);

		// When
		sut.MoveWindowToNextMonitor(window);

		// Then
		ctx.Store.Received(1).WhimDispatch(new MoveWindowToAdjacentMonitorTransform(window.Handle, Reverse: false));
	}

	[Theory, AutoSubstituteData]
	internal void MoveWindowToWorkspace(IContext ctx, IWindow window)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);
		Butler sut = new(ctx);

		// When
		sut.MoveWindowToWorkspace(workspace, window);

		// Then
		ctx.Store.Received(1).Dispatch(new MoveWindowToWorkspaceTransform(workspace.Id, window.Handle));
	}

	[Theory, AutoSubstituteData]
	internal void MergeWorkspaceWindows(IContext ctx)
	{
		// Given
		Workspace source = CreateWorkspace(ctx);
		Workspace target = CreateWorkspace(ctx);

		Butler sut = new(ctx);

		// When
		sut.MergeWorkspaceWindows(source, target);

		// Then
		ctx.Store.Received(1).WhimDispatch(new MergeWorkspaceWindowsTransform(source.Id, target.Id));
	}

	[Theory, AutoSubstituteData]
	internal void SwapWorkspaceWithAdjacentMonitor(IContext ctx)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);
		Butler sut = new(ctx);

		// When
		sut.SwapWorkspaceWithAdjacentMonitor(workspace);

		// Then
		ctx.Store.Received(1).Dispatch(new SwapWorkspaceWithAdjacentMonitorTransform(workspace.Id, false));
	}
}
