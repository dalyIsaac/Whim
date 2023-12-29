using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using WindowPathDict = System.Collections.Immutable.ImmutableDictionary<
	Whim.IWindow,
	System.Collections.Immutable.ImmutableArray<int>
>;

namespace Whim.TreeLayout;

internal record NonRootWindowData(
	ISplitNode RootSplitNode,
	WindowNode WindowNode,
	IReadOnlyList<ISplitNode> WindowAncestors,
	ImmutableArray<int> WindowPath,
	IRectangle<double> WindowRectangle
);

/// <summary>
/// A tree layout engine allows users to create arbitrary window grid layouts.
/// </summary>
public record TreeLayoutEngine : ILayoutEngine
{
	private readonly IContext _context;
	private readonly ITreeLayoutPlugin _plugin;
	private readonly INode? _root;

	/// <summary>
	/// Map of windows to their paths in the tree.
	/// </summary>
	private readonly WindowPathDict _windows;

	/// <inheritdoc/>
	public string Name { get; init; } = "Tree";

	/// <inheritdoc/>
	public int Count => _windows.Count;

	/// <inheritdoc/>
	public LayoutEngineIdentity Identity { get; }

	private TreeLayoutEngine(TreeLayoutEngine engine, INode root, WindowPathDict windows)
		: this(engine._context, engine._plugin, engine.Identity)
	{
		Name = engine.Name;
		_root = root;
		_windows = windows;
	}

	/// <summary>
	/// Creates a new instance of the tree layout engine.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="plugin"></param>
	/// <param name="identity"></param>
	public TreeLayoutEngine(IContext context, ITreeLayoutPlugin plugin, LayoutEngineIdentity identity)
	{
		_context = context;
		_plugin = plugin;
		Identity = identity;
		_root = null;
		_windows = WindowPathDict.Empty;
	}

	/// <summary>
	/// Creates a new <see cref="TreeLayoutEngine"/>, replacing the old node from
	/// <paramref name="oldNodePath"/> with <paramref name="newNode"/>.
	/// </summary>
	/// <param name="oldNodeAncestors">The ancestors of the old node.</param>
	/// <param name="oldNodePath">The path to the old node.</param>
	/// <param name="windows">The map of windows to their paths.</param>
	/// <param name="newNode">The new node to replace the old node.</param>
	/// <returns></returns>
	private TreeLayoutEngine CreateNewEngine(
		IReadOnlyList<ISplitNode> oldNodeAncestors,
		ImmutableArray<int> oldNodePath,
		WindowPathDict windows,
		INode newNode
	)
	{
		// Rebuild the tree from the bottom up.
		INode current = newNode;

		for (int idx = oldNodePath.Length - 1; idx >= 0; idx--)
		{
			ISplitNode parent = oldNodeAncestors[idx];
			ISplitNode newParent = parent.Replace(oldNodePath[idx], current);

			current = newParent;
		}

		return new TreeLayoutEngine(
			this,
			current,
			TreeHelpers.CreateUpdatedPaths(windows, oldNodePath, (ISplitNode)current)
		);
	}

	/// <summary>
	/// Creates a new dictionary. It contains only <code>{ newWindow: [0]; }</code>.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	private static WindowPathDict CreateRootNodeDict(IWindow window) =>
		ImmutableDictionary.Create<IWindow, ImmutableArray<int>>().Add(window, ImmutableArray.Create(0));

	/// <summary>
	/// Creates a new dictionary with the top-level split node.
	/// </summary>
	/// <param name="splitNode"></param>
	/// <returns></returns>
	private static WindowPathDict CreateTopSplitNodeDict(ISplitNode splitNode)
	{
		WindowPathDict.Builder dictBuilder = ImmutableDictionary.CreateBuilder<IWindow, ImmutableArray<int>>();

		int idx = 0;
		foreach (INode child in splitNode.Children)
		{
			dictBuilder.Add(((WindowNode)child).Window, ImmutableArray.Create(idx));
			idx++;
		}

		return dictBuilder.ToImmutable();
	}

	/// <inheritdoc/>
	public ILayoutEngine AddWindow(IWindow window)
	{
		Logger.Debug($"Adding window {window} to layout engine {Name}");

		if (_windows.ContainsKey(window))
		{
			Logger.Error($"Window {window} already exists in layout engine {Name}");
			return this;
		}

		WindowNode newWindowNode = new(window);

		switch (_root)
		{
			case WindowNode rootNode:
				Logger.Debug($"Root is window node, replacing with split node");
				ISplitNode newRoot = new SplitNode(rootNode, newWindowNode, _plugin.GetAddWindowDirection(this));
				return new TreeLayoutEngine(this, newRoot, CreateTopSplitNodeDict(newRoot));

			case ISplitNode rootNode:
				return AddToSplitNode(newWindowNode, rootNode);

			default:
				Logger.Debug($"Root is null, creating new window node");
				return new TreeLayoutEngine(this, newWindowNode, CreateRootNodeDict(window));
		}
	}

