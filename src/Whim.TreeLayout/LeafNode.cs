namespace Whim.TreeLayout;

public class LeafNode : Node
{
	public IWindow Window { get; set; }

	public LeafNode(IWindow window, SplitNode? parent = null)
	{
		Window = window;
		Parent = parent;
	}

	public override string? ToString()
	{
		return Window?.ToString();
	}
}
