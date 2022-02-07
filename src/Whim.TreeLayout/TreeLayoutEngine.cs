using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Whim.TreeLayout;

public partial class TreeLayoutEngine : ILayoutEngine
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
		Count++;

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
			Count--;
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
		Count--;

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
		Logger.Debug($"Clearing layout engine {Name}");
		Root = null;
	}

	public bool Contains(IWindow item)
	{
		Logger.Debug($"Checking if layout engine {Name} contains window {item.Title}");
		return _windows.ContainsKey(item);
	}

	public void CopyTo(IWindow[] array, int arrayIndex)
	{
		foreach (IWindow window in this)
		{
			array[arrayIndex++] = window;
		}
	}

	public IEnumerator<IWindow> GetEnumerator()
	{
		if (Root == null)
		{
			yield break;
		}

		foreach (IWindowLocation location in DoLayout(Root, new Location(0, 0, 0, 0)))
		{
			yield return location.Window;
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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
}
