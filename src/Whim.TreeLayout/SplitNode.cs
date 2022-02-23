using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Whim.TreeLayout;

public class SplitNode : Node, IEnumerable<(double Weight, Node Node)>
{
	/// <summary>
	/// When <see langword="true"/>, the <see cref="_children"/> are split
	/// within the parent node equally. This overrides the <see cref="Node.Weight"/>.
	/// </summary>
	public bool EqualWeight { get; protected set; } = true;

	/// <summary>
	/// The direction to split the <see cref="_children"/>.
	/// </summary>
	public NodeDirection Direction { get; set; }

	protected readonly List<double> _weights = new();

	protected readonly List<Node> _children = new();

	public int Count => _children.Count;

	public SplitNode(NodeDirection direction, SplitNode? parent = null)
	{
		Parent = parent;
		Direction = direction;
	}

	public (double weight, Node node) this[int index] => (
		EqualWeight ? 1d / _weights.Count : _weights[index],
		_children[index]
	);

	internal void Add(Node node)
	{
		// See https://stackoverflow.com/questions/27242588/cannot-access-protected-member
		node.Parent = this;
		_children.Add(node);

		if (EqualWeight || _weights.Count == 0)
		{
			_weights.Add(1d);
		}
		else
		{
			double half = _weights[^1] / 2;
			_weights[^1] = half;
			_weights.Add(half);
		}
	}

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

	internal bool Replace(Node oldNode, Node newNode)
	{
		int idx = _children.IndexOf(oldNode);
		if (idx < 0)
		{
			Logger.Error($"Node {oldNode} not found in {this}");
			return false;
		}

		Replace(idx, newNode);
		return true;
	}

	internal void Replace(int idx, Node newNode)
	{
		_children[idx] = newNode;

		// Update the parent references.
		foreach (Node child in _children)
		{
			child.Parent = this;
		}
	}

	internal void AdjustChildWeight(Node node, double delta)
	{
		int idx = _children.IndexOf(node);
		if (idx < 0)
		{
			Logger.Error($"Node {node} not found in {this}");
			return;
		}

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

		_weights[idx] += delta;
	}

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

	public (double weight, double precedingWeight)? GetWeightAndPrecedingWeight(Node node)
	{
		int idx = _children.IndexOf(node);
		if (idx < 0)
		{
			Logger.Error($"Node {node} not found in {this}");
			return null;
		}

		double weight, precedingWeight;

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

	// override object.Equals
	public override bool Equals(object? obj)
	{
		//
		// See the full list of guidelines at
		//   http://go.microsoft.com/fwlink/?LinkID=85237
		// and also the guidance for operator== at
		//   http://go.microsoft.com/fwlink/?LinkId=85238
		//

		if (obj == null || obj is not SplitNode)
		{
			return false;
		}

		return obj is SplitNode node &&
			node.EqualWeight == EqualWeight &&
			node.Direction == Direction &&
			// Checking for parent equality is too dangerous, as there are cycles.
			((node.Parent == null) == (Parent == null)) &&
			node.SequenceEqual(this);
	}

	// override object.GetHashCode
	public override int GetHashCode() => HashCode.Combine(EqualWeight, Direction, this);
}
