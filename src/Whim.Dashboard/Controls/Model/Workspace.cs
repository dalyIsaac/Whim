using System;
using System.ComponentModel;
using Whim.Core;

namespace Whim.Dashboard.Controls.Model;

/// <summary>
/// Workspace model used by <see cref="ViewModel.WorkspaceDashboardViewModel"/> and <see cref="WorkspaceDashboard"/>.
/// </summary>
internal class Workspace : INotifyPropertyChanged, IDisposable
{
	private readonly IWorkspace _workspace;
	private bool disposedValue;

	public string Name => _workspace.Name;

	private IMonitor? _monitor;

	public IMonitor? Monitor
	{
		get => _monitor;
		set
		{
			if (_monitor == value)
			{
				return;
			}

			_monitor = value;
			OnPropertyChanged(nameof(Monitor));
		}
	}

	public ILayoutEngine? LayoutEngine => _workspace.ActiveLayoutEngine;

	public Workspace(IWorkspace workspace, IMonitor? monitor = null)
	{
		_workspace = workspace;
		_monitor = monitor;
		workspace.ActiveLayoutEngineChanged += Workspace_ActiveLayoutEngineChanged;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged(string? propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private void Workspace_ActiveLayoutEngineChanged(object? sender, ActiveLayoutEngineChangedEventArgs args)
	{
		OnPropertyChanged(nameof(Monitor));
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				_workspace.ActiveLayoutEngineChanged -= Workspace_ActiveLayoutEngineChanged;
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
}
