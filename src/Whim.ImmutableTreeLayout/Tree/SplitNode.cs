using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Whim.ImmutableTreeLayout;

/// <summary>
/// SplitNodes dictate the layout of the windows. They have a specific direction, and
/// children.
/// </summary>
internal class SplitNode : Node, IEnumerable<(double Weight, Node Node)>
{
	/// <summary>
	/// The weights of the children. If <see cref="EqualWeight"/> is <see langword="true"/>, then
	/// the weights here are ignored in favour of <code>1d / _children.Count</code>.
	/// </summary>
	private readonly ImmutableList<double> _weights;

	/// <summary>
	/// The child nodes of this <see cref="SplitNode"/>. These will likely be either <see cref="SplitNode"/>s,
	/// or child classes of <see cref="LeafNode"/>.
	/// </summary>
	protected readonly ImmutableList<Node> _children;

	/// <summary>
	/// The number of nodes in this <see cref="SplitNode"/>.
	/// </summary>
	public int Count => _children.Count;

	/// <summary>
	/// When <see langword="true"/>, the <see cref="_children"/> are split
	/// within the parent node equally. This overrides the weights in <see cref="_weights"/>.
	/// </summary>
	public bool EqualWeight { get; }

	/// <summary>
	/// When <see langword="true"/>, the <see cref="_children"/> are arranged horizontally.
	/// Otherwise, they are arranged vertically.
	/// </summary>
	public bool IsHorizontal { get; }

	/// <summary>
	/// Gets the weight and node of the child at the given index.
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public (double weight, Node node) this[int index] =>
		(EqualWeight ? 1d / _weights.Count : _weights[index], _children[index]);

	/// <summary>
	/// Creates a new <see cref="SplitNode"/> to replace and absorb <paramref name="focusedNode"/>.
	/// </summary>
	/// <param name="focusedNode">The currently focused node.</param>
	/// <param name="newNode">The new node to add.</param>
	/// <param name="direction">The direction to add the split node in.</param>
	public SplitNode(Node focusedNode, Node newNode, Direction direction)
	{
		ImmutableList<Node>.Builder children = ImmutableList.CreateBuilder<Node>();
		ImmutableList<double>.Builder weights = ImmutableList.CreateBuilder<double>();

		if (direction.IsPositiveIndex())
		{
			children.Add(focusedNode);
			children.Add(newNode);
		}
		else
		{
			children.Add(newNode);
			children.Add(focusedNode);
		}

		double half = 1d / 2;
		weights.Add(half);
		weights.Add(half);

		_children = children.ToImmutable();
		_weights = weights.ToImmutable();
		IsHorizontal = direction.IsHorizontal();
		EqualWeight = true;
	}

	/// <summary>
	/// Creates a new <see cref="SplitNode"/>. This is used to create a replacement for an existing
	/// <see cref="SplitNode"/>.
	/// </summary>
	/// <param name="equalWeight"></param>
	/// <param name="isHorizontal"></param>
	/// <param name="children"></param>
	/// <param name="weights"></param>
	private SplitNode(bool equalWeight, bool isHorizontal, ImmutableList<Node> children, ImmutableList<double> weights)
	{
		EqualWeight = equalWeight;
		IsHorizontal = isHorizontal;

		_children = children;
		_weights = weights;
	}

	/// <summary>
	/// Adds the given <paramref name="newNode"/> to this <see cref="SplitNode"/>, in the given
	/// <paramref name="direction"/>, in relation to the given <paramref name="focusedNode"/>.
	/// If <paramref name="focusedNode"/> is <see langword="null"/>, then the new node is added
	/// to the end of the list.
	/// </summary>
	/// <param name="focusedNode"></param>
	/// <param name="newNode"></param>
	/// <param name="direction"></param>
	/// <returns></returns>
	public SplitNode Add(Node focusedNode, Node newNode, Direction direction)
	{
		Logger.Verbose($"Adding {newNode} to {this}, in direction {direction} in relation to {focusedNode}");

		// Find the index of the focused node.
		int idx = _children.IndexOf(focusedNode);
		if (idx < 0)
		{
			Logger.Error($"Could not find {focusedNode} in {this}");
			return this;
		}

		int focusedNodeIdx = idx;

		// Find the index of the new node.
		int delta;
		switch (direction)
		{
			case Direction.Right:
			case Direction.Down:
				delta = 1;
				break;
			case Direction.Left:
			case Direction.Up:
				delta = 0;
				break;
			default:
				Logger.Error($"{direction} is not a valid direction to add a node in, defaulting to a delta of 1");
				delta = 1;
				break;
		}

		int newNodeIdx = Math.Clamp(focusedNodeIdx + delta, 0, _children.Count);

		// Insert the node.
		ImmutableList<Node> children = _children.Insert(newNodeIdx, newNode);
		ImmutableList<double> weights;

		if (EqualWeight || _weights.Count == 0)
		{
			// Add the weight 1d, since it doesn't matter.
			weights = _weights.Add(1d);
		}
		else
		{
			// Take half of the last window's space.
			double half = _weights[^1] / 2;
			weights = _weights.SetItem(_weights.Count - 1, half);
			weights = weights.Add(half);
		}

		return new SplitNode(EqualWeight, IsHorizontal, children, weights);
	}

