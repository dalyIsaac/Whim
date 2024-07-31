using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.Win32.Foundation;

namespace Whim.FocusIndicator;

/// <inheritdoc/>
public class FocusIndicatorPlugin : IFocusIndicatorPlugin
{
	private bool _isEnabled = true;
	private readonly IContext _context;
	private readonly FocusIndicatorConfig _focusIndicatorConfig;
	private readonly CancellationTokenSource _cancellationTokenSource;
	private readonly CancellationToken _cancellationToken;
	private FocusIndicatorWindow? _focusIndicatorWindow;
	private int _lastFocusStartTime;
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

		_cancellationTokenSource = new CancellationTokenSource();
		_cancellationToken = _cancellationTokenSource.Token;
	}

	/// <inheritdoc/>
	public void PreInitialize()
	{
		_context.FilterManager.AddTitleMatchFilter(FocusIndicatorConfig.Title);
		_context.WindowManager.WindowFocused += WindowManager_WindowFocused;
	}

	private void WindowManager_WindowFocused(object? sender, WindowFocusedEventArgs e)
	{
		_lastFocusStartTime = Environment.TickCount;
	}

	/// <inheritdoc/>
	public void PostInitialize()
	{
		// The window must be created on the UI thread (so don't do it in the constructor).
		_focusIndicatorWindow = new FocusIndicatorWindow(_context, _focusIndicatorConfig);

		// Activate the window so it renders.
		_focusIndicatorWindow.Activate();
		_focusIndicatorWindow.Hide(_context);

		// TODO: cancellation
		Task.Factory.StartNew(
			LongRunningTask,
			_cancellationToken,
			TaskCreationOptions.LongRunning,
			TaskScheduler.Default
		);
	}

	// TODO: Rename
	private void LongRunningTask()
	{
		while (true)
		{
			NewMethod();

			Thread.Sleep(16);
		}
	}

	// TODO: Rename
	private void NewMethod()
	{
		if (_isEnabled)
		{
			if (_focusIndicatorConfig.FadeEnabled)
			{
				int now = Environment.TickCount;
				if (now - _lastFocusStartTime >= _focusIndicatorConfig.FadeTimeout.TotalMilliseconds)
				{
					Hide();
					return;
				}
			}

			// If the fade is not enabled, or the fade is not over, show the focus indicator.
			Show();
		}
		else if (IsVisible)
		{
			Hide();
		}
	}

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

		IsVisible = true;
		_focusIndicatorWindow?.Activate(handle, rect);
	}

	/// <inheritdoc/>
	private void Hide()
	{
		Logger.Verbose("Hiding focus indicator");
		_focusIndicatorWindow?.Hide(_context);
		IsVisible = false;
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
				_cancellationTokenSource.Dispose();
				_focusIndicatorWindow?.Dispose();
				_focusIndicatorWindow?.Close();
				_context.WindowManager.WindowFocused -= WindowManager_WindowFocused;
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
