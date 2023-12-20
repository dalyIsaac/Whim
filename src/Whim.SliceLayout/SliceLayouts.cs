using System;

namespace Whim.SliceLayout;

public static class SliceLayouts
{
	/// <summary>
	/// Creates a primary stack layout, where the first window takes up half the screen, and the
	/// remaining windows are stacked vertically on the other half.
	/// </summary>
	/// <param name="plugin"></param>
	/// <param name="identity"></param>
	/// <returns></returns>
	public static ILayoutEngine CreatePrimaryStackLayout(ISliceLayoutPlugin plugin, LayoutEngineIdentity identity) =>
		new SliceLayoutEngine(
			plugin,
			identity,
			new ParentArea(isHorizontal: true, (0.5, new SliceArea(maxChildren: 1, order: 0)), (0.5, new BaseArea()))
		);

	/// <summary>
	/// Creates a multi-column layout with the given number of columns.
	/// <br />
	/// For example, new <c>uint[] { 2, 1, 0 }</c> will create a layout with 3 columns, where the
	/// first column has 2 rows, the second column has 1 row, and the third column has infinite rows.
	/// </summary>
	/// <param name="plugin">The <see cref="ISliceLayoutPlugin"/> to use</param>
	/// <param name="identity">The identity of the layout engine</param>
	/// <param name="capacities">The number of rows in each column</param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"></exception>
	public static ILayoutEngine CreateMultiColumnLayout(
		ISliceLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		params uint[] capacities
	)
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
				areas[idx] = (weight, new SliceArea(maxChildren: capacity, order: (uint)idx));
			}
		}

		return new SliceLayoutEngine(plugin, identity, new ParentArea(isHorizontal: true, areas));
	}
}
