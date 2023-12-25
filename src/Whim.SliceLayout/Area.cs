using System.Collections.Immutable;

namespace Whim.SliceLayout;

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
	internal int StartIndex { get; set; }
}

public record SliceArea : BaseSliceArea
{
	/// <summary>
	/// 0-indexed order of this area in the layout engine.
	/// </summary>
	public int Order { get; }

	/// <summary>
	/// Maximum number of children this area can have. This must be a non-negative integer.
	/// </summary>
	public int MaxChildren { get; }

	public SliceArea(uint order = 0, uint maxChildren = 1, bool isRow = false)
	{
		Order = (int)order;
		MaxChildren = (int)maxChildren;
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
