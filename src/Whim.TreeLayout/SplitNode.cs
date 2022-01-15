using System.Collections.Generic;

namespace Whim.TreeLayout;

public class SplitNode : Node
{
	/// <summary>
	/// When <see langword="true"/>, the <see cref="Children"/> are split
	/// within the parent node equally. This overrides the <see cref="Node.Weight"/>.
	/// </summary>
	public bool EqualWeight { get; set; } = true;

	/// <summary>
	/// The direction to split the <see cref="Children"/>.
	/// </summary>
	public NodeDirection Direction { get; set; }

	public List<Node> Children { get; set; } = new();

	public SplitNode(NodeDirection direction, SplitNode? parent = null)
	{
		Parent = parent;
		Direction = direction;
	}
}
