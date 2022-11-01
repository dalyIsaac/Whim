namespace Whim.TreeLayout;

/// <summary>
/// TreeLayoutPlugin provides commands and functionality for the <see cref="ITreeLayoutEngine"/>.
/// TreeLayoutPlugin does not load the <see cref="ITreeLayoutEngine"/> - that is done when creating
/// a workspace via <see cref="IWorkspace.CreateWorkspace"/>, or in <see cref="IWorkspaceManager.WorkspaceFactory"/>.
/// </summary>
public interface ITreeLayoutPlugin : IPlugin
{
	/// <summary>
	/// Returns a <see cref="ITreeLayoutEngine"/> if the layout engine is or contains a tree layout engine.
	/// Otherwise, returns null.
	/// </summary>
	public ITreeLayoutEngine? GetTreeLayoutEngine();

	/// <summary>
	/// Set the direction in which to add new windows to the tree layout.
	///
	/// This will only work if the layout engine is or contains a tree layout engine.
	/// </summary>
	/// <param name="direction">The direction.</param>
	public void SetAddWindowDirection(Direction direction);
}
