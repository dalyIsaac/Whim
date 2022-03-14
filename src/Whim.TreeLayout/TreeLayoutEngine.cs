using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Whim.TreeLayout;

public partial class TreeLayoutEngine : ILayoutEngine
{
	private readonly IConfigContext _configContext;
	private readonly Dictionary<IWindow, LeafNode> _windows = new();
	private readonly HashSet<IWindow> _phantomWindows = new();

	public Node? Root { get; private set; }

	/// <summary>
	/// The direction which we will use for any following operations.
	/// </summary>
	public Direction AddNodeDirection = Direction.Right;

	public string Name { get; set; }

	public int Count { get; private set; }

	public bool IsReadOnly => false;

	public Commander Commander { get; } = new();

	public TreeLayoutEngine(IConfigContext configContext, string name = "Tree")
	{
		_configContext = configContext;
		Name = name;
	}

	/// <summary>
	/// Add the <paramref name="window"/> to the layout engine, in a
	/// <paramref name="direction"/> to the currently focused window.
	/// </summary>
	/// <param name="window">The window to add.</param>
	/// <param name="direction">
	/// The direction to add the window, in relation to the currently focused window.
	/// </param>
	public void Add(IWindow window, Direction direction)
	{
		AddNodeDirection = direction;
		Add(window);
	}

	/// <summary>
	/// Add the <paramref name="window"/> to the layout engine.
	/// The direction it is added in is determined by this instance's <see cref="AddNodeDirection"/> property.
	/// </summary>
	/// <param name="window">The window to add.</param>
	public void Add(IWindow window)
	{
		AddWindow(window);
	}

	/// <summary>
	/// Adds a window to the layout engine, and returns the node that represents it.
	/// Please use <see cref="IWindow.Add"/> instead of this method, as
	/// the return value was added for testing, and does not match <see cref="ILayoutEngine"/>.
	/// The <paramref name="window"/> is added in the direction specified by this instance's
	/// <see cref="AddNodeDirection"/> property.
	/// </summary>
	/// <param name="window">The window to add.</param>
	/// <returns>The node that represents the window.</returns>
	public WindowNode? AddWindow(IWindow window)
	{
		Logger.Debug($"Adding window {window.Title} to layout engine {Name}");
		Count++;

		WindowNode node = new(window);
		if (AddLeafNode(node))
		{
			return node;
		}

		Count--;
		return null;
	}

	/// <summary>
	/// Add the given <paramref name="newLeaf"/> node to the layout engine,
	/// in the direction specified by this instance's <see cref="AddNodeDirection"/> property.
	/// </summary>
	/// <param name="newLeaf">The node to add.</param>
	/// <returns>True if the node was added, false otherwise.</returns>
	private bool AddLeafNode(LeafNode newLeaf)
	{
		IWindow window = newLeaf.Window;

		// Add the window to the window-node map.
		_windows.Add(window, newLeaf);

		// If there is no root, then the window is the new root.
		if (Root == null)
		{
			Root = newLeaf;
			return true;
		}

		// Get the focused window node
		IWindow? focusedWindow = _configContext.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;

		Logger.Verbose($"Focused window is {focusedWindow}");
		if (focusedWindow == null || !_windows.TryGetValue(focusedWindow, out LeafNode? focusedLeaf))
		{
			Logger.Verbose($"No focused window found. Looking for the right-most leaf node.");

			// We can't find the focused window, so we'll just add it to the right-most node.
			focusedLeaf = Root switch
			{
				LeafNode leaf => leaf,
				SplitNode split => split.GetRightMostLeaf(),
				_ => null
			};
		}
		Logger.Verbose($"Focused leaf node is {focusedLeaf}");

		// If we really can't find a focused window, then we'll exit early.
		// Ideally, we should never enter this block here.
		if (focusedLeaf == null)
		{
			Logger.Error($"Could not find a leaf node to add window {window.Title} to layout engine {Name}");
			_windows.Remove(window);
			return false;
		}

		// If the parent node is null, then the focused leaf is the root and we need to create a new split node.
		// In this scenario, there's no possibility of a sibling phantom node.
		if (focusedLeaf.Parent == null)
		{
			Logger.Verbose($"Focused leaf node {focusedLeaf.Window.Title} is the root node. Creating a new split node.");

			// Create a new split node, and update the root.
			SplitNode splitNode = new(focusedLeaf, newLeaf, AddNodeDirection);

			Root = splitNode;
			return true;
		}

		SplitNode parent = focusedLeaf.Parent;

		if (newLeaf is WindowNode newWindowNode)
		{
			// If the focused window is a phantom node, then we'll replace it with the new node.
			if (focusedLeaf is PhantomNode focusedPhantomNode)
			{
				ReplacePhantomNode(parent, focusedPhantomNode, newWindowNode);
				return true;
			}

			// Alternatively, If we're inserting a WindowNode, then we need to check if the adjacent node is a phantom node.
			// If so, we'll replace the phantom node with the new window node.
			if (GetAdjacentNode(focusedLeaf, AddNodeDirection) is PhantomNode adjPhantomNode)
			{
				ReplacePhantomNode(parent, adjPhantomNode, newWindowNode);
				return true;
			}
		}

		// If the parent node is a split node and the direction matches, then all we need to do is
		// add the window to the split node.
		if (parent.IsHorizontal == AddNodeDirection.IsHorizontal())
		{
			Logger.Verbose($"Focused leaf node {focusedLeaf.Window.Title} is in a split node with direction {AddNodeDirection}. Adding window {window.Title} to the split node.");

			parent.Add(existingFocusedNode: focusedLeaf, newNode: newLeaf, AddNodeDirection);
			return true;
		}

		// If the parent node is a split node and the direction doesn't match, then we need to
		// create a new split node and add the window to the split node.
		// The focused leaf will also be added to the new split node.
		Logger.Verbose($"Replacing the focused leaf node {focusedLeaf.Window.Title} with a split node with direction {AddNodeDirection}.");

		SplitNode newSplitNode = new(focusedLeaf, newLeaf, AddNodeDirection, parent);

		// Replace the focused leaf with the new split node.
		parent.Replace(focusedLeaf, newSplitNode);

		return true;
	}

