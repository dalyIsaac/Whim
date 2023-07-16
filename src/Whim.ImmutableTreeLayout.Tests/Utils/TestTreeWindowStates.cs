namespace Whim.ImmutableTreeLayout.Tests;

/// <summary>
/// This contains the window states for <see cref="TestTree"/>.
/// </summary>
internal static class TestTreeWindowStates
{
	public static ILocation<double> Left = new Location<double>() { Width = 0.5, Height = 1 };
	public static ILocation<double> RightBottom = new Location<double>()
	{
		X = 0.5,
		Y = 0.5,
		Width = 0.5,
		Height = 0.5
	};
	public static ILocation<double> RightTopLeftTop = new Location<double>()
	{
		X = 0.5,
		Y = 0,
		Width = 0.25,
		Height = 0.25
	};
	public static ILocation<double> RightTopLeftBottomLeft = new Location<double>()
	{
		X = 0.5,
		Y = 0.25,
		Width = 0.125,
		Height = 0.25
	};
	public static ILocation<double> RightTopLeftBottomRightTop = new Location<double>()
	{
		X = 0.625,
		Y = 0.25,
		Width = 0.125,
		Height = 0.175
	};
	public static ILocation<double> RightTopLeftBottomRightBottom = new Location<double>()
	{
		X = 0.625,
		Y = 0.425,
		Width = 0.125,
		Height = 0.075
	};
	public static ILocation<double> RightTopRight1 = new Location<double>()
	{
		X = 0.75,
		Y = 0,
		Width = 0.25,
		Height = 0.5 * 1d / 3
	};
	public static ILocation<double> RightTopRight2 = new Location<double>()
	{
		X = 0.75,
		Y = 0.5 * 1d / 3,
		Width = 0.25,
		Height = 0.5 * 1d / 3
	};
	public static ILocation<double> RightTopRight3 = new Location<double>()
	{
		X = 0.75,
		Y = 1d / 3,
		Width = 0.25,
		Height = 0.5 * 1d / 3
	};

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

	public static IWindowState[] GetAllWindowStates(
		ILocation<int> screen,
		IWindow leftWindow,
		IWindow rightTopLeftTopWindow,
		IWindow rightTopLeftBottomLeftWindow,
		IWindow rightTopLeftBottomRightTopWindow,
		IWindow rightTopLeftBottomRightBottomWindow,
		IWindow rightTopRight1Window,
		IWindow rightTopRight2Window,
		IWindow rightTopRight3Window,
		IWindow rightBottomWindow
	)
	{
		return new IWindowState[]
		{
			new WindowState()
			{
				Window = leftWindow,
				Location = Left.Scale(screen),
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = rightTopLeftTopWindow,
				Location = RightTopLeftTop.Scale(screen),
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = rightTopLeftBottomLeftWindow,
				Location = RightTopLeftBottomLeft.Scale(screen),
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = rightTopLeftBottomRightTopWindow,
				Location = RightTopLeftBottomRightTop.Scale(screen),
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = rightTopLeftBottomRightBottomWindow,
				Location = RightTopLeftBottomRightBottom.Scale(screen),
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = rightTopRight1Window,
				Location = RightTopRight1.Scale(screen),
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = rightTopRight2Window,
				Location = RightTopRight2.Scale(screen),
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = rightTopRight3Window,
				Location = RightTopRight3.Scale(screen),
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = rightBottomWindow,
				Location = RightBottom.Scale(screen),
				WindowSize = WindowSize.Normal
			}
		};
	}
}
