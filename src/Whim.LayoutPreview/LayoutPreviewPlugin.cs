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
	public void PostInitialize() { }

	/// <inheritdoc />
	public void LoadState(JsonElement state) { }

	/// <inheritdoc />
	public JsonElement? SaveState() => null;

	private void WindowManager_WindowMoveStart(object? sender, WindowMovedEventArgs e)
	{
		if (e.CursorDraggedPoint == null)
		{
			return;
		}

		_layoutPreviewWindow ??= new(_context);
		WindowMoved(this, e);
	}

	private void WindowMoved(object? sender, WindowMovedEventArgs e)
	{
		// Only run if the window is being dragged.
		if (e.CursorDraggedPoint is not IPoint<int> cursorDraggedPoint)
		{
			return;
		}

		IMonitor monitor = _context.MonitorManager.GetMonitorAtPoint(cursorDraggedPoint);
		IPoint<int> monitorPoint = monitor.WorkingArea.ToMonitorCoordinates(cursorDraggedPoint);
		IPoint<double> normalizedPoint = monitor.WorkingArea.ToUnitSquare(monitorPoint);

		IWorkspace? workspace = _context.WorkspaceManager.GetWorkspaceForMonitor(monitor);
		if (workspace == null)
		{
			return;
		}

		ILayoutEngine layoutEngine = workspace.ActiveLayoutEngine.MoveWindowToPoint(e.Window, normalizedPoint);

		Location<int> location = new() { Height = monitor.WorkingArea.Height, Width = monitor.WorkingArea.Width };

		// Adjust the cursor point so that it's relative to the monitor's location.
		Point<int> adjustedCursorPoint =
			new()
			{
				X = cursorDraggedPoint.X - monitor.WorkingArea.X,
				Y = cursorDraggedPoint.Y - monitor.WorkingArea.Y
			};

		_layoutPreviewWindow?.Update(
			layoutEngine.DoLayout(location, monitor).ToArray(),
			adjustedCursorPoint,
			e.Window,
			monitor
		);
	}

	private void WindowManager_WindowMoveEnd(object? sender, WindowEventArgs e)
	{
		_layoutPreviewWindow?.Hide(_context);
	}
}