	private ILayoutEngine AddToSplitNode(WindowNode newWindowNode, ISplitNode rootNode)
	{
		Logger.Debug($"Root is split node, adding new window node");

		WindowNode? focusedNode;
		IReadOnlyList<ISplitNode> ancestors;
		ImmutableArray<int> path;

		// Try get the focused window, and use its parent. Otherwise, use the right-most window.
		if (
			_context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow is IWindow focusedWindow
			&& _windows.TryGetValue(focusedWindow, out ImmutableArray<int> focusedWindowPath)
			&& rootNode.GetNodeAtPath(focusedWindowPath) is NodeState nodeState
			&& nodeState.Node is WindowNode focusedWindowNode
		)
		{
			focusedNode = focusedWindowNode;
			ancestors = nodeState.Ancestors;
			path = focusedWindowPath;
		}
		else
		{
			(focusedNode, ancestors, path) = rootNode.GetRightMostWindow();
		}

		// We're assuming that there is a parent node - otherwise we wouldn't have reached this point.
		ISplitNode parentNode = ancestors[^1];
		ISplitNode newParent;

		Direction addNodeDirection = _plugin.GetAddWindowDirection(this);
		if (parentNode.IsHorizontal == addNodeDirection.IsHorizontal())
		{
			newParent = parentNode.Add(focusedNode, newWindowNode, addNodeDirection.InsertAfter());
		}
		else
		{
			SplitNode newChild = new(focusedNode, newWindowNode, addNodeDirection);
			newParent = parentNode.Replace(path[^1], newChild);
		}

		return CreateNewEngine(ancestors, path.RemoveAt(path.Length - 1), _windows, newParent);
	}

	/// <inheritdoc />
	public ILayoutEngine MoveWindowToPoint(IWindow window, IPoint<double> point)
	{
		Logger.Debug($"Adding window {window} to layout engine {Name}");

		TreeLayoutEngine intermediateTree = (TreeLayoutEngine)RemoveWindow(window);
		return intermediateTree.AddWindowAtPoint(window, point);
	}

	private ILayoutEngine AddWindowAtPoint(IWindow window, IPoint<double> point)
	{
		WindowNode newWindowNode = new(window);

		if (_root is WindowNode focusedWindowNode)
		{
			Logger.Debug($"Root is window node, replacing with split node");
			Direction newNodeDirection = Rectangle.UnitSquare<double>().GetDirectionToPoint(point);

			ISplitNode newRoot = new SplitNode(focusedWindowNode, newWindowNode, newNodeDirection);
			return new TreeLayoutEngine(this, newRoot, CreateTopSplitNodeDict(newRoot));
		}

		if (_root is ISplitNode splitNode)
		{
			return MoveWindowToPointSplitNode(point, newWindowNode, splitNode);
		}

		Logger.Debug($"Root is null, creating new window node");
		return new TreeLayoutEngine(this, newWindowNode, CreateRootNodeDict(window));
	}

	private ILayoutEngine MoveWindowToPointSplitNode(
		IPoint<double> point,
		WindowNode newWindowNode,
		ISplitNode rootNode
	)
	{
		WindowNodeStateAtPoint? result = rootNode.GetNodeContainingPoint(point);
		if (result is null)
		{
			Logger.Error($"Failed to find node containing point {point}");
			return this;
		}

		(WindowNode focusedNode, ImmutableArray<ISplitNode> ancestors, ImmutableArray<int> path, Direction direction) =
			result;

		ISplitNode parentNode = ancestors[^1];
		if (parentNode.IsHorizontal == direction.IsHorizontal())
		{
			// Add the node to the parent.
			ISplitNode newParent = parentNode.Add(focusedNode, newWindowNode, direction.InsertAfter());
			return CreateNewEngine(
				oldNodeAncestors: ancestors,
				oldNodePath: path.RemoveAt(path.Length - 1),
				_windows,
				newNode: newParent
			);
		}

		// Replace the current node with a split node.
		ISplitNode windowNodeReplacement = new SplitNode(focusedNode, newWindowNode, direction);
		ISplitNode newParentNode = parentNode.Replace(path[^1], windowNodeReplacement);
		return CreateNewEngine(
			oldNodeAncestors: ancestors,
			oldNodePath: path.RemoveAt(path.Length - 1),
			_windows,
			newNode: newParentNode
		);
	}

