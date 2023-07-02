namespace Whim.ImmutableTreeLayout;

/// <summary>
/// A leaf node in a tree.
/// </summary>
internal abstract class LeafNode : Node
{
	/// <summary>
	/// The window contained by the node.
	/// </summary>
	public IWindow Window { get; }

	internal LeafNode(IWindow windowModel)
	{
		Window = windowModel;
	}

	/// <summary>
	/// Moves the focus to this node.
	/// </summary>
	public void Focus() => Window.Focus();
}
