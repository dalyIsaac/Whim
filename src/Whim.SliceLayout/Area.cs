using System;
using System.Collections.Immutable;
using System.Linq;

namespace Whim.SliceLayout;

/// <summary>
/// Represents an area in the layout engine.
/// </summary>
public interface IArea
{
	/// <summary>
	/// When <see langword="true"/>, its children are arranged horizontally. Otherwise, they are
	/// arranged vertically.
	/// </summary>
	bool IsRow { get; }
}

/// <inheritdoc cref="IArea"/>
public abstract record BaseArea : IArea
{
	/// <inheritdoc cref="IArea.IsRow"/>
	public bool IsRow { get; protected set; }
}

/// <summary>
/// Represents an area that can have any <see cref="IArea"/> as a child.
/// </summary>
public record ParentArea : BaseArea
{
	/// <summary>
	/// Weights of the children. The sum of the weights should be 1.0.
	/// </summary>
	public ImmutableList<double> Weights { get; }

	/// <summary>
	/// Children of this area.
	/// </summary>
	public ImmutableList<IArea> Children { get; }

	/// <summary>
	/// Creates a new <see cref="ParentArea"/> with the given children.
	/// </summary>
	/// <param name="isRow"></param>
	/// <param name="children">
	/// A tuple of the weight and the child. The sum of the weights should be 1.0.
	/// </param>
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

	/// <summary>
	/// Determines whether the specified object is equal to the current object. This compares
	/// the contents of the weights and children.
	/// </summary>
	/// <param name="other"></param>
	/// <returns></returns>
	public virtual bool Equals(ParentArea? other)
	{
		if (other is null)
		{
			return false;
		}

		if (ReferenceEquals(this, other))
		{
			return true;
		}

		return IsRow == other.IsRow && Weights.SequenceEqual(other.Weights) && Children.SequenceEqual(other.Children);
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		// Get the hash code of the weights and children
		int hash = HashCode.Combine(IsRow);
		foreach (double weight in Weights)
		{
			hash = HashCode.Combine(hash, weight);
		}
		foreach (IArea child in Children)
		{
			hash = HashCode.Combine(hash, child);
		}

		return hash;
	}
}

/// <summary>
/// Represents an implicit slice of the list of <see cref="IWindow"/>s. The windows are contained
/// by the <see cref="SliceLayoutEngine"/>.
/// </summary>
public record BaseSliceArea : BaseArea
{
	internal int StartIndex { get; set; }
}

/// <summary>
/// An area that can have <see cref="IWindow"/>s as children. There can be multiple
/// <see cref="SliceArea"/>s in a layout engine, ordered by their <see cref="Order"/>s.
/// </summary>
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

	/// <summary>
	/// Creates a new <see cref="SliceArea"/> with the given order and maximum number of children.
	/// </summary>
	/// <param name="order"></param>
	/// <param name="maxChildren"></param>
	/// <param name="isRow"></param>
	public SliceArea(uint order = 0, uint maxChildren = 1, bool isRow = false)
	{
		Order = (int)order;
		MaxChildren = (int)maxChildren;
		IsRow = isRow;
	}
}

/// <summary>
/// An area that can have an infinite number of <see cref="IWindow"/>s as a child.
/// There can be only one <see cref="OverflowArea"/> in a layout engine - any additional
/// <see cref="OverflowArea"/>s will be ignored.
///
/// <see cref="OverflowArea"/>s implicitly are the last area in the layout engine, in comparison
/// to all <see cref="SliceArea"/>s.
/// </summary>
public record OverflowArea : BaseSliceArea
{
	/// <summary>
	/// Creates a new <see cref="OverflowArea"/>.
	/// </summary>
	/// <param name="isRow"></param>
	public OverflowArea(bool isRow = false)
	{
		IsRow = isRow;
	}
}