	/// <inheritdoc />
	public bool ContainsWindow(IWindow window)
	{
		Logger.Debug($"Checking if layout engine {Name} contains window {window}");

		return _windows.ContainsKey(window);
	}

	/// <inheritdoc />
	public IEnumerable<IWindowState> DoLayout(IRectangle<int> rectangle, IMonitor monitor)
	{
		Logger.Debug($"Doing layout for engine {Name}");

		if (_root == null)
		{
			yield break;
		}

		foreach (WindowNodeRectangleState item in _root.GetWindowRectangles(rectangle))
		{
			yield return new WindowState()
			{
				Window = item.WindowNode.Window,
				Rectangle = item.Rectangle,
				WindowSize = item.WindowSize
			};
		}
	}

	/// <inheritdoc />
	public ILayoutEngine FocusWindowInDirection(Direction direction, IWindow window)
	{
		Logger.Debug($"Focusing window {window} in direction {direction}");

		if (_root is null)
		{
			Logger.Error($"Root is null, cannot focus window {window}");
			return this;
		}

		if (!_windows.TryGetValue(window, out ImmutableArray<int> path))
		{
			Logger.Error($"Window {window} not found in layout engine {Name}");
			return this;
		}

		WindowNodeStateAtPoint? adjacentNodeResult = TreeHelpers.GetAdjacentWindowNode(
			_root,
			path,
			direction,
			_context.MonitorManager.ActiveMonitor
		);

		adjacentNodeResult?.WindowNode.Focus();
		return this;
	}

	/// <inheritdoc />
	public IWindow? GetFirstWindow()
	{
		Logger.Debug($"Getting first window in layout engine {Name}");

		return _root switch
		{
			WindowNode windowNode => windowNode.Window,
			ISplitNode ISplitNode => ISplitNode.GetLeftMostWindow().WindowNode.Window,
			_ => null
		};
	}

	/// <summary>
	/// Gets the data for a non-root window.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	private NonRootWindowData? GetNonRootWindowData(IWindow window)
	{
		if (_root is not ISplitNode rootSplitNode)
		{
			Logger.Error($"Root is not split node, cannot get window data");
			return null;
		}

		if (
			!_windows.TryGetValue(window, out ImmutableArray<int> windowPath)
			|| rootSplitNode.GetNodeAtPath(windowPath) is not NodeState windowResult
		)
		{
			Logger.Error($"Window {window} not found in layout engine");
			return null;
		}

		return new NonRootWindowData(
			rootSplitNode,
			(WindowNode)windowResult.Node,
			windowResult.Ancestors,
			windowPath,
			windowResult.Rectangle
		);
	}

	/// <inheritdoc />
	public ILayoutEngine MoveWindowEdgesInDirection(Direction edges, IPoint<double> deltas, IWindow focusedWindow)
	{
		Logger.Debug($"Moving window edges {edges} in direction {deltas} for window {focusedWindow}");

		if (GetNonRootWindowData(focusedWindow) is not NonRootWindowData windowData)
		{
			return this;
		}
		(
			ISplitNode rootSplitNode,
			INode _,
			IReadOnlyList<ISplitNode> windowAncestors,
			ImmutableArray<int> windowPath,
			IRectangle<double> windowRectangle
		) = windowData;

		IMonitor monitor = _context.MonitorManager.ActiveMonitor;
		WindowNodeStateAtPoint? xAdjacentResult = null;
		WindowNodeStateAtPoint? yAdjacentResult = null;

		// We need to adjust the deltas, because MoveSingleWindowEdgeInDirection works by moving the
		// edge in the direction when the delta is positive.
		// For example, if we want to move the left edge to the left, we need to have a positive
		// X value for MoveSingleWindowEdgeInDirection, but a negative X value for the call to
		// MoveWindowEdgesInDirection.
		Point<double> directionAdjustedDeltas = new(deltas);

		if (edges.HasFlag(Direction.Left))
		{
			xAdjacentResult = TreeHelpers.GetAdjacentWindowNode(
				windowData.RootSplitNode,
				Direction.Left,
				monitor,
				windowRectangle
			);

			directionAdjustedDeltas.X = -directionAdjustedDeltas.X;
		}
		else if (edges.HasFlag(Direction.Right))
		{
			xAdjacentResult = TreeHelpers.GetAdjacentWindowNode(
				rootSplitNode,
				Direction.Right,
				monitor,
				windowRectangle
			);
		}

		if (edges.HasFlag(Direction.Up))
		{
			yAdjacentResult = TreeHelpers.GetAdjacentWindowNode(rootSplitNode, Direction.Up, monitor, windowRectangle);

			directionAdjustedDeltas.Y = -directionAdjustedDeltas.Y;
		}
		else if (edges.HasFlag(Direction.Down))
		{
			yAdjacentResult = TreeHelpers.GetAdjacentWindowNode(
				rootSplitNode,
				Direction.Down,
				monitor,
				windowRectangle
			);
		}

		if (xAdjacentResult == null && yAdjacentResult == null)
		{
			Logger.Error($"Could not find adjacent node for focused window in layout engine {Name}");
			return this;
		}

		// For each adjacent node, move the window edge in the given direction.
		TreeLayoutEngine newEngine = this;

		if (xAdjacentResult != null)
		{
			newEngine = MoveSingleWindowEdgeInDirection(
				rootSplitNode,
				directionAdjustedDeltas.X,
				true,
				windowAncestors,
				windowPath,
				xAdjacentResult.Ancestors,
				xAdjacentResult.Path
			);

			rootSplitNode = (ISplitNode)newEngine._root!;
		}

		if (yAdjacentResult != null)
		{
			newEngine = MoveSingleWindowEdgeInDirection(
				rootSplitNode,
				directionAdjustedDeltas.Y,
				false,
				windowAncestors,
				windowPath,
				yAdjacentResult.Ancestors,
				yAdjacentResult.Path
			);
		}

		return newEngine;
	}

