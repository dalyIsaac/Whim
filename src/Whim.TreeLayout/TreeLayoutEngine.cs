using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Whim.TreeLayout;

public class TreeLayoutEngine : ILayoutEngine
{
	private readonly IConfigContext _configContext;
	private readonly Dictionary<IWindow, LeafNode> _windows = new();
	public Node? Root { get; set; }
	public NodeDirection Direction { get; set; } = NodeDirection.Right;

	public string Name { get; set; }

	public int Count { get; private set; }

	public bool IsReadOnly => false;

	public Commander Commander { get; } = new();

	public TreeLayoutEngine(IConfigContext configContext, string name)
	{
		_configContext = configContext;
		Name = name;
	}

	public void Add(IWindow window)
	{
	}

	public void Remove(IWindow window)
	{

	public IWindow? GetFirstWindow()
	{
		Logger.Debug($"Getting first window from layout engine {Name}");
		return GetLeftMostLeaf(Root)?.Window;
	}

	public IEnumerable<IWindowLocation> DoLayout(ILocation<int> location)
	{
		throw new System.NotImplementedException();
	}

	public void FocusWindowInDirection(WindowDirection direction, IWindow window)
	{
		throw new System.NotImplementedException();
	}

	public void SwapWindowInDirection(WindowDirection direction, IWindow window)
	{
		throw new System.NotImplementedException();
	}

	public void Clear()
	{
		throw new System.NotImplementedException();
	}

	public bool Contains(IWindow item)
	{
		throw new System.NotImplementedException();
	}

	public void CopyTo(IWindow[] array, int arrayIndex)
	{
		throw new System.NotImplementedException();
	}

	bool ICollection<IWindow>.Remove(IWindow item)
	{
		throw new System.NotImplementedException();
	}

	public IEnumerator<IWindow> GetEnumerator()
	{
		throw new System.NotImplementedException();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		throw new System.NotImplementedException();
	}

	public static ILocation<double>? GetNodeLocation(Node node, NodeLocation? location = null)
	{
		if (location == null)
		{
			location = new NodeLocation() { X = 0, Y = 0, Width = 1, Height = 1 };
		}

		if (node.Parent == null)
		{
			return location;
		}

		SplitNode parent = node.Parent;
		(double weight, double precedingWeight) = GetWeightAndIndex(parent, node);

		// We translate by the preceding weight.
		// We then scale by the weight.
		if (parent.Direction == NodeDirection.Right)
		{
			location.X *= weight;
			location.X += precedingWeight;
			location.Width *= weight;
		}
		else if (parent.Direction == NodeDirection.Bottom)
		{
			location.Y *= weight;
			location.Y += precedingWeight;
			location.Height *= weight;
		}

		return GetNodeLocation(parent, location);
	}

	public static (double weight, double precedingWeight) GetWeightAndIndex(SplitNode parent, Node node)
	{
		int idx = parent.Children.IndexOf(node);
		double weight, precedingWeight;

		if (parent.EqualWeight)
		{
			weight = 1d / parent.Children.Count;
			precedingWeight = idx * weight;
		}
		else
		{
			weight = node.Weight;
			precedingWeight = parent.Children.Take(idx).Sum(child => child.Weight);
		}

		return (weight, precedingWeight);
	}

	public static LeafNode? GetLeftMostLeaf(Node? root)
	{
		if (root == null)
		{
			return null;
		}

		Node node = root;

		while (node is SplitNode splitNode)
		{
			if (splitNode.Children.Count == 0)
			{
				return null;
			}

			node = splitNode.Children[0];
		}

		return (LeafNode)node;
	}

	public static LeafNode? GetRightMostLeaf(Node? root)
	{
		if (root == null)
		{
			return null;
		}

		Node node = root;

		while (node is SplitNode splitNode)
		{
			if (splitNode.Children.Count == 0)
			{
				return null;
			}

			node = splitNode.Children[^1];
		}

		return (LeafNode)node;
	}
}
