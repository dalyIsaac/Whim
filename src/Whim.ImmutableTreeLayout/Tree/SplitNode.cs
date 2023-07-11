using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Whim.ImmutableTreeLayout;

/// <summary>
/// SplitNodes dictate the layout of the windows. They have a specific direction, and
/// children.
/// </summary>
internal class SplitNode : ISplitNode
{
	public ImmutableList<double> Weights { get; }

	public ImmutableList<INode> Children { get; }

	public int Count => Children.Count;

	public bool EqualWeight { get; }

	public bool IsHorizontal { get; }

	/// <summary>
	/// Creates a new <see cref="SplitNode"/> to replace and absorb <paramref name="focusedNode"/>.
	/// </summary>
	/// <param name="focusedNode">The currently focused node.</param>
	/// <param name="newNode">The new node to add.</param>
	/// <param name="direction">The direction to add the split node in.</param>
	public SplitNode(INode focusedNode, INode newNode, Direction direction)
	{
		ImmutableList<INode>.Builder children = ImmutableList.CreateBuilder<INode>();
		ImmutableList<double>.Builder weights = ImmutableList.CreateBuilder<double>();

		if (direction.InsertAfter())
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

		Children = children.ToImmutable();
		Weights = weights.ToImmutable();
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
	public SplitNode(bool equalWeight, bool isHorizontal, ImmutableList<INode> children, ImmutableList<double> weights)
	{
		EqualWeight = equalWeight;
		IsHorizontal = isHorizontal;

		Children = children;
		Weights = weights;
	}

	public ISplitNode Add(INode focusedNode, INode newNode, bool insertAfter)
	{
		Logger.Verbose($"Adding {newNode} to {this}, {(insertAfter ? "after" : "before")} {focusedNode}");

		// Find the index of the focused node.
		int idx = Children.IndexOf(focusedNode);
		if (idx < 0)
		{
			Logger.Error($"Could not find {focusedNode} in {this}");
			return this;
		}

		int focusedNodeIdx = idx;

		// Find the index of the new node.
		int delta = insertAfter ? 1 : 0;
		int newNodeIdx = Math.Clamp(focusedNodeIdx + delta, 0, Children.Count);

		// Insert the node.
		ImmutableList<INode> children = Children.Insert(newNodeIdx, newNode);
		ImmutableList<double> weights;

		if (EqualWeight || Weights.Count == 0)
		{
			// Add the weight 1d, since it doesn't matter.
			weights = Weights.Add(1d);
		}
		else
		{
			// Take half of the last window's space.
			double half = Weights[^1] / 2;
			weights = Weights.SetItem(Weights.Count - 1, half);
			weights = weights.Add(half);
		}

		return new SplitNode(EqualWeight, IsHorizontal, children, weights);
	}

	public ISplitNode Remove(int index)
	{
		Logger.Verbose($"Removing node at index {index} from {this}");

		if (index < 0 || index >= Children.Count)
		{
			Logger.Error($"Index {index} is out of range for {this}");
			return this;
		}

		ImmutableList<INode> children = Children.RemoveAt(index);
		ImmutableList<double> weights;

		// Redistribute weights.
		if (EqualWeight)
		{
			// We don't care about the weights, so just remove the weight.
			weights = Weights.RemoveAt(index);
		}
		else
		{
			// Give the extra weight to the last child.
			weights = Weights.SetItem(Weights.Count - 1, Weights[^1] + Weights[index]);
			weights = weights.RemoveAt(index);
		}

		return new SplitNode(EqualWeight, IsHorizontal, children, weights);
	}

	public ISplitNode Replace(int index, INode newNode)
	{
		Logger.Verbose($"Replacing the node at index {index} with {newNode}");

		if (index < 0 || index >= Children.Count)
		{
			Logger.Error($"Index {index} is out of range for {this}");
			return this;
		}

		ImmutableList<INode> children = Children.SetItem(index, newNode);
		return new SplitNode(EqualWeight, IsHorizontal, children, Weights);
	}

	public ISplitNode Swap(int aIndex, int bIndex)
	{
		if (aIndex < 0 || aIndex >= Children.Count)
		{
			Logger.Error($"Index {aIndex} is out of range for {this}");
			return this;
		}

		if (bIndex < 0 || bIndex >= Children.Count)
		{
			Logger.Error($"Index {bIndex} is out of range for {this}");
			return this;
		}

		ImmutableList<INode> children = Children.SetItem(aIndex, Children[bIndex]);
		children = children.SetItem(bIndex, Children[aIndex]);

		return new SplitNode(EqualWeight, IsHorizontal, children, Weights);
	}

	/// <summary>
	/// Returns a new list of weights that are distributed evenly. Before calling this method,
	/// we didn't care about the weights and calculated them manually based on the count.
	/// </summary>
	/// <returns></returns>
	private ImmutableList<double> DistributeWeights()
	{
		double weight = 1d / Children.Count;

		ImmutableList<double>.Builder builder = ImmutableList.CreateBuilder<double>();
		for (int i = 0; i < Children.Count; i++)
		{
			builder.Add(weight);
		}
		return builder.ToImmutable();
	}

	public ISplitNode AdjustChildWeight(int index, double delta)
	{
		if (index < 0 || index >= Children.Count)
		{
			Logger.Error($"Index {index} is out of range for {this}");
			return this;
		}

		ImmutableList<double> weights = EqualWeight ? DistributeWeights() : Weights;
		weights = weights.SetItem(index, weights[index] + delta);

		return new SplitNode(false, IsHorizontal, Children, weights);
	}

	public ISplitNode ToggleEqualWeight()
	{
		Logger.Verbose($"Toggling EqualWeight of {this}");

		return new SplitNode(!EqualWeight, IsHorizontal, Children, DistributeWeights());
	}

	public IEnumerator<(double Weight, INode Node)> GetEnumerator()
	{
		if (EqualWeight)
		{
			double weight = 1d / Children.Count;
			foreach (INode child in Children)
			{
				yield return (weight, child);
			}
		}
		else
		{
			for (int i = 0; i < Weights.Count; i++)
			{
				yield return (Weights[i], Children[i]);
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	// TODO: test
	public (double Weight, INode Node) this[int index] => (Weights[index], Children[index]);
}
