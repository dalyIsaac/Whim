using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Whim.SliceLayout;

internal static class AreaHelpers
{
	/// <summary>
	/// Prune the tree of empty areas.
	/// </summary>
	/// <param name="area"></param>
	/// <param name="windowCount"></param>
	/// <returns></returns>
	public static ParentArea Prune(this ParentArea area, int windowCount)
	{
		ImmutableList<IArea>.Builder childrenBuilder = ImmutableList.CreateBuilder<IArea>();
		ImmutableList<double>.Builder weightsBuilder = ImmutableList.CreateBuilder<double>();

		double ignoredWeight = 0;
		bool foundOverflowArea = false;
		for (int i = 0; i < area.Children.Count; i++)
		{
			IArea child = area.Children[i];
			if (child is ParentArea parentArea)
			{
				// Prune the child area. If the child tree has changed, then rebuild the tree.
				ParentArea prunedParentArea = parentArea.Prune(windowCount);
				if (prunedParentArea.Children.Count == 0)
				{
					ignoredWeight += area.Weights[i];
					continue;
				}

				child = prunedParentArea;
			}
			else if (child is OverflowArea overflowArea)
			{
				if (foundOverflowArea)
				{
					Logger.Error($"Found multiple overflow areas, ignoring");
					ignoredWeight += area.Weights[i];
					continue;
				}

				if (overflowArea.StartIndex >= windowCount)
				{
					ignoredWeight += area.Weights[i];
					continue;
				}

				foundOverflowArea = true;
			}
			else if (child is SliceArea sliceArea)
			{
				if (sliceArea.StartIndex >= windowCount || sliceArea.MaxChildren == 0)
				{
					ignoredWeight += area.Weights[i];
					continue;
				}
			}

			childrenBuilder.Add(child);
			weightsBuilder.Add(area.Weights[i]);
		}

		// Redistribute the weight of the ignored children to the remaining children.
		if (ignoredWeight > 0)
		{
			double redistributedWeight = ignoredWeight / childrenBuilder.Count;
			for (int i = 0; i < weightsBuilder.Count; i++)
			{
				weightsBuilder[i] += redistributedWeight;
			}
		}

		return new ParentArea(area.IsRow, weightsBuilder.ToImmutable(), childrenBuilder.ToImmutable());
	}

	/// <summary>
	/// Set the start indexes of the areas. If there is no <see cref="OverflowArea"/>, then the last
	/// <see cref="SliceArea"/> is replaced with an <see cref="OverflowArea"/>.
	/// </summary>
	/// <param name="rootArea"></param>
	/// <returns>
	/// The <see cref="ParentArea"/> root area to use, and the areas which directly contain windows, in order.
	/// </returns>
	public static (ParentArea RootArea, BaseSliceArea[] OrderedSliceAreas) SetStartIndexes(this ParentArea rootArea)
	{
		if (rootArea.Children.Count == 0)
		{
			Logger.Error($"No children found, creating single overflow area");
			return SingleOverflowArea();
		}

		(List<SliceArea> sliceAreas, OverflowArea? overflowArea) = GetAreasSorted(rootArea);

		// Set the start indexes.
		int currIdx = 0;
		foreach (SliceArea sliceArea in sliceAreas)
		{
			sliceArea.StartIndex = currIdx;
			currIdx += sliceArea.MaxChildren;
		}

		if (overflowArea is null)
		{
			// Replace the last slice area with an overflow area.
			Logger.Error($"No overflow area found, replacing last slice area with overflow area");
			SliceArea sliceToReplace = sliceAreas[^1];

			overflowArea = new(isRow: sliceToReplace.IsRow);
			rootArea = RebuildTree(rootArea, sliceToReplace, overflowArea);
			sliceAreas.RemoveAt(sliceAreas.Count - 1);
		}

		// If there are multiple slice areas, then the overflow area should start after the last slice area.
		if (sliceAreas.Count > 0)
		{
			SliceArea lastSlice = sliceAreas[^1];
			overflowArea.StartIndex = lastSlice.StartIndex + lastSlice.MaxChildren;
		}

		// Create an array of the slice areas, and add the overflow area to the end.
		BaseSliceArea[] windowAreas = new BaseSliceArea[sliceAreas.Count + 1];
		for (int i = 0; i < sliceAreas.Count; i++)
		{
			windowAreas[i] = sliceAreas[i];
		}
		windowAreas[^1] = overflowArea;

		return (rootArea, windowAreas);
	}

	private static (ParentArea Parent, BaseSliceArea[] OrderedSliceAreas) SingleOverflowArea()
	{
		OverflowArea overflowArea = new(isRow: true);
		ParentArea parentArea = new(isRow: true, (1.0, overflowArea));
		return (parentArea, new BaseSliceArea[] { overflowArea });
	}

