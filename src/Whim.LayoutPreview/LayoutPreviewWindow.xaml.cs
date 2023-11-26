using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim.LayoutPreview;

/// <summary>
/// Window showing a preview of the layout.
/// </summary>
internal sealed partial class LayoutPreviewWindow : Window, IDisposable
{
	/// <summary>
	/// The default title of the window.
	/// </summary>
	public static string WindowTitle { get; } = "Whim Layout Preview";

	private readonly IContext _context;
	private readonly IWindow _window;
	private readonly TransparentWindowController _transparentWindowController;
	private IWindowState[] _prevWindowStates = Array.Empty<IWindowState>();
	private int _prevHoveredIndex = -1;
	private bool _disposedValue;

	/// <summary>
	/// Initializes a new instance of the <see cref="LayoutPreviewWindow"/> class.
	/// </summary>
	public LayoutPreviewWindow(IContext context)
	{
		_context = context;
		_window = this.InitializeBorderlessWindow(_context, "Whim.LayoutPreview", "LayoutPreviewWindow");
		this.SetIsShownInSwitchers(false);

		Title = WindowTitle;
		_transparentWindowController = _context.NativeManager.CreateTransparentWindowController(this);
		_context.NativeManager.SetWindowExTransparent(_window.Handle);
	}

	/// <summary>
	/// Update the preview window with the given window states.
	/// </summary>
	/// <param name="windowStates">The window states to display.</param>
	/// <param name="cursorPoint">The cursor's location inside the monitor.</param>
	/// <param name="movingWindow">The window which is currently being dragged.</param>
	/// <param name="monitor">The monitor corresponding to the <paramref name="cursorPoint"/>.</param>
	public void Update(IWindowState[] windowStates, IPoint<int> cursorPoint, IWindow movingWindow, IMonitor monitor)
	{
		// Only update the rendered window if the window states have changed.
		if (!ShouldContinue(_prevWindowStates, _prevHoveredIndex, windowStates, cursorPoint))
		{
			return;
		}

		// Update the rendered window.
		_prevWindowStates = windowStates;
		_prevHoveredIndex = -1;

		LayoutPreviewWindowItem[] items = new LayoutPreviewWindowItem[windowStates.Length];
		bool isFirst = true;
		for (int idx = 0; idx < windowStates.Length; idx++)
		{
			bool isHovered = isFirst && windowStates[idx].Rectangle.ContainsPoint(cursorPoint);
			if (isHovered)
			{
				_prevHoveredIndex = idx;
				isFirst = false;
			}

			items[idx] = new LayoutPreviewWindowItem(_context, windowStates[idx], isHovered);

			Canvas.SetLeft(items[idx], windowStates[idx].Rectangle.X);
			Canvas.SetTop(items[idx], windowStates[idx].Rectangle.Y);
		}

		LayoutPreviewCanvas.Children.Clear();
		LayoutPreviewCanvas.Children.AddRange(items);
		Activate(_context, _window, movingWindow, monitor);
	}

	/// <summary>
	/// Detects whether the new window states are different from the previous window states.
	/// </summary>
	/// <param name="prevWindowStates"></param>
	/// <param name="prevHoveredIndex"></param>
	/// <param name="windowStates"></param>
	/// <param name="cursorPoint"></param>
	/// <returns></returns>
	internal static bool ShouldContinue(
		IWindowState[] prevWindowStates,
		int prevHoveredIndex,
		IWindowState[] windowStates,
		IPoint<int> cursorPoint
	)
	{
		if (prevWindowStates.Length != windowStates.Length || prevHoveredIndex == -1)
		{
			return true;
		}

		for (int idx = 0; idx < windowStates.Length; idx++)
		{
			if (!prevWindowStates[idx].Equals(windowStates[idx]))
			{
				return true;
			}
		}

		IWindowState prevHoveredState = prevWindowStates[prevHoveredIndex];
		if (!prevHoveredState.Rectangle.ContainsPoint(cursorPoint))
		{
			return true;
		}

		return false;
	}

	internal static void Activate(IContext context, IWindow layoutWindow, IWindow movingWindow, IMonitor monitor)
	{
		using DeferWindowPosHandle handle = context.NativeManager.DeferWindowPos();
		handle.DeferWindowPos(
			new WindowState()
			{
				Window = layoutWindow,
				Rectangle = monitor.WorkingArea,
				WindowSize = WindowSize.Normal
			},
			movingWindow.Handle,
			SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW
		);
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
