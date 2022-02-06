using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Whim.TreeLayout;

public class TreeLayoutEngine : ILayoutEngine
{
	private readonly IConfigContext _configContext;
	private readonly Dictionary<IWindow, LeafNode> _windows = new();
	public Node? Root { get; private set; }
	public NodeDirection Direction = NodeDirection.Right;

	public string Name { get; set; }

	public int Count { get; private set; }

	public bool IsReadOnly => false;

	public Commander Commander { get; } = new();

	public TreeLayoutEngine(IConfigContext configContext, string name = "Tree")
	{
		_configContext = configContext;
		Name = name;
	}

	public void Add(IWindow window)
	{
		AddWindow(window);
	}

	/// <summary>
	/// Adds a window to the layout engine, and returns the node that represents it.
	/// Please use the <see cref="IWindow.Add"/> method instead of this method, as
	/// the return value is used for testing.
	/// </summary>
	/// <param name="window">The window to add.</param>
	/// <returns>The node that represents the window.</returns>
	public LeafNode? AddWindow(IWindow window)
	{
		Logger.Debug($"Adding window {window.Title} to layout engine {Name}");

		LeafNode newLeaf = new(window);
		_windows.Add(window, newLeaf);

		if (Root == null)
		{
			Root = newLeaf;
			return newLeaf;
		}

		// Get the focused window node
		IWindow? focusedWindow = _configContext.WorkspaceManager.ActiveWorkspace.FocusedWindow;

		if (focusedWindow == null || !_windows.TryGetValue(focusedWindow, out LeafNode? focusedLeaf))
		{
			// We can't find the focused window, so we'll just add it to the right-most node.
			focusedLeaf = GetRightMostLeaf(Root);
		}

		if (focusedLeaf == null)
		{
			Logger.Error($"Could not find a leaf node to add window {window.Title} to layout engine {Name}");
			return null;
		}

		// If the parent node is null, then it's the root and we need to create a new split node
		if (focusedLeaf.Parent == null)
		{
			// Create a new split node
			SplitNode splitNode = new(Direction)
			{
				EqualWeight = true,
				Children = new List<Node>() { focusedLeaf, newLeaf }
			};

			focusedLeaf.Parent = splitNode;
			focusedLeaf.Weight = 0.5;

			newLeaf.Parent = splitNode;
			newLeaf.Weight = 0.5;

			Root = splitNode;
			return newLeaf;
		}

		SplitNode parent = focusedLeaf.Parent;
		// If the parent node is a split node and the direction matches, then we need to
		// add the window to the split node.
		if (parent.Direction == Direction)
		{
			parent.Children.Add(newLeaf);
			newLeaf.Parent = parent;

			if (parent.EqualWeight)
			{
				// We need to distribute the weight evenly.
				foreach (Node child in parent.Children)
				{
					child.Weight = 1d / parent.Children.Count;
				}
			}
			else
			{
				// Split the existingLeaf in half.
				newLeaf.Weight = focusedLeaf.Weight / 2;
				focusedLeaf.Weight /= 2;
			}

			return newLeaf;
		}

		// If the parent node is a split node and the direction doesn't match, then we need to
		// create a new split node and add the window to the split node.
		SplitNode newSplitNode = new(Direction)
		{
			EqualWeight = true,
			Children = new List<Node> { focusedLeaf, newLeaf },
			Weight = focusedLeaf.Weight,
			Parent = parent
		};

		// Update the weights
		newLeaf.Weight = 0.5;
		newLeaf.Parent = newSplitNode;
		focusedLeaf.Weight = 0.5;

		// Update the parent node
		int idx = parent.Children.IndexOf(focusedLeaf);
		parent.Children[idx] = newSplitNode;

		// Update the existing leaf's parent
		focusedLeaf.Parent = newSplitNode;
		Count += 1;

		return newLeaf;
	}

