using System;
using System.Linq;
using System.Text.Json;
using Whim.FloatingLayout;

namespace Whim.LayoutPreview;

/// <inheritdoc/>
public class LayoutPreviewPlugin : IPlugin, IDisposable
{
	private readonly IContext _context;
	private LayoutPreviewWindow? _layoutPreviewWindow;
	private bool _disposedValue;

	private readonly object _previewLock = new();

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

	/// <summary>
	/// Initializes a new instance of the <see cref="LayoutPreviewPlugin"/> class.
	/// </summary>
	public LayoutPreviewPlugin(IContext context)
	{
		_context = context;
	}

	/// <inheritdoc	/>
	public void PreInitialize()
	{
		_context.WindowManager.WindowMoveStart += WindowManager_WindowMoveStart;
		_context.WindowManager.WindowMoved += WindowMoved;
		_context.WindowManager.WindowMoveEnd += WindowManager_WindowMoveEnd;
		_context.WindowManager.WindowRemoved += WindowManager_WindowRemoved;
		_context.WindowManager.WindowFocused += WindowManager_WindowFocused;
		_context.FilterManager.AddTitleMatchFilter(LayoutPreviewWindow.WindowTitle);
	}

	/// <inheritdoc	/>
	public void PostInitialize() { }

	/// <inheritdoc />
	public void LoadState(JsonElement state) { }

	/// <inheritdoc />
	public JsonElement? SaveState() => null;

	private void WindowManager_WindowMoveStart(object? sender, WindowMoveStartedEventArgs e)
	{
		if (e.CursorDraggedPoint == null)
		{
			return;
		}

		_layoutPreviewWindow ??= new(_context);
		WindowMoved(this, e);
	}

	private void WindowMoved(object? sender, WindowMoveEventArgs e)
	{
		lock (_previewLock)
		{
			// Only run if the window is being dragged. If the window is being resized, we don't want to do anything.
			if (e.CursorDraggedPoint is not IPoint<int> cursorDraggedPoint || e.MovedEdges is not null)
			{
				return;
			}

			IMonitor monitor = _context.MonitorManager.GetMonitorAtPoint(cursorDraggedPoint);
			IPoint<double> normalizedPoint = monitor.WorkingArea.NormalizeAbsolutePoint(cursorDraggedPoint);

			IWorkspace? workspace = _context.Butler.Pantry.GetWorkspaceForMonitor(monitor);
			if (workspace == null)
			{
				return;
			}

			DraggedWindow = e.Window;
			ILayoutEngine layoutEngine = workspace.ActiveLayoutEngine.MoveWindowToPoint(e.Window, normalizedPoint);
			if (layoutEngine.GetLayoutEngine<FloatingLayoutEngine>() is not null)
			{
				Logger.Debug("Skip LayoutPreview as LeafLayoutEngine is a FloatingLayoutEngine");
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
	}

	private void WindowManager_WindowRemoved(object? sender, WindowEventArgs e)
	{
		lock (_previewLock)
		{
			if (DraggedWindow == e.Window)
			{
				_layoutPreviewWindow?.Hide(_context);
				DraggedWindow = null;
			}
		}
	}

	private void WindowManager_WindowFocused(object? sender, WindowFocusedEventArgs e)
	{
		lock (_previewLock)
		{
			_layoutPreviewWindow?.Hide(_context);
			DraggedWindow = null;
		}
	}

	private void WindowManager_WindowMoveEnd(object? sender, WindowEventArgs e)
	{
		lock (_previewLock)
		{
			_layoutPreviewWindow?.Hide(_context);
			DraggedWindow = null;
		}
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
