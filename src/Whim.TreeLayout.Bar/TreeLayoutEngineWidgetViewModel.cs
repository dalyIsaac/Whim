using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whim.TreeLayout.Bar;

/// <summary>
/// View model for the active tree layout engine on the given monitor.
/// </summary>
public class TreeLayoutEngineWidgetViewModel : INotifyPropertyChanged, IDisposable
{
	private readonly IConfigContext _configContext;
	private bool disposedValue;

	/// <summary>
	/// The monitor for this widget.
	/// </summary>
	public IMonitor Monitor { get; }

	/// <inheritdoc />
	public event PropertyChangedEventHandler? PropertyChanged;

	public TreeLayoutEngineWidgetViewModel(IConfigContext configContext, IMonitor monitor)
	{
		_configContext = configContext;
		Monitor = monitor;

		// TODO: subscribe to focused monitor changed event.
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