	private TreeLayoutEngine MoveSingleWindowEdgeInDirection(
		ISplitNode root,
		double delta,
		bool isXAxis,
		IReadOnlyList<ISplitNode> focusedNodeAncestors,
		ImmutableArray<int> focusedNodePath,
		IReadOnlyList<ISplitNode> adjacentNodeAncestors,
		ImmutableArray<int> adjacentNodePath
	)
	{
		// Get the index of the last common ancestor.
		int parentDepth = TreeHelpers.GetLastCommonAncestorIndex(focusedNodeAncestors, adjacentNodeAncestors);
		if (parentDepth == -1)
		{
			Logger.Error($"Failed to find common parent for focused window and adjacent node");
			return this;
		}

		// Adjust the weight of the focused node.
		// First, we need to find the rectangle of the parent node.
		ImmutableArray<int> parentNodePath = focusedNodePath.Take(parentDepth).ToImmutableArray();
		NodeState? parentResult = root.GetNodeAtPath(parentNodePath);
		if (parentResult is null || parentResult.Node is not ISplitNode parentSplitNode)
		{
			Logger.Error($"Failed to find parent node for focused window");
			return this;
		}

		// Figure out what the relative delta of pixelsDeltas is given the unit square.
		double relativeDelta = delta / (isXAxis ? parentResult.Rectangle.Width : parentResult.Rectangle.Height);

		// Now we can adjust the weights.
		ISplitNode focusedNodeParent = parentSplitNode.AdjustChildWeight(focusedNodePath[parentDepth], relativeDelta);
		if (focusedNodeParent == parentSplitNode)
		{
			Logger.Error($"Failed to adjust child weight for focused window");
			return this;
		}

		ISplitNode newParent = focusedNodeParent.AdjustChildWeight(adjacentNodePath[parentDepth], -relativeDelta);
		if (newParent == focusedNodeParent)
		{
			Logger.Error($"Failed to adjust child weight for adjacent node");
			return this;
		}

		return CreateNewEngine(parentResult.Ancestors, parentNodePath, _windows, newParent);
	}

	/// <inheritdoc />
	public ILayoutEngine RemoveWindow(IWindow window)
	{
		Logger.Debug($"Removing window {window} from layout engine {Name}");

		if (_root is null)
		{
			Logger.Error($"Root is null, cannot remove window {window} from layout engine {Name}");
			return this;
		}

		if (_root is WindowNode windowNode)
		{
			if (windowNode.Window.Equals(window))
			{
				return new TreeLayoutEngine(_context, _plugin, Identity);
			}
			else
			{
				Logger.Error($"Root is window node, but window {window} is not the root");
				return this;
			}
		}

		if (
			!_windows.TryGetValue(window, out ImmutableArray<int> windowPath)
			|| _root.GetNodeAtPath(windowPath) is not NodeState windowResult
		)
		{
			Logger.Error($"Window {window} not found in layout engine {Name}");
			return this;
		}

		ISplitNode parentNode = windowResult.Ancestors[^1];
		WindowPathDict windows = _windows.Remove(window);

		// If the parent node has more than two children, just remove the window.
		if (parentNode.Children.Count != 2)
		{
			ISplitNode newParentNode = parentNode.Remove(windowPath[^1]);
			return CreateNewEngine(windowResult.Ancestors, windowPath[..^1], windows, newParentNode);
		}

		// If the parent node has just two children, remove the parent node and replace it with the other child.
		INode otherChild =
			parentNode.Children[0] == windowResult.Node ? parentNode.Children[1] : parentNode.Children[0];

		if (parentNode == _root && otherChild is WindowNode otherWindowChild)
		{
			return new TreeLayoutEngine(this, otherChild, CreateRootNodeDict(otherWindowChild.Window));
		}

		return CreateNewEngine(windowResult.Ancestors, windowPath[..^1], windows, otherChild);
	}

