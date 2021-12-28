using System;
using System.Collections.Generic;

namespace Whim.Bar;
public class BarPlugin : IPlugin, IDisposable
{
	private readonly IConfigContext _configContext;
	private readonly BarConfig _barConfig;

	private readonly Dictionary<IMonitor, BarWindow> _monitorBarMap = new();
	private bool disposedValue;

	public BarPlugin(IConfigContext configContext, BarConfig barConfig)
	{
		_configContext = configContext;
		_barConfig = barConfig;
		_configContext.MonitorManager.MonitorsChanged += MonitorManager_MonitorsChanged;
	}

	public void Initialize()
	{
		foreach (IMonitor monitor in _configContext.MonitorManager)
		{
			BarWindow barWindow = new(_configContext, _barConfig, monitor);
			barWindow.Render();
			_monitorBarMap.Add(monitor, barWindow);
		}
	}

	private void MonitorManager_MonitorsChanged(object? sender, MonitorEventArgs e)
	{
		// Remove the removed monitors
		foreach (IMonitor monitor in e.RemovedMonitors)
		{
			_monitorBarMap[monitor].Close();
			_monitorBarMap.Remove(monitor);
		}

		// Add the new monitors
		foreach (IMonitor monitor in e.AddedMonitors)
		{
			BarWindow barWindow = new(_configContext, _barConfig, monitor);
			_monitorBarMap.Add(monitor, barWindow);
		}

		// Show all windows
		foreach (BarWindow barWindow in _monitorBarMap.Values)
		{
			barWindow.Render();
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				foreach (BarWindow barWindow in _monitorBarMap.Values)
				{
					barWindow.Close();
				}

				_monitorBarMap.Clear();
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
