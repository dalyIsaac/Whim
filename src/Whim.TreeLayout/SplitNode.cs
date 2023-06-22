using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Whim.TreeLayout;

/// <summary>
/// SplitNodes dictate the layout of the windows. They have a specific direction, and
/// children.
/// </summary>
internal class SplitNode : Node, IEnumerable<(double Weight, Node Node)>
{
	/// <summary>
	/// When <see langword="true"/>, the <see cref="_children"/> are split
	/// within the parent node equally. This overrides the weights in <see cref="_weights"/>.
	/// </summary>
	public bool EqualWeight { get; protected set; } = true;

	/// <summary>
	/// When <see langword="true"/>, the <see cref="_children"/> are arranged horizontally.
	/// Otherwise, they are arranged vertically.
	/// </summary>
	public bool IsHorizontal { get; private set; }

	/// <summary>
	/// The weights of the children. If <see cref="EqualWeight"/> is <see langword="true"/>, then
	/// the weights here are ignored in favour of <code>1d / _children.Count</code>.
	/// </summary>
	protected readonly List<double> _weights = new();

	/// <summary>
	/// The child nodes of this <see cref="SplitNode"/>. These will likely be either <see cref="SplitNode"/>s,
	/// or child classes of <see cref="LeafNode"/>.
	/// </summary>
	protected readonly List<Node> _children = new();

	/// <summary>
	/// The number of nodes in this <see cref="SplitNode"/>.
	/// </summary>
	public int Count => _children.Count;

	/// <summary>
	/// Creates a new <see cref="SplitNode"/>.
	/// </summary>
	/// <param name="isHorizontal">The direction of this node.</param>
	/// <param name="parent">The parent of this node.</param>
	public SplitNode(bool isHorizontal = true, SplitNode? parent = null)
	{
		Parent = parent;
		IsHorizontal = isHorizontal;
	}

	/// <summary>
	/// Creates a new <see cref="SplitNode"/> to replace and absorb <paramref name="focusedNode"/>.
	/// </summary>
	/// <param name="focusedNode">The currently focused node.</param>
	/// <param name="newNode">The new node to add.</param>
	/// <param name="direction">The direction to add the split node in.</param>
	/// <param name="parent">The parent of this node.</param>
	public SplitNode(Node focusedNode, Node newNode, Direction direction, SplitNode? parent = null)
		: this(direction.IsHorizontal(), parent)
	{
		if (direction.IsPositiveIndex())
		{
			Add(focusedNode);
			Add(newNode);
		}
		else
		{
			Add(newNode);
			Add(focusedNode);
		}
	}

	/// <summary>
	/// Gets the weight and node of the child at the given index.
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public (double weight, Node node) this[int index] =>
		(EqualWeight ? 1d / _weights.Count : _weights[index], _children[index]);

	/// <summary>
	/// Add the given node as a child of this <see cref="SplitNode"/>. This assumes that
	/// the window has already been added to the layout engine.
	/// This method will also update <paramref name="node"/>'s <see cref="Node.Parent"/>
	/// </summary>
	/// <param name="node"></param>
	internal void Add(Node node)
	{
		Logger.Verbose($"Adding {node} to {this}");

		node.Parent = this;
		_children.Add(node);

		if (EqualWeight || _weights.Count == 0)
		{
			// Add the weight 1d, since it doesn't matter.
			_weights.Add(1d);
		}
		else
		{
			// Take half of the last window's space.
			double half = _weights[^1] / 2;
			_weights[^1] = half;
			_weights.Add(half);
		}
	}

	/// <summary>
	/// Add the <paramref name="newNode"/> as a child of this <see cref="SplitNode"/>,
	/// in relation to the <paramref name="existingFocusedNode"/>.
	/// </summary>
	/// <param name="existingFocusedNode"></param>
	/// <param name="newNode"></param>
	/// <param name="direction"></param>
	internal void Add(Node existingFocusedNode, Node newNode, Direction direction)
	{
		Logger.Debug($"Adding {newNode} to {this}, in direction {direction}");

		// Find the index of the focused node.
		int index = _children.IndexOf(existingFocusedNode);
		if (index == -1)
		{
			Logger.Error($"Failed to find focused node {existingFocusedNode} in {this}");
			return;
		}

		// TODO: These two if statements can be combined/removed
		int newNodeIndex = index + (direction.IsPositiveIndex() ? 1 : 0);

		// Bound the index.
		if (newNodeIndex < 0)
		{
			newNodeIndex = 0;
		}
		else if (newNodeIndex > _children.Count)
		{
			newNodeIndex = _children.Count;
		}

		// Insert the node.
		_children.Insert(newNodeIndex, newNode);
		newNode.Parent = this;

		// Insert the weight.
		if (EqualWeight || _weights.Count == 0)
		{
			// Add the weight 1d, since it doesn't matter.
			_weights.Insert(newNodeIndex, 1d);
		}
		else
		{
			// Take half of the focused node's space.
			double half = _weights[index] / 2;
			_weights[index] = half;
			_weights.Insert(newNodeIndex, half);
		}
	}

