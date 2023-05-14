using Microsoft.UI.Xaml;
using System;
using System.Text.Json;

namespace Whim.FocusIndicator;

/// <inheritdoc/>
public class FocusIndicatorPlugin : IFocusIndicatorPlugin
{
	private readonly IContext _context;
	private readonly FocusIndicatorConfig _focusIndicatorConfig;
	private FocusIndicatorWindow? _focusIndicatorWindow;
	private DispatcherTimer? _dispatcherTimer;
	private bool _disposedValue;

	/// <inheritdoc />
	public string Name => "whim.focus_indicator";

	/// <summary>
	/// Creates a new instance of the focus indicator plugin.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="focusIndicatorConfig"></param>
	public FocusIndicatorPlugin(IContext context, FocusIndicatorConfig focusIndicatorConfig)
	{
		_context = context;
		_focusIndicatorConfig = focusIndicatorConfig;
	}

	/// <inheritdoc/>
	public void PreInitialize()
	{
		_context.FilterManager.IgnoreTitleMatch(FocusIndicatorConfig.Title);

		_context.WindowManager.WindowFocused += WindowManager_EventSink_Show;
		_context.WindowManager.WindowAdded += WindowManager_EventSink_Show;
		_context.WindowManager.WindowRemoved += WindowManager_EventSink_Show;
		_context.WindowManager.WindowMoveStart += WindowManager_EventSink_Hide;
		_context.WindowManager.WindowMinimizeStart += WindowManager_EventSink_Hide;
		_context.WindowManager.WindowMinimizeEnd += WindowManager_EventSink_Show;
	}

	/// <inheritdoc/>
	public void PostInitialize()
	{
		// The window must be created on the UI thread (so don't do it in the constructor).
		_focusIndicatorWindow = new FocusIndicatorWindow(_context, _focusIndicatorConfig);

		// Activate the window so it renders.
		_focusIndicatorWindow.Activate();
		_focusIndicatorWindow.Hide(_context);

		// Only subscribe to workspace changes once the indicator window has been created - we shouldn't
		// show a window which doesn't yet exist (it'll just crash Whim).
		_context.WorkspaceManager.MonitorWorkspaceChanged += WorkspaceManager_MonitorWorkspaceChanged;
	}

	private void DispatcherTimer_Tick(object? sender, object e) => Hide();

	private void WindowManager_EventSink_Show(object? sender, WindowEventArgs e) => Show();

	private void WindowManager_EventSink_Hide(object? sender, WindowEventArgs e) => Hide();

	private void WorkspaceManager_MonitorWorkspaceChanged(object? sender, MonitorWorkspaceChangedEventArgs e) => Show();

	private void Show(IWindow? window = null)
	{
		Logger.Debug("Showing focus indicator");
		IWorkspace activeWorkspace = _context.WorkspaceManager.ActiveWorkspace;
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
			_dispatcherTimer?.Stop();

			_dispatcherTimer = new DispatcherTimer();
			_dispatcherTimer.Tick += DispatcherTimer_Tick;
			_dispatcherTimer.Interval = _focusIndicatorConfig.FadeTimeout;
			_dispatcherTimer.Start();
		}
	}

	private void Hide()
	{
		Logger.Debug("Hiding focus indicator");
		_focusIndicatorWindow?.Hide(_context);
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
				_context.WindowManager.WindowFocused -= WindowManager_EventSink_Show;
				_context.WindowManager.WindowAdded -= WindowManager_EventSink_Show;
				_context.WindowManager.WindowRemoved -= WindowManager_EventSink_Show;
				_context.WindowManager.WindowMoveStart -= WindowManager_EventSink_Show;
				_context.WindowManager.WindowMinimizeStart -= WindowManager_EventSink_Show;
				_context.WindowManager.WindowMinimizeEnd -= WindowManager_EventSink_Show;
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

	/// <inheritdoc />
	public IPluginCommands PluginCommands => new PluginCommands(Name);

	/// <inheritdoc />
	public void LoadState(JsonElement state) { }

	/// <inheritdoc />
	public JsonElement? SaveState() => null;
}
