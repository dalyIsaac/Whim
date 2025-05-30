using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;

namespace Whim.Bar;

/// <summary>
/// View model containing the active layout.
/// </summary>
internal class ActiveLayoutWidgetViewModel : INotifyPropertyChanged, IDisposable
{
	private readonly IContext _ctx;

	/// <summary>
	/// The monitor that the widget is displayed on.
	/// </summary>
	public IMonitor Monitor { get; }

	private bool _disposedValue;

	private ImmutableList<string> _layoutEngines = ImmutableList<string>.Empty;

	/// <summary>
	/// The available layout engines.
	/// </summary>
	public IReadOnlyList<string> LayoutEngines => _layoutEngines;

	/// <summary>
	/// The name of the active layout engine.
	/// </summary>
	public string ActiveLayoutEngine
	{
		get
		{
			if (
				_ctx
					.Store.Pick(Pickers.PickActiveLayoutEngineByMonitor(Monitor.Handle))
					.TryGet(out ILayoutEngine layoutEngine)
			)
			{
				return layoutEngine.Name;
			}

			return "";
		}
		set
		{
			if (!_ctx.Store.Pick(Pickers.PickWorkspaceByMonitor(Monitor.Handle)).TryGet(out IWorkspace workspace))
			{
				return;
			}

			ILayoutEngine layoutEngine = WorkspaceUtils.GetActiveLayoutEngine(workspace);

			if (layoutEngine.Name != value)
			{
				_ctx.Store.WhimDispatch(new SetLayoutEngineFromNameTransform(workspace.Id, value));
				OnPropertyChanged(nameof(ActiveLayoutEngine));
			}
		}
	}

	private bool _isDropDownOpen;

	/// <summary>
	/// Whether the combobox's drop down is open.
	/// </summary>
	public bool IsDropDownOpen
	{
		get => _isDropDownOpen;
		set
		{
			_isDropDownOpen = value;
			OnPropertyChanged(nameof(IsDropDownOpen));
		}
	}

	/// <summary>
	/// Creates a new instance of the <see cref="ActiveLayoutWidgetViewModel"/> class.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="monitor"></param>
	public ActiveLayoutWidgetViewModel(IContext context, IMonitor monitor)
	{
		_ctx = context;
		Monitor = monitor;

		_ctx.Store.WorkspaceEvents.ActiveLayoutEngineChanged += Store_ActiveLayoutEngineChanged;
		_ctx.Store.MapEvents.MonitorWorkspaceChanged += Store_MonitorWorkspaceChanged;
		_ctx.Store.WindowEvents.WindowFocused += WindowEvents_WindowFocused;
	}

	private void Store_ActiveLayoutEngineChanged(object? sender, ActiveLayoutEngineChangedEventArgs e)
	{
		OnPropertyChanged(nameof(ActiveLayoutEngine));
	}

	private void Store_MonitorWorkspaceChanged(object? sender, MonitorWorkspaceChangedEventArgs e)
	{
		if (e.Monitor.Handle != Monitor.Handle)
		{
			return;
		}

		_layoutEngines = e.CurrentWorkspace.LayoutEngines.Select(engine => engine.Name).ToImmutableList();

		OnPropertyChanged(nameof(LayoutEngines));
		OnPropertyChanged(nameof(ActiveLayoutEngine));
	}

	private void WindowEvents_WindowFocused(object? sender, WindowFocusedEventArgs e)
	{
		IsDropDownOpen = false;
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
				_ctx.Store.WorkspaceEvents.ActiveLayoutEngineChanged -= Store_ActiveLayoutEngineChanged;
				_ctx.Store.MapEvents.MonitorWorkspaceChanged -= Store_MonitorWorkspaceChanged;
				_ctx.Store.WindowEvents.WindowFocused -= WindowEvents_WindowFocused;
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
