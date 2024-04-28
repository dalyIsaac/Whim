using System.Threading.Tasks;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

internal class ButlerEventHandlers : IButlerEventHandlers
{
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;
	private readonly IButlerPantry _pantry;
	private readonly IButlerChores _chores;

	private int _monitorsChangingTasks;
	public bool AreMonitorsChanging => _monitorsChangingTasks > 0;

	public int MonitorsChangedDelay { init; get; } = 3 * 1000;

	public ButlerEventHandlers(
		IContext context,
		IInternalContext internalContext,
		IButlerPantry pantry,
		IButlerChores chores
	)
	{
		_context = context;
		_internalContext = internalContext;
		_pantry = pantry;
		_chores = chores;
	}

	public void OnWindowMinimizeEnd(WindowEventArgs args)
	{
		IWindow window = args.Window;
		Logger.Debug($"Window minimize end: {window}");

		if (_pantry.GetWorkspaceForWindow(window) is not IWorkspace workspace)
		{
			Logger.Error($"Window {window} was not found in any workspace");
			return;
		}

		workspace.MinimizeWindowEnd(window);
		workspace.DoLayout();
	}

	public void OnMonitorsChanged(MonitorsChangedEventArgs e)
	{
		Logger.Debug($"Monitors changed: {e}");

		_monitorsChangingTasks++;

		// Deactivate all workspaces.
		foreach (IWorkspace visibleWorkspace in _pantry.GetAllActiveWorkspaces())
		{
			visibleWorkspace.Deactivate();
		}

		// If a monitor was removed, remove the workspace from the map.
		foreach (IMonitor monitor in e.RemovedMonitors)
		{
			IWorkspace? workspace = _pantry.GetWorkspaceForMonitor(monitor);
			_pantry.RemoveMonitor(monitor);

			if (workspace is null)
			{
				Logger.Error($"Could not find workspace for monitor {monitor}");
				continue;
			}

			workspace.Deactivate();
		}

		// If a monitor was added, set it to an inactive workspace.
		foreach (IMonitor monitor in e.AddedMonitors)
		{
			// Try find a workspace which doesn't have a monitor.
			IWorkspace? workspace = null;
			foreach (IWorkspace w in _context.WorkspaceManager)
			{
				if (_pantry.GetMonitorForWorkspace(w) is null)
				{
					workspace = w;
					_pantry.SetMonitorWorkspace(monitor, w);
					break;
				}
			}

			// If there's no workspace, create one.
			if (workspace is null)
			{
				if (_context.WorkspaceManager.Add() is IWorkspace newWorkspace)
				{
					_pantry.SetMonitorWorkspace(monitor, newWorkspace);
				}
				else
				{
					continue;
				}
			}
		}

		// Hack to only accept window events after Windows has been given a chance to stop moving
		// windows around after a monitor change.
		// NOTE: ButlerEventHandlersTests has a test for this which only runs locally - it is
		// turned off in CI as it has proved flaky when running on GitHub Actions.
		_context.NativeManager.TryEnqueue(async () =>
		{
			await Task.Delay(MonitorsChangedDelay).ConfigureAwait(true);

			_monitorsChangingTasks--;
			if (_monitorsChangingTasks > 0)
			{
				Logger.Debug("Monitors changed: More tasks are pending");
				return;
			}

			Logger.Debug("Cleared AreMonitorsChanging");

			// For each workspace which is active in a monitor, do a layout.
			// This will handle cases when the monitor's properties have changed.
			_chores.LayoutAllActiveWorkspaces();
		});
	}
}
