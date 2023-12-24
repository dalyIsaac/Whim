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

	public string PromoteActionName => $"{Name}.stack.promote";

	public string DemoteActionName => $"{Name}.stack.demote";

	public IPluginCommands PluginCommands => new SliceLayoutCommands(this);

	public WindowInsertionType WindowInsertionType { get; set; }

	public void PreInitialize() { }

	public void PostInitialize() { }

	public void LoadState(JsonElement state) { }

	public JsonElement? SaveState() => null;

	private void ChangeWindowRank(IWindow? window, bool promote)
	{
		window ??= _context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;

		if (window is null)
		{
			Logger.Debug("No window to change rank for");
			return;
		}

		IWorkspace? workspace = _context.WorkspaceManager.GetWorkspaceForWindow(window);
		if (workspace is null)
		{
			Logger.Debug("Window is not in a workspace");
			return;
		}

		workspace.PerformCustomLayoutEngineAction(
			new LayoutEngineCustomAction() { Name = promote ? PromoteActionName : DemoteActionName, Window = window, }
		);
	}

	public void PromoteWindowInStack(IWindow? window = null) => ChangeWindowRank(window, promote: true);

	public void DemoteWindowInStack(IWindow? window = null) => ChangeWindowRank(window, promote: false);
}
