using Microsoft.UI.Xaml;
using System.ComponentModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Whim.FocusIndicator;

public class FocusIndicatorPlugin : IPlugin
{
	private readonly IConfigContext _configContext;
	private readonly FocusIndicatorConfig _focusIndicatorConfig;
	private readonly FocusIndicatorWindow _focusIndicatorWindow;
	private readonly DispatcherTimer _dispatcherTimer;

	public FocusIndicatorPlugin(IConfigContext configContext, FocusIndicatorConfig focusIndicatorConfig)
	{
		_configContext = configContext;
		_focusIndicatorConfig = focusIndicatorConfig;
		_focusIndicatorWindow = new FocusIndicatorWindow(focusIndicatorConfig);
		_dispatcherTimer = new DispatcherTimer();
	}

	public void Initialize()
	{
		// When the window has focused or the timer has ticked, as change the config's
		// IsVisible property.
		// The FocusIndicatorConfig_PropertyChanged listener listens to changes on IsVisible
		// and handles them.
		_configContext.WindowManager.WindowFocused += WindowManager_WindowFocused;
		_dispatcherTimer.Tick += DispatcherTimer_Tick;

		_focusIndicatorConfig.PropertyChanged += FocusIndicatorConfig_PropertyChanged;

		// TODO: filter.
		// It might be better in the constructor.
	}

	private void WindowManager_WindowFocused(object? sender, WindowEventArgs e)
	{
		_focusIndicatorConfig.IsVisible = true;
	}

	private void DispatcherTimer_Tick(object? sender, object e)
	{
		_focusIndicatorConfig.IsVisible = false;
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
			Logger.Error("No window to show focus indicator for");
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
		_focusIndicatorWindow.Activate(windowLocation);

		// If the fade is enabled, start the timer.
		if (_focusIndicatorConfig.FadeEnabled)
		{
			_dispatcherTimer.Interval = _focusIndicatorConfig.FadeTimeout;
			_dispatcherTimer.Start();
		}
	}

	private void Hide()
	{
		_dispatcherTimer.Stop();
		_focusIndicatorWindow.Hide();
	}
}
