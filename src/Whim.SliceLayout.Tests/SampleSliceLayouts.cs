namespace Whim.SliceLayout.Tests;

public static class SampleSliceLayouts
{
	// ------------------------------------------------------------------
	// |                               |                                |
	// |                               |                                |
	// |                               |                                |
	// |                               |                                |
	// |                               |             Slice 1            |
	// |                               |            2 windows           |
	// |                               |                                |
	// |                               |                                |
	// |                               |                                |
	// |                               |                                |
	// |           Slice 0             |--------------------------------|
	// |          2 windows            |                                |
	// |                               |                                |
	// |                               |                                |
	// |                               |                                |
	// |                               |                                |
	// |                               |           Overflow             |
	// |                               |                                |
	// |                               |                                |
	// |                               |                                |
	// |                               |                                |
	// |                               |                                |
	// |                               |                                |
	// ------------------------------------------------------------------
	public static ParentArea CreateNestedLayout() =>
		new(
			isRow: true,
			(0.5, new SliceArea(order: 0, maxChildren: 2)),
			(
				0.5,
				new ParentArea(isRow: false, (0.5, new SliceArea(order: 1, maxChildren: 2)), (0.5, new OverflowArea()))
			)
		);

	// --------------------------------
	// |                              |
	// |                              |
	// |                              |
	// |                              |
	// |                              |
	// |                              |
	// |                              |
	// |                              |
	// |                              |
	// |                              |
	// |                              |
	// |          Overflow            |
	// |                              |
	// |                              |
	// |                              |
	// |                              |
	// |                              |
	// |                              |
	// |                              |
	// |                              |
	// |                              |
	// |                              |
	// |                              |
	// --------------------------------
	public static ParentArea CreateOverflowColumnLayout() => new(isRow: false, (1.0, new OverflowArea()));

	// ------------------------------------------------------------------
	// |                                                                |
	// |                                                                |
	// |                                                                |
	// |                           Overflow                             |
	// |                                                                |
	// |                                                                |
	// |                                                                |
	// ------------------------------------------------------------------
	public static ParentArea CreateOverflowRowLayout() => new(isRow: true, (1.0, new OverflowArea(isRow: true)));
}
