using System.Text.Json;

namespace Whim.SliceLayout;

/// <inheritdoc />
public class SliceLayoutPlugin : ISliceLayoutPlugin
{
	private readonly IContext _context;

	/// <summary>
	/// Create a new <see cref="SliceLayoutPlugin"/>.
	/// </summary>
	/// <param name="context"></param>
	public SliceLayoutPlugin(IContext context)
	{
		_context = context;
	}

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
	public void PreInitialize() { }

	/// <inheritdoc />
	public void PostInitialize() { }

	/// <inheritdoc />
	public void LoadState(JsonElement state) { }

	/// <inheritdoc />
	public JsonElement? SaveState() => null;

	/// <inheritdoc />
	public bool PromoteWindowInStack(IWindow? window = null) => ChangeWindowRank(window, promote: true);

	/// <inheritdoc />
	public bool DemoteWindowInStack(IWindow? window = null) => ChangeWindowRank(window, promote: false);

	private bool ChangeWindowRank(IWindow? window, bool promote)
	{
		if (GetWindowWithRankDelta(window, promote) is not (IWindow definedWindow, IWorkspace workspace))
		{
			return false;
		}

		workspace.PerformCustomLayoutEngineAction(
			new LayoutEngineCustomAction()
			{
				Name = promote ? PromoteWindowActionName : DemoteWindowActionName,
				Window = definedWindow
			}
		);

		return true;
	}

	private (IWindow, IWorkspace)? GetWindowWithRankDelta(IWindow? window, bool promote)
	{
		window ??= _context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;

		if (window is null)
		{
			Logger.Debug("No window to change rank for");
			return null;
		}

		if (!_context.Store.Pick(Pickers.GetWorkspaceForWindow(window)).TryGet(out IWorkspace workspace))
		{
			Logger.Debug("Window is not in a workspace");
			return null;
		}

		return (window, workspace);
	}

	/// <inheritdoc />
	public bool PromoteFocusInStack(IWindow? window = null) => FocusWindowRank(window, promote: true);

	/// <inheritdoc />
	public bool DemoteFocusInStack(IWindow? window = null) => FocusWindowRank(window, promote: false);

	private bool FocusWindowRank(IWindow? window, bool promote)
	{
		if (GetWindowWithRankDelta(window, promote) is not (IWindow definedWindow, IWorkspace workspace))
		{
			return false;
		}

		workspace.PerformCustomLayoutEngineAction(
			new LayoutEngineCustomAction()
			{
				Name = promote ? PromoteFocusActionName : DemoteFocusActionName,
				Window = definedWindow
			}
		);
		return true;
	}
}
