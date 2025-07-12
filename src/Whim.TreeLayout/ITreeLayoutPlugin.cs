using System;

namespace Whim.TreeLayout;

/// <summary>
/// <see cref="TreeLayoutPlugin"/> provides commands and functionality for the <see cref="TreeLayoutEngine"/>.
/// <see cref="TreeLayoutPlugin"/> does not load the <see cref="TreeLayoutEngine"/> - that is done when creating
/// a workspace dispatching <see cref="AddWorkspaceTransform"/>.
/// </summary>
public interface ITreeLayoutPlugin : IPlugin
{
	/// <summary>
	/// Event raised when the direction in which to add new windows to the tree layout for a
	/// monitor changes.
	/// </summary>
	event EventHandler<AddWindowDirectionChangedEventArgs>? AddWindowDirectionChanged;

	/// <summary>
	/// Set the direction in which to add new windows to the tree layout for the given
	/// <paramref name="monitor"/>.
	///
	/// This will only work if the <paramref name="monitor"/>'s current workspace's active layout
	/// engine is or contains a tree layout engine.
	/// </summary>
	/// <param name="monitor"></param>
	/// <param name="direction"></param>
	void SetAddWindowDirection(IMonitor monitor, Direction direction);

	/// <summary>
	/// Set the direction in which to add new windows to the tree layout for the given
	/// layout engine <paramref name="engine"/>.
	/// </summary>
	/// <param name="engine"></param>
	/// <param name="direction"></param>
	void SetAddWindowDirection(TreeLayoutEngine engine, Direction direction);

	/// <summary>
	/// Get the current direction for adding new windows to the tree layout for the given
	/// <paramref name="monitor"/>.
	///
	/// This will only work if the <paramref name="monitor"/>'s current workspace's active layout
	/// engine is or contains a tree layout engine.
	/// </summary>
	/// <param name="monitor"></param>
	/// <returns></returns>
	Direction? GetAddWindowDirection(IMonitor monitor);

	/// <summary>
	/// Get the current direction for adding new windows to the tree layout for the given
	/// layout engine <paramref name="engine"/>.
	/// </summary>
	/// <param name="engine"></param>
	/// <returns></returns>
	Direction GetAddWindowDirection(TreeLayoutEngine engine);
}
