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

public record BaseArea : IArea
{
	public bool IsHorizontal { get; init; }
}

public record SliceArea : BaseArea
{
	public uint Priority { get; init; }

	public uint MaxChildren { get; init; }

	public ImmutableList<double> Weights { get; }

	public ImmutableList<IArea> Children { get; }

	public SliceArea(params (double Weight, IArea Child)[] children)
	{
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

	public static SliceArea SingleWindowArea => new((1, new BaseArea()));
}

public static class SliceLayouts
{
	/// <summary>
	/// Creates a primary stack layout, where the first window takes up half the screen, and the
	/// remaining windows are stacked vertically on the other half.
	/// </summary>
	/// <param name="identity"></param>
	/// <returns></returns>
	public static ILayoutEngine PrimaryStackLayout(LayoutEngineIdentity identity) =>
		new SliceLayoutEngine(
			identity,
			new SliceArea((0.5, SliceArea.SingleWindowArea), (0.5, new BaseArea() { IsHorizontal = false }))
		);

	/// <summary>
	/// Creates a multi-column layout with the given number of columns.
	/// <br />
	/// For example, new <c>uint[] { 2, 1, 0 }</c> will create a layout with 3 columns, where the
	/// first column has 2 rows, the second column has 1 row, and the third column has infinite rows.
	/// </summary>
	/// <param name="identity">The identity of the layout engine</param>
	/// <param name="capacities">The number of rows in each column</param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"></exception>
	public static ILayoutEngine CreateMultiColumnLayout(LayoutEngineIdentity identity, params uint[] capacities)
	{
		double weight = 1.0 / capacities.Length;
		(double, IArea)[] areas = new (double, IArea)[capacities.Length];

		bool createdBaseArea = false;
		for (int idx = 0; idx < capacities.Length; idx++)
		{
			uint capacity = capacities[idx];
			if (capacity == 0)
			{
				if (createdBaseArea)
				{
					throw new ArgumentException("Cannot have multiple base areas");
				}

				areas[idx] = (weight, new BaseArea());
				createdBaseArea = true;
			}
			else
			{
				areas[idx] = (weight, new SliceArea((1.0 / capacity, SliceArea.SingleWindowArea)));
			}
		}

		return new SliceLayoutEngine(identity, new SliceArea(areas));
	}
}
