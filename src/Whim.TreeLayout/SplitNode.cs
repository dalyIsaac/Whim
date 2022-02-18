using System;
using System.Collections.Generic;
using System.Linq;

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

	// override object.Equals
	public override bool Equals(object? obj)
	{
		//
		// See the full list of guidelines at
		//   http://go.microsoft.com/fwlink/?LinkID=85237
		// and also the guidance for operator== at
		//   http://go.microsoft.com/fwlink/?LinkId=85238
		//

		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}

		var result = obj is SplitNode node &&
			node.EqualWeight == EqualWeight &&
			node.Direction == Direction &&
			// Checking for parent equality is too dangerous, as there are cycles.
			((node.Parent == null) == (Parent == null)) &&
			node.Children.SequenceEqual(Children);

		if (result == false)
		{
			//throw new Exception("LeafNode equality check failed");
			Console.WriteLine("Hello world!");
		}

		return result;
	}

	// override object.GetHashCode
	public override int GetHashCode() => HashCode.Combine(EqualWeight, Direction, Children);
}
