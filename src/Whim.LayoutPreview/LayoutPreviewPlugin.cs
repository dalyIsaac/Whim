using System.Text.Json;

namespace Whim.LayoutPreview;

/// <inheritdoc/>
public class LayoutPreviewPlugin : IPlugin
{
	private readonly IContext _context;

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

	private void WindowManager_WindowMoveStart(object? sender, WindowEventArgs e)
	{
		// TODO
	}

	private void WindowManager_WindowMoved(object? sender, WindowEventArgs e)
	{
		// TODO
	}

	/// <inheritdoc	/>
	public void PostInitialize() { }

	/// <inheritdoc />
	public void LoadState(JsonElement state) { }

	/// <inheritdoc />
	public JsonElement? SaveState() => null;
}