	/// <summary>
	/// Get the areas sorted by order, and the overflow area if it exists.
	/// We don't handle the case where there are multiple overflow areas, as overflow areas are
	/// greedy.
	/// </summary>
	/// <param name="rootArea"></param>
	/// <returns></returns>
	private static (List<SliceArea> SliceAreas, OverflowArea? OverflowArea) GetAreasSorted(ParentArea rootArea)
	{
		// Iterate through the tree and add the areas to the list, using DFS.
		List<SliceArea> sliceAreas = new();
		OverflowArea? overflowArea = null;
		List<IArea> areas = new() { rootArea };

		while (areas.Count > 0)
		{
			IArea currArea = areas[^1];
			areas.RemoveAt(areas.Count - 1);

			if (currArea is ParentArea parentArea)
			{
				for (int i = 0; i < parentArea.Children.Count; i++)
				{
					areas.Add(parentArea.Children[i]);
				}
			}
			else if (currArea is SliceArea sliceArea)
			{
				sliceAreas.Add(sliceArea);
			}
			else if (currArea is OverflowArea currOverflowArea)
			{
				overflowArea = currOverflowArea;
			}
		}

		// Sort the areas by order.
		sliceAreas.Sort((a, b) => a.Order.CompareTo(b.Order));

		return (sliceAreas, overflowArea);
	}

	/// <summary>
	/// Use recursive DFS to find the parent area which contains the child area, then replace the
	/// child area with the new child area.
	/// </summary>
	/// <param name="rootArea"></param>
	/// <param name="oldChild"></param>
	/// <param name="newChild"></param>
	/// <returns></returns>
	private static ParentArea RebuildTree(ParentArea rootArea, IArea oldChild, IArea newChild)
	{
		foreach (IArea child in rootArea.Children)
		{
			if (child == oldChild)
			{
				return ReplaceNode(rootArea, oldChild, newChild);
			}
			else if (child is ParentArea parentArea)
			{
				ParentArea newParentArea = RebuildTree(parentArea, oldChild, newChild);
				if (newParentArea != parentArea)
				{
					return ReplaceNode(rootArea, parentArea, newParentArea);
				}
			}
		}

		return rootArea;
	}

	private static ParentArea ReplaceNode(ParentArea rootArea, IArea oldChild, IArea newChild)
	{
		ImmutableList<IArea> newRootAreaChildren = rootArea.Children;
		newRootAreaChildren = newRootAreaChildren.SetItem(newRootAreaChildren.IndexOf(oldChild), newChild);
		return new ParentArea(isRow: rootArea.IsRow, rootArea.Weights, newRootAreaChildren);
	}

	/// <summary>
	/// Perform a general layout of the given <see cref="ParentArea"/>. This will recursively layout
	/// the children of the <see cref="ParentArea"/>, and place the generated rectangles will be
	/// placed inside <paramref name="items"/>.
	///
	/// <br />
	///
	/// The <paramref name="items"/> are in order of the tree, not the order of the windows.
	/// </summary>
	/// <param name="area"></param>
	/// <param name="rectangle"></param>
	/// <param name="items"></param>
	public static void DoParentLayout(this ParentArea area, IRectangle<int> rectangle, SliceRectangleItem[] items)
	{
		int x = rectangle.X;
		int y = rectangle.Y;
		int width = rectangle.Width;
		int height = rectangle.Height;

		for (int childIdx = 0; childIdx < area.Children.Count; childIdx++)
		{
			double weight = area.Weights[childIdx];
			IArea childArea = area.Children[childIdx];

			if (area.IsRow)
			{
				width = Convert.ToInt32(rectangle.Width * weight);
			}
			else
			{
				height = Convert.ToInt32(rectangle.Height * weight);
			}

			Rectangle<int> childRectangle = new(x, y, width, height);

			if (childArea is ParentArea parentArea)
			{
				parentArea.DoParentLayout(childRectangle, items);
			}
			else if (childArea is BaseSliceArea sliceArea)
			{
				sliceArea.DoSliceLayout(childRectangle, items);
			}

			if (area.IsRow)
			{
				x += width;
			}
			else
			{
				y += height;
			}
		}
	}

	/// <summary>
	/// Perform a layout of the given <see cref="BaseSliceArea"/>. This will place the generated
	/// rectangles inside <paramref name="items"/>.
	/// </summary>
	/// <param name="area"></param>
	/// <param name="rectangle"></param>
	/// <param name="items"></param>
	private static void DoSliceLayout(this BaseSliceArea area, IRectangle<int> rectangle, SliceRectangleItem[] items)
	{
		int x = rectangle.X;
		int y = rectangle.Y;
		int width = rectangle.Width;
		int height = rectangle.Height;

		int deltaX = 0;
		int deltaY = 0;

		int sliceItemsCount = items.Length - area.StartIndex;
		if (area is SliceArea sliceArea)
		{
			sliceItemsCount = Math.Min(sliceArea.MaxChildren, sliceItemsCount);
		}
		int maxIdx = area.StartIndex + sliceItemsCount;

		if (area.IsRow)
		{
			deltaX = rectangle.Width / sliceItemsCount;
			width = deltaX;
		}
		else
		{
			deltaY = rectangle.Height / sliceItemsCount;
			height = deltaY;
		}

		for (int windowCurrIdx = area.StartIndex; windowCurrIdx < maxIdx; windowCurrIdx++)
		{
			items[windowCurrIdx] = new SliceRectangleItem(windowCurrIdx, new Rectangle<int>(x, y, width, height));

			if (area.IsRow)
			{
				x += deltaX;
			}
			else
			{
				y += deltaY;
			}
		}
	}
}
