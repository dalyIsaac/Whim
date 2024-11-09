using System.IO;
using System.Text.Json;

namespace Whim;

internal class CoreSavedStateManager : ICoreSavedStateManager
{
	private readonly IContext _context;
	private readonly string _savedStateFilePath;
	private bool _disposedValue;

	public CoreSavedState? SavedState { get; private set; }

	public CoreSavedStateManager(IContext context)
	{
		_context = context;
		_savedStateFilePath = Path.Combine(_context.FileManager.SavedStateDir, "core.json");
	}

	public void PreInitialize()
	{
		_context.FileManager.EnsureDirExists(_context.FileManager.SavedStateDir);

		// If the saved plugin state file doesn't yet exist, we don't need to load anything.
		if (!_context.FileManager.FileExists(_savedStateFilePath))
		{
			// Wipe the saved state file so that we don't try to load it again.
			_context.FileManager.DeleteFile(_savedStateFilePath);
			return;
		}

		try
		{
			SavedState = JsonSerializer.Deserialize<CoreSavedState>(
				_context.FileManager.ReadAllText(_savedStateFilePath)
			);
		}
		catch (Exception e)
		{
			Logger.Error($"Failed to deserialize saved state: {e}");
		}

		// Wipe the saved state file so that we don't try to load it again.
		_context.FileManager.DeleteFile(_savedStateFilePath);
	}

	public void PostInitialize()
	{
		SavedState = null;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				try
				{
					SaveState();
				}
				catch (Exception) { }
			}

			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void SaveState()
	{
		List<SavedWorkspace> savedWorkspaces = [];
		IMonitor monitor = _context.Store.Pick(PickPrimaryMonitor());

		IRectangle<int> fakeMonitorRect = new Rectangle<int>() { Height = 1000, Width = 1000 };

		foreach (IWorkspace workspace in _context.Store.Pick(PickWorkspaces()))
		{
			savedWorkspaces.Add(CreateSavedWorkspace(workspace, fakeMonitorRect, monitor));
		}

		CoreSavedState coreSavedState = new(savedWorkspaces);
		_context.FileManager.WriteAllText(_savedStateFilePath, JsonSerializer.Serialize(coreSavedState));
	}

	private SavedWorkspace CreateSavedWorkspace(IWorkspace workspace, IRectangle<int> fakeMonitorRect, IMonitor monitor)
	{
		List<SavedWindow> savedWindows = [];

		foreach (IWindowState windowState in workspace.GetActiveLayoutEngine().DoLayout(fakeMonitorRect, monitor))
		{
			Rectangle<double> scaled =
				(Rectangle<double>)MonitorHelpers.NormalizeRectangle(fakeMonitorRect, windowState.Rectangle);
			savedWindows.Add(new SavedWindow(windowState.Window.Handle, scaled));
		}

		int[]? stickyMonitorIndices = _context
			.Store.Pick(PickExplicitStickyMonitorIndicesByWorkspace(workspace.Id))
			.TryGet(out IReadOnlyList<int> indices)
			? [.. indices]
			: null;

		return new SavedWorkspace(workspace.BackingName, savedWindows, stickyMonitorIndices);
	}
}