	public bool Remove(IWindow window)
	{
		Logger.Debug($"Removing window {window.Title} from layout engine {Name}");

		// Get the node for the window.
		if (!_windows.TryGetValue(window, out LeafNode? removingNode))
		{
			Logger.Error($"Could not find node for window {window.Title} in layout engine {Name}");
			return false;
		}

		_windows.Remove(window);
		Count -= 1;

		// Remove the node from the tree.
		if (removingNode.Parent == null)
		{
			// The node is the root node.
			Root = null;
			return true;
		}

		SplitNode parent = removingNode.Parent;
		bool success = parent.Children.Remove(removingNode);

		// If the parent node has only one child, then we need to remove the split node.
		if (parent.Children.Count == 1)
		{
			Node child = parent.Children[0];
			child.Parent = parent.Parent;

			if (child.Parent == null)
			{
				// The parent is the root node.
				Root = child;
			}
			else
			{
				child.Parent.Children.Add(child);
				child.Parent.Children.Remove(parent);
			}
		}

		// If the parent node has more than one child, then we need to redistribute the weights.
		else
		{
			if (parent.EqualWeight)
			{
				// We need to distribute the weight evenly.
				foreach (Node child in parent.Children)
				{
					child.Weight = 100 / parent.Children.Count;
				}
			}
			else
			{
				// Give the extra weight to the last child.
				Node lastChild = parent.Children[^1];
				lastChild.Weight += removingNode.Weight;
			}
		}

		return success;
	}

	public IWindow? GetFirstWindow()
	{
		Logger.Debug($"Getting first window from layout engine {Name}");
		return GetLeftMostLeaf(Root)?.Window;
	}

	public IEnumerable<IWindowLocation> DoLayout(ILocation<int> location) => Root != null ? DoLayout(Root, location) : Enumerable.Empty<IWindowLocation>();

	public void FocusWindowInDirection(WindowDirection direction, IWindow window)
	{
		Logger.Debug($"Focusing window {window.Title} in direction {direction} in layout engine {Name}");

		if (!_windows.TryGetValue(window, out LeafNode? node))
		{
			Logger.Error($"Could not find node for window {window.Title} in layout engine {Name}");
			return;
		}

		LeafNode? targetNode = GetAdjacentNode(node, direction, _configContext.MonitorManager.FocusedMonitor);
		targetNode?.Window.Focus();
	}