	/// <summary>
	/// Removes the given <paramref name="node"/> from this <see cref="SplitNode"/>.
	/// If this <see cref="SplitNode"/> is not <see cref="EqualWeight"/>, then the weight of the
	/// last node is increased by the weight of the removed node.
	/// </summary>
	/// <param name="node"></param>
	/// <returns></returns>
	public SplitNode Remove(Node node)
	{
		Logger.Verbose($"Removing {node} from {this}");

		int idx = _children.IndexOf(node);
		if (idx < 0)
		{
			Logger.Error($"Could not find {node} in {this}");
			return this;
		}

		ImmutableList<Node> children = _children.RemoveAt(idx);
		ImmutableList<double> weights;

		// Redistribute weights.
		if (EqualWeight)
		{
			// We don't care about the weights, so just remove the weight.
			weights = _weights.RemoveAt(idx);
		}
		else
		{
			// Give the extra weight to the last child.
			weights = _weights.SetItem(_weights.Count - 1, _weights[^1] + _weights[idx]);
			weights = weights.RemoveAt(idx);
		}

		return new SplitNode(EqualWeight, IsHorizontal, children, weights);
	}

	/// <summary>
	/// Replaces the given <paramref name="oldNode"/> with the given <paramref name="newNode"/>.
	/// </summary>
	/// <param name="oldNode">The node to replace.</param>
	/// <param name="newNode">The node to replace <paramref name="oldNode"/> with.</param>
	/// <returns></returns>
	public SplitNode Replace(Node oldNode, Node newNode)
	{
		Logger.Verbose($"Replacing {oldNode} with {newNode} in {this}");

		int idx = _children.IndexOf(oldNode);
		if (idx < 0)
		{
			Logger.Error($"Could not find {oldNode} in {this}");
			return this;
		}

		ImmutableList<Node> children = _children.SetItem(idx, newNode);
		return new SplitNode(EqualWeight, IsHorizontal, children, _weights);
	}

	/// <summary>
	/// Swaps the given <paramref name="a"/> and <paramref name="b"/> in this <see cref="SplitNode"/>.
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns></returns>
	public SplitNode Swap(Node a, Node b)
	{
		Logger.Verbose($"Swapping {a} and {b} in {this}");

		int aIdx = -1;
		int bIdx = -1;

		for (int i = 0; i < _children.Count; i++)
		{
			if (_children[i] == a)
			{
				aIdx = i;
			}
			else if (_children[i] == b)
			{
				bIdx = i;
			}

			if (aIdx != -1 && bIdx != -1)
			{
				break;
			}
		}

		if (aIdx == -1 || bIdx == -1)
		{
			Logger.Error($"Failed to swap {a} and {b} in {this}");
			return this;
		}

		ImmutableList<Node> children = _children.SetItem(aIdx, b);
		children = children.SetItem(bIdx, a);

		return new SplitNode(EqualWeight, IsHorizontal, children, _weights);
	}

	/// <summary>
	/// Returns a new list of weights that are distributed evenly. Before calling this method,
	/// we didn't care about the weights and calculated them manually based on the count.
	/// </summary>
	/// <returns></returns>
	private ImmutableList<double> DistributeWeights()
	{
		double weight = 1d / _children.Count;

		ImmutableList<double>.Builder builder = ImmutableList.CreateBuilder<double>();
		for (int i = 0; i < _children.Count; i++)
		{
			builder.Add(weight);
		}
		return builder.ToImmutable();
	}

