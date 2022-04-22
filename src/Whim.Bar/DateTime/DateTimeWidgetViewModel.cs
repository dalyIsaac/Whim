using Microsoft.UI.Xaml;
using System;
using System.ComponentModel;

namespace Whim.Bar;

public class DateTimeWidgetViewModel : INotifyPropertyChanged, IDisposable
{
	private readonly DispatcherTimer _timer = new();
	private readonly string _format;
	private bool disposedValue;

	public string Value => DateTime.Now.ToString(_format);

	public DateTimeWidgetViewModel(int interval, string format)
	{
		_format = format;
		_timer.Interval = TimeSpan.FromMilliseconds(interval);
		_timer.Tick += Timer_Tick;
		_timer.Start();
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private void Timer_Tick(object? sender, object e)
	{
		OnPropertyChanged(nameof(Value));
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_timer.Stop();
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
