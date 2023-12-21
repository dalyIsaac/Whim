using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Whim.SliceLayout;

// TODO: docs
public interface IArea
{
	/// <summary>
	/// When <see langword="true"/>, the <see cref="Children"/> are arranged horizontally.
	/// Otherwise, they are arranged vertically.
	/// </summary>
	bool IsRow { get; }
}

public abstract record BaseArea : IArea
{
	public bool IsRow { get; protected set; }
}

public record ParentArea : BaseArea
{
	public ImmutableList<double> Weights { get; }

	public ImmutableList<IArea> Children { get; }

	public ParentArea(bool isRow, params (double Weight, IArea Child)[] children)
	{
		IsRow = isRow;
		ImmutableList<double>.Builder weightsBuilder = ImmutableList.CreateBuilder<double>();
		ImmutableList<IArea>.Builder childrenBuilder = ImmutableList.CreateBuilder<IArea>();

		foreach ((double weight, IArea child) in children)
		{
			weightsBuilder.Add(weight);
			childrenBuilder.Add(child);
		}

		Weights = weightsBuilder.ToImmutable();
		Children = childrenBuilder.ToImmutable();
	}

	internal ParentArea(bool isRow, ImmutableList<double> weights, ImmutableList<IArea> children)
	{
		IsRow = isRow;
		Weights = weights;
		Children = children;
	}
}

public record BaseSliceArea : BaseArea
{
	internal uint StartIndex { get; set; }
}

public record SliceArea : BaseSliceArea
{
	/// <summary>
	/// 0-indexed order of this area in the layout engine.
	/// </summary>
	public uint Order { get; }

	public uint MaxChildren { get; }

	public SliceArea(uint order = 0, uint maxChildren = 1, bool isRow = false)
	{
		Order = order;
		MaxChildren = maxChildren;
		IsRow = isRow;
	}
}

internal record OverflowArea : BaseSliceArea
{
	public OverflowArea(bool isRow = false)
	{
		IsRow = isRow;
	}
}

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
		// Set the start indexes of the areas.
		SetStartIndexes(area);

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

	private static void SetStartIndexes(this ParentArea area)
	{
		if (area.Children.Count == 0)
		{
			return;
		}

		List<SliceArea> sliceAreas = new();
		List<ParentArea> parentAreas = new();
		OverflowArea? overflowArea = null;

		// Iterate through the tree and add the areas to the list.
		Queue<IArea> areas = new();
		areas.Enqueue(area);

		while (areas.Count > 0)
		{
			IArea currArea = areas.Dequeue();

			if (currArea is ParentArea parentArea)
			{
				for (int i = 0; i < parentArea.Children.Count; i++)
				{
					areas.Enqueue(parentArea.Children[i]);
				}

				parentAreas.Add(parentArea);
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

		// Set the start indexes.
		uint currIdx = 0;
		foreach (SliceArea sliceArea in sliceAreas)
		{
			sliceArea.StartIndex = currIdx;
			currIdx += sliceArea.MaxChildren;
		}

		if (overflowArea is null)
		{
			Logger.Error($"No overflow area found, replacing last slice area with overflow area");
			overflowArea = ReplaceLastSliceAreaWithOverflowArea(area, sliceAreas[^1].Order);
		}

		if (overflowArea is not null)
		{
			SliceArea lastSlice = sliceAreas[^1];
			overflowArea.StartIndex = lastSlice.StartIndex + lastSlice.MaxChildren;
		}
	}

	private static OverflowArea? ReplaceLastSliceAreaWithOverflowArea(ParentArea rootArea, uint lastSliceOrder)
	{
		// DFS
		List<IArea> areaStack = new() { rootArea };

		while (areaStack.Count > 0)
		{
			ParentArea currParentArea = (ParentArea)areaStack[^1];
			areaStack.RemoveAt(areaStack.Count - 1);

			// Go over each of the children. If the child is the last slice, replace it.
			// Otherwise, add it to the stack.
			for (int i = 0; i < currParentArea.Children.Count; i++)
			{
				IArea child = currParentArea.Children[i];
				if (child is ParentArea childParentArea)
				{
					areaStack.Add(childParentArea);
				}
				else if (child is SliceArea sliceArea && sliceArea.Order == lastSliceOrder)
				{
					OverflowArea newOverflow = new(sliceArea.IsRow);
					areaStack.Add(newOverflow);
					RebuildWithOverflowArea(areaStack);
					return newOverflow;
				}
			}
		}

		Logger.Error("Could not replace last slice with overflow area");
		return null;
	}

	private static ParentArea RebuildWithOverflowArea(List<IArea> areaStack)
	{
		for (int idx = areaStack.Count - 2; idx >= 0; idx--)
		{
			ParentArea currArea = (ParentArea)areaStack[idx];
			IArea nextArea = areaStack[idx + 1];

			ImmutableList<IArea> currAreaChildren = currArea.Children;
			currAreaChildren = currAreaChildren.SetItem(currAreaChildren.IndexOf(nextArea), areaStack[idx + 1]);

			ImmutableList<double> currAreaWeights = currArea.Weights;

			areaStack[idx] = new ParentArea(currArea.IsRow, currAreaWeights, currAreaChildren);
		}

		return (ParentArea)areaStack[0];
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

	public static int DoSliceLayout(
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
