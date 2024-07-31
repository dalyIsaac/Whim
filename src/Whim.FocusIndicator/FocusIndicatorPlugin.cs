using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Windows.Win32.Foundation;

namespace Whim.FocusIndicator;

/// <inheritdoc/>
public class FocusIndicatorPlugin : IFocusIndicatorPlugin
{
	private bool _isEnabled = true;
	private readonly IContext _context;
	private readonly FocusIndicatorConfig _focusIndicatorConfig;
	private readonly CancellationToken _cancellationToken = new();
	private FocusIndicatorWindow? _focusIndicatorWindow;
	private DispatcherTimer? _dispatcherTimer;
	private bool _disposedValue;

	/// <summary>
	/// <c>whim.focus_indicator</c>
	/// </summary>
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
		_context.FilterManager.AddTitleMatchFilter(FocusIndicatorConfig.Title);
	}

	/// <inheritdoc/>
	public void PostInitialize()
	{
		// The window must be created on the UI thread (so don't do it in the constructor).
		_focusIndicatorWindow = new FocusIndicatorWindow(_context, _focusIndicatorConfig);

		// Activate the window so it renders.
		_focusIndicatorWindow.Activate();
		_focusIndicatorWindow.Hide(_context);

		// TODO: Start loop.
		Task.Factory.StartNew(
			LongRunningTask,
			_cancellationToken,
			TaskCreationOptions.LongRunning,
			TaskScheduler.Default
		);
	}

	private void LongRunningTask()
	{
		while (true)
		{
			// TODO: Get the focused window.
			Show();
			Thread.Sleep(10);
		}
	}

	// TODO: Hide
	// private void DispatcherTimer_Tick(object? sender, object e)
	// {
	// 	Logger.Verbose("Focus indicator timer ticked");
	// 	Hide();
	// }

	/// <inheritdoc/>
	public void Show(IWindow? window = null)
	{
		Logger.Verbose("Showing focus indicator");

		HWND handle = window?.Handle ?? default;
		if (handle == default)
		{
			if (_context.Store.Pick(Pickers.PickLastFocusedWindowHandle()).TryGet(out HWND hwnd))
			{
				handle = hwnd;
			}
			else
			{
				Logger.Verbose("No last focused window to show focus indicator for");
				Hide();
				return;
			}
		}

		IRectangle<int>? rect = _context.NativeManager.DwmGetWindowRectangle(handle);
		if (rect == null)
		{
			Logger.Error($"Could not find window rectangle for window {handle}");
			Hide();
			return;
		}

		// Get the window rectangle.

		// Get the window rectangle.
		// IWindowState? windowRect = activeWorkspace.TryGetWindowState(window);
		// if (windowRect == null)
		// {
		// 	Logger.Error($"Could not find window rectangle for window {window}");
		// 	Hide();
		// 	return;
		// }

		// if (windowRect.WindowSize == WindowSize.Minimized)
		// {
		// 	Logger.Verbose($"Window {window} is minimized");
		// 	Hide();
		// 	return;
		// }

		IsVisible = true;
		_focusIndicatorWindow?.Activate(handle, rect);

		// If the fade is enabled, start the timer.
		// if (_focusIndicatorConfig.FadeEnabled)
		// {
		// 	_dispatcherTimer?.Stop();

		// 	_dispatcherTimer = new DispatcherTimer();
		// 	_dispatcherTimer.Tick += DispatcherTimer_Tick;
		// 	_dispatcherTimer.Interval = _focusIndicatorConfig.FadeTimeout;
		// 	_dispatcherTimer.Start();
		// }
	}

	/// <inheritdoc/>
	private void Hide()
	{
		Logger.Verbose("Hiding focus indicator");
		_focusIndicatorWindow?.Hide(_context);
		IsVisible = false;

		// if (_dispatcherTimer != null)
		// {
		// 	_dispatcherTimer.Stop();
		// 	_dispatcherTimer.Tick -= DispatcherTimer_Tick;
		// }
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
				_focusIndicatorWindow?.Dispose();
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
