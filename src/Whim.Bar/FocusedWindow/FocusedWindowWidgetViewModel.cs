using System;
using System.ComponentModel;

namespace Whim.Bar;

public class FocusedWindowWidgetViewModel : INotifyPropertyChanged, IDisposable
{
	private readonly IConfigContext _configContext;
	private bool disposedValue;

	public string? Value { get => _configContext.WorkspaceManager.ActiveWorkspace.LastFocusedWindow?.Title; }

	public FocusedWindowWidgetViewModel(IConfigContext configContext)
	{
		_configContext = configContext;
		_configContext.WindowManager.WindowFocused += WindowManager_WindowFocused;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged(string propertyName)
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
				_configContext.WindowManager.WindowFocused -= WindowManager_WindowFocused;
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

	private void WindowManager_WindowFocused(object? sender, WindowEventArgs e)
	{
		OnPropertyChanged(nameof(Value));
	}
}
