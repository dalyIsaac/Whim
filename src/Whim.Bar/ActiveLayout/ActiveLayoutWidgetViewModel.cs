using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Whim.Bar;

public class ActiveLayoutWidgetViewModel : INotifyPropertyChanged, IDisposable
{
	private readonly IConfigContext _configContext;
	public IMonitor Monitor { get; }
	private bool disposedValue;

	private readonly HashSet<IWorkspace> _workspaces = new();
	public string ActiveLayoutEngine { get => _configContext.WorkspaceManager.GetWorkspaceForMonitor(Monitor)?.ActiveLayoutEngine.Name ?? ""; }

	public System.Windows.Input.ICommand NextLayoutEngineCommand { get; }

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

	private void WorkspaceManager_ActiveWorkspaceChanged(object? sender, EventArgs e)
	{
		OnPropertyChanged(nameof(ActiveLayoutEngine));
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged(string? propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
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
			disposedValue = true;
		}
	}

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
