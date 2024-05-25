using System;

namespace Whim;

internal partial class Butler : IButler, IInternalButler
{
	private readonly IContext _context;

	public IButlerPantry Pantry { get; }

	public Butler(IContext context)
	{
		_context = context;

		Pantry = new ButlerPantry(_context);
	}

	public event EventHandler<RouteEventArgs>? WindowRouted;

	public event EventHandler<MonitorWorkspaceChangedEventArgs>? MonitorWorkspaceChanged;

	public void TriggerWindowRouted(RouteEventArgs args) => WindowRouted?.Invoke(this, args);

	public void TriggerMonitorWorkspaceChanged(MonitorWorkspaceChangedEventArgs args) =>
		MonitorWorkspaceChanged?.Invoke(this, args);

	#region Chores
	public void Activate(IWorkspace workspace, IMonitor? monitor = null) =>
		_context.Store.Dispatch(new ActivateWorkspaceTransform(workspace.Id, monitor?.Handle ?? default));

	public void ActivateAdjacent(IMonitor? monitor = null, bool reverse = false, bool skipActive = false) =>
		_context.Store.Dispatch(
			new ActivateAdjacentWorkspaceTransform(monitor?.Handle ?? default, reverse, skipActive)
		);

	public void LayoutAllActiveWorkspaces() => _context.Store.Dispatch(new LayoutAllActiveWorkspacesTransform());

	public void FocusMonitorDesktop(IMonitor monitor) =>
		_context.Store.Dispatch(new FocusMonitorDesktopTransform(monitor?.Handle ?? default));

	public bool MoveWindowEdgesInDirection(Direction edges, IPoint<int> pixelsDeltas, IWindow? window = null) =>
		_context
			.Store.Dispatch(new MoveWindowEdgesInDirectionTransform(edges, pixelsDeltas, window?.Handle ?? default))
			.IsSuccessful;

	public void MoveWindowToAdjacentWorkspace(IWindow? window = null, bool reverse = false, bool skipActive = false) =>
		_context.Store.Dispatch(
			new MoveWindowToAdjacentWorkspaceTransform(window?.Handle ?? default, reverse, skipActive)
		);

	public void MoveWindowToMonitor(IMonitor monitor, IWindow? window = null) =>
		_context.Store.Dispatch(
			new MoveWindowToMonitorTransform(monitor?.Handle ?? default, window?.Handle ?? default)
		);

	public void MoveWindowToPreviousMonitor(IWindow? window = null) =>
		_context.Store.Dispatch(new MoveWindowToAdjacentMonitorTransform(window?.Handle ?? default, Reverse: true));

	public void MoveWindowToNextMonitor(IWindow? window = null) =>
		_context.Store.Dispatch(new MoveWindowToAdjacentMonitorTransform(window?.Handle ?? default, Reverse: false));

	public void MoveWindowToPoint(IWindow window, IPoint<int> point) =>
		_context.Store.Dispatch(new MoveWindowToPointTransform(window?.Handle ?? default, point));

	public void MoveWindowToWorkspace(IWorkspace targetWorkspace, IWindow? window = null) =>
		_context.Store.Dispatch(new MoveWindowToWorkspaceTransform(targetWorkspace.Id, window?.Handle ?? default));

	public void MergeWorkspaceWindows(IWorkspace source, IWorkspace target) =>
		_context.Store.Dispatch(new MergeWorkspaceWindowsTransform(source.Id, target.Id));

	public void SwapWorkspaceWithAdjacentMonitor(IWorkspace? workspace = null, bool reverse = false) =>
		_context.Store.Dispatch(new SwapWorkspaceWithAdjacentMonitorTransform(workspace?.Id ?? default, reverse));
	#endregion
}
