using System.Text.Json;

namespace Whim.SliceLayout;

/// <inheritdoc />
/// <summary>
/// Create a new <see cref="SliceLayoutPlugin"/>.
/// </summary>
/// <param name="context"></param>
public class SliceLayoutPlugin(IContext context) : ISliceLayoutPlugin
{
	private readonly IContext _context = context;

	/// <summary>
	/// <c>whim.slice_layout</c>
	/// </summary>
	public string Name => "whim.slice_layout";

	/// <inheritdoc />
	public string PromoteWindowActionName => $"{Name}.window.promote";

	/// <inheritdoc />
	public string DemoteWindowActionName => $"{Name}.window.demote";

	/// <inheritdoc />
	public string PromoteFocusActionName => $"{Name}.focus.promote";

	/// <inheritdoc />
	public string DemoteFocusActionName => $"{Name}.focus.demote";

	/// <inheritdoc />
	public IPluginCommands PluginCommands => new SliceLayoutCommands(this);

	/// <inheritdoc />
	public WindowInsertionType WindowInsertionType { get; set; }

	/// <inheritdoc />
	public void PreInitialize()
	{
		_context.Store.WindowEvents.WindowMinimizeStarted += WindowEvents_WindowMinimizeStarted;
	}

	private void WindowEvents_WindowMinimizeStarted(object? sender, WindowEventArgs e)
	{
		System.Guid workspaceId = _context.Store.Pick(Pickers.PickActiveWorkspaceId());
		_context.Store.Dispatch(new DoWorkspaceLayoutTransform(workspaceId));
	}

	/// <inheritdoc />
	public void PostInitialize() { }

	/// <inheritdoc />
	public void LoadState(JsonElement state) { }

	/// <inheritdoc />
	public JsonElement? SaveState() => null;

	/// <inheritdoc />
	public void PromoteWindowInStack(IWindow? window = null) => ChangeWindowRank(window, promote: true);

	/// <inheritdoc />
	public void DemoteWindowInStack(IWindow? window = null) => ChangeWindowRank(window, promote: false);

	private void ChangeWindowRank(IWindow? window, bool promote)
	{
		if (GetWindowWithRankDelta(window, promote) is not (IWindow definedWindow, IWorkspace workspace))
		{
			return;
		}

		LayoutEngineCustomAction action = new()
		{
			Name = promote ? PromoteWindowActionName : DemoteWindowActionName,
			Window = definedWindow,
		};
		_context.Store.Dispatch(new LayoutEngineCustomActionTransform(workspace.Id, action));
	}

	private (IWindow, IWorkspace)? GetWindowWithRankDelta(IWindow? window, bool promote)
	{
		if (window is null && !_context.Store.Pick(Pickers.PickLastFocusedWindow()).TryGet(out window))
		{
			Logger.Debug("No window to change rank for");
			return null;
		}

		Result<IWorkspace> workspaceResult = _context.Store.Pick(Pickers.PickWorkspaceByWindow(window.Handle));
		if (!workspaceResult.TryGet(out IWorkspace? workspace))
		{
			Logger.Debug($"Window {window} is not in a workspace");
			return null;
		}

		return (window, workspace);
	}

	/// <inheritdoc />
	public void PromoteFocusInStack(IWindow? window = null) => FocusWindowRank(window, promote: true);

	/// <inheritdoc />
	public void DemoteFocusInStack(IWindow? window = null) => FocusWindowRank(window, promote: false);

	private void FocusWindowRank(IWindow? window, bool promote)
	{
		if (GetWindowWithRankDelta(window, promote) is not (IWindow definedWindow, IWorkspace workspace))
		{
			return;
		}

		LayoutEngineCustomAction action = new()
		{
			Name = promote ? PromoteFocusActionName : DemoteFocusActionName,
			Window = definedWindow,
		};
		_context.Store.Dispatch(new LayoutEngineCustomActionTransform(workspace.Id, action));
	}
}
