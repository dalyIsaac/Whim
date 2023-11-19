using System;
using System.Text.Json;
using Microsoft.UI.Xaml;

namespace Whim.FocusIndicator;

/// <inheritdoc/>
public class FocusIndicatorPlugin : IFocusIndicatorPlugin
{
	private bool _isEnabled = true;
	private readonly IContext _context;
	private readonly FocusIndicatorConfig _focusIndicatorConfig;
	private FocusIndicatorWindow? _focusIndicatorWindow;
	private DispatcherTimer? _dispatcherTimer;
	private bool _disposedValue;

	/// <inheritdoc />
	public string Name => "whim.focus_indicator";

	/// <inheritdoc />
	public bool IsVisible { get; private set; }

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

		_context.WindowManager.WindowMoveStart += WindowManager_WindowMoveStart;
		_context.WindowManager.WindowFocused += WindowManager_WindowFocused;
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
		_context.WorkspaceManager.WorkspaceLayoutStarted += WorkspaceManager_WorkspaceLayoutStarted;
		_context.WorkspaceManager.WorkspaceLayoutCompleted += WorkspaceManager_WorkspaceLayoutCompleted;
	}

	private void DispatcherTimer_Tick(object? sender, object e) => Hide();

	private void WindowManager_WindowMoveStart(object? sender, WindowMovedEventArgs e) => Hide();

	private void WindowManager_WindowFocused(object? sender, WindowFocusedEventArgs e)
	{
		if (!_isEnabled)
		{
			Logger.Debug("Focus indicator is disabled");
			return;
		}

		if (e.Window == null)
		{
			Hide();
			return;
		}

		Show();
	}

	private void WorkspaceManager_WorkspaceLayoutStarted(object? sender, WorkspaceEventArgs e) => Hide();

	private void WorkspaceManager_WorkspaceLayoutCompleted(object? sender, WorkspaceEventArgs e) => Show();

	/// <inheritdoc/>
	public void Show(IWindow? window = null)
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
		IWindowState? windowLocation = activeWorkspace.TryGetWindowState(window);
		if (windowLocation == null)
		{
			Logger.Error($"Could not find window location for window {window}");
			return;
		}

		IsVisible = true;
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

	/// <inheritdoc/>
	private void Hide()
	{
		Logger.Debug("Hiding focus indicator");
		_focusIndicatorWindow?.Hide(_context);
		IsVisible = false;

		if (_dispatcherTimer != null)
		{
			_dispatcherTimer.Stop();
			_dispatcherTimer.Tick -= DispatcherTimer_Tick;
		}
	}

	/// <inheritdoc/>
	public void Toggle()
	{
		if (IsVisible)
		{
			Hide();
		}
		else
		{
			Show();
		}
	}

	/// <inheritdoc/>
	public void ToggleFade() => _focusIndicatorConfig.FadeEnabled = !_focusIndicatorConfig.FadeEnabled;

	/// <inheritdoc/>
	public void ToggleEnabled()
	{
		_isEnabled = !_isEnabled;
		if (_isEnabled)
		{
			Show();
		}
		else
		{
			Hide();
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
				_context.WindowManager.WindowFocused -= WindowManager_WindowFocused;
				_context.WorkspaceManager.WorkspaceLayoutStarted -= WorkspaceManager_WorkspaceLayoutStarted;
				_context.WorkspaceManager.WorkspaceLayoutCompleted -= WorkspaceManager_WorkspaceLayoutCompleted;
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
	public IPluginCommands PluginCommands => new FocusIndicatorCommands(this);

	/// <inheritdoc />
	public void LoadState(JsonElement state) { }

	/// <inheritdoc />
	public JsonElement? SaveState() => null;
}