	/// <inheritdoc />
	public ILayoutEngine SwapWindowInDirection(Direction direction, IWindow window)
	{
		Logger.Debug($"Swapping window {window} in direction {direction} in layout engine {Name}");

		if (GetNonRootWindowData(window) is not NonRootWindowData windowData)
		{
			Logger.Error($"Window {window} not found in layout engine {Name}");
			return this;
		}

		WindowNodeStateAtPoint? adjacentResult = TreeHelpers.GetAdjacentWindowNode(
			windowData.RootSplitNode,
			direction,
			_context.MonitorManager.ActiveMonitor,
			windowData.WindowRectangle
		);
		if (adjacentResult is null)
		{
			Logger.Error($"Could not find adjacent node for focused window in layout engine {Name}");
			return this;
		}

		// Get the parents of each node.
		ISplitNode windowParent = windowData.WindowAncestors[^1];
		ISplitNode adjacentParent = adjacentResult.Ancestors[^1];

		// If the parents are the same, we can just swap the windows.
		if (windowParent == adjacentParent)
		{
			ISplitNode newParent = windowParent.Swap(windowData.WindowPath[^1], adjacentResult.Path[^1]);
			return CreateNewEngine(windowData.WindowAncestors, windowData.WindowPath[..^1], _windows, newParent);
		}

		// If the parents are different, we need to swap the window nodes.
		return SwapAdjacentNodes(
			windowData.WindowPath,
			windowData.WindowAncestors,
			windowData.WindowNode,
			adjacentResult.Path,
			adjacentResult.Ancestors,
			adjacentResult.WindowNode
		);
	}

	private TreeLayoutEngine SwapAdjacentNodes(
		ImmutableArray<int> aPath,
		IReadOnlyList<ISplitNode> aNodeAncestors,
		WindowNode aNode,
		ImmutableArray<int> bPath,
		IReadOnlyList<ISplitNode> bNodeAncestors,
		WindowNode bNode
	)
	{
		// A's path should be longer than B's path.
		if (aPath.Length < bPath.Length)
		{
			(aPath, aNode, aNodeAncestors, bPath, bNode, _) = (
				bPath,
				bNode,
				bNodeAncestors,
				aPath,
				aNode,
				aNodeAncestors
			);
		}

		// Rebuild the tree from the bottom up for the new position for B.
		INode currentNode = bNode;
		ISplitNode? commonAncestor = null;
		int commonAncestorIdx = 0;

		for (int idx = aPath.Length - 1; idx >= 0; idx--)
		{
			ISplitNode parent = aNodeAncestors[idx];
			ISplitNode newParent = parent.Replace(aPath[idx], currentNode);

			if (idx < bPath.Length && bNodeAncestors[idx] == parent)
			{
				commonAncestor = newParent;
				commonAncestorIdx = idx;
				break;
			}

			currentNode = newParent;
		}

		if (commonAncestor is null)
		{
			Logger.Error($"Failed to find common ancestor for nodes {aNode} and {bNode}");
			return this;
		}

		// Rebuild the tree from the bottom up for the new position for A.
		currentNode = aNode;
		for (int idx = bPath.Length - 1; idx >= 0; idx--)
		{
			ISplitNode parent = idx == commonAncestorIdx ? commonAncestor : bNodeAncestors[idx];
			ISplitNode newParent = parent.Replace(bPath[idx], currentNode);
			currentNode = newParent;
		}

		WindowPathDict newWindows = TreeHelpers.CreateUpdatedPaths(
			_windows,
			aPath[..commonAncestorIdx],
			(ISplitNode)currentNode
		);
		return new TreeLayoutEngine(this, currentNode, newWindows);
	}

	/// <inheritdoc />
	public ILayoutEngine PerformCustomAction<T>(LayoutEngineCustomAction<T> action) => this;
}
