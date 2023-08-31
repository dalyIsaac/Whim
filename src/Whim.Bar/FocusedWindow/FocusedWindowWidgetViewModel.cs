using System;
using System.ComponentModel;

namespace Whim.Bar;

/// <summary>
/// View model containing the focused window.
/// </summary>
internal class FocusedWindowWidgetViewModel : INotifyPropertyChanged, IDisposable
{
	private readonly IContext _context;
	private bool _disposedValue;
	private readonly IMonitor _monitor;
	private readonly Func<IWindow, string> _getTitle;

	private string? _title;

	/// <summary>
	/// The title of the last focused window.
	/// </summary>
	public string? Title
	{
		private set
		{
			if (_title != value)
			{
				_title = value;
				OnPropertyChanged(nameof(Title));
			}
		}
		get => _title;
	}

	/// <summary>
	/// Creates a new instance of the view model <see cref="FocusedWindowWidgetViewModel"/>.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="monitor"></param>
	/// <param name="getTitle">The function to get the title of the window.</param>
	public FocusedWindowWidgetViewModel(IContext context, IMonitor monitor, Func<IWindow, string> getTitle)
	{
		_context = context;
		_monitor = monitor;
		_getTitle = getTitle;
		_context.WindowManager.WindowFocused += WindowManager_WindowFocused;
	}

	/// <inheritdoc/>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <inheritdoc/>
	protected virtual void OnPropertyChanged(string propertyName) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

	/// <inheritdoc/>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_context.WindowManager.WindowFocused -= WindowManager_WindowFocused;
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

	private void WindowManager_WindowFocused(object? sender, WindowEventArgs e)
	{
		IMonitor? monitor = _context.WorkspaceManager.GetMonitorForWindow(e.Window);

		if (_monitor.Equals(monitor))
		{
			Title = _getTitle(e.Window);
		}
		else
		{
			Title = null;
		}
	}
}
