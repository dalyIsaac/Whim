using System;
using System.Collections.Immutable;

namespace Whim.SliceLayout;

// TODO: docs
public interface IArea
{
	/// <summary>
	/// When <see langword="true"/>, the <see cref="Children"/> are arranged horizontally.
	/// Otherwise, they are arranged vertically.
	/// </summary>
	bool IsHorizontal { get; }
}

public abstract record BaseArea : IArea
{
	public bool IsHorizontal { get; protected set; }
}

public record ParentArea : BaseArea
{
	public ImmutableList<double> Weights { get; }

	public ImmutableList<IArea> Children { get; }

	public ParentArea(bool isHorizontal = true, params (double Weight, IArea Child)[] children)
	{
		IsHorizontal = isHorizontal;
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

	internal ParentArea(bool isHorizontal, ImmutableList<double> weights, ImmutableList<IArea> children)
	{
		IsHorizontal = isHorizontal;
		Weights = weights;
		Children = children;
	}
}

public record BaseSliceArea : BaseArea
{
	public uint StartIndex { get; }
}

public record SliceArea : BaseSliceArea
{
	/// <summary>
	/// 0-indexed order of this area in the layout engine.
	/// </summary>
	public uint Order { get; }

	public uint MaxChildren { get; }

	public SliceArea(uint order = 0, uint maxChildren = 1, bool isHorizontal = true)
	{
		Order = order;
		MaxChildren = maxChildren;
		IsHorizontal = isHorizontal;
	}
}

internal record OverflowArea : BaseSliceArea { }

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

		for (int i = 0; i < area.Children.Count; i++)
		{
			IArea child = area.Children[i];
			if (child is ParentArea parentArea)
			{
				parentArea = parentArea.Prune(windowCount);
				if (parentArea.Children.Count == 0)
				{
					continue;
				}
			}
			else if (child is BaseSliceArea baseSliceArea)
			{
				if (baseSliceArea.StartIndex >= windowCount)
				{
					continue;
				}

				if (baseSliceArea is SliceArea sliceArea && sliceArea.MaxChildren == 0)
				{
					continue;
				}

				childrenBuilder.Add(child);
				weightsBuilder.Add(area.Weights[i]);
			}

			childrenBuilder.Add(child);
			weightsBuilder.Add(area.Weights[i]);
		}

		return new ParentArea(area.IsHorizontal, weightsBuilder.ToImmutable(), childrenBuilder.ToImmutable());
	}

	public static void DoParentLayout(
		this SliceRectangleItem[] items,
		int startIdx,
		IRectangle<int> rectangle,
		ParentArea area
	)
	{
		if (startIdx >= items.Length)
		{
			return;
		}

		int x = rectangle.X;
		int y = rectangle.Y;
		int width = rectangle.Width;
		int height = rectangle.Height;

		for (int currIdx = 0; currIdx < area.Children.Count; currIdx++)
		{
			double weight = area.Weights[currIdx];
			IArea childArea = area.Children[currIdx];

			if (area.IsHorizontal)
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
				items.DoParentLayout(startIdx + currIdx, childRectangle, parentArea);
			}
			else if (childArea is BaseSliceArea sliceArea)
			{
				items.DoSliceLayout(startIdx + currIdx, childRectangle, sliceArea);
			}

			if (area.IsHorizontal)
			{
				x += width;
			}
			else
			{
				y += height;
			}
		}
	}

	public static void DoSliceLayout(
		this SliceRectangleItem[] items,
		int startIdx,
		IRectangle<int> rectangle,
		BaseSliceArea area
	)
	{
		if (startIdx >= items.Length)
		{
			return;
		}

		int x = rectangle.X;
		int y = rectangle.Y;
		int width = rectangle.Width;
		int height = rectangle.Height;

		int deltaX = 0;
		int deltaY = 0;

		int remainingItemsCount = items.Length - startIdx;
		int sliceItemsCount = remainingItemsCount;
		if (area is SliceArea sliceArea)
		{
			sliceItemsCount = Convert.ToInt32(Math.Min(sliceArea.MaxChildren, remainingItemsCount));
		}
		int maxIdx = startIdx + sliceItemsCount;

		if (area.IsHorizontal)
		{
			deltaX = rectangle.Width / sliceItemsCount;
			width = deltaX;
		}
		else
		{
			deltaY = rectangle.Height / sliceItemsCount;
			height = deltaY;
		}

		for (int currIdx = startIdx; currIdx < maxIdx; currIdx++)
		{
			items[currIdx] = new SliceRectangleItem(currIdx, new Rectangle<int>(x, y, width, height));

			if (area.IsHorizontal)
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