	/// <summary>
	/// Remove the <paramref name="node"/> from this <see cref="SplitNode"/>.
	/// </summary>
	/// <param name="node"></param>
	/// <returns>Whether the removal was successful.</returns>
	internal bool Remove(Node node)
	{
		int idx = _children.IndexOf(node);
		if (idx < 0)
		{
			Logger.Error($"Node {node} not found in {this}");
			return false;
		}

		_children.RemoveAt(idx);

		double weight = _weights[idx];
		_weights.RemoveAt(idx);

		// Redistribute weights.
		if (!EqualWeight)
		{
			// Give the extra weight to the last child.
			_weights[^1] += weight;
		}

		node.Parent = null;

		return true;
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

	/// <summary>
	/// Replace the old node with the new node, in this <see cref="SplitNode"/>'s list of children.
	/// This does not update the <see cref="Node.Parent"/> or children of the old node.
	/// </summary>
	/// <param name="oldNode">The node to be replaced.</param>
	/// <param name="newNode">The node to take the place of the old node.</param>
	/// <returns><see langword="true"/> if the node was found and replaced, <see langword="false"/> otherwise.</returns>
	internal bool Replace(Node oldNode, Node newNode)
	{
		int idx = _children.IndexOf(oldNode);
		if (idx < 0)
		{
			Logger.Error($"Node {oldNode} not found in {this}");
			return false;
		}

		// Replace the child at idx with the new node.
		_children[idx] = newNode;
		newNode.Parent = this;

		return true;
	}

	/// <summary>
	/// Swap the two nodes in this <see cref="SplitNode"/>'s list of children.
	/// Both nodes must be children of this <see cref="SplitNode"/>.
	/// </summary>
	/// <param name="a">The first node to swap.</param>
	/// <param name="b">The second node to swap.</param>
	/// <returns><see langword="true"/> if the nodes were found and swapped, <see langword="false"/> otherwise.</returns>
	internal bool Swap(Node a, Node b)
	{
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
			return false;
		}

		// Swap the nodes.
		(_children[aIdx], _children[bIdx]) = (_children[bIdx], _children[aIdx]);
		return true;
	}

	/// <summary>
	/// If <see cref="SplitNode.EqualWeight"/> is <see langword="true"/>, this will
	/// distribute the weights of the children of this <see cref="SplitNode"/>,
	/// so the children have equal weight, but <see cref="SplitNode.EqualWeight"/>
	/// is <see langword="false"/>.
	/// </summary>
	private void DistributeWeight()
	{
		if (EqualWeight)
		{
			// Distribute weights, because previously we didn't
			// care about them and calculated weights based on
			// the count.
			double weight = 1d / _children.Count;
			for (int i = 0; i < _weights.Count; i++)
			{
				_weights[i] = weight;
			}
			EqualWeight = false;
		}
	}

	/// <summary>
	/// This changes the weight of the <paramref name="node"/> to by
	/// <paramref name="delta"/>.
	///
	/// The <paramref name="delta"/> must be a child of this <see cref="SplitNode"/>
	/// instance.
	///
	/// This method does not guarantee that the weights of the children will
	/// sum to 1.
	/// </summary>
	/// <param name="node">The node whose weight is to be changed.</param>
	/// <param name="delta">
	/// The amount to change the weight by. The delta should continue the assumption
	/// the coordinates for this node are in the range [0, 1].
	/// </param>
	internal void AdjustChildWeight(Node node, double delta)
	{
		int idx = _children.IndexOf(node);
		if (idx < 0)
		{
			Logger.Error($"Node {node} not found in {this}");
			return;
		}

		DistributeWeight();

		_weights[idx] += delta;
	}

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

		return EqualWeight ? 1d / _children.Count : _weights[idx];
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
	/// Flip the direction.
	/// </summary>
	internal void Flip() => IsHorizontal = !IsHorizontal;

	/// <summary>
	/// Merge the given child <see cref="SplitNode"/> into this one, at it's current
	/// position.
	/// </summary>
	/// <param name="child">The child to merge.</param>
	internal void MergeChild(SplitNode child)
	{
		int idx = _children.IndexOf(child);
		if (idx < 0)
		{
			Logger.Error($"Node {child} not found in {this}");
			return;
		}

		DistributeWeight();

		// Insert the child's children into this node.
		double childWeight = GetChildWeight(child) ?? 1d;
		_children.RemoveAt(idx);
		_children.InsertRange(idx, child._children);
		_weights.RemoveAt(idx);
		_weights.InsertRange(idx, child.Select(c => c.Weight * childWeight));

		// Update parents.
		foreach (Node grandChild in child._children)
		{
			grandChild.Parent = this;
		}
	}

	/// <inheritdoc/>
	public override bool Equals(object? obj)
	{
		//
		// See the full list of guidelines at
		//   http://go.microsoft.com/fwlink/?LinkID=85237
		// and also the guidance for operator== at
		//   http://go.microsoft.com/fwlink/?LinkId=85238
		//

		if (obj is null or not SplitNode)
		{
			return false;
		}

		return obj is SplitNode node
			&& node.EqualWeight == EqualWeight
			&& node.IsHorizontal == IsHorizontal
			&&
			// Checking for parent equality is too dangerous, as there are cycles.
			((node.Parent == null) == (Parent == null))
			&& node.SequenceEqual(this);
	}

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(EqualWeight, IsHorizontal, this);
}
