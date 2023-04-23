using System;

namespace Whim.TreeLayout;

/// <summary>
/// TreeLayoutPlugin provides commands and functionality for the <see cref="TreeLayoutEngine"/>.
/// TreeLayoutPlugin does not load the <see cref="TreeLayoutEngine"/> - that is done when creating
/// a workspace via <see cref="IWorkspace.CreateWorkspace"/>, or in <see cref="IWorkspaceManager.WorkspaceFactory"/>.
/// </summary>
public interface ITreeLayoutPlugin : IPlugin
{
	/// <summary>
	/// Event raised when the direction in which to add new windows to the tree layout for a
	/// monitor changes.
	/// </summary>
	public event EventHandler<AddWindowDirectionChangedEventArgs>? AddWindowDirectionChanged;

	/// <summary>
	/// Set the direction in which to add new windows to the tree layout for the given
	/// <paramref name="monitor"/>.
	///
	/// This will only work if the <paramref name="monitor"/>'s current workspace's active layout
	/// engine is or contains a tree layout engine.
	/// </summary>
	/// <param name="monitor"></param>
	/// <param name="direction"></param>
	public void SetAddWindowDirection(IMonitor monitor, Direction direction);

	/// <summary>
	/// Get the current direction for adding new windows to the tree layout for the given
	/// <paramref name="monitor"/>.
	///
	/// This will only work if the <paramref name="monitor"/>'s current workspace's active layout
	/// engine is or contains a tree layout engine.
	/// </summary>
	/// <param name="monitor"></param>
	/// <returns></returns>
	public Direction? GetAddWindowDirection(IMonitor monitor);

	/// <summary>
	/// Split the focused window in two, with the space in the direction of the tree layout filled
	/// by a phantom window.
	/// </summary>
	public void SplitFocusedWindow();
}
