using System.Linq;
using System.Text.Json;

namespace Whim.LayoutPreview;

/// <inheritdoc/>
public class LayoutPreviewPlugin : IPlugin
{
	private readonly IContext _context;
	private LayoutPreviewWindow? _layoutPreviewWindow;

	/// <inheritdoc/>
	public string Name => "whim.layout_preview";

	/// <inheritdoc/>
	public IPluginCommands PluginCommands => new LayoutPreviewCommands(_context, this);

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
		_context.FilterManager.IgnoreTitleMatch(LayoutPreviewConfig.Title);
	}

	/// <inheritdoc	/>
	public void PostInitialize()
	{
		_layoutPreviewWindow = new(_context);
	}

	/// <inheritdoc />
	public void LoadState(JsonElement state) { }

	/// <inheritdoc />
	public JsonElement? SaveState() => null;

	private void WindowManager_WindowMoveStart(object? sender, WindowMovedEventArgs e)
	{
		if (e.CursorDraggedPoint == null || _layoutPreviewWindow == null)
		{
			return;
		}

		_layoutPreviewWindow.Activate(e.Window, _context.MonitorManager.ActiveMonitor);
		WindowMoved(this, e);
	}

	private void WindowMoved(object? sender, WindowMovedEventArgs e)
	{
		// Only run if the window is being dragged.
		if (e.CursorDraggedPoint is not IPoint<int> cursorDraggedPoint)
		{
			return;
		}

		IMonitor monitor = _context.MonitorManager.ActiveMonitor;
		IPoint<int> monitorPoint = monitor.WorkingArea.ToMonitorCoordinates(cursorDraggedPoint);
		IPoint<double> normalizedPoint = monitor.WorkingArea.ToUnitSquare(monitorPoint);

		ILayoutEngine layoutEngine = _context.WorkspaceManager.ActiveWorkspace.ActiveLayoutEngine.MoveWindowToPoint(
			e.Window,
			normalizedPoint
		);

		Location<int> location = new() { Height = monitor.WorkingArea.Height, Width = monitor.WorkingArea.Width };

		_layoutPreviewWindow?.Update(layoutEngine.DoLayout(location, monitor).ToArray(), cursorDraggedPoint);
	}

	private void WindowManager_WindowMoveEnd(object? sender, WindowEventArgs e)
	{
		_layoutPreviewWindow?.Hide(_context);
	}
}