	public void SwapWindowInDirection(WindowDirection direction, IWindow window)
	{
		Logger.Debug($"Swapping window {window.Title} in direction {direction} in layout engine {Name}");

		if (!_windows.TryGetValue(window, out LeafNode? node))
		{
			Logger.Error($"Could not find node for window {window.Title} in layout engine {Name}");
			return;
		}

		LeafNode? targetNode = GetAdjacentNode(node, direction, _configContext.MonitorManager.FocusedMonitor);
		if (targetNode == null)
		{
			Logger.Error($"Could not find adjacent node for window {window.Title} in layout engine {Name}");
			return;
		}

		// Swap the windows.
		(targetNode.Window, node.Window) = (node.Window, targetNode.Window);
		window.Focus();
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

	public IEnumerator<IWindow> GetEnumerator()
	{
		throw new System.NotImplementedException();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		throw new System.NotImplementedException();
	}

	/// <summary>
	/// Gets the adjacent node in the given <paramref name="direction"/>.
	/// </summary>
	/// <param name="node">The node to get the adjacent node for.</param>
	/// <param name="direction">The direction to get the adjacent node in.</param>
	/// <param name="monitor">
	/// The monitor which the engine is currently focused for. This is used to
	/// determine the delta we use for the internal calculations.
	/// </param>
	/// <returns>
	/// The adjacent node in the given <paramref name="direction"/>.
	/// <see langword="null"/> if there is no adjacent node in the given <paramref name="direction"/>,
	/// or an error occurred.
	/// </returns>
	public LeafNode? GetAdjacentNode(LeafNode node, WindowDirection direction, IMonitor monitor)
	{
		Logger.Debug($"Getting node in direction {Direction} for window {node.Window.Title}");

		if (Root == null)
		{
			Logger.Error($"No root node in layout engine {Name}");
			return null;
		}

		// Get the coordinates of the node.
		ILocation<double> nodeLocation = GetNodeLocation(node);

		// Next, we figure out the adjacent point of the nodeLocation.
		ILocation<double> adjacentLocation = new NodeLocation()
		{
			X = nodeLocation.X + (direction switch
			{
				WindowDirection.Left => -1d / monitor.Width,
				WindowDirection.Right => nodeLocation.Width + (1d / monitor.Width),
				_ => 0d
			}),
			Y = nodeLocation.Y + (direction switch
			{
				WindowDirection.Up => -1d / monitor.Height,
				WindowDirection.Down => nodeLocation.Height + (1d / monitor.Height),
				_ => 0d
			}),
		};

		return GetNodeContainingPoint(Root, new NodeLocation() { Height = 1, Width = 1 }, adjacentLocation, node);
	}

	/// <summary>
	/// Gets the node which contains the given <paramref name="searchPoint"/>.
	/// This works by performing a breadth-first search.
	/// </summary>
	/// <param name="root">The root node to start the search from.</param>
	/// <param name="rootLocation">
	/// The location of the parent node. This is used to calculate the
	/// relative location of the point.
	/// </param>
	/// <param name="searchPoint">The point of the leaf node to search for.</param>
	/// <param name="originalNode">
	/// The leaf node to search for. The returned node cannot be the same as this node.
	/// </param>
	public static LeafNode? GetNodeContainingPoint(Node root,
												ILocation<double> rootLocation,
												ILocation<double> searchPoint,
												LeafNode originalNode)
	{
		if (root is LeafNode leaf)
		{
			return leaf == originalNode ? null : leaf;
		}

		if (root is not SplitNode splitNode)
		{
			return null;
		}

		SplitNode parent = splitNode;

		NodeLocation childLocation = new(rootLocation);

		foreach (Node child in parent.Children)
		{
			// Set up the width/height of the child.
			if (parent.Direction == NodeDirection.Right)
			{
				childLocation.Width = child.Weight * rootLocation.Width;
			}
			else if (parent.Direction == NodeDirection.Down)
			{
				childLocation.Height = child.Weight * rootLocation.Height;
			}

			if (childLocation.IsPointInside(searchPoint.X, searchPoint.Y))
			{
				LeafNode? result = GetNodeContainingPoint(root: child,
											  rootLocation: childLocation,
											  searchPoint: searchPoint,
											  originalNode: originalNode);
				if (result != null)
				{
					return result;
				}
			}

			// Since it wasn't a match, update the position of the child.
			if (parent.Direction == NodeDirection.Right)
			{
				childLocation.X += childLocation.Width;
			}
			else if (parent.Direction == NodeDirection.Down)
			{
				childLocation.Y += childLocation.Height;
			}
		}

		return null;
	}

	/// <summary>
	/// Gets a node's location within the unit square. This works by moving up
	/// the tree until the root is reached.
	/// </summary>
	/// <param name="node">The node to get the location for.</param>
	/// <returns>Location of the node. Used for recursion.</returns>
	public static ILocation<double> GetNodeLocation(Node node, NodeLocation? location = null)
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
		else if (parent.Direction == NodeDirection.Down)
		{
			location.Y *= weight;
			location.Y += precedingWeight;
			location.Height *= weight;
		}

		return GetNodeLocation(parent, location);
	}

	/// <summary>
	/// Gets the weight and index of a node within its parent.
	/// </summary>
	/// <param name="parent">The parent node.</param>
	/// <param name="node">The node to get the weight and index for.</param>
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

	/// <summary>
	/// Gets the <see cref="WindowLocation"/> for all windows, within the unit square.
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<IWindowLocation> DoLayout(Node node, ILocation<int> location)
	{
		// If the node is a leaf node, then we can return the location, and break.
		if (node is LeafNode leafNode)
		{
			yield return new WindowLocation(leafNode.Window, location, WindowState.Normal);
			yield break;
		}

		// If the node is not a leaf node, it's a split node.
		SplitNode parent = (SplitNode)node;

		// Perform an in-order traversal of the tree.
		double precedingWeight = 0;
		foreach (Node child in parent.Children)
		{
			double weight = parent.EqualWeight ? 1d / parent.Children.Count : child.Weight;

			Location childLocation = new(
				x: location.X,
				y: location.Y,
				width: location.Width,
				height: location.Height
			);

			// NOTE: We assume that NodeDirection is always either Right or Bottom.
			if (parent.Direction == NodeDirection.Right)
			{
				childLocation.X += Convert.ToInt32(precedingWeight * location.Width);
				childLocation.Width = Convert.ToInt32(weight * location.Width);
			}
			else
			{
				childLocation.Y += Convert.ToInt32(precedingWeight * location.Height);
				childLocation.Height = Convert.ToInt32(weight * location.Height);
			}

			foreach (IWindowLocation childLocationResult in DoLayout(child, childLocation))
			{
				yield return childLocationResult;
			}

			precedingWeight += weight;
		}
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
