using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Win32.UI.WindowsAndMessaging;
using WinRT;

namespace Whim.LayoutPreview;

/// <summary>
/// Window showing a preview of the layout.
/// </summary>
public sealed partial class LayoutPreviewWindow : Window
{
	private readonly IContext _context;
	private readonly IWindow _window;
	private IWindowState[] _prevWindowStates = Array.Empty<IWindowState>();

	/// <summary>
	/// Initializes a new instance of the <see cref="LayoutPreviewWindow"/> class.
	/// </summary>
	public LayoutPreviewWindow(IContext context)
	{
		_context = context;
		_window = this.InitializeBorderlessWindow(_context, "Whim.LayoutPreview", "LayoutPreviewWindow");
		this.SetIsShownInSwitchers(false);

		Title = LayoutPreviewConfig.Title;

		// Make the window transparent.
		_context.NativeManager.EnableBlurBehindWindow(_window.Handle);
		ICompositionSupportsSystemBackdrop brushHolder = this.As<ICompositionSupportsSystemBackdrop>();
		brushHolder.SystemBackdrop = _context.NativeManager.Compositor.CreateColorBrush(
			Windows.UI.Color.FromArgb(0, 255, 255, 255)
		);
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
		if (!CheckIsDifferent(windowStates))
		{
			return;
		}

		_prevWindowStates = windowStates;

		LayoutPreviewCanvas.Children.Clear();

		LayoutPreviewWindowItem[] items = new LayoutPreviewWindowItem[windowStates.Length];
		for (int i = 0; i < windowStates.Length; i++)
		{
			items[i] = new LayoutPreviewWindowItem(_context, windowStates[i]);

			Canvas.SetLeft(items[i], windowStates[i].Location.X);
			Canvas.SetTop(items[i], windowStates[i].Location.Y);
		}

		LayoutPreviewCanvas.Children.AddRange(items);
	}

	private bool CheckIsDifferent(IWindowState[] windowStates)
	{
		if (_prevWindowStates.Length != windowStates.Length)
		{
			return true;
		}

		for (int i = 0; i < windowStates.Length; i++)
		{
			if (!_prevWindowStates[i].Equals(windowStates[i]))
			{
				return true;
			}
		}

		return false;
	}
}
