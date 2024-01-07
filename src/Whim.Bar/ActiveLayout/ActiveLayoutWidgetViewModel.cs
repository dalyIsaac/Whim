using System;
using System.ComponentModel;

namespace Whim.Bar;

/// <summary>
/// View model containing the active layout.
/// </summary>
internal class ActiveLayoutWidgetViewModel : INotifyPropertyChanged, IDisposable
{
	private readonly IContext _context;

	/// <summary>
	/// The monitor that the widget is displayed on.
	/// </summary>
	public IMonitor Monitor { get; }

	private bool _disposedValue;

	/// <summary>
	/// The name of the active layout engine.
	/// </summary>
	public string ActiveLayoutEngine => _context.Butler.GetWorkspaceForMonitor(Monitor)?.ActiveLayoutEngine.Name ?? "";

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

		_context.WorkspaceManager.ActiveLayoutEngineChanged += WorkspaceManager_ActiveLayoutEngineChanged;
		_context.Butler.MonitorWorkspaceChanged += WorkspaceManager_MonitorWorkspaceChanged;
	}

	private void WorkspaceManager_ActiveLayoutEngineChanged(object? sender, EventArgs e) =>
		OnPropertyChanged(nameof(ActiveLayoutEngine));

	private void WorkspaceManager_MonitorWorkspaceChanged(object? sender, MonitorWorkspaceChangedEventArgs e)
	{
		if (e.Monitor.Equals(Monitor))
		{
			OnPropertyChanged(nameof(ActiveLayoutEngine));
		}
	}

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
				_context.WorkspaceManager.ActiveLayoutEngineChanged -= WorkspaceManager_ActiveLayoutEngineChanged;
				_context.Butler.MonitorWorkspaceChanged -= WorkspaceManager_MonitorWorkspaceChanged;
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
}
