using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Whim.Bar;

/// <summary>
/// View model containing the active layout.
/// </summary>
public class ActiveLayoutWidgetViewModel : INotifyPropertyChanged, IDisposable
{
	private readonly IConfigContext _configContext;

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
		_configContext.WorkspaceManager.GetWorkspaceForMonitor(Monitor)?.ActiveLayoutEngine.Name ?? "";

	/// <summary>
	/// Command to switch to the next layout engine.
	/// </summary>
	public System.Windows.Input.ICommand NextLayoutEngineCommand { get; }

	/// <summary>
	///
	/// </summary>
	/// <param name="configContext"></param>
	/// <param name="monitor"></param>
	public ActiveLayoutWidgetViewModel(IConfigContext configContext, IMonitor monitor)
	{
		_configContext = configContext;
		Monitor = monitor;
		NextLayoutEngineCommand = new NextLayoutEngineCommand(configContext, this);

		_configContext.WorkspaceManager.WorkspaceAdded += WorkspaceManager_WorkspaceAdded;
		_configContext.WorkspaceManager.WorkspaceRemoved += WorkspaceManager_WorkspaceRemoved;
		_configContext.WorkspaceManager.ActiveLayoutEngineChanged += WorkspaceManager_ActiveLayoutEngineChanged;

		foreach (IWorkspace workspace in _configContext.WorkspaceManager)
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
				_configContext.WorkspaceManager.WorkspaceAdded -= WorkspaceManager_WorkspaceAdded;
				_configContext.WorkspaceManager.WorkspaceRemoved -= WorkspaceManager_WorkspaceRemoved;
				_configContext.WorkspaceManager.ActiveLayoutEngineChanged -= WorkspaceManager_ActiveLayoutEngineChanged;
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
