namespace Whim;

/// <summary>
/// Workspaces contain windows to be organized by layout engines.
/// </summary>
public sealed partial record Workspace : IWorkspace
{
	/// <inheritdoc />
	public WorkspaceId Id { get; internal set; } = WorkspaceId.NewGuid();

	/// <inheritdoc />
	public string BackingName { get; internal set; } = string.Empty;

	/// <inheritdoc />
	public int ActiveLayoutEngineIndex { get; internal set; }

	/// <inheritdoc />
	public int PreviousLayoutEngineIndex { get; internal set; }

	/// <inheritdoc />
	public HWND LastFocusedWindowHandle { get; internal set; }

	/// <inheritdoc />
	public ImmutableList<ILayoutEngine> LayoutEngines { get; internal set; } = ImmutableList<ILayoutEngine>.Empty;

	/// <inheritdoc />
	public ImmutableDictionary<HWND, WindowPosition> WindowPositions { get; internal set; } =
		ImmutableDictionary<HWND, WindowPosition>.Empty;

	/// <summary>
	/// Internal-only implementation of a copy constructor.
	/// </summary>
	/// <param name="workspace"></param>
	internal Workspace(Workspace workspace)
	{
		_context = workspace._context;
		Id = workspace.Id;
		BackingName = workspace.BackingName;
		ActiveLayoutEngineIndex = workspace.ActiveLayoutEngineIndex;
		PreviousLayoutEngineIndex = workspace.PreviousLayoutEngineIndex;
		LastFocusedWindowHandle = workspace.LastFocusedWindowHandle;
		LayoutEngines = workspace.LayoutEngines;
		WindowPositions = workspace.WindowPositions;
	}
}
