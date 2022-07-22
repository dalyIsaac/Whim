using System;
using System.Collections.Generic;

namespace Whim.Bar;

/// <summary>
/// BarPlugin displays an interactive bar at the top of the screen for Whim. It can be configured
/// with various <see cref="BarComponent"/>s to display on the left, center, and right sides of the bar.
/// </summary>
public class BarPlugin : IPlugin, IDisposable
{
	private readonly IConfigContext _configContext;
	private readonly BarConfig _barConfig;

	private readonly Dictionary<IMonitor, BarWindow> _monitorBarMap = new();
	private bool _disposedValue;

	/// <summary>
	/// Create the bar plugin.
	/// </summary>
	/// <param name="configContext"></param>
	/// <param name="barConfig"></param>
	public BarPlugin(IConfigContext configContext, BarConfig barConfig)
	{
		_configContext = configContext;
		_barConfig = barConfig;
	}

	/// <inheritdoc />
	public void PreInitialize()
	{
		_configContext.MonitorManager.MonitorsChanged += MonitorManager_MonitorsChanged;
		_configContext.FilterManager.IgnoreTitleMatch("Whim Bar");
		_configContext.WorkspaceManager.AddProxyLayoutEngine(layout => new BarLayoutEngine(_barConfig, layout));
	}

	/// <inheritdoc />
	public void PostInitialize()
	{
		foreach (IMonitor monitor in _configContext.MonitorManager)
		{
			BarWindow barWindow = new(_configContext, _barConfig, monitor);
			_monitorBarMap.Add(monitor, barWindow);
		}

		ShowAll();
	}

	private void MonitorManager_MonitorsChanged(object? sender, MonitorsChangedEventArgs e)
	{
		// Remove the removed monitors
		foreach (IMonitor monitor in e.RemovedMonitors)
		{
			_monitorBarMap[monitor]?.Close();
			_monitorBarMap.Remove(monitor);
		}

		// Add the new monitors
		foreach (IMonitor monitor in e.AddedMonitors)
		{
			BarWindow barWindow = new(_configContext, _barConfig, monitor);
			_monitorBarMap.Add(monitor, barWindow);
		}

		ShowAll();
	}

	/// <summary>
	/// Show all the bar windows.
	/// </summary>
	private void ShowAll()
	{
		using WindowDeferPosHandle deferPosHandle = new(_monitorBarMap.Count);

		foreach (BarWindow barWindow in _monitorBarMap.Values)
		{
			deferPosHandle.DeferWindowPos(barWindow.WindowState);
		}
	}

	/// <inheritdoc />
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				foreach (BarWindow barWindow in _monitorBarMap.Values)
				{
					barWindow.Close();
				}

				_monitorBarMap.Clear();
				_configContext.MonitorManager.MonitorsChanged -= MonitorManager_MonitorsChanged;
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			_disposedValue = true;
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
