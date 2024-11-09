namespace Whim;

/// <summary>
/// A window's handle, and it's rectangle in its last active <see cref="ILayoutEngine"/>.
/// </summary>
/// <param name="Handle"></param>
/// <param name="Rectangle"></param>
internal record SavedWindow(long Handle, Rectangle<double> Rectangle);

/// <summary>
/// A workspace's name, the windows it contains, and the sticky monitors it can reside on.
/// </summary>
/// <param name="Name"></param>
/// <param name="Windows"></param>
/// <param name="MonitorIndices"></param>
internal record SavedWorkspace(string Name, List<SavedWindow> Windows, int[]? MonitorIndices);

/// <summary>
/// The saved state for Whim's core.
/// </summary>
/// <param name="Workspaces">The workspaces that were saved.</param>
internal record CoreSavedState(List<SavedWorkspace> Workspaces);

/// <summary>
/// Saves and restores the state for Whim's core.
/// </summary>
internal interface ICoreSavedStateManager : IDisposable
{
	/// <summary>
	/// The loaded saved state.
	/// </summary>
	CoreSavedState? SavedState { get; }

	/// <summary>
	/// Load the saved state. Each manager is responsible for loading its own state.
	/// </summary>
	void PreInitialize();

	/// <summary>
	/// Clears the saved state, as it is no longer needed.
	/// </summary>
	void PostInitialize();
}
