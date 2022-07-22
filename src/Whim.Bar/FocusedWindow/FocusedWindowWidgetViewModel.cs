using System;
using System.ComponentModel;

namespace Whim.Bar;

/// <summary>
/// View model containing the focused window.
/// </summary>
public class FocusedWindowWidgetViewModel : INotifyPropertyChanged, IDisposable
{
	private readonly IConfigContext _configContext;
	private bool _disposedValue;

	/// <summary>
	/// The title of the last focused window.
	/// </summary>
	public string? Value => _configContext.WorkspaceManager.ActiveWorkspace.LastFocusedWindow?.Title;

	/// <summary>
	/// Creates a new instance of the view model <see cref="FocusedWindowWidgetViewModel"/>.
	/// </summary>
	/// <param name="configContext"></param>
	public FocusedWindowWidgetViewModel(IConfigContext configContext)
	{
		_configContext = configContext;
		_configContext.WindowManager.WindowFocused += WindowManager_WindowFocused;
	}

	/// <inheritdoc/>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <inheritdoc/>
	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	/// <inheritdoc/>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_configContext.WindowManager.WindowFocused -= WindowManager_WindowFocused;
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
		OnPropertyChanged(nameof(Value));
	}
}
