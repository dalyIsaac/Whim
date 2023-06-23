using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Whim.TreeLayout;

/// <inheritdoc />
public partial class TreeLayoutEngine : ITreeLayoutEngine
{
	private readonly IContext _context;
	private readonly Dictionary<IWindow, LeafNode> _windows = new();
	private readonly HashSet<IWindow> _phantomWindows = new();

	/// <summary>
	/// Cheekily keep track of the last location passed to <see cref="DoLayout" />.
	/// </summary>
	private ILocation<int>? _location;

	/// <inheritdoc/>
	internal Node? Root { get; private set; }

	/// <summary>
	/// The direction which we will use for any following operations.
	/// </summary>
	internal Direction AddNodeDirection { get; set; } = Direction.Right;

	/// <inheritdoc/>
	public string Name { get; set; }

	/// <inheritdoc/>
	public int Count { get; private set; }

	/// <inheritdoc/>
	public bool IsReadOnly => false;

	/// <summary>
	/// Creates a new tree layout engine.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="name"></param>
	public TreeLayoutEngine(IContext context, string name = "Tree")
	{
		_context = context;
		Name = name;
	}

	/// <summary>
	/// Add the <paramref name="window"/> to the layout engine.
	/// The direction it is added in is determined by this instance's <see cref="AddNodeDirection"/> property.
	/// </summary>
	/// <param name="window">The window to add.</param>
	public void Add(IWindow window) => AddWindow(window);

	/// <summary>
	/// Adds a window to the layout engine, and returns the node that represents it.
	/// The <paramref name="window"/> is added in the direction specified by this instance's
	/// <see cref="AddNodeDirection"/> property.
	/// </summary>
	/// <param name="window">The window to add.</param>
	/// <param name="focusedWindow">The currently focused window from whom to get the node.</param>
	/// <returns>The node that represents the window.</returns>
	internal WindowNode? AddWindow(IWindow window, IWindow? focusedWindow = null)
	{
		Logger.Debug($"Adding window {window} to layout engine {Name}");

		// Get the focused window node
		focusedWindow ??= _context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;

		WindowNode node = new(window);
		if (AddLeafNode(node, focusedWindow))
		{
			Count++;
			return node;
		}

		return null;
	}

	/// <summary>
	/// Add the given <paramref name="newLeaf"/> node to the layout engine,
	/// in the direction specified by this instance's <see cref="AddNodeDirection"/> property.
	/// </summary>
	/// <param name="newLeaf">The node to add.</param>
	/// <param name="focusedWindow">
	/// The focused window. If <paramref name="focusedWindow"/> is null, then
	/// the focused window is set to the <see cref="IWorkspace.LastFocusedWindow"/>.
	/// </param>
	/// <returns>True if the node was added, false otherwise.</returns>
	private bool AddLeafNode(LeafNode newLeaf, IWindow? focusedWindow = null)
	{
		IWindow window = newLeaf.Window;

		// Get the focused window node
		focusedWindow ??= _context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;

		// If the engine doesn't have the focused window, set the focused window to null.
		// This can occur when the focused window was a floating window.
		if (focusedWindow != null && !_windows.ContainsKey(focusedWindow))
		{
			focusedWindow = null;
		}

		// Add the window to the window-node map.
		_windows.Add(window, newLeaf);

		// If there is no root, then the window is the new root.
		if (Root == null)
		{
			Root = newLeaf;
			return true;
		}

		Logger.Verbose($"Focused window is {focusedWindow}");
		if (focusedWindow == null || !_windows.TryGetValue(focusedWindow, out LeafNode? focusedLeaf))
		{
			Logger.Verbose($"No focused window found. Looking for the right-most leaf node.");

			// We can't find the focused window, so we'll just add it to the right-most node.
			focusedLeaf = Root switch
			{
				LeafNode leaf => leaf,
				SplitNode split => split.RightMostLeaf,
				_ => null
			};
		}
		Logger.Verbose($"Focused leaf node is {focusedLeaf}");

		// If we really can't find a focused window, then we'll exit early.
		// Ideally, we should never enter this block here.
		if (focusedLeaf == null)
		{
			Logger.Error($"Could not find a leaf node to add window {window} to layout engine {Name}");
			_windows.Remove(window);
			return false;
		}

		// If the parent node is null, then the focused leaf is the root and we need to create a new split node.
		// In this scenario, there's no possibility of a sibling phantom node.
		if (focusedLeaf.Parent == null)
		{
			Logger.Verbose($"Focused leaf node {focusedLeaf} is the root node. Creating a new split node.");

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
			Logger.Verbose(
				$"Focused leaf node {focusedLeaf} is in a split node with direction {AddNodeDirection}. Adding window {window} to the split node."
			);

			parent.Add(existingFocusedNode: focusedLeaf, newNode: newLeaf, AddNodeDirection);
			return true;
		}

		// If the parent node is a split node and the direction doesn't match, then we need to
		// create a new split node and add the window to the split node.
		// The focused leaf will also be added to the new split node.
		Logger.Verbose(
			$"Replacing the focused leaf node {focusedLeaf} with a split node with direction {AddNodeDirection}."
		);

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
		_context.WorkspaceManager.ActiveWorkspace.RemovePhantomWindow(this, phantomNode.Window);
	}

