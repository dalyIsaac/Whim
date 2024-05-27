using System.Collections.Immutable;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// Workspaces contain windows to be organized by layout engines.
/// </summary>
internal partial record Workspace : IWorkspace
{
	/// <summary>
	/// The unique id of the workspace.
	/// </summary>
	public WorkspaceId Id { get; internal init; } = WorkspaceId.NewGuid();

	/// <summary>
	/// The name of the workspace.
	/// </summary>
	/// <remarks>
	/// Until the legacy <see cref="Workspace"/> implementation is removed, this is called
	/// <c>BackingName</c> to avoid confusion with <see cref="Name"/>.
	/// Once the legacy <see cref="Workspace"/> implementation is removed, this will be
	/// called <c>Name</c>.
	/// </remarks>
	public string BackingName { get; internal init; } = string.Empty;

	/// <summary>
	/// The index of the layout engine in <see cref="LayoutEngines"/> which is currently active.
	/// </summary>
	public int ActiveLayoutEngineIndex { get; internal init; }

	/// <summary>
	/// The index of the layout engine in <see cref="LayoutEngines"/> which was previously active.
	/// </summary>
	public int PreviousLayoutEngineIndex { get; internal init; }

	/// <summary>
	/// The index of the last focused window in the workspace.
	/// WARNING: When the value is 0, it means that no window is focused. Check this with <see cref="HWND.IsNull"/>.
	/// </summary>
	public HWND LastFocusedWindowHandle { get; internal init; }

	/// <summary>
	/// All the windows in the workspace.
	/// </summary>
	public ImmutableHashSet<HWND> WindowHandles { get; internal init; } = ImmutableHashSet<HWND>.Empty;

	/// <summary>
	/// All the layout engines currently in the workspace.
	/// </summary>
	public ImmutableList<ILayoutEngine> LayoutEngines { get; internal init; } = ImmutableList<ILayoutEngine>.Empty;
}