	/// <summary>
	/// Changes the weight of the given <paramref name="node"/> by <paramref name="delta"/>.
	///
	/// This method does not change the weight of the other children.
	/// </summary>
	/// <param name="node"></param>
	/// <param name="delta"></param>
	/// <returns></returns>
	public SplitNode AdjustChildWeight(Node node, double delta)
	{
		int idx = _children.IndexOf(node);
		if (idx < 0)
		{
			Logger.Error($"Node {node} not found in {this}");
			return this;
		}

		ImmutableList<double> weights = EqualWeight ? DistributeWeights() : _weights;
		weights = weights.SetItem(idx, weights[idx] + delta);

		return new SplitNode(false, IsHorizontal, _children, weights);
	}

	/// <summary>
	/// Gets the weight of the node at the given index.
	/// </summary>
	/// <param name="idx"></param>
	/// <returns></returns>
	private double GetChildWeight(int idx) => EqualWeight ? 1d / _children.Count : _weights[idx];

	/// <summary>
	/// Get the weight of the given <paramref name="node"/>.
	/// The <paramref name="node"/> must be a child of this <see cref="SplitNode"/>.
	/// </summary>
	/// <param name="node"></param>
	/// <returns></returns>
	public double? GetChildWeight(Node node)
	{
		int idx = _children.IndexOf(node);
		if (idx < 0)
		{
			Logger.Error($"Node {node} not found in {this}");
			return null;
		}

		return GetChildWeight(idx);
	}

	/// <summary>
	/// Gets the weight of the given <paramref name="node"/>, and the sum of its
	/// preceding siblings.
	/// The <paramref name="node"/> must be a child of this <see cref="SplitNode"/>.
	/// </summary>
	/// <param name="node"></param>
	/// <returns></returns>
	public (double weight, double precedingWeight)? GetWeightAndPrecedingWeight(Node node)
	{
		int idx = _children.IndexOf(node);
		if (idx < 0)
		{
			Logger.Error($"Node {node} not found in {this}");
			return null;
		}

		double weight;
		double precedingWeight;

		if (EqualWeight)
		{
			weight = 1d / _children.Count;
			precedingWeight = idx * weight;
		}
		else
		{
			weight = _weights[idx];
			precedingWeight = _weights.Take(idx).Sum();
		}

		return (weight, precedingWeight);
	}

	/// <summary>
	/// Flips the orientation of this <see cref="SplitNode"/>.
	/// </summary>
	/// <returns></returns>
	public SplitNode Flip()
	{
		Logger.Verbose($"Flipping {this}");

		return new SplitNode(EqualWeight, !IsHorizontal, _children, _weights);
	}

	/// <summary>
	/// Toggles the <see cref="EqualWeight"/> property of this <see cref="SplitNode"/>.
	/// </summary>
	/// <returns></returns>
	public SplitNode ToggleEqualWeight()
	{
		Logger.Verbose($"Toggling EqualWeight of {this}");

		return new SplitNode(!EqualWeight, IsHorizontal, _children, DistributeWeights());
	}

	/// <summary>
	/// Merge the given <paramref name="child"/> into this <see cref="SplitNode"/>, at its current
	/// position.
	/// </summary>
	/// <param name="child">The child to merge.</param>
	/// <returns></returns>
	public SplitNode MergeChild(SplitNode child)
	{
		Logger.Verbose($"Merging {child} into {this}");

		int idx = _children.IndexOf(child);
		if (idx < 0)
		{
			Logger.Error($"Node {child} not found in {this}");
			return this;
		}

		ImmutableList<Node> children = _children.RemoveAt(idx);
		children = children.InsertRange(idx, child._children);

		double childWeight = GetChildWeight(idx);
		ImmutableList<double> weights = _weights.RemoveAt(idx);
		if (child.EqualWeight)
		{
			double grandchildrenWeight = childWeight / child._children.Count;
			weights = weights.InsertRange(idx, Enumerable.Repeat(grandchildrenWeight, child._children.Count));
		}
		else
		{
			weights = weights.InsertRange(idx, child._weights.Select(w => w * childWeight));
		}

		return new SplitNode(false, IsHorizontal, children, weights);
	}

	/// <summary>
	/// Gets the child nodes and their weights.
	/// </summary>
	/// <returns></returns>
	public IEnumerator<(double Weight, Node Node)> GetEnumerator()
	{
		if (EqualWeight)
		{
			double weight = 1d / _children.Count;
			foreach (Node child in _children)
			{
				yield return (weight, child);
			}
		}
		else
		{
			for (int i = 0; i < _weights.Count; i++)
			{
				yield return (_weights[i], _children[i]);
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
