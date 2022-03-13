namespace Whim.TreeLayout;

public abstract class LeafNode : Node
{
	public IWindow Window { get; }

	internal LeafNode(IWindow windowModel, SplitNode? parent = null)
	{
		Window = windowModel;
		Parent = parent;
	}

	public void Focus()
	{
		Window.Focus();
	}
}
