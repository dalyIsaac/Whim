namespace Whim.ImmutableTreeLayout;

/// <summary>
/// A leaf node in a tree.
/// </summary>
internal abstract class LeafNode : INode
{
	/// <summary>
	/// The window contained by the node.
	/// </summary>
	public IWindow Window { get; }

	internal LeafNode(IWindow window)
	{
		Window = window;
	}

	/// <summary>
	/// Moves the focus to this node.
	/// </summary>
	public void Focus() => Window.Focus();
}