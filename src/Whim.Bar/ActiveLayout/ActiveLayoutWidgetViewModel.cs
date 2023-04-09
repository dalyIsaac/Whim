using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Whim.Bar;

/// <summary>
/// View model containing the active layout.
/// </summary>
public class ActiveLayoutWidgetViewModel : INotifyPropertyChanged, IDisposable
{
	private readonly IContext _context;

	/// <summary>
	/// The monitor that the widget is displayed on.
	/// </summary>
	public IMonitor Monitor { get; }

	private bool _disposedValue;

	private readonly HashSet<IWorkspace> _workspaces = new();

	/// <summary>
	/// The name of the active layout engine.
	/// </summary>
	public string ActiveLayoutEngine =>
		_context.WorkspaceManager.GetWorkspaceForMonitor(Monitor)?.ActiveLayoutEngine.Name ?? "";

	/// <summary>
	/// Command to switch to the next layout engine.
	/// </summary>
	public System.Windows.Input.ICommand NextLayoutEngineCommand { get; }

	/// <summary>
	///
	/// </summary>
	/// <param name="context"></param>
	/// <param name="monitor"></param>
	public ActiveLayoutWidgetViewModel(IContext context, IMonitor monitor)
	{
		_context = context;
		Monitor = monitor;
		NextLayoutEngineCommand = new NextLayoutEngineCommand(context, this);

		_context.WorkspaceManager.WorkspaceAdded += WorkspaceManager_WorkspaceAdded;
		_context.WorkspaceManager.WorkspaceRemoved += WorkspaceManager_WorkspaceRemoved;
		_context.WorkspaceManager.ActiveLayoutEngineChanged += WorkspaceManager_ActiveLayoutEngineChanged;

		foreach (IWorkspace workspace in _context.WorkspaceManager)
		{
			_workspaces.Add(workspace);
		}
	}

	private void WorkspaceManager_ActiveWorkspaceChanged(object? sender, EventArgs e) =>
		OnPropertyChanged(nameof(ActiveLayoutEngine));

	/// <inheritdoc/>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <inheritdoc/>
	protected virtual void OnPropertyChanged(string? propertyName) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

	/// <inheritdoc/>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_context.WorkspaceManager.WorkspaceAdded -= WorkspaceManager_WorkspaceAdded;
				_context.WorkspaceManager.WorkspaceRemoved -= WorkspaceManager_WorkspaceRemoved;
				_context.WorkspaceManager.ActiveLayoutEngineChanged -= WorkspaceManager_ActiveLayoutEngineChanged;
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			_disposedValue = true;
		}
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void WorkspaceManager_WorkspaceAdded(object? sender, WorkspaceEventArgs e)
	{
		_workspaces.Add(e.Workspace);
	}

	private void WorkspaceManager_WorkspaceRemoved(object? sender, WorkspaceEventArgs e)
	{
		_workspaces.Remove(e.Workspace);
	}

	private void WorkspaceManager_ActiveLayoutEngineChanged(object? sender, EventArgs e)
	{
		OnPropertyChanged(nameof(ActiveLayoutEngine));
	}
}
