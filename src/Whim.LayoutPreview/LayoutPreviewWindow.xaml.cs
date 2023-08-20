using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim.LayoutPreview;

/// <summary>
/// Window showing a preview of the layout.
/// </summary>
public sealed partial class LayoutPreviewWindow : Window, IDisposable
{
	private readonly IContext _context;
	private readonly IWindow _window;
	private readonly TransparentWindowController _transparentWindowController;
	private IWindowState[] _prevWindowStates = Array.Empty<IWindowState>();
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
		_transparentWindowController = _context.NativeManager.CreateTransparentWindowController(this);
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

	private void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_transparentWindowController.Dispose();
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
