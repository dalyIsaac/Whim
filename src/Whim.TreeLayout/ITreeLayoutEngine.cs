namespace Whim.TreeLayout;

/// <summary>
/// A tree layout engine allows users to create arbitrary window grid layouts.
/// </summary>
public interface ITreeLayoutEngine : ILayoutEngine
{
	/// <summary>
	/// The direction which we will use for any following operations.
	/// </summary>
	Direction AddNodeDirection { get; set; }

	/// <summary>
	/// The root node of the tree.
	/// </summary>
	Node? Root { get; }

	/// <summary>
	/// Add the <paramref name="window"/> to the layout engine.
	/// The direction it is added in is determined by this instance's <see cref="AddNodeDirection"/> property.
	/// </summary>
	/// <param name="window">The window to add.</param>
	new void Add(IWindow window);

	/// <summary>
	/// Add the <paramref name="window"/> to the layout engine, in a
	/// <paramref name="direction"/> to the currently focused window.
	/// </summary>
	/// <param name="window">The window to add.</param>
	/// <param name="direction">
	/// The direction to add the window, in relation to the currently focused window.
	/// </param>
	void Add(IWindow window, Direction direction);

	/// <summary>
	/// Adds a window to the layout engine, and returns the node that represents it.
	/// The <paramref name="window"/> is added in the direction specified by this instance's
	/// <see cref="AddNodeDirection"/> property.
	/// </summary>
	/// <param name="window">The window to add.</param>
	/// <param name="focusedWindow">The currently focused window from whom to get the node.</param>
	/// <returns>The node that represents the window.</returns>
	WindowNode? AddWindow(IWindow window, IWindow? focusedWindow = null);

	/// <summary>
	/// Flip the direction of the <see cref="SplitNode"/> parent of the currently focused window, and merge it with
	/// the grandparent <see cref="SplitNode"/>.
	/// </summary>
	void FlipAndMerge();

	/// <summary>
	/// Gets the adjacent node in the given <paramref name="direction"/>.
	/// </summary>
	/// <param name="node">The node to get the adjacent node for.</param>
	/// <param name="direction">The direction to get the adjacent node in.</param>
	/// <returns>
	/// The adjacent node in the given <paramref name="direction"/>.
	/// <see langword="null"/> if there is no adjacent node in the given <paramref name="direction"/>,
	/// or an error occurred.
	/// </returns>
	LeafNode? GetAdjacentNode(LeafNode node, Direction direction);

	/// <summary>
	/// Split the focused window in two, and insert a phantom window in the direction
	/// of <see cref="AddNodeDirection"/>.
	/// </summary>
	void SplitFocusedWindow();
}
