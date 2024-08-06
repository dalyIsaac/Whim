using System;
using System.Linq;
using System.Text.Json;
using Whim.FloatingWindow;

namespace Whim.LayoutPreview;

/// <inheritdoc/>
/// <summary>
/// Initializes a new instance of the <see cref="LayoutPreviewPlugin"/> class.
/// </summary>
public class LayoutPreviewPlugin(IContext context) : IPlugin, IDisposable
{
	private readonly IContext _context = context;
	private LayoutPreviewWindow? _layoutPreviewWindow;
	private bool _disposedValue;

	/// <summary>
	/// The window that is currently being dragged.
	/// </summary>
	public IWindow? DraggedWindow { get; private set; }

	/// <summary>
	/// <c>whim.layout_preview</c>
	/// </summary>
	public string Name => "whim.layout_preview";

	/// <inheritdoc/>
	public IPluginCommands PluginCommands => new LayoutPreviewCommands(this);

	/// <inheritdoc	/>
	public void PreInitialize()
	{
		_context.Store.WindowEvents.WindowMoveStarted += WindowEvents_WindowMoveStart;
		_context.Store.WindowEvents.WindowMoved += WindowMoved;
		_context.Store.WindowEvents.WindowMoveEnded += WindowEvents_WindowMoveEnd;
		_context.Store.WindowEvents.WindowRemoved += WindowEvents_WindowRemoved;
		_context.Store.WindowEvents.WindowFocused += WindowEvents_WindowFocused;
		_context.FilterManager.AddTitleMatchFilter(LayoutPreviewWindow.WindowTitle);
	}

	/// <inheritdoc	/>
	public void PostInitialize() { }

	/// <inheritdoc />
	public void LoadState(JsonElement state) { }

	/// <inheritdoc />
	public JsonElement? SaveState() => null;

	private void WindowEvents_WindowMoveStart(object? sender, WindowMoveStartedEventArgs e)
	{
		if (e.CursorDraggedPoint == null)
		{
			return;
		}

		// We don't hit this code path in tests to avoid UI code not working in tests.
		_layoutPreviewWindow ??= new(_context);
		WindowMoved(this, e);
	}

	private void WindowMoved(object? sender, WindowMoveEventArgs e)
	{
		// Only run if the window is being dragged. If the window is being resized, we don't want to do anything.
		// If the given point does not correspond to a monitor, ignore it.
		// If the given point does not correspond to a workspace, ignore it.
		if (
			e.CursorDraggedPoint is not IPoint<int> cursorDraggedPoint
			|| e.MovedEdges is not null
			|| !_context.Store.Pick(Pickers.PickMonitorAtPoint(cursorDraggedPoint)).TryGet(out IMonitor monitor)
			|| !_context.Store.Pick(Pickers.PickWorkspaceByMonitor(monitor.Handle)).TryGet(out IWorkspace workspace)
		)
		{
			Hide();
			return;
		}

		DraggedWindow = e.Window;

		IPoint<double> normalizedPoint = monitor.WorkingArea.NormalizeAbsolutePoint(cursorDraggedPoint);
		ILayoutEngine layoutEngine = workspace.ActiveLayoutEngine.MoveWindowToPoint(e.Window, normalizedPoint);
		if (layoutEngine.GetLayoutEngine<FloatingLayoutEngine>() is not null)
		{
			Logger.Debug("Skip LayoutPreview as LeafLayoutEngine is a FloatingLayoutEngine");

			Hide();
			return;
		}

		Rectangle<int> rect = new() { Height = monitor.WorkingArea.Height, Width = monitor.WorkingArea.Width };

		// Adjust the cursor point so that it's relative to the monitor's rectangle.
		Point<int> adjustedCursorPoint =
			new()
			{
				X = cursorDraggedPoint.X - monitor.WorkingArea.X,
				Y = cursorDraggedPoint.Y - monitor.WorkingArea.Y
			};

		_layoutPreviewWindow?.Update(
			layoutEngine.DoLayout(rect, monitor).ToArray(),
			adjustedCursorPoint,
			e.Window,
			monitor
		);
	}

	private void WindowEvents_WindowRemoved(object? sender, WindowEventArgs e)
	{
		if (DraggedWindow == e.Window)
		{
			Hide();
		}
	}

	private void WindowEvents_WindowFocused(object? sender, WindowFocusedEventArgs e) => Hide();

	private void WindowEvents_WindowMoveEnd(object? sender, WindowEventArgs e) => Hide();

	private void Hide()
	{
		_layoutPreviewWindow?.Hide(_context);
		DraggedWindow = null;
	}

	/// <inheritdoc />
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_layoutPreviewWindow?.Dispose();
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			_disposedValue = true;
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
