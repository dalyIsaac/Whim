using System;
using System.Collections.Generic;
using System.Text.Json;
using Windows.Win32.Graphics.Dwm;

namespace Whim.Bar;

/// <inheritdoc/>
/// <summary>
/// Create the bar plugin.
/// </summary>
/// <param name="context"></param>
/// <param name="barConfig"></param>
public class BarPlugin(IContext context, BarConfig barConfig) : IBarPlugin
{
	private readonly IContext _context = context;
	private readonly BarConfig _barConfig = barConfig;

	private readonly Dictionary<IMonitor, BarWindow> _monitorBarMap = [];
	private bool _disposedValue;

	/// <summary>
	/// <c>whim.bar</c>
	/// </summary>
	public string Name => "whim.bar";

	/// <inheritdoc />
	public void PreInitialize()
	{
		_context.MonitorManager.MonitorsChanged += MonitorManager_MonitorsChanged;
		_context.FilterManager.AddTitleMatchFilter("Whim Bar");
		_context.WorkspaceManager.AddProxyLayoutEngine(layout => new BarLayoutEngine(_barConfig, layout));
	}

	/// <inheritdoc />
	public void PostInitialize()
	{
		foreach (IMonitor monitor in _context.MonitorManager)
		{
			BarWindow barWindow = new(_context, _barConfig, monitor);
			_monitorBarMap[monitor] = barWindow;
		}

		ShowAll();
	}

	private void MonitorManager_MonitorsChanged(object? sender, MonitorsChangedEventArgs e)
	{
		// Remove the removed monitors
		foreach (IMonitor monitor in e.RemovedMonitors)
		{
			_monitorBarMap.TryGetValue(monitor, out BarWindow? value);
			_context.NativeManager.TryEnqueue(() => value?.Close());
			_monitorBarMap.Remove(monitor);
		}

		// Add the new monitors
		foreach (IMonitor monitor in e.AddedMonitors)
		{
			BarWindow barWindow = new(_context, _barConfig, monitor);
			_monitorBarMap[monitor] = barWindow;
		}

		ShowAll();
	}

	/// <summary>
	/// Show all the bar windows.
	/// </summary>
	private void ShowAll()
	{
		using DeferWindowPosHandle deferPosHandle = _context.NativeManager.DeferWindowPos();
		foreach (BarWindow barWindow in _monitorBarMap.Values)
		{
			barWindow.UpdateRect();
			IWindowState state = barWindow.WindowState;

			deferPosHandle.DeferWindowPos(
				new DeferWindowPosState(state.Window.Handle, state.WindowSize, state.Rectangle),
				forceTwoPasses: true
			);
			_context.NativeManager.SetWindowCorners(
				state.Window.Handle,
				DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_DONOTROUND
			);
		}
	}

	/// <inheritdoc />
	public void LoadState(JsonElement pluginSavedState) { }

	/// <inheritdoc />
	public JsonElement? SaveState() => null;

	/// <inheritdoc />
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				foreach (BarWindow barWindow in _monitorBarMap.Values)
				{
					barWindow.Dispose();
					barWindow.Close();
				}

				_monitorBarMap.Clear();
				_context.MonitorManager.MonitorsChanged -= MonitorManager_MonitorsChanged;
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
	public IPluginCommands PluginCommands => new PluginCommands(Name);
}
