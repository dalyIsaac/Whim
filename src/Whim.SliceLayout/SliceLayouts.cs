using System;

namespace Whim.SliceLayout;

/// <summary>
/// Methods to create common slice layouts.
/// </summary>
public static class SliceLayouts
{
	/// <summary>
	/// Creates a primary stack layout, where the first window takes up half the screen, and the
	/// remaining windows are stacked vertically on the other half.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="plugin"></param>
	/// <param name="identity"></param>
	/// <returns></returns>
	public static ILayoutEngine CreatePrimaryStackLayout(
		IContext context,
		ISliceLayoutPlugin plugin,
		LayoutEngineIdentity identity
	) => new SliceLayoutEngine(context, plugin, identity, CreatePrimaryStackArea()) { Name = "Primary stack" };

	internal static ParentArea CreatePrimaryStackArea() =>
		new(isRow: true, (0.5, new SliceArea(order: 0, maxChildren: 1)), (0.5, new OverflowArea()));

	/// <summary>
	/// Creates a multi-column layout with the given number of columns.
	/// <br />
	/// For example, new <c>uint[] { 2, 1, 0 }</c> will create a layout with 3 columns, where the
	/// first column has 2 rows, the second column has 1 row, and the third column has infinite rows.
	///
	/// For example:
	///
	/// <example>
	/// <code>
	/// -------------------------------------------------------------------------------------------------
	/// |                               |                               |                               |
	/// |                               |                               |                               |
	/// |                               |                               |                               |
	/// |                               |                               |                               |
	/// |                               |                               |                               |
	/// |                               |                               |                               |
	/// |                               |                               |                               |
	/// |                               |                               |                               |
	/// |                               |                               |                               |
	/// |                               |                               |                               |
	/// |           Slice 1             |           Slice 2             |           Overflow            |
	/// |          2 windows            |          1 window             |                               |
	/// |                               |                               |                               |
	/// |                               |                               |                               |
	/// |                               |                               |                               |
	/// |                               |                               |                               |
	/// |                               |                               |                               |
	/// |                               |                               |                               |
	/// |                               |                               |                               |
	/// |                               |                               |                               |
	/// |                               |                               |                               |
	/// |                               |                               |                               |
	/// |                               |                               |                               |
	/// -------------------------------------------------------------------------------------------------
	/// </code>
	/// </example>
	/// </summary>
	/// <param name="context">The <see cref="IContext"/> to use</param>
	/// <param name="plugin">The <see cref="ISliceLayoutPlugin"/> to use</param>
	/// <param name="identity">The identity of the layout engine</param>
	/// <param name="capacities">The number of rows in each column</param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"></exception>
	public static ILayoutEngine CreateMultiColumnLayout(
		IContext context,
		ISliceLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		params uint[] capacities
	) => new SliceLayoutEngine(context, plugin, identity, CreateMultiColumnArea(capacities)) { Name = "Multi-column " };

	internal static ParentArea CreateMultiColumnArea(uint[] capacities)
	{
		double weight = 1.0 / capacities.Length;
		(double, IArea)[] areas = new (double, IArea)[capacities.Length];

		int overflowIdx = -1;
		for (int idx = 0; idx < capacities.Length; idx++)
		{
			uint capacity = capacities[idx];
			if (capacity == 0)
			{
				if (overflowIdx != -1)
				{
					// Replace the last overflow area with a slice area
					areas[overflowIdx] = (weight, new SliceArea(order: (uint)overflowIdx, maxChildren: 0));
				}

				areas[idx] = (weight, new OverflowArea());
				overflowIdx = idx;
			}
			else
			{
				areas[idx] = (weight, new SliceArea(order: (uint)idx, maxChildren: capacity));
			}
		}

		ParentArea parentArea = new(isRow: true, areas);
		return parentArea;
	}

	/// <summary>
	/// Creates a three-column layout, where the primary column is in the middle, the secondary
	/// column is on the left, and the overflow column is on the right.
	///
	/// The middle column takes up 50% of the screen, and the left and right columns take up 25%.
	///
	/// For example:
	///
	/// <example>
	/// <code>
	/// ------------------------------------------------------------------------
	/// |                 |                             |                      |
	/// |                 |                             |                      |
	/// |                 |                             |                      |
	/// |                 |                             |                      |
	/// |                 |                             |                      |
	/// |                 |                             |                      |
	/// |                 |                             |                      |
	/// |                 |                             |                      |
	/// |                 |                             |                      |
	/// |                 |                             |                      |
	/// |     Slice 2     |           Slice 1           |       Overflow       |
	/// |    2 windows    |          1 window           |                      |
	/// |                 |                             |                      |
	/// |                 |                             |                      |
	/// |                 |                             |                      |
	/// |                 |                             |                      |
	/// |                 |                             |                      |
	/// |                 |                             |                      |
	/// |                 |                             |                      |
	/// |                 |                             |                      |
	/// |                 |                             |                      |
	/// |                 |                             |                      |
	/// |                 |                             |                      |
	/// ------------------------------------------------------------------------
	/// </code>
	/// </example>
	/// </summary>
	/// <param name="context"></param>
	/// <param name="plugin"></param>
	/// <param name="identity"></param>
	/// <param name="primaryColumnCapacity">
	/// The number of rows in the primary column. This must be a non-negative integer.
	/// </param>
	/// <param name="secondaryColumnCapacity">
	/// The number of rows in the secondary column. This must be a non-negative integer.
	/// </param>
	/// <returns></returns>
	public static ILayoutEngine CreateSecondaryPrimaryLayout(
		IContext context,
		ISliceLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		uint primaryColumnCapacity = 1,
		uint secondaryColumnCapacity = 2
	) =>
		new SliceLayoutEngine(
			context,
			plugin,
			identity,
			CreateSecondaryPrimaryArea(primaryColumnCapacity, secondaryColumnCapacity)
		)
		{
			Name = "Secondary primary"
		};

	internal static ParentArea CreateSecondaryPrimaryArea(uint primaryColumnCapacity, uint secondaryColumnCapacity)
	{
		return new ParentArea(
			isRow: true,
			(0.25, new SliceArea(order: 1, maxChildren: secondaryColumnCapacity)),
			(0.5, new SliceArea(order: 0, maxChildren: primaryColumnCapacity)),
			(0.25, new OverflowArea())
		);
	}
}
