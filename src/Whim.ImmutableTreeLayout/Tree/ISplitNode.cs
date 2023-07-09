using System.Collections.Generic;
using System.Collections.Immutable;

namespace Whim.ImmutableTreeLayout;

/// <summary>
/// SplitNodes dictate the layout of the windows. They have a specific direction and children.
/// </summary>
internal interface ISplitNode : INode, IEnumerable<(double Weight, INode Node)>
{
	/// <summary>
	/// The weights of the children. If <see cref="EqualWeight"/> is <see langword="true"/>, then
	/// the weights here are ignored in favour of <code>1d / _children.Count</code>.
	/// </summary>
	ImmutableList<double> Weights { get; }

	/// <summary>
	/// The child nodes of this <see cref="SplitNode"/>. These will likely be either <see cref="SplitNode"/>s,
	/// or child classes of <see cref="LeafNode"/>.
	/// </summary>
	ImmutableList<INode> Children { get; }

	/// <summary>
	/// The number of nodes in this <see cref="SplitNode"/>.
	/// </summary>
	int Count { get; }

	/// <summary>
	/// When <see langword="true"/>, the <see cref="Children"/> are split
	/// within the parent node equally. This overrides the weights in <see cref="Weights"/>.
	/// </summary>
	bool EqualWeight { get; }

	/// <summary>
	/// When <see langword="true"/>, the <see cref="Children"/> are arranged horizontally.
	/// Otherwise, they are arranged vertically.
	/// </summary>
	bool IsHorizontal { get; }

	/// <summary>
	/// Adds a new node to this <see cref="SplitNode"/>. The new node is added either before or after
	/// the focused node, depending on the value of <paramref name="insertAfter"/>.
	/// </summary>
	/// <param name="focusedNode">The node to add to the split node.</param>
	/// <param name="newNode">The new node to add.</param>
	/// <param name="insertAfter">Whether to insert the new node after the focused node, or before it.</param>
	/// <returns>The new <see cref="SplitNode"/> with the new node added.</returns>
	SplitNode Add(INode focusedNode, INode newNode, bool insertAfter);

	/// <summary>
	/// Removes the node at the given <paramref name="index"/> from this <see cref="SplitNode"/>.
	/// If this <see cref="SplitNode"/> is not <see cref="EqualWeight"/>, then the weight of the
	/// last node is increased by the weight of the removed node.
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	SplitNode Remove(int index);

	/// <summary>
	/// Replaces the node at the given <paramref name="index"/> with the given <paramref name="newNode"/>.
	/// </summary>
	/// <param name="index">The index of the node to replace.</param>
	/// <param name="newNode">The node to replace the old node with.</param>
	/// <returns></returns>
	SplitNode Replace(int index, INode newNode);

	/// <summary>
	/// Swaps the nodes at the given indices.
	/// </summary>
	/// <param name="aIndex"></param>
	/// <param name="bIndex"></param>
	/// <returns></returns>
	SplitNode Swap(int aIndex, int bIndex);

	/// <summary>
	/// Changes the weight of the node at the given <paramref name="index"/> by the given <paramref name="delta"/>.
	///
	/// This method does not change the weight of the other children.
	/// </summary>
	/// <param name="index"></param>
	/// <param name="delta"></param>
	/// <returns></returns>
	SplitNode AdjustChildWeight(int index, double delta);

	/// <summary>
	/// Get the weight of the given <paramref name="node"/>.
	/// The <paramref name="node"/> must be a child of this <see cref="SplitNode"/>.
	/// </summary>
	/// <param name="node"></param>
	/// <returns></returns>
	double? GetChildWeight(INode node);

	/// <summary>
	/// Toggles the <see cref="EqualWeight"/> property of this <see cref="SplitNode"/>.
	/// </summary>
	/// <returns></returns>
	SplitNode ToggleEqualWeight();

	/// <summary>
	/// Merge the given <paramref name="child"/> into this <see cref="SplitNode"/>, at its current
	/// position.
	/// </summary>
	/// <param name="child">The child to merge.</param>
	/// <returns></returns>
	SplitNode MergeChild(SplitNode child);
}
