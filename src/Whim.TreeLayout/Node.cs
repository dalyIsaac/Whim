namespace Whim.TreeLayout;

public abstract class Node
{
	public SplitNode? Parent { get; set; }

	public double Weight { get; set; } = 1;
}
