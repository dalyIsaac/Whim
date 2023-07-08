using System.Collections.Generic;
using System.Collections.Immutable;

using WindowDict = System.Collections.Immutable.ImmutableDictionary<
	Whim.IWindow,
	System.Collections.Immutable.ImmutableArray<int>
>;

namespace Whim.ImmutableTreeLayout;

/// <summary>
/// A tree layout engine allows users to create arbitrary window grid layouts.
/// </summary>
public class TreeLayoutEngine : IImmutableLayoutEngine
{
	private readonly IContext _context;
	private readonly IImmutableInternalTreePlugin _plugin;
	private readonly Node? _root;

	/// <summary>
	/// Map of windows to their paths in the tree.
	/// </summary>
	private readonly WindowDict _windows;

	/// <inheritdoc/>
	public string Name { get; init; } = "Tree";

	/// <inheritdoc/>
	public int Count => _windows.Count;

	public Direction AddNodeDirection { get; } = Direction.Right;

	private TreeLayoutEngine(TreeLayoutEngine engine, Node root, WindowDict windows)
	{
		_context = engine._context;
		_plugin = engine._plugin;
		AddNodeDirection = engine.AddNodeDirection;
		_root = root;
		_windows = windows;
	}

	/// <summary>
	/// Creates a new <see cref="TreeLayoutEngine"/>, replacing the old node from
	/// <paramref name="oldNodePath"/> with <paramref name="newNode"/>.
	///
	/// It rebuilds the tree from the bottom up.
	/// </summary>
	/// <param name="oldNodeAncestors">The ancestors of the old node.</param>
	/// <param name="oldNodePath">The path to the old node.</param>
	/// <param name="newNode">The new node to replace the old node.</param>
	/// <returns></returns>
	private TreeLayoutEngine CreateNewEngine(
		SplitNode[] oldNodeAncestors,
		ImmutableArray<int> oldNodePath,
		Node newNode
	)
	{
		Node current = newNode;

		for (int idx = oldNodePath.Length - 1; idx >= 0; idx--)
		{
			SplitNode parent = oldNodeAncestors[idx];
			SplitNode newParent = parent.Replace(oldNodePath[idx], current);

			current = newParent;
		}

		return new TreeLayoutEngine(
			this,
			current,
			newNode is LeafNode leafNode ? _windows.SetItem(leafNode.Window, oldNodePath) : _windows
		);
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
				SplitNode newRoot = new(rootNode, newLeafNode, AddNodeDirection);
				return new TreeLayoutEngine(this, newRoot, CreateTopSplitNodeDict(rootNode.Window, window));

			case SplitNode rootNode:
				Logger.Debug($"Root is split node, adding new leaf node");

				LeafNode? focusedNode;
				SplitNode[] ancestors;
				ImmutableArray<int> path;

				// Try get the focused window, and use its parent. Otherwise, use the right-most leaf.
				if (
					_context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow is IWindow focusedWindow
					&& _windows.TryGetValue(focusedWindow, out ImmutableArray<int> focusedWindowPath)
					&& _root.GetNodeAtPath(focusedWindowPath)
						is
						(SplitNode[] pathAncestors, LeafNode leafNode, ILocation<double>)
				)
				{
					focusedNode = leafNode;
					ancestors = pathAncestors;
					path = focusedWindowPath;
				}
				else
				{
					(ancestors, path, focusedNode) = rootNode.GetRightMostLeaf();
				}

				// TODO: Test this assumption.
				// We're assuming that there is a parent node - otherwise we wouldn't have reached this point.
				SplitNode parentNode = ancestors[^1];
				SplitNode newParent = parentNode.Add(focusedNode, newLeafNode, AddNodeDirection);
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
			Direction newNodeDirection = new Location<double>() { Width = 1, Height = 1 }.GetDirectionToPoint(point);

			if (newNodeDirection.IsPositiveIndex())
			{
				SplitNode newRoot = new(windowNode, newLeafNode, newNodeDirection);
				return new TreeLayoutEngine(this, newRoot, CreateTopSplitNodeDict(windowNode.Window, window));
			}
			else
			{
				SplitNode newRoot = new(newLeafNode, windowNode, newNodeDirection);
				return new TreeLayoutEngine(this, newRoot, CreateTopSplitNodeDict(window, windowNode.Window));
			}
		}

		if (_root is not SplitNode rootNode)
		{
			Logger.Error($"Unexpected root node type: {_root.GetType()}");
			return this;
		}

		(SplitNode[], ImmutableArray<int>, LeafNode, Direction)? result = rootNode.GetNodeContainingPoint(point);
		if (result is null)
		{
			Logger.Error($"Failed to find node containing point {point}");
			return this;
		}

		(SplitNode[] ancestors, ImmutableArray<int> path, LeafNode focusedNode, Direction direction) = result.Value;

		SplitNode parentNode = ancestors[^1];
		if (parentNode.IsHorizontal == direction.IsHorizontal())
		{
			// Add the node to the parent.
			SplitNode newParent = parentNode.Add(focusedNode, newLeafNode, direction);
			return CreateNewEngine(
				oldNodeAncestors: ancestors,
				oldNodePath: path.RemoveAt(path.Length - 1),
				newNode: newParent
			);
		}

		// Replace the current node with a split node.
		SplitNode leafNodeReplacement = new(focusedNode, newLeafNode, direction);
		// TODO: Test path empty path?
		SplitNode newParentNode = parentNode.Replace(path[^1], leafNodeReplacement);
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

		foreach (NodeState? item in _root.GetWindowLocations(location))
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

		(SplitNode[], ImmutableArray<int>, LeafNode LeafNode)? adjacentNodeResult = TreeHelpers.GetAdjacentNode(
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
			SplitNode splitNode => splitNode.GetLeftMostLeaf().LeafNode.Window,
			_ => null
		};
	}

	public IImmutableLayoutEngine HidePhantomWindows() => throw new System.NotImplementedException();

	public IImmutableLayoutEngine MoveWindowEdgesInDirection(Direction edges, IPoint<double> deltas, IWindow window) =>
		throw new System.NotImplementedException();

	public IImmutableLayoutEngine Remove(IWindow window) => throw new System.NotImplementedException();

	public IImmutableLayoutEngine SwapWindowInDirection(Direction direction, IWindow window) =>
		throw new System.NotImplementedException();
}
