namespace Whim.TreeLayout.Tests;

/// <summary>
/// This contains the window locations for <see cref="TestTree"/>.
/// </summary>
internal static class TestTreeWindowLocations
{
	public static ILocation<double> Left = new DoubleLocation() { X = 0, Y = 0, Width = 0.5, Height = 1 };
	public static ILocation<double> RightBottom = new DoubleLocation() { X = 0.5, Y = 0.5, Width = 0.5, Height = 0.5 };
	public static ILocation<double> RightTopLeftTop = new DoubleLocation() { X = 0.5, Y = 0, Width = 0.25, Height = 0.25 };
	public static ILocation<double> RightTopLeftBottomLeft = new DoubleLocation() { X = 0.5, Y = 0.25, Width = 0.125, Height = 0.25 };
	public static ILocation<double> RightTopLeftBottomRightTop = new DoubleLocation() { X = 0.625, Y = 0.25, Width = 0.125, Height = 0.175 };
	public static ILocation<double> RightTopLeftBottomRightBottom = new DoubleLocation() { X = 0.625, Y = 0.425, Width = 0.125, Height = 0.075 };
	public static ILocation<double> RightTopRight1 = new DoubleLocation() { X = 0.75, Y = 0, Width = 0.25, Height = 0.5 * 1d / 3 };
	public static ILocation<double> RightTopRight2 = new DoubleLocation() { X = 0.75, Y = 0.5 * 1d / 3, Width = 0.25, Height = 0.5 * 1d / 3 };
	public static ILocation<double> RightTopRight3 = new DoubleLocation() { X = 0.75, Y = 1d / 3, Width = 0.25, Height = 0.5 * 1d / 3 };

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

	public static IWindowLocation[] GetAllWindowLocations(ILocation<int> screen, IWindow leftWindow,
		IWindow rightTopLeftTopWindow,
		IWindow rightTopLeftBottomLeftWindow,
		IWindow rightTopLeftBottomRightTopWindow,
		IWindow rightTopLeftBottomRightBottomWindow,
		IWindow rightTopRight1Window,
		IWindow rightTopRight2Window,
		IWindow rightTopRight3Window,
		IWindow rightBottomWindow)
	=> new IWindowLocation[]
	{
		new WindowLocation(leftWindow, Left.Scale(screen), WindowState.Normal),
		new WindowLocation(rightTopLeftTopWindow, RightTopLeftTop.Scale(screen), WindowState.Normal),
		new WindowLocation(rightTopLeftBottomLeftWindow, RightTopLeftBottomLeft.Scale(screen), WindowState.Normal),
		new WindowLocation(rightTopLeftBottomRightTopWindow, RightTopLeftBottomRightTop.Scale(screen), WindowState.Normal),
		new WindowLocation(rightTopLeftBottomRightBottomWindow, RightTopLeftBottomRightBottom.Scale(screen), WindowState.Normal),
		new WindowLocation(rightTopRight1Window, RightTopRight1.Scale(screen), WindowState.Normal),
		new WindowLocation(rightTopRight2Window, RightTopRight2.Scale(screen), WindowState.Normal),
		new WindowLocation(rightTopRight3Window, RightTopRight3.Scale(screen), WindowState.Normal),
		new WindowLocation(rightBottomWindow, RightBottom.Scale(screen), WindowState.Normal)
	};
}
