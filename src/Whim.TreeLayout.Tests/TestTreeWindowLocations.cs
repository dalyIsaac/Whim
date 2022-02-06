namespace Whim.TreeLayout.Tests;

/// <summary>
/// This contains the window locations for <see cref="TestTree"/>.
/// </summary>
internal static class TestTreeWindowLocations
{
	public static ILocation<double> Left = new NodeLocation() { X = 0, Y = 0, Width = 0.5, Height = 1 };
	public static ILocation<double> RightBottom = new NodeLocation() { X = 0.5, Y = 0.5, Width = 0.5, Height = 0.5 };
	public static ILocation<double> RightTopLeftTop = new NodeLocation() { X = 0.5, Y = 0, Width = 0.25, Height = 0.25 };
	public static ILocation<double> RightTopLeftBottomLeft = new NodeLocation() { X = 0.5, Y = 0.25, Width = 0.125, Height = 0.25 };
	public static ILocation<double> RightTopLeftBottomRightTop = new NodeLocation() { X = 0.625, Y = 0.25, Width = 0.125, Height = 0.175 };
	public static ILocation<double> RightTopLeftBottomRightBottom = new NodeLocation() { X = 0.625, Y = 0.425, Width = 0.125, Height = 0.075 };
	public static ILocation<double> RightTopRight1 = new NodeLocation() { X = 0.75, Y = 0, Width = 0.25, Height = 0.5 * 1d / 3 };
	public static ILocation<double> RightTopRight2 = new NodeLocation() { X = 0.75, Y = 0.5 * 1d / 3, Width = 0.25, Height = 0.5 * 1d / 3 };
	public static ILocation<double> RightTopRight3 = new NodeLocation() { X = 0.75, Y = 1d / 3, Width = 0.25, Height = 0.5 * 1d / 3 };

	public static ILocation<double>[] All = new ILocation<double>[]
	{
			Left,
			RightTopLeftTop,
			RightTopLeftBottomLeft,
			RightTopLeftBottomRightTop,
			RightTopLeftBottomRightBottom,
			RightTopRight1,
			RightTopRight2,
			RightTopRight3,
			RightBottom
	};
}