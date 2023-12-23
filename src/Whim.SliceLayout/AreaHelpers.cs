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
		for (int i = 0; i < area.Children.Count; i++)
		{
			IArea child = area.Children[i];
			if (child is ParentArea parentArea)
			{
				parentArea = parentArea.Prune(windowCount);
				if (parentArea.Children.Count == 0)
				{
					ignoredWeight += area.Weights[i];
					continue;
				}
			}
			else if (child is BaseSliceArea baseSliceArea)
			{
				if (
					baseSliceArea.StartIndex >= windowCount
					|| (baseSliceArea is SliceArea sliceArea && sliceArea.MaxChildren == 0)
				)
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
		uint currIdx = 0;
		foreach (SliceArea sliceArea in sliceAreas)
		{
			sliceArea.StartIndex = currIdx;
			currIdx += sliceArea.MaxChildren;
		}

		if (overflowArea is null)
		{
			// Replace the last slice area with an overflow area.
			Logger.Error($"No overflow area found, replacing last slice area with overflow area");
			(ParentArea, OverflowArea)? result = ReplaceLastSliceAreaWithOverflowArea(rootArea, sliceAreas[^1].Order);

			if (result is null)
			{
				Logger.Error($"Failed to replace last slice area with overflow area, creating single overflow area");
				return SingleOverflowArea();
			}

			(rootArea, overflowArea) = result.Value;
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

	private static (ParentArea Parent, OverflowArea Overflow)? ReplaceLastSliceAreaWithOverflowArea(
		ParentArea rootArea,
		uint lastSliceOrder
	)
	{
		// Recursive DFS
		foreach (IArea child in rootArea.Children)
		{
			if (child is ParentArea parentArea)
			{
				(ParentArea Parent, OverflowArea Overflow)? result = ReplaceLastSliceAreaWithOverflowArea(
					parentArea,
					lastSliceOrder
				);
				if (result is not null)
				{
					return (ReplaceParentArea(rootArea, parentArea, result.Value.Parent), result.Value.Overflow);
				}
			}
			else if (child is SliceArea sliceArea && sliceArea.Order == lastSliceOrder)
			{
				OverflowArea newOverflow = new(sliceArea.IsRow) { StartIndex = sliceArea.StartIndex };

				ImmutableList<IArea> newRootAreaChildren = rootArea.Children;
				newRootAreaChildren = newRootAreaChildren.SetItem(newRootAreaChildren.IndexOf(sliceArea), newOverflow);

				return (new ParentArea(isRow: rootArea.IsRow, rootArea.Weights, newRootAreaChildren), newOverflow);
			}
		}

		return null;
	}

	private static ParentArea ReplaceParentArea(ParentArea rootArea, IArea oldChild, IArea newChild)
	{
		ImmutableList<IArea> newRootAreaChildren = rootArea.Children;
		newRootAreaChildren = newRootAreaChildren.SetItem(newRootAreaChildren.IndexOf(oldChild), newChild);
		return new ParentArea(isRow: rootArea.IsRow, rootArea.Weights, newRootAreaChildren);
	}

	public static int DoParentLayout(
		this SliceRectangleItem[] items,
		int windowStartIdx,
		IRectangle<int> rectangle,
		ParentArea area
	)
	{
		if (windowStartIdx >= items.Length)
		{
			return windowStartIdx;
		}

		int x = rectangle.X;
		int y = rectangle.Y;
		int width = rectangle.Width;
		int height = rectangle.Height;

		int windowCurrIdx = 0;
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
				windowCurrIdx = items.DoParentLayout(windowStartIdx + windowCurrIdx, childRectangle, parentArea);
			}
			else if (childArea is BaseSliceArea sliceArea)
			{
				windowCurrIdx = items.DoSliceLayout(windowStartIdx + windowCurrIdx, childRectangle, sliceArea);
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

		return windowStartIdx + windowCurrIdx;
	}

	private static int DoSliceLayout(
		this SliceRectangleItem[] items,
		int windowIdx,
		IRectangle<int> rectangle,
		BaseSliceArea area
	)
	{
		if (windowIdx >= items.Length)
		{
			return windowIdx;
		}

		int x = rectangle.X;
		int y = rectangle.Y;
		int width = rectangle.Width;
		int height = rectangle.Height;

		int deltaX = 0;
		int deltaY = 0;

		int remainingItemsCount = items.Length - windowIdx;
		int sliceItemsCount = remainingItemsCount;
		if (area is SliceArea sliceArea)
		{
			sliceItemsCount = Math.Min((int)sliceArea.MaxChildren, remainingItemsCount);
		}
		int maxIdx = windowIdx + sliceItemsCount;

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

		for (int windowCurrIdx = windowIdx; windowCurrIdx < maxIdx; windowCurrIdx++)
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

		return maxIdx;
	}
}
