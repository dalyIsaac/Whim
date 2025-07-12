using System;
using System.ComponentModel;
using Microsoft.UI.Xaml;

namespace Whim.TreeLayout.Bar;

/// <summary>
/// View model for the active tree layout engine on the given monitor.
/// </summary>
public class TreeLayoutEngineWidgetViewModel : INotifyPropertyChanged, IDisposable
{
	private readonly IContext _context;
	private readonly ITreeLayoutPlugin _plugin;
	private readonly IMonitor _monitor;
	private bool _disposedValue;

	/// <summary>
	/// Shortcut to indicate if <see cref="AddNodeDirection"/> is <see langword="null"/>.
	/// </summary>
	public Visibility IsVisible =>
		_plugin.GetAddWindowDirection(_monitor) == null ? Visibility.Collapsed : Visibility.Visible;

	/// <summary>
	/// The string representation of the current tree layout engine's add node direction.
	/// If the current workspace's active layout engine is not a tree layout engine, this will be
	/// <see langword="null"/>.
	/// </summary>
	public string? AddNodeDirection => _plugin.GetAddWindowDirection(_monitor)?.ToString();

	/// <summary>
	/// Command to toggle through the directions.
	/// </summary>
	public ToggleDirectionCommand ToggleDirectionCommand { get; }

	/// <inheritdoc />
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>
	/// Initializes a new instance of <see cref="TreeLayoutEngineWidgetViewModel"/>.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="plugin"></param>
	/// <param name="monitor"></param>
	public TreeLayoutEngineWidgetViewModel(IContext context, ITreeLayoutPlugin plugin, IMonitor monitor)
	{
		_context = context;
		_plugin = plugin;
		_monitor = monitor;
		ToggleDirectionCommand = new ToggleDirectionCommand(this);

		_context.Store.MapEvents.MonitorWorkspaceChanged += MapEvents_MonitorWorkspaceChanged;
		_context.Store.WorkspaceEvents.ActiveLayoutEngineChanged += WorkspaceEvents_ActiveLayoutEngineChanged;
		_plugin.AddWindowDirectionChanged += Plugin_AddWindowDirectionChanged;
	}

	private void MapEvents_MonitorWorkspaceChanged(object? sender, MonitorWorkspaceChangedEventArgs e)
	{
		if (e.Monitor.Equals(_monitor))
		{
			OnPropertyChanged(string.Empty);
		}
	}

	private void WorkspaceEvents_ActiveLayoutEngineChanged(object? sender, ActiveLayoutEngineChangedEventArgs e)
	{
		if (!_context.Store.Pick(Pickers.PickWorkspaceByMonitor(_monitor.Handle)).TryGet(out IWorkspace workspace))
		{
			Logger.Error($"Could not find workspace for monitor {_monitor.Handle}");
			return;
		}

		if (e.Workspace.Id == workspace.Id)
		{
			OnPropertyChanged(string.Empty);
		}
	}

	private void Plugin_AddWindowDirectionChanged(object? sender, AddWindowDirectionChangedEventArgs e) =>
		OnPropertyChanged(string.Empty);

	/// <summary>
	/// Toggle the <see cref="AddNodeDirection"/> in a clockwise direction.
	/// </summary>
	public void ToggleDirection()
	{
		Direction? direction = _plugin.GetAddWindowDirection(_monitor);

		if (direction == null)
		{
			Logger.Error("Tree layout engine not found");
			return;
		}

		Direction nextDirection;
		switch (direction)
		{
			case Direction.Left:
				nextDirection = Direction.Up;
				break;
			case Direction.Up:
				nextDirection = Direction.Right;
				break;
			case Direction.Right:
				nextDirection = Direction.Down;
				break;
			case Direction.Down:
				nextDirection = Direction.Left;
				break;
			default:
				Logger.Error($"We don't support adding windows in {direction}");
				nextDirection = Direction.Right;
				break;
		}

		_plugin.SetAddWindowDirection(_monitor, nextDirection);
	}

	/// <inheritdoc/>
	protected virtual void OnPropertyChanged(string? propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	/// <inheritdoc />
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_context.Store.MapEvents.MonitorWorkspaceChanged -= MapEvents_MonitorWorkspaceChanged;
				_context.Store.WorkspaceEvents.ActiveLayoutEngineChanged -= WorkspaceEvents_ActiveLayoutEngineChanged;
				_plugin.AddWindowDirectionChanged -= Plugin_AddWindowDirectionChanged;
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			_disposedValue = true;
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
