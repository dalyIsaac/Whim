using System;
using System.Collections.Generic;

namespace Whim.TreeLayout;

/// <summary>
/// The state of a node.
/// </summary>
public class NodeState
{
	/// <summary>
	/// The node.
	/// </summary>
	public Node Node { get; }

	/// <summary>
	/// The location of the node.
	/// </summary>
	public ILocation<int> Location { get; }

	/// <summary>
	/// The <see cref="WindowSize"/>.
	/// </summary>
	public WindowSize WindowSize { get; }

	/// <summary>
	/// Creates a new <see cref="NodeState"/>.
	/// </summary>
	/// <param name="node"></param>
	/// <param name="location"></param>
	/// <param name="windowSize"></param>
	public NodeState(Node node, ILocation<int> location, WindowSize windowSize)
	{
		Node = node;
		Location = location;
		WindowSize = windowSize;
	}
}

public partial class TreeLayoutEngine
{
	/// <summary>
	/// Gets the <see cref="WindowState"/> for all windows, within the unit square.
	/// </summary>
	/// <param name="node">The root node of the tree.</param>
	/// <param name="location">The location of the root node.</param>
	/// <returns></returns>
	public static IEnumerable<NodeState> GetWindowLocations(Node node, ILocation<int> location)
	{
		// If the node is a leaf node, then we can return the location, and break.
		if (node is LeafNode)
		{
			yield return new NodeState(node, location, WindowSize.Normal);

			yield break;
		}

		// If the node is not a leaf node, it's a split node.
		SplitNode parent = (SplitNode)node;

		// Perform an in-order traversal of the tree.
		double precedingWeight = 0;
		foreach ((double weight, Node child) in parent)
		{
			Location childLocation = new(x: location.X, y: location.Y, width: location.Width, height: location.Height);

			if (parent.IsHorizontal)
			{
				childLocation.X += Convert.ToInt32(precedingWeight * location.Width);
				childLocation.Width = Convert.ToInt32(weight * location.Width);
			}
			else
			{
				childLocation.Y += Convert.ToInt32(precedingWeight * location.Height);
				childLocation.Height = Convert.ToInt32(weight * location.Height);
			}

			foreach (NodeState childLocationResult in GetWindowLocations(child, childLocation))
			{
				yield return childLocationResult;
			}

			precedingWeight += weight;
		}
	}
}
