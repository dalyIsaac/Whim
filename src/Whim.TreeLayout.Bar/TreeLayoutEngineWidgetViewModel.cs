using System;
using System.ComponentModel;

namespace Whim.TreeLayout.Bar;

/// <summary>
/// View model for the active tree layout engine on the given monitor.
/// </summary>
public class TreeLayoutEngineWidgetViewModel : INotifyPropertyChanged, IDisposable
{
	private readonly IConfigContext _configContext;
	private readonly IMonitor _monitor;
	private bool disposedValue;

	private Direction? _addNodeDirection;

	/// <summary>
	/// Shortcut to indicate if <see cref="AddNodeDirection"/> is <see langword="null"/>.
	/// </summary>
	public bool IsVisible => _addNodeDirection == null;

	/// <summary>
	/// The direction in which windows will be added. If this is <see langword="null"/>, then the
	/// monitor for this widget is not focused, or does not have a <see cref="TreeLayoutEngine"/>
	/// as the active layout engine.
	/// </summary>
	public Direction? AddNodeDirection
	{
		get => _addNodeDirection;
		set
		{
			if (_addNodeDirection != value)
			{
				_addNodeDirection = value;
				OnPropertyChanged(nameof(AddNodeDirection));
				OnPropertyChanged(nameof(IsVisible));
			}
		}
	}

	/// <summary>
	/// Command to toggle through the directions.
	/// </summary>
	public ToggleDirectionCommand ToggleDirectionCommand { get; }

	/// <inheritdoc />
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>
	/// Initializes a new instance of <see cref="TreeLayoutEngineWidgetViewModel"/>.
	/// </summary>
	/// <param name="configContext"></param>
	/// <param name="monitor"></param>
	public TreeLayoutEngineWidgetViewModel(IConfigContext configContext, IMonitor monitor)
	{
		_configContext = configContext;
		_monitor = monitor;
		ToggleDirectionCommand = new ToggleDirectionCommand(this);

		_configContext.WorkspaceManager.MonitorWorkspaceChanged += WorkspaceManager_MonitorWorkspaceChanged;
		_configContext.WorkspaceManager.ActiveLayoutEngineChanged += WorkspaceManager_ActiveLayoutEngineChanged;
	}

	private void WorkspaceManager_MonitorWorkspaceChanged(object? sender, MonitorWorkspaceChangedEventArgs e)
	{
		if (e.Monitor != _monitor || _configContext.MonitorManager.FocusedMonitor != _monitor)
		{
			AddNodeDirection = null;
			return;
		}

		UpdateNodeDirection();
	}

	private void WorkspaceManager_ActiveLayoutEngineChanged(object? sender, ActiveLayoutEngineChangedEventArgs e)
	{
		if (_monitor != _configContext.MonitorManager.FocusedMonitor)
		{
			AddNodeDirection = null;
			return;
		}

		UpdateNodeDirection();
	}

	private void UpdateNodeDirection()
	{
		ILayoutEngine rootEngine = _configContext.WorkspaceManager.ActiveWorkspace.ActiveLayoutEngine;
		TreeLayoutEngine? engine = ILayoutEngine.GetLayoutEngine<TreeLayoutEngine>(rootEngine);

		if (engine is null)
		{
			AddNodeDirection = null;
			return;
		}

		AddNodeDirection = engine.AddNodeDirection;
	}

	/// <summary>
	/// Toggle the <see cref="AddNodeDirection"/> in a clockwise direction.
	/// </summary>
	public void ToggleDirection()
	{
		if (AddNodeDirection == null)
		{
			return;
		}

		ILayoutEngine rootEngine = _configContext.WorkspaceManager.ActiveWorkspace.ActiveLayoutEngine;
		TreeLayoutEngine? engine = ILayoutEngine.GetLayoutEngine<TreeLayoutEngine>(rootEngine);

		if (engine is null)
		{
			AddNodeDirection = null;
			return;
		}

		switch (AddNodeDirection)
		{
			case Direction.Left:
				engine.AddNodeDirection = Direction.Up;
				break;
			case Direction.Up:
				engine.AddNodeDirection = Direction.Right;
				break;
			case Direction.Right:
				engine.AddNodeDirection = Direction.Down;
				break;
			case Direction.Down:
				engine.AddNodeDirection = Direction.Left;
				break;
			default:
				Logger.Error("Invalid direction");
				break;
		}

		AddNodeDirection = engine.AddNodeDirection;
	}

	/// <inheritdoc/>
	protected virtual void OnPropertyChanged(string? propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	/// <inheritdoc />
	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				// TODO: dispose managed state (managed objects)
			}

			// TODO: free unmanaged resources (unmanaged objects) and override finalizer
			// TODO: set large fields to null
			disposedValue = true;
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
