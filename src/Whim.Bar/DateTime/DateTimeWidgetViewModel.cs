using System;
using System.ComponentModel;
using System.Timers;

namespace Whim.Bar;

public class DateTimeWidgetViewModel : INotifyPropertyChanged, IDisposable
{
	private readonly Timer _timer;
	private string _format;
	private bool disposedValue;

	public string Value { get => DateTime.Now.ToString(_format); }

	public DateTimeWidgetViewModel(int interval, string format)
	{
		_format = format;
		_timer = new Timer(interval);
		_timer.Elapsed += Timer_Elapsed;
		_timer.Start();
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
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
				_timer.Dispose();
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