	/// <inheritdoc/>
	public bool Remove(IWindow window)
	{
		Logger.Debug($"Removing window {window} from layout engine {Name}");

		// Get the node for the window.
		if (!_windows.TryGetValue(window, out LeafNode? removingNode))
		{
			Logger.Error($"Could not find node for window {window} in layout engine {Name}");
			return false;
		}

		_windows.Remove(window);
		Count--;

		if (removingNode is PhantomNode phantomNode)
		{
			_phantomWindows.Remove(phantomNode.Window);
			_context.WorkspaceManager.ActiveWorkspace.RemovePhantomWindow(this, phantomNode.Window);
		}

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

	/// <inheritdoc/>
	public IWindow? GetFirstWindow()
	{
		Logger.Debug($"Getting first window from layout engine {Name}");
		return Root?.LeftMostLeaf?.Window;
	}

	/// <inheritdoc/>
	public IEnumerable<IWindowState> DoLayout(ILocation<int> location, IMonitor monitor)
	{
		_location = location;

		if (Root == null)
		{
			yield break;
		}

		foreach (NodeState? item in GetWindowLocations(Root, location))
		{
			if (item.Node is LeafNode leafNode)
			{
				yield return new WindowState()
				{
					Window = leafNode.Window,
					Location = item.Location,
					WindowSize = item.WindowSize
				};
			}
		}
	}

	/// <inheritdoc/>
	public void FocusWindowInDirection(Direction direction, IWindow window)
	{
		Logger.Debug($"Focusing window {window} in direction {direction} in layout engine {Name}");

		if (!_windows.TryGetValue(window, out LeafNode? node))
		{
			Logger.Error($"Could not find node for window {window} in layout engine {Name}");
			return;
		}

		LeafNode? adjacentNode = GetAdjacentNode(node, direction);
		adjacentNode?.Focus();
	}

	/// <inheritdoc/>
	public void SwapWindowInDirection(Direction direction, IWindow window)
	{
		Logger.Debug($"Swapping window {window} in direction {direction} in layout engine {Name}");

		if (!_windows.TryGetValue(window, out LeafNode? node))
		{
			Logger.Error($"Could not find node for window {window} in layout engine {Name}");
			return;
		}

		LeafNode? targetNode = GetAdjacentNode(node, direction);
		if (targetNode == null)
		{
			Logger.Error($"Could not find adjacent node for window {window} in layout engine {Name}");
			return;
		}

		Logger.Verbose($"Swapping window {window} with window {targetNode}");

		// Get the parents.
		SplitNode? targetNodeParent = targetNode.Parent;
		SplitNode? nodeParent = node.Parent;

		// Swap the nodes.
		if (targetNodeParent == nodeParent)
		{
			targetNodeParent?.Swap(node, targetNode);
		}
		else
		{
			targetNodeParent?.Replace(targetNode, node);
			nodeParent?.Replace(node, targetNode);
		}

		window.Focus();
	}

	/// <inheritdoc/>
	public void Clear()
	{
		Logger.Debug($"Clearing layout engine {Name}");
		Root = null;
		_windows.Clear();

		HidePhantomWindows();
		_phantomWindows.Clear();

		Count = 0;
	}

	/// <inheritdoc/>
	public bool Contains(IWindow item)
	{
		Logger.Debug($"Checking if layout engine {Name} contains window {item}");
		return _windows.ContainsKey(item) || _phantomWindows.Contains(item);
	}

	/// <inheritdoc/>
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

		foreach (NodeState location in GetWindowLocations(Root, new Location<int>()))
		{
			if (location.Node is LeafNode leafNode)
			{
				yield return leafNode.Window;
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <inheritdoc />
	public void MoveWindowEdgesInDirection(Direction edges, IPoint<int> pixelDeltas, IWindow window)
	{
		Logger.Debug($"Moving window {window} edges in direction {edges} by {pixelDeltas} in layout engine {Name}");

		if (!_windows.TryGetValue(window, out LeafNode? focusedNode))
		{
			Logger.Error($"Could not find node for focused window in layout engine {Name}");
			return;
		}

		// Get the adjacent nodes.
		LeafNode? xAdjacentNode = null;
		LeafNode? yAdjacentNode = null;

		if (edges.HasFlag(Direction.Left))
		{
			xAdjacentNode = GetAdjacentNode(focusedNode, Direction.Left);
		}
		else if (edges.HasFlag(Direction.Right))
		{
			xAdjacentNode = GetAdjacentNode(focusedNode, Direction.Right);
		}

		if (edges.HasFlag(Direction.Up))
		{
			yAdjacentNode = GetAdjacentNode(focusedNode, Direction.Up);
		}
		else if (edges.HasFlag(Direction.Down))
		{
			yAdjacentNode = GetAdjacentNode(focusedNode, Direction.Down);
		}

		if (xAdjacentNode == null && yAdjacentNode == null)
		{
			Logger.Error($"Could not find adjacent node for focused window in layout engine {Name}");
			return;
		}

		// For each adjacent node, move the window edge in the given direction.
		Node[] focusedNodeLineage = focusedNode.Lineage.ToArray();
		MoveSingleWindowEdgeInDirection(pixelDeltas.X, true, focusedNodeLineage, xAdjacentNode);
		MoveSingleWindowEdgeInDirection(pixelDeltas.Y, false, focusedNodeLineage, yAdjacentNode);
	}

	private void MoveSingleWindowEdgeInDirection(
		double delta,
		bool isWidth,
		Node[] focusedNodeLineage,
		LeafNode? adjacentNode
	)
	{
		if (_location is null)
		{
			Logger.Error($"DoLayout has not been called in layout engine {Name}");
			return;
		}

		if (adjacentNode == null)
		{
			Logger.Error($"Could not find adjacent node for focused window in layout engine {Name}");
			return;
		}

		// Get the common parent node.
		Node[] adjacentNodeLineage = adjacentNode.Lineage.ToArray();
		SplitNode? parentNode = Node.GetCommonParent(focusedNodeLineage, adjacentNodeLineage);

		if (parentNode == null)
		{
			Logger.Error(
				$"Could not find common parent node for the focused and adjacent windows in layout engine {Name}"
			);
			return;
		}

		// Adjust the weight of the focused node.
		// First, we need to find the location of the parent node.
		ILocation<double> parentLocation = GetNodeLocation(parentNode);

		// Figure out what the relative delta of pixelDelta is, first given the unit square, then
		// given the dimensions of the parent node.
		double unitSquareDelta = delta / (isWidth ? _location.Width : _location.Height);
		double relativeDelta = unitSquareDelta / (isWidth ? parentLocation.Width : parentLocation.Height);

		// Now we can adjust the weights.
		int parentDepth = parentNode.Depth;

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
	internal LeafNode? GetAdjacentNode(LeafNode node, Direction direction)
	{
		Logger.Debug($"Getting node in direction {direction} for window {node}");

		if (Root == null)
		{
			Logger.Error($"No root node in layout engine {Name}");
			return null;
		}

		// We use this monitor to determine the delta we use for the internal calculations.
		IMonitor monitor = _context.MonitorManager.ActiveMonitor;

		// Get the coordinates of the node.
		ILocation<double> nodeLocation = GetNodeLocation(node);

		// Next, we figure out the adjacent point of the nodeLocation.
		double x = nodeLocation.X;
		double y = nodeLocation.Y;

		if (direction.HasFlag(Direction.Left))
		{
			x -= 1d / monitor.WorkingArea.Width;
		}
		else if (direction.HasFlag(Direction.Right))
		{
			x += nodeLocation.Width + (1d / monitor.WorkingArea.Width);
		}

		if (direction.HasFlag(Direction.Up))
		{
			y -= 1d / monitor.WorkingArea.Height;
		}
		else if (direction.HasFlag(Direction.Down))
		{
			y += nodeLocation.Height + (1d / monitor.WorkingArea.Height);
		}

		return GetNodeContainingPoint(
			Root,
			new Location<double>() { Height = 1, Width = 1 },
			new Point<double>() { X = x, Y = y }
		);
	}

	/// <summary>
	/// Flip the direction of the <see cref="SplitNode"/> parent of the currently focused window, and merge it with
	/// the grandparent <see cref="SplitNode"/>.
	/// </summary>
	public void FlipAndMerge()
	{
		Logger.Debug($"Flipping and merging split node for the focused window in layout engine {Name}");
		IWindow? focusedWindow = _context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;

		if (focusedWindow == null)
		{
			Logger.Error($"No focused window in layout engine {Name}");
			return;
		}

		if (!_windows.TryGetValue(focusedWindow, out LeafNode? node))
		{
			Logger.Error($"Could not find node for window {focusedWindow} in layout engine {Name}");
			return;
		}

		if (node.Parent == null)
		{
			Logger.Debug($"Node for window {focusedWindow} has no parent in layout engine {Name}");
			return;
		}

		SplitNode parent = node.Parent;
		if (parent.Parent == null)
		{
			// There is no grandparent, so just flip.
			parent.Flip();

			_context.WorkspaceManager.ActiveWorkspace.DoLayout();
			return;
		}

		// We need to merge the parent and grandparent.
		SplitNode grandparent = parent.Parent;
		grandparent.MergeChild(parent);

		_context.WorkspaceManager.ActiveWorkspace.DoLayout();
	}

	/// <summary>
	/// Split the focused window in two, and insert a phantom window in the direction
	/// of <see cref="AddNodeDirection"/>.
	/// </summary>
	public void SplitFocusedWindow()
	{
		Logger.Debug($"Splitting focused window in layout engine {Name}");
		SplitFocusedWindow(null);
	}

	/// <summary>
	/// Splits the focused window in two, and inserts the given <paramref name="phantomNode"/>
	/// in the direction of <see cref="AddNodeDirection"/>.
	/// </summary>
	/// <param name="focusedWindow"></param>
	/// <param name="phantomNode"></param>
	internal void SplitFocusedWindow(IWindow? focusedWindow = null, PhantomNode? phantomNode = null)
	{
		Logger.Debug($"Splitting focused window in layout engine {Name} with focused window {focusedWindow}");

		// Create the phantom window.
		phantomNode ??= PhantomNode.CreatePhantomNode(_context);
		if (phantomNode == null)
		{
			Logger.Error($"Could not create phantom node for layout engine {Name}");
			return;
		}

		// Try add the phantom node.
		if (!AddLeafNode(phantomNode, focusedWindow))
		{
			phantomNode.Close();
			Logger.Error($"Could not add phantom node for layout engine {Name}");
			return;
		}

		_phantomWindows.Add(phantomNode.Window);
		_context.WorkspaceManager.ActiveWorkspace.AddPhantomWindow(this, phantomNode.Window);
		phantomNode.Focus();
	}

	/// <inheritdoc/>
	public void HidePhantomWindows()
	{
		Logger.Debug($"Hiding phantom windows in layout engine {Name}");

		foreach (IWindow window in _phantomWindows)
		{
			window.Hide();
		}
	}

	/// <inheritdoc/>
	public void AddWindowAtPoint(IWindow window, IPoint<double> point)
	{
		if (Root == null)
		{
			// Add the window normally.
			MoveWindowToPointAddWindow(window, null);
			return;
		}

		// Find the node at the point.
		LeafNode? node = GetNodeContainingPoint(Root, new Location<double>() { Height = 1, Width = 1 }, point);
		if (node == null)
		{
			Logger.Error($"Could not find node containing point {point} in layout engine {Name}");
			return;
		}

		// Get the parent node.
		SplitNode? parent = node.Parent;

		// Get the direction.
		bool isHorizontal = parent?.IsHorizontal ?? AddNodeDirection.IsHorizontal();

		// Get the node's location.
		ILocation<double> nodeLocation = GetNodeLocation(node);

		// Save the old direction. AddWindow relies on AddNodeDirection to determine the direction.
		// However, we want to retain the user's current direction after the window is added.
		Direction oldAddNodeDirection = AddNodeDirection;

		// Update AddNodeDirection with the direction based on the point.
		if (isHorizontal)
		{
			AddNodeDirection = point.X < nodeLocation.X + (nodeLocation.Width / 2) ? Direction.Left : Direction.Right;
		}
		else
		{
			AddNodeDirection = point.Y < nodeLocation.Y + (nodeLocation.Height / 2) ? Direction.Up : Direction.Down;
		}

		MoveWindowToPointAddWindow(window, node.Window);

		// Restore the old direction.
		AddNodeDirection = oldAddNodeDirection;
	}

	/// <summary>
	/// Helper method to add a window to the layout engine.
	/// This calls <see cref="SplitFocusedWindow(IWindow?, PhantomNode?)"/> if the window is a phantom window.
	/// Otherwise, it calls <see cref="AddWindow(IWindow, IWindow?)"/>.
	/// </summary>
	/// <param name="window">The window to add.</param>
	/// <param name="focusedWindow">The focused window.</param>
	private void MoveWindowToPointAddWindow(IWindow window, IWindow? focusedWindow)
	{
		if (_phantomWindows.Contains(window))
		{
			// We don't actually care about this phantom window, as we'll spawn a new one.
			window.Close();
			SplitFocusedWindow(focusedWindow);
		}
		else
		{
			AddWindow(window, focusedWindow);
		}
	}
}
