namespace Whim.TreeLayout;

/// <summary>
/// A leaf node in the <see cref="ITreeLayoutEngine"/>
/// </summary>
internal abstract class LeafNode : Node
{
	/// <summary>
	/// The window contained by the node.
	/// </summary>
	public IWindow Window { get; }

	internal LeafNode(IWindow windowModel, SplitNode? parent = null)
	{
		Window = windowModel;
		Parent = parent;
	}

	/// <summary>
	/// Moves the focus to this node.
	/// </summary>
	public void Focus() => Window.Focus();
}
