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
		_context.WindowManager.WindowMoved += WindowManager_WindowMoved;
	}

	/// <inheritdoc	/>
	public void PostInitialize()
	{
		_layoutPreviewWindow = new(_context);
		_layoutPreviewWindow.Activate();
	}

	/// <inheritdoc />
	public void LoadState(JsonElement state) { }

	/// <inheritdoc />
	public JsonElement? SaveState() => null;

	private void WindowManager_WindowMoveStart(object? sender, WindowEventArgs e)
	{
		// TODO: Show window
	}

	private void WindowManager_WindowMoved(object? sender, WindowMovedEventArgs e)
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

		_layoutPreviewWindow?.Update(
			layoutEngine.DoLayout(new Location<int>() { Height = 500, Width = 500 }, monitor).ToArray()
		);
	}
}
