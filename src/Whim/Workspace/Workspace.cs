namespace Whim;

/// <summary>
/// Workspaces contain windows to be organized by layout engines.
/// </summary>
public sealed record Workspace : IWorkspace
{
	/// <inheritdoc />
	public WorkspaceId Id { get; internal set; } = WorkspaceId.NewGuid();

	/// <inheritdoc />
	public string Name { get; internal set; } = string.Empty;

	/// <inheritdoc />
	public int ActiveLayoutEngineIndex { get; internal set; }

	/// <inheritdoc />
	public int PreviousLayoutEngineIndex { get; internal set; }

	/// <inheritdoc />
	public HWND LastFocusedWindowHandle { get; internal set; }

	/// <inheritdoc />
	public ImmutableList<ILayoutEngine> LayoutEngines { get; internal set; } = [];

	/// <inheritdoc />
	public ImmutableDictionary<HWND, WindowPosition> WindowPositions { get; internal set; } =
		ImmutableDictionary<HWND, WindowPosition>.Empty;

	internal Workspace(WorkspaceId id)
	{
		Id = id;
	}

	/// <summary>
	/// Internal-only implementation of a copy constructor.
	/// </summary>
	/// <param name="workspace"></param>
	internal Workspace(Workspace workspace)
	{
		Id = workspace.Id;
		Name = workspace.Name;
		ActiveLayoutEngineIndex = workspace.ActiveLayoutEngineIndex;
		PreviousLayoutEngineIndex = workspace.PreviousLayoutEngineIndex;
		LastFocusedWindowHandle = workspace.LastFocusedWindowHandle;
		LayoutEngines = workspace.LayoutEngines;
		WindowPositions = workspace.WindowPositions;
	}

	/// <inheritdoc/>
	public override string ToString() => Name;
}
