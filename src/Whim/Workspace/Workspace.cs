using System.Collections.Immutable;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// Workspaces contain windows to be organized by layout engines.
/// </summary>
public sealed partial record Workspace : IWorkspace
{
	/// <inheritdoc />
	public WorkspaceId Id { get; internal init; } = WorkspaceId.NewGuid();

	/// <inheritdoc />
	public string BackingName { get; internal init; } = string.Empty;

	/// <inheritdoc />
	public int ActiveLayoutEngineIndex { get; internal init; }

	/// <inheritdoc />
	public int PreviousLayoutEngineIndex { get; internal init; }

	/// <inheritdoc />
	public HWND LastFocusedWindowHandle { get; internal init; }

	/// <inheritdoc />
	public ImmutableList<ILayoutEngine> LayoutEngines { get; internal init; } = ImmutableList<ILayoutEngine>.Empty;

	/// <inheritdoc />
	public ImmutableDictionary<HWND, IWindowState> WindowStates { get; internal init; } =
		ImmutableDictionary<HWND, IWindowState>.Empty;

	/// <summary>
	/// Internal-only implementation of a copy constructor.
	/// </summary>
	/// <param name="workspace"></param>
	internal Workspace(Workspace workspace)
	{
		_context = workspace._context;
		_internalContext = workspace._internalContext;
		Id = workspace.Id;
		BackingName = workspace.BackingName;
		ActiveLayoutEngineIndex = workspace.ActiveLayoutEngineIndex;
		PreviousLayoutEngineIndex = workspace.PreviousLayoutEngineIndex;
		LastFocusedWindowHandle = workspace.LastFocusedWindowHandle;
		LayoutEngines = workspace.LayoutEngines;
		WindowStates = workspace.WindowStates;
	}
}