	/// <summary>
	/// Replace the given <paramref name="phantomNode"/> with the given <paramref name="windowNode"/>.
	/// The <paramref name="phantomNode"/> will be removed from its parent node, which must be
	/// <paramref name="parent"/>.
	/// </summary>
	/// <param name="parent">The parent node of the <paramref name="phantomNode"/>.</param>
	/// <param name="phantomNode">The phantom node to replace.</param>
	/// <param name="windowNode">The window node to replace the phantom node with.</param>
	private void ReplacePhantomNode(SplitNode parent, PhantomNode phantomNode, WindowNode windowNode)
	{
		parent.Replace(phantomNode, windowNode);
		phantomNode.Close();
		_phantomWindows.Remove(phantomNode.Window);
		_configContext.WorkspaceManager.ActiveWorkspace.UnregisterPhantomWindow(this, phantomNode.Window);
	}

	/// <summary>
	/// Removes the <paramref name="window"/> from the layout engine.
	/// </summary>
	/// <param name="window">The window to remove.</param>
	/// <returns><see langword="true"/> if the window was removed, <see langword="false"/> otherwise.</returns>
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
			Logger.Verbose($"The removed node was the root node");

			Root = null;
			return true;
		}

		SplitNode parent = removingNode.Parent;
		parent.Remove(removingNode);

		// If the parent node has only one child, then we need to remove the split node.
		if (parent.Count == 1)
		{
			Logger.Verbose($"The parent node had just one child. Removing the split node.");

			(double _, Node child) = parent[0];
			SplitNode? grandParent = parent.Parent;

			if (grandParent == null)
			{
				// The parent is the root node.
				Logger.Verbose($"The parent node was the root node");

				Root = child;
				child.Parent = null;
				return true;
			}

			// Since parent had just a single child, then replace parent with child.
			Logger.Verbose("Replacing parent with child");
			grandParent.Replace(parent, child);
		}

		return true;
	}

	public IWindow? GetFirstWindow()
	{
		Logger.Debug($"Getting first window from layout engine {Name}");
		return Root?.GetLeftMostLeaf()?.Window;
	}

	public IEnumerable<IWindowLocation> DoLayout(ILocation<int> location)
	{
		if (Root == null)
		{
			yield break;
		}


		foreach (TreeLayoutWindowLocation? item in GetWindowLocations(Root, location))
		{
			if (item.Node is LeafNode leafNode)
			{
				yield return new WindowLocation(leafNode.Window, item.Location, item.WindowState);
			}
		}
	}

	public void FocusWindowInDirection(Direction direction, IWindow window)
	{
		Logger.Debug($"Focusing window {window.Title} in direction {direction} in layout engine {Name}");

		if (!_windows.TryGetValue(window, out LeafNode? node))
		{
			Logger.Error($"Could not find node for window {window.Title} in layout engine {Name}");
			return;
		}

		LeafNode? adjacentNode = GetAdjacentNode(node, direction);
		adjacentNode?.Focus();
	}

	public void SwapWindowInDirection(Direction direction, IWindow window)
	{
		Logger.Debug($"Swapping window {window.Title} in direction {direction} in layout engine {Name}");

		if (!_windows.TryGetValue(window, out LeafNode? node))
		{
			Logger.Error($"Could not find node for window {window.Title} in layout engine {Name}");
			return;
		}

		LeafNode? targetNode = GetAdjacentNode(node, direction);
		if (targetNode == null)
		{
			Logger.Error($"Could not find adjacent node for window {window.Title} in layout engine {Name}");
			return;
		}

		// Get the parents.
		SplitNode? targetNodeParent = targetNode.Parent;
		SplitNode? nodeParent = node.Parent;

		targetNodeParent?.Replace(targetNode, node);
		nodeParent?.Replace(node, targetNode);

		window.Focus();
	}

	public void Clear()
	{
		Logger.Debug($"Clearing layout engine {Name}");
		Root = null;
		_windows.Clear();
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

	/// <summary>
	/// Iterates over the windows in the layout engine.
	/// This utilises the <see cref="DoLayout"/> method to iterate over the windows.
	/// </summary>
	public IEnumerator<IWindow> GetEnumerator()
	{
		if (Root == null)
		{
			yield break;
		}

		foreach (TreeLayoutWindowLocation location in GetWindowLocations(Root, new Location(0, 0, 0, 0)))
		{
			if (location.Node is LeafNode leafNode)
			{
				yield return leafNode.Window;
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <summary>
	/// The maximum relative delta for moving a window's edge, within its parent.
	/// </summary>
	private const double MAX_RELATIVE_DELTA = 0.5;

	public void MoveWindowEdgeInDirection(Direction edge, double fractionDelta, IWindow window)
	{
		Logger.Debug($"Moving window {window} edge in direction {edge} by {fractionDelta} in layout engine {Name}");

		if (!_windows.TryGetValue(window, out LeafNode? focusedNode))
		{
			Logger.Error($"Could not find node for focused window in layout engine {Name}");
			return;
		}

		LeafNode? adjacentNode = GetAdjacentNode(focusedNode, edge);
		if (adjacentNode == null)
		{
			Logger.Error($"Could not find adjacent node for focused window in layout engine {Name}");
			return;
		}

		// Get the common parent node.
		Node[] focusedNodeLineage = focusedNode.GetLineage().ToArray();
		Node[] adjacentNodeLineage = adjacentNode.GetLineage().ToArray();
		SplitNode? parentNode = Node.GetCommonParent(focusedNodeLineage, adjacentNodeLineage);

		if (parentNode == null)
		{
			Logger.Error($"Could not find common parent node for the focused and adjacent windows in layout engine {Name}");
			return;
		}

		// Adjust the weight of the focused node.
		// First, we need to find the location of the parent node.
		ILocation<double> parentLocation = GetNodeLocation(parentNode);

		bool? isWidth = edge switch
		{
			Whim.Direction.Left => true,
			Whim.Direction.Right => true,
			Whim.Direction.Up => false,
			Whim.Direction.Down => false,
			_ => null
		};

		if (isWidth == null)
		{
			Logger.Error($"Invalid edge {edge} in layout engine {Name}");
			return;
		}

		double relativeDelta = fractionDelta / ((bool)isWidth ? parentLocation.Width : parentLocation.Height);

		// We cap the relative delta to MAX_RELATIVE_DELTA of the parent node's weight, to avoid nasty cases.
		if (relativeDelta > MAX_RELATIVE_DELTA)
		{
			Logger.Debug($"Capping relative delta of {relativeDelta} to {MAX_RELATIVE_DELTA * 100}%");
			relativeDelta = MAX_RELATIVE_DELTA;
		}
		else if (relativeDelta < -MAX_RELATIVE_DELTA)
		{
			Logger.Debug($"Capping relative delta of {relativeDelta} to {-MAX_RELATIVE_DELTA * 100}%");
			relativeDelta = -MAX_RELATIVE_DELTA;
		}

		// Now we can adjust the weight.
		int parentDepth = parentNode.GetDepth();

		Node focusedAncestorNode = focusedNodeLineage[focusedNodeLineage.Length - parentDepth - 2];
		Node adjacentAncestorNode = adjacentNodeLineage[adjacentNodeLineage.Length - parentDepth - 2];

		parentNode.AdjustChildWeight(focusedAncestorNode, relativeDelta);
		parentNode.AdjustChildWeight(adjacentAncestorNode, -relativeDelta);
	}

	/// <summary>
	/// Gets the adjacent node in the given <paramref name="direction"/>.
	/// </summary>
	/// <param name="node">The node to get the adjacent node for.</param>
	/// <param name="direction">The direction to get the adjacent node in.</param>
	/// <returns>
	/// The adjacent node in the given <paramref name="direction"/>.
	/// <see langword="null"/> if there is no adjacent node in the given <paramref name="direction"/>,
	/// or an error occurred.
	/// </returns>
	public LeafNode? GetAdjacentNode(LeafNode node, Direction direction)
	{
		Logger.Debug($"Getting node in direction {direction} for window {node}");

		if (Root == null)
		{
			Logger.Error($"No root node in layout engine {Name}");
			return null;
		}

		// We use this monitor to determine the delta we use for the internal calculations.
		IMonitor monitor = _configContext.MonitorManager.FocusedMonitor;

		// Get the coordinates of the node.
		ILocation<double> nodeLocation = GetNodeLocation(node);

		// Next, we figure out the adjacent point of the nodeLocation.
		ILocation<double> adjacentLocation = new NodeLocation()
		{
			X = nodeLocation.X + (direction switch
			{
				Whim.Direction.Left => -1d / monitor.Width,
				Whim.Direction.Right => nodeLocation.Width + (1d / monitor.Width),
				_ => 0d
			}),
			Y = nodeLocation.Y + (direction switch
			{
				Whim.Direction.Up => -1d / monitor.Height,
				Whim.Direction.Down => nodeLocation.Height + (1d / monitor.Height),
				_ => 0d
			}),
		};

		return GetNodeContainingPoint(Root, new NodeLocation() { Height = 1, Width = 1 }, adjacentLocation, node);
	}

	/// <summary>
	/// Flip the direction of the <see cref="SplitNode"/> parent of the currently focused window, and merge it with
	/// the grandparent <see cref="SplitNode"/>.
	/// </summary>
	public void FlipAndMerge()
	{
		Logger.Debug($"Flipping and merging split node for the focused window in layout engine {Name}");
		IWindow? focusedWindow = _configContext.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;

		if (focusedWindow == null)
		{
			Logger.Error($"No focused window in layout engine {Name}");
			return;
		}

		if (!_windows.TryGetValue(focusedWindow, out LeafNode? node))
		{
			Logger.Error($"Could not find node for window {focusedWindow.Title} in layout engine {Name}");
			return;
		}

		if (node.Parent == null)
		{
			Logger.Debug($"Node for window {focusedWindow.Title} has no parent in layout engine {Name}");
			return;
		}

		SplitNode parent = node.Parent;
		if (parent.Parent == null)
		{
			// There is no grandparent, so just flip.
			parent.Flip();

			_configContext.WorkspaceManager.ActiveWorkspace.DoLayout();
			return;
		}

		// We need to merge the parent and grandparent.
		SplitNode grandparent = parent.Parent;
		grandparent.MergeChild(parent);

		_configContext.WorkspaceManager.ActiveWorkspace.DoLayout();
	}

	public void SplitFocusedWindow()
	{
		Logger.Debug($"Splitting focused window in layout engine {Name}");

		// Create the phantom window.
		PhantomNode? phantomNode = PhantomNode.CreatePhantomNode(_configContext);
		if (phantomNode == null)
		{
			Logger.Error($"Could not create phantom node for layout engine {Name}");
			return;
		}

		// Try add the phantom node.
		if (!AddLeafNode(phantomNode))
		{
			phantomNode.Close();
			Logger.Error($"Could not add phantom node for layout engine {Name}");
			return;
		}

		_phantomWindows.Add(phantomNode.Window);
		_configContext.WorkspaceManager.ActiveWorkspace.RegisterPhantomWindow(this, phantomNode.Window);
		_configContext.WorkspaceManager.ActiveWorkspace.DoLayout();
		phantomNode.Focus();
	}

	public void HidePhantomWindows()
	{
		Logger.Debug($"Hiding phantom windows in layout engine {Name}");

		foreach (IWindow window in _phantomWindows)
		{
			window.Hide();
		}
	}
}
