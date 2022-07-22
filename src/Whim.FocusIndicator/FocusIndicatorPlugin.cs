using Microsoft.UI.Xaml;
using System;
using System.ComponentModel;

namespace Whim.FocusIndicator;

/// <summary>
/// FocusIndicatorPlugin is the plugin that displays a focus indicator on the focused window.
/// </summary>
public class FocusIndicatorPlugin : IPlugin, IDisposable
{
	private readonly IConfigContext _configContext;
	private readonly FocusIndicatorConfig _focusIndicatorConfig;
	private FocusIndicatorWindow? _focusIndicatorWindow;
	private DispatcherTimer? _dispatcherTimer;
	private bool _disposedValue;

	/// <summary>
	/// Creates a new instance of the focus indicator plugin.
	/// </summary>
	/// <param name="configContext"></param>
	/// <param name="focusIndicatorConfig"></param>
	public FocusIndicatorPlugin(IConfigContext configContext, FocusIndicatorConfig focusIndicatorConfig)
	{
		_configContext = configContext;
		_focusIndicatorConfig = focusIndicatorConfig;
	}

	/// <inheritdoc/>
	public void PreInitialize()
	{
		_configContext.FilterManager.IgnoreTitleMatch(FocusIndicatorConfig.Title);

		_focusIndicatorConfig.PropertyChanged += FocusIndicatorConfig_PropertyChanged;

		_configContext.WindowManager.WindowFocused += WindowManager_WindowFocused;

		_configContext.WindowManager.WindowRegistered += WindowManager_EventSink;
		_configContext.WindowManager.WindowUnregistered += WindowManager_EventSink;
		_configContext.WindowManager.WindowMoveStart += WindowManager_EventSink;
		_configContext.WindowManager.WindowMoved += WindowManager_EventSink;
		_configContext.WindowManager.WindowMinimizeStart += WindowManager_EventSink;
		_configContext.WindowManager.WindowMinimizeEnd += WindowManager_EventSink;
	}

	/// <inheritdoc/>
	public void PostInitialize()
	{
		// The window must be created on the UI thread (so don't do it in the constructor).
		_focusIndicatorWindow = new FocusIndicatorWindow(_configContext, _focusIndicatorConfig);

		// Activate the window so it renders.
		_focusIndicatorWindow.Activate();
		_focusIndicatorWindow.Hide();
	}

	private void WindowManager_WindowFocused(object? sender, WindowEventArgs e)
	{
		_focusIndicatorConfig.IsVisible = true;
	}

	private void DispatcherTimer_Tick(object? sender, object e)
	{
		_focusIndicatorConfig.IsVisible = false;
	}

	private void WindowManager_EventSink(object? sender, WindowEventArgs e)
	{
		Show();
	}

	private void FocusIndicatorConfig_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(FocusIndicatorConfig.IsVisible))
		{
			if (_focusIndicatorConfig.IsVisible)
			{
				Show();
			}
			else
			{
				Hide();
			}
		}
	}

	private void Show(IWindow? window = null)
	{
		Logger.Debug("Showing focus indicator");
		IWorkspace activeWorkspace = _configContext.WorkspaceManager.ActiveWorkspace;
		window ??= activeWorkspace.LastFocusedWindow;
		if (window == null)
		{
			Logger.Debug("No window to show focus indicator for");
			Hide();
			return;
		}

		// Get the window location.
		IWindowState? windowLocation = activeWorkspace.TryGetWindowLocation(window);
		if (windowLocation == null)
		{
			Logger.Error($"Could not find window location for window {window}");
			return;
		}

		_focusIndicatorWindow?.Activate(windowLocation);

		// If the fade is enabled, start the timer.
		if (_focusIndicatorConfig.FadeEnabled)
		{
			if (_dispatcherTimer != null)
			{
				_dispatcherTimer.Stop();
			}

			_dispatcherTimer = new DispatcherTimer();
			_dispatcherTimer.Tick += DispatcherTimer_Tick;
			_dispatcherTimer.Interval = _focusIndicatorConfig.FadeTimeout;
			_dispatcherTimer.Start();
		}
	}

	private void Hide()
	{
		Logger.Debug("Hiding focus indicator");
		_focusIndicatorWindow?.Hide();
		if (_dispatcherTimer != null)
		{
			_dispatcherTimer.Stop();
			_dispatcherTimer.Tick -= DispatcherTimer_Tick;
		}
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
				_configContext.WindowManager.WindowRegistered -= WindowManager_EventSink;
				_configContext.WindowManager.WindowUnregistered -= WindowManager_EventSink;
				_configContext.WindowManager.WindowMoveStart -= WindowManager_EventSink;
				_configContext.WindowManager.WindowMoved -= WindowManager_EventSink;
				_configContext.WindowManager.WindowMinimizeStart -= WindowManager_EventSink;
				_configContext.WindowManager.WindowMinimizeEnd -= WindowManager_EventSink;
				_focusIndicatorWindow?.Close();
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
}
