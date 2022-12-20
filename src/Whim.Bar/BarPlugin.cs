﻿using System;
using System.Collections.Generic;
using Windows.Win32.Graphics.Dwm;

namespace Whim.Bar;

/// <inheritdoc/>
public class BarPlugin : IBarPlugin
{
	private readonly IConfigContext _configContext;
	private readonly BarConfig _barConfig;

	private readonly Dictionary<IMonitor, BarWindow> _monitorBarMap = new();
	private bool _disposedValue;

	/// <inheritdoc />
	public string Name => "whim.bar";

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
		using WindowDeferPosHandle deferPosHandle = new(_configContext, _monitorBarMap.Count);

		foreach (BarWindow barWindow in _monitorBarMap.Values)
		{
			barWindow.UpdateLocation();
			deferPosHandle.DeferWindowPos(barWindow.WindowState);
			_configContext.NativeManager.SetWindowCorners(
				barWindow.WindowState.Window.Handle,
				DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_DONOTROUND
			);
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

	/// <inheritdoc />
	public IEnumerable<CommandItem> Commands { get; } = Array.Empty<CommandItem>();
}
