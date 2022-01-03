using System.ComponentModel;

namespace Whim.Dashboard.Workspaces;

/// <summary>
/// Workspace model used by <see cref="WorkspaceDashboardViewModel"/> and <see cref="WorkspaceDashboard"/>.
/// </summary>
internal class Workspace : INotifyPropertyChanged
{
	private readonly IWorkspace _workspace;

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
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged(string? propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	internal void Workspace_ActiveLayoutEngineChanged()
	{
		OnPropertyChanged(nameof(LayoutEngine));
	}
}
