using Microsoft.UI.Xaml;
using System;
using System.ComponentModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Whim.FocusIndicator;

public class FocusIndicatorPlugin : IPlugin
{
	private readonly IConfigContext _configContext;
	private readonly FocusIndicatorConfig _focusIndicatorConfig;
	private FocusIndicatorWindow? _focusIndicatorWindow;
	private DispatcherTimer? _dispatcherTimer;

	public FocusIndicatorPlugin(IConfigContext configContext, FocusIndicatorConfig focusIndicatorConfig)
	{
		_configContext = configContext;
		_focusIndicatorConfig = focusIndicatorConfig;
	}

	public void Initialize()
	{
		_configContext.FilterManager.IgnoreTitleMatch(FocusIndicatorConfig.Title);
		_configContext.WindowManager.WindowFocused += WindowManager_WindowFocused;
		_configContext.WindowManager.WindowUnregistered += WindowManager_WindowUnregistered;
		_configContext.WindowManager.WindowUpdated += WindowManager_WindowUpdated;
		_focusIndicatorConfig.PropertyChanged += FocusIndicatorConfig_PropertyChanged;
	}

	private void WindowManager_WindowFocused(object? sender, WindowEventArgs e)
	{
		_focusIndicatorConfig.IsVisible = true;
	}

	private void DispatcherTimer_Tick(object? sender, object e)
	{
		_focusIndicatorConfig.IsVisible = false;
	}

	private void WindowManager_WindowStartedMoving(object? sender, WindowEventArgs e)
	{
		_focusIndicatorConfig.IsVisible = false;
	}

	private void WindowManager_WindowUnregistered(object? sender, WindowEventArgs e)
	{
		Show();
	}

	private void WindowManager_WindowUpdated(object? sender, WindowUpdateEventArgs e)
	{
		switch (e.UpdateType)
		{
			case WindowUpdateType.Uncloaked:
			case WindowUpdateType.Cloaked:
			case WindowUpdateType.MoveStart:
			case WindowUpdateType.MinimizeStart:
				Hide();
				break;
			case WindowUpdateType.Foreground:
			case WindowUpdateType.MoveEnd:
			case WindowUpdateType.MinimizeEnd:
				Show();
				break;
			case WindowUpdateType.Move:
			default:
				break;
		}
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
		IWorkspace activeWorkspace = _configContext.WorkspaceManager.ActiveWorkspace;
		window ??= activeWorkspace.LastFocusedWindow;
		if (window == null)
		{
			Hide();
			return;
		}

		// Get the window location.
		IWindowLocation? windowLocation = activeWorkspace.TryGetWindowLocation(window);
		if (windowLocation == null)
		{
			Logger.Error($"Could not find window location for window {window}");
			return;
		}

		// Activate the window.
		if (_focusIndicatorWindow == null)
		{
			_focusIndicatorWindow = new FocusIndicatorWindow(_configContext, _focusIndicatorConfig);
		}
		_focusIndicatorWindow.Activate(windowLocation);

		// If the fade is enabled, start the timer.
		if (_focusIndicatorConfig.FadeEnabled)
		{
			_dispatcherTimer = new DispatcherTimer();
			_dispatcherTimer.Tick += DispatcherTimer_Tick;
			_dispatcherTimer.Interval = _focusIndicatorConfig.FadeTimeout;
			_dispatcherTimer.Start();
		}
	}

	private void Hide()
	{
		_focusIndicatorWindow?.Hide();
		if (_dispatcherTimer != null)
		{
			_dispatcherTimer.Stop();
			_dispatcherTimer.Tick -= DispatcherTimer_Tick;
		}
	}
}
