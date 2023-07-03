namespace Whim.ImmutableTreeLayout;

/// <summary>
/// A phantom node represents a phantom window within the layout tree.
/// Unlike a <see cref="WindowNode"/>, a phantom node is specific to the
/// layout engine.
/// As such phantom nodes have to manage the window itself, instead of relying
/// on the <see cref="IWindowManager"/>.
/// </summary>
internal class PhantomNode : LeafNode
{
	/// <summary>
	/// Creates a new phantom node.
	/// </summary>
	/// <param name="window"></param>
	public PhantomNode(IWindow window)
		: base(window) { }
}
