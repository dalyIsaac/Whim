using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Win32.UI.WindowsAndMessaging;
using WinRT;

namespace Whim.LayoutPreview;

/// <summary>
/// Window showing a preview of the layout.
/// </summary>
public sealed partial class LayoutPreviewWindow : Window, IDisposable
{
	private readonly IContext _context;
	private readonly IWindow _window;
	private readonly ISystemBackdropControllerWithTargets _systemBackdropController;
	private readonly SystemBackdropConfiguration _systemBackdropConfiguration;
	private bool _disposedValue;

	/// <summary>
	/// Initializes a new instance of the <see cref="LayoutPreviewWindow"/> class.
	/// </summary>
	public LayoutPreviewWindow(IContext context)
	{
		_context = context;
		_window = this.InitializeBorderlessWindow(_context, "Whim.LayoutPreview", "LayoutPreviewWindow");
		this.SetIsShownInSwitchers(false);

		Title = LayoutPreviewConfig.Title;

		_systemBackdropConfiguration = new SystemBackdropConfiguration() { IsInputActive = true };
		SetConfigurationSourceTheme();

		// TODO: Play around with this to get as much transparency as possible.
		if (MicaController.IsSupported())
		{
			_systemBackdropController = new MicaController()
			{
				Kind = MicaKind.BaseAlt,
				// LuminosityOpacity = 0.9f,
				FallbackColor = Colors.Transparent,
				// TintOpacity = 0.9f,
			};
		}
		else
		{
			_systemBackdropController = new DesktopAcrylicController()
			{
				TintOpacity = 0.9f,
				LuminosityOpacity = 0.9f,
			};
		}

		((FrameworkElement)Content).ActualThemeChanged += Window_ThemeChanged;

		_systemBackdropController.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
		_systemBackdropController.SetSystemBackdropConfiguration(_systemBackdropConfiguration);
	}

	private void Window_ThemeChanged(FrameworkElement sender, object args)
	{
		if (_systemBackdropConfiguration != null)
		{
			SetConfigurationSourceTheme();
		}
	}

	private void SetConfigurationSourceTheme()
	{
		switch (((FrameworkElement)Content).ActualTheme)
		{
			case ElementTheme.Dark:
				_systemBackdropConfiguration.Theme = SystemBackdropTheme.Dark;
				break;
			case ElementTheme.Light:
				_systemBackdropConfiguration.Theme = SystemBackdropTheme.Light;
				break;
			case ElementTheme.Default:
				_systemBackdropConfiguration.Theme = SystemBackdropTheme.Default;
				break;
		}
	}

	public void Activate(IWindow movingWindow, IMonitor? monitor)
	{
		if (monitor == null)
		{
			return;
		}

		using WindowDeferPosHandle handle = new(_context);
		handle.DeferWindowPos(
			new WindowState()
			{
				Window = _window,
				Location = monitor.WorkingArea,
				WindowSize = WindowSize.Normal
			},
			movingWindow.Handle,
			SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW
		);
	}

	public void Update(IWindowState[] windowStates)
	{
		LayoutPreviewCanvas.Children.Clear();

		LayoutPreviewWindowItem[] items = new LayoutPreviewWindowItem[windowStates.Length];
		for (int i = 0; i < windowStates.Length; i++)
		{
			items[i] = new LayoutPreviewWindowItem(windowStates[i]);

			Canvas.SetLeft(items[i], windowStates[i].Location.X);
			Canvas.SetTop(items[i], windowStates[i].Location.Y);
		}

		LayoutPreviewCanvas.Children.AddRange(items);
	}

	private void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_systemBackdropController.Dispose();
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
