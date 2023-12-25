using System.Text.Json;

namespace Whim.SliceLayout;

public class SliceLayoutPlugin : ISliceLayoutPlugin
{
	private readonly IContext _context;

	public SliceLayoutPlugin(IContext context)
	{
		_context = context;
	}

	public string Name => "whim.slice_layout";

	public string PromoteWindowActionName => $"{Name}.window.promote";

	public string DemoteWindowActionName => $"{Name}.window.demote";

	public string PromoteFocusActionName => $"{Name}.focus.promote";

	public string DemoteFocusActionName => $"{Name}.focus.demote";

	public IPluginCommands PluginCommands => new SliceLayoutCommands(this);

	public WindowInsertionType WindowInsertionType { get; set; }

	public void PreInitialize() { }

	public void PostInitialize() { }

	public void LoadState(JsonElement state) { }

	public JsonElement? SaveState() => null;

	public void PromoteWindowInStack(IWindow? window = null) => ChangeWindowRank(window, promote: true);

	public void DemoteWindowInStack(IWindow? window = null) => ChangeWindowRank(window, promote: false);

	private void ChangeWindowRank(IWindow? window, bool promote)
	{
		if (GetWindowWithRankDelta(window, promote) is not (IWindow definedWindow, IWorkspace workspace))
		{
			return;
		}

		workspace.PerformCustomLayoutEngineAction(
			new LayoutEngineCustomAction()
			{
				Name = promote ? PromoteWindowActionName : DemoteWindowActionName,
				Window = definedWindow
			}
		);
	}

	private (IWindow, IWorkspace)? GetWindowWithRankDelta(IWindow? window, bool promote)
	{
		window ??= _context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;

		if (window is null)
		{
			Logger.Debug("No window to change rank for");
			return null;
		}

		IWorkspace? workspace = _context.WorkspaceManager.GetWorkspaceForWindow(window);
		if (workspace is null)
		{
			Logger.Debug("Window is not in a workspace");
			return null;
		}

		return (window, workspace);
	}

	public void PromoteFocusInStack(IWindow? window = null) => FocusWindowRank(window, promote: true);

	public void DemoteFocusInStack(IWindow? window = null) => FocusWindowRank(window, promote: false);

	private void FocusWindowRank(IWindow? window, bool promote)
	{
		if (GetWindowWithRankDelta(window, promote) is not (IWindow definedWindow, IWorkspace workspace))
		{
			return;
		}

		workspace.PerformCustomLayoutEngineAction(
			new LayoutEngineCustomAction()
			{
				Name = promote ? PromoteFocusActionName : DemoteFocusActionName,
				Window = definedWindow
			}
		);
	}
}
