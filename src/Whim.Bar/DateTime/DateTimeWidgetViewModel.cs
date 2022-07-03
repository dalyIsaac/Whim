using Microsoft.UI.Xaml;
using System;
using System.ComponentModel;

namespace Whim.Bar;

/// <summary>
/// View model containing the current date and time.
/// </summary>
public class DateTimeWidgetViewModel : INotifyPropertyChanged, IDisposable
{
	private readonly DispatcherTimer _timer = new();
	private readonly string _format;
	private bool disposedValue;

	/// <summary>
	/// The current date and time.
	/// </summary>
	public string Value => DateTime.Now.ToString(_format);

	/// <summary>
	/// Creates a new instance of <see cref="DateTimeWidgetViewModel"/>.
	/// </summary>
	/// <param name="interval">The interval to update the date and time, in milliseconds.</param>
	/// <param name="format">The format to use for the date and time.</param>
	public DateTimeWidgetViewModel(int interval, string format)
	{
		_format = format;
		_timer.Interval = TimeSpan.FromMilliseconds(interval);
		_timer.Tick += Timer_Tick;
		_timer.Start();
	}

	/// <inheritdoc/>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <inheritdoc/>
	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private void Timer_Tick(object? sender, object e)
	{
		OnPropertyChanged(nameof(Value));
	}

	/// <inheritdoc/>
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

	/// <inheritdoc/>
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
