using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using WindowDict = System.Collections.Immutable.ImmutableDictionary<
	Whim.IWindow,
	System.Collections.Immutable.ImmutableArray<int>
>;

namespace Whim.ImmutableTreeLayout;

internal record NonRootWindowData(
	ISplitNode RootSplitNode,
	INode WindowNode,
	IReadOnlyList<ISplitNode> WindowAncestors,
	ImmutableArray<int> WindowPath,
	ILocation<double> WindowLocation
);

/// <summary>
/// A tree layout engine allows users to create arbitrary window grid layouts.
/// </summary>
public class TreeLayoutEngine : IImmutableLayoutEngine
{
	private readonly IContext _context;
	private readonly IImmutableInternalTreePlugin _plugin;
	private readonly INode? _root;

	/// <summary>
	/// Map of windows to their paths in the tree.
	/// </summary>
	private readonly WindowDict _windows;

	/// <inheritdoc/>
	public string Name { get; init; } = "Tree";

	/// <inheritdoc/>
	public int Count => _windows.Count;

	private readonly Direction _addNodeDirection = Direction.Right;

	/// <summary>
	/// The direction to add new windows to the tree, when there isn't an explicit direction.
	/// This must be one of the cardinal directions.
	/// </summary>
	public Direction AddNodeDirection
	{
		get => _addNodeDirection;
		init
		{
			switch (value)
			{
				case Direction.Left:
				case Direction.Right:
				case Direction.Up:
				case Direction.Down:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(value), value, "Invalid direction.");
			}

			_addNodeDirection = value;
		}
	}

	private TreeLayoutEngine(TreeLayoutEngine engine, INode root, WindowDict windows)
		: this(engine._context, engine._plugin)
	{
		Name = engine.Name;
		AddNodeDirection = engine.AddNodeDirection;
		_root = root;
		_windows = windows;
	}

	internal TreeLayoutEngine(IContext context, IImmutableInternalTreePlugin plugin)
	{
		_context = context;
		_plugin = plugin;
		_root = null;
		_windows = WindowDict.Empty;
	}

	/// <summary>
	/// Creates a new <see cref="TreeLayoutEngine"/>, replacing the old node from
	/// <paramref name="oldNodePath"/> with <paramref name="newNode"/>.
	/// </summary>
	/// <param name="oldNodeAncestors">The ancestors of the old node.</param>
	/// <param name="oldNodePath">The path to the old node.</param>
	/// <param name="newNode">The new node to replace the old node.</param>
	/// <returns></returns>
	private TreeLayoutEngine CreateNewEngine(
		IReadOnlyList<ISplitNode> oldNodeAncestors,
		ImmutableArray<int> oldNodePath,
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

		WindowDict updatedPaths = TreeHelpers.CreateUpdatedPaths(_windows, oldNodePath, (ISplitNode)current);

		return new TreeLayoutEngine(this, current, updatedPaths);
	}

	/// <summary>
	/// Creates a new dictionary. It contains only <code>{ newWindow: [0]; }</code>.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	private static WindowDict CreateRootNodeDict(IWindow window)
	{
		WindowDict.Builder dictBuilder = ImmutableDictionary.CreateBuilder<IWindow, ImmutableArray<int>>();

		ImmutableArray<int>.Builder pathBuilder = ImmutableArray.CreateBuilder<int>();
		pathBuilder.Add(0);

		dictBuilder.Add(window, pathBuilder.ToImmutable());
		return dictBuilder.ToImmutable();
	}

	/// <summary>
	/// Creates a new dictionary with the top-level split node.
	/// </summary>
	/// <param n="windowA"></param>
	/// <param n="windowB"></param>
	/// <returns></returns>
	private static WindowDict CreateTopSplitNodeDict(IWindow windowA, IWindow windowB)
	{
		WindowDict.Builder dictBuilder = ImmutableDictionary.CreateBuilder<IWindow, ImmutableArray<int>>();

		ImmutableArray<int>.Builder pathA = ImmutableArray.CreateBuilder<int>();
		pathA.Add(0);
		pathA.Add(0);

		ImmutableArray<int>.Builder pathB = ImmutableArray.CreateBuilder<int>();
		pathB.Add(0);
		pathB.Add(1);

		dictBuilder.Add(windowA, pathA.ToImmutable());
		dictBuilder.Add(windowB, pathB.ToImmutable());

		return dictBuilder.ToImmutable();
	}

	/// <summary>
	/// Gets the data for a non-root window.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	private NonRootWindowData? GetNonRootWindowData(IWindow window)
	{
		if (_root is null)
		{
			Logger.Error($"Root is null, cannot get window data");
			return null;
		}

		if (_root is not ISplitNode rootSplitNode)
		{
			Logger.Error($"Root is not split node, cannot get window data");
			return null;
		}

		if (!_windows.TryGetValue(window, out ImmutableArray<int> windowPath))
		{
			Logger.Error($"Window {window} not found in layout engine");
			return null;
		}

		var windowResult = rootSplitNode.GetNodeAtPath(windowPath);
		if (windowResult is null)
		{
			Logger.Error($"Window {window} not found in layout engine");
			return null;
		}

		return new NonRootWindowData(
			rootSplitNode,
			windowResult.Node,
			windowResult.Ancestors,
			windowPath,
			windowResult.Location
		);
	}

	/// <inheritdoc/>
	public IImmutableLayoutEngine Add(IWindow window)
	{
		Logger.Debug($"Adding window {window} to layout engine {Name}");

		// Create the new leaf node.
		LeafNode newLeafNode = _plugin.PhantomWindows.Contains(window)
			? new PhantomNode(window)
			: new WindowNode(window);

		switch (_root)
		{
			case null:
				Logger.Debug($"Root is null, creating new window node");
				return new TreeLayoutEngine(this, newLeafNode, CreateRootNodeDict(window));

			case PhantomNode:
				Logger.Debug($"Root is phantom node, replacing with new window node");
				return new TreeLayoutEngine(this, newLeafNode, CreateRootNodeDict(window));

			case WindowNode rootNode:
				Logger.Debug($"Root is window node, replacing with split node");
				ISplitNode newRoot = new SplitNode(rootNode, newLeafNode, AddNodeDirection);
				return new TreeLayoutEngine(this, newRoot, CreateTopSplitNodeDict(rootNode.Window, window));

			case ISplitNode rootNode:
				Logger.Debug($"Root is split node, adding new leaf node");

				LeafNode? focusedNode;
				IReadOnlyList<ISplitNode> ancestors;
				ImmutableArray<int> path;

				// Try get the focused window, and use its parent. Otherwise, use the right-most leaf.
				if (
					_context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow is IWindow focusedWindow
					&& _windows.TryGetValue(focusedWindow, out ImmutableArray<int> focusedWindowPath)
					&& _root.GetNodeAtPath(focusedWindowPath) is NodeState nodeState
					&& nodeState.Node is LeafNode focusedLeafNode
				)
				{
					focusedNode = focusedLeafNode;
					ancestors = nodeState.Ancestors;
					path = focusedWindowPath;
				}
				else
				{
					(focusedNode, ancestors, path) = rootNode.GetRightMostLeaf();
				}

				// TODO: Test this assumption.
				// We're assuming that there is a parent node - otherwise we wouldn't have reached this point.
				ISplitNode parentNode = ancestors[^1];
				ISplitNode newParent = parentNode.Add(focusedNode, newLeafNode, AddNodeDirection.InsertAfter());
				return CreateNewEngine(ancestors, path.RemoveAt(path.Length - 1), newParent);

			default:
				Logger.Error($"Unexpected root node type: {_root.GetType()}");
				return this;
		}
	}

	/// <inheritdoc />
	public IImmutableLayoutEngine AddAtPoint(IWindow window, IPoint<double> point)
	{
		Logger.Debug($"Adding window {window} to layout engine {Name}");

		// Create the new leaf node.
		LeafNode newLeafNode = _plugin.PhantomWindows.Contains(window)
			? new PhantomNode(window)
			: new WindowNode(window);

		// Handle the different root cases.
		if (_root is null)
		{
			Logger.Debug($"Root is null, creating new window node");
			return new TreeLayoutEngine(this, newLeafNode, CreateRootNodeDict(window));
		}

		if (_root is PhantomNode)
		{
			Logger.Debug($"Root is phantom node, replacing with new window node");
			return new TreeLayoutEngine(this, newLeafNode, CreateRootNodeDict(window));
		}

		if (_root is WindowNode windowNode)
		{
			Logger.Debug($"Root is window node, replacing with split node");
			Direction newNodeDirection = Location.UnitSquare<double>().GetDirectionToPoint(point);

			if (newNodeDirection.InsertAfter())
			{
				ISplitNode newRoot = new SplitNode(windowNode, newLeafNode, newNodeDirection);
				return new TreeLayoutEngine(this, newRoot, CreateTopSplitNodeDict(windowNode.Window, window));
			}
			else
			{
				ISplitNode newRoot = new SplitNode(newLeafNode, windowNode, newNodeDirection);
				return new TreeLayoutEngine(this, newRoot, CreateTopSplitNodeDict(window, windowNode.Window));
			}
		}

		if (_root is not ISplitNode rootNode)
		{
			Logger.Error($"Unexpected root node type: {_root.GetType()}");
			return this;
		}

		var result = rootNode.GetNodeContainingPoint(point);
		if (result is null)
		{
			Logger.Error($"Failed to find node containing point {point}");
			return this;
		}

		(LeafNode focusedNode, IReadOnlyList<ISplitNode> ancestors, ImmutableArray<int> path, Direction direction) =
			result;

		ISplitNode parentNode = ancestors[^1];
		if (parentNode.IsHorizontal == direction.IsHorizontal())
		{
			// Add the node to the parent.
			ISplitNode newParent = parentNode.Add(focusedNode, newLeafNode, direction.InsertAfter());
			return CreateNewEngine(
				oldNodeAncestors: ancestors,
				oldNodePath: path.RemoveAt(path.Length - 1),
				newNode: newParent
			);
		}

		// Replace the current node with a split node.
		ISplitNode leafNodeReplacement = new SplitNode(focusedNode, newLeafNode, direction);
		// TODO: Test path empty path?
		ISplitNode newParentNode = parentNode.Replace(path[^1], leafNodeReplacement);
		return CreateNewEngine(
			oldNodeAncestors: ancestors,
			oldNodePath: path.RemoveAt(path.Length - 1),
			newNode: newParentNode
		);
	}

	/// <inheritdoc />
	public bool Contains(IWindow window)
	{
		Logger.Debug($"Checking if layout engine {Name} contains window {window}");

		return _windows.ContainsKey(window);
	}

	/// <inheritdoc />
	public IEnumerable<IWindowState> DoLayout(ILocation<int> location, IMonitor monitor)
	{
		Logger.Debug($"Doing layout for engine {Name}");

		if (_root == null)
		{
			yield break;
		}

		foreach (LeafNodeWindowLocationState? item in _root.GetWindowLocations(location))
		{
			if (item.LeafNode is LeafNode leafNode)
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

	/// <inheritdoc />
	public void FocusWindowInDirection(Direction direction, IWindow window)
	{
		Logger.Debug($"Focusing window {window} in direction {direction}");

		if (_root is null)
		{
			Logger.Error($"Root is null, cannot focus window {window}");
			return;
		}

		if (!_windows.TryGetValue(window, out ImmutableArray<int> path))
		{
			Logger.Error($"Window {window} not found in layout engine {Name}");
			return;
		}

		LeafNodeStateAtPoint? adjacentNodeResult = TreeHelpers.GetAdjacentNode(
			_root,
			path,
			direction,
			_context.MonitorManager.ActiveMonitor
		);

		adjacentNodeResult?.LeafNode?.Focus();
	}

	/// <inheritdoc />
	public IWindow? GetFirstWindow()
	{
		Logger.Debug($"Getting first window in layout engine {Name}");

		return _root switch
		{
			null => null,
			WindowNode windowNode => windowNode.Window,
			PhantomNode => null,
			ISplitNode ISplitNode => ISplitNode.GetLeftMostLeaf().LeafNode.Window,
			_ => null
		};
	}

	/// <inheritdoc />
	public void HidePhantomWindows()
	{
		Logger.Debug($"Hiding phantom windows in layout engine {Name}");

		foreach (IWindow window in _windows.Keys)
		{
			if (_plugin.PhantomWindows.Contains(window))
			{
				window.Hide();
			}
		}
	}

	/// <inheritdoc />
	public IImmutableLayoutEngine MoveWindowEdgesInDirection(
		Direction edges,
		IPoint<double> deltas,
		IWindow focusedWindow
	)
	{
		Logger.Debug($"Moving window edges {edges} in direction {deltas} for window {focusedWindow}");

		if (GetNonRootWindowData(focusedWindow) is not NonRootWindowData windowData)
		{
			Logger.Error($"Window {focusedWindow} not found in layout engine {Name}");
			return this;
		}
		var (rootSplitNode, _, windowAncestors, windowPath, windowLocation) = windowData;

		IMonitor monitor = _context.MonitorManager.ActiveMonitor;
		LeafNodeStateAtPoint? xAdjacentResult = null;
		LeafNodeStateAtPoint? yAdjacentResult = null;

		if (edges.HasFlag(Direction.Left))
		{
			xAdjacentResult = TreeHelpers.GetAdjacentNode(
				windowData.RootSplitNode,
				Direction.Left,
				monitor,
				windowLocation
			);
		}
		else if (edges.HasFlag(Direction.Right))
		{
			xAdjacentResult = TreeHelpers.GetAdjacentNode(rootSplitNode, Direction.Right, monitor, windowLocation);
		}

		if (edges.HasFlag(Direction.Up))
		{
			yAdjacentResult = TreeHelpers.GetAdjacentNode(rootSplitNode, Direction.Up, monitor, windowLocation);
		}
		else if (edges.HasFlag(Direction.Down))
		{
			yAdjacentResult = TreeHelpers.GetAdjacentNode(rootSplitNode, Direction.Down, monitor, windowLocation);
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
				deltas.X,
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
				deltas.Y,
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
		// First, we need to find the location of the parent node.
		ImmutableArray<int> parentNodePath = focusedNodePath.Take(parentDepth).ToImmutableArray();
		var parentResult = root.GetNodeAtPath(parentNodePath);
		if (parentResult is null)
		{
			Logger.Error($"Failed to find parent node for focused window");
			return this;
		}

		if (parentResult.Node is not ISplitNode parentSplitNode)
		{
			Logger.Error($"Focused ancestor node is not a split node");
			return this;
		}

		// Figure out what the relative delta of pixelsDeltas is given the unit square.
		double relativeDelta = delta / (isXAxis ? parentResult.Location.Width : parentResult.Location.Height);

		// Now we can adjust the weights.
		ISplitNode newParent = parentSplitNode
			.AdjustChildWeight(focusedNodePath[parentDepth], relativeDelta)
			.AdjustChildWeight(adjacentNodePath[parentDepth], -relativeDelta);

		return CreateNewEngine(parentResult.Ancestors, parentNodePath, newParent);
	}

	/// <inheritdoc />
	public IImmutableLayoutEngine Remove(IWindow window)
	{
		Logger.Debug($"Removing window {window} from layout engine {Name}");

		if (_root is null)
		{
			Logger.Error($"Root is null, cannot remove window {window} from layout engine {Name}");
			return this;
		}

		if (_root is LeafNode leafNode)
		{
			if (leafNode.Window == window)
			{
				return new TreeLayoutEngine(_context, _plugin);
			}
			else
			{
				Logger.Error($"Root is leaf node, but window {window} is not the root");
				return this;
			}
		}

		if (!_windows.TryGetValue(window, out ImmutableArray<int> windowPath))
		{
			Logger.Error($"Window {window} not found in layout engine {Name}");
			return this;
		}

		var windowResult = _root.GetNodeAtPath(windowPath);
		if (windowResult is null)
		{
			Logger.Error($"Failed to find window {window} in layout engine {Name}");
			return this;
		}

		ISplitNode parentNode = windowResult.Ancestors[^1];

		// If the parent node has just two children, remove the parent node and replace it with the other child.
		if (parentNode.Children.Count == 2)
		{
			INode otherChild =
				parentNode.Children[0] == windowResult.Node ? parentNode.Children[1] : parentNode.Children[0];

			return CreateNewEngine(windowResult.Ancestors, windowPath[..^1], otherChild);
		}

		ISplitNode newParentNode = parentNode.Remove(windowPath[^1]);
		return CreateNewEngine(windowResult.Ancestors, windowPath[..^1], newParentNode);
	}

	/// <inheritdoc />
	public IImmutableLayoutEngine SwapWindowInDirection(Direction direction, IWindow window)
	{
		Logger.Debug($"Swapping window {window} in direction {direction} in layout engine {Name}");

		if (GetNonRootWindowData(window) is not NonRootWindowData windowData)
		{
			Logger.Error($"Window {window} not found in layout engine {Name}");
			return this;
		}

		var adjacentResult = TreeHelpers.GetAdjacentNode(
			windowData.RootSplitNode,
			direction,
			_context.MonitorManager.ActiveMonitor,
			windowData.WindowLocation
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
			return CreateNewEngine(windowData.WindowAncestors, windowData.WindowPath[..^1], newParent);
		}

		// If the parents are different, we need to swap the leaf nodes.
		ISplitNode newWindowParent = windowParent.Replace(windowData.WindowPath[^1], adjacentResult.LeafNode);
		ISplitNode newAdjacentParent = adjacentParent.Replace(adjacentResult.Path[^1], windowData.WindowNode);

		return CreateNewEngine(windowData.WindowAncestors, windowData.WindowPath[..^1], newWindowParent)
			.CreateNewEngine(adjacentResult.Ancestors, adjacentResult.Path[..^1], newAdjacentParent);
	}
}
