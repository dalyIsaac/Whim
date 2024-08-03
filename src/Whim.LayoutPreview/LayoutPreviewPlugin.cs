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
		_context.Store.WindowEvents.WindowMoveStarted += WindowManager_WindowMoveStart;
		_context.Store.WindowEvents.WindowMoved += WindowMoved;
		_context.Store.WindowEvents.WindowMoveEnded += WindowManager_WindowMoveEnd;
		_context.Store.WindowEvents.WindowRemoved += WindowManager_WindowRemoved;
		_context.Store.WindowEvents.WindowFocused += WindowManager_WindowFocused;
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
		if (layoutEngine.GetLayoutEngine<FreeLayoutEngine>() is not null)
		{
			// To be renamed when FreeLayoutEngine will be renamed
			Logger.Debug("Skip LayoutPreview as LeafLayoutEngine is a FreeLayoutEngine");

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

	private void WindowManager_WindowRemoved(object? sender, WindowEventArgs e)
	{
		if (DraggedWindow == e.Window)
		{
			Hide();
		}
	}

	private void WindowManager_WindowFocused(object? sender, WindowFocusedEventArgs e) => Hide();

	private void WindowManager_WindowMoveEnd(object? sender, WindowEventArgs e) => Hide();

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
