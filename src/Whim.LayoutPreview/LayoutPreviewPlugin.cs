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
		_context.WorkspaceManager.WorkspaceLayoutCompleted += WorkspaceManager_WorkspaceLayoutCompleted;
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
		// TODO
	}

	private void WindowManager_WindowMoved(object? sender, WindowEventArgs e)
	{
		// TODO
	}

	private void WorkspaceManager_WorkspaceLayoutCompleted(object? sender, WorkspaceEventArgs e)
	{
		IMonitor monitor = _context.MonitorManager.ActiveMonitor;
		_layoutPreviewWindow?.Update(
			_context.WorkspaceManager.ActiveWorkspace.ActiveLayoutEngine
				.DoLayout(new Location<int>() { Height = 500, Width = 500 }, monitor)
				.ToArray()
		);
	}
}
