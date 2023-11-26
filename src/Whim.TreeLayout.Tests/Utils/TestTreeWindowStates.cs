namespace Whim.TreeLayout.Tests;

/// <summary>
/// This contains the window states for <see cref="TestTree"/>.
/// </summary>
internal static class TestTreeWindowStates
{
	public static IRectangle<double> Left = new Rectangle<double>() { Width = 0.5, Height = 1 };
	public static IRectangle<double> RightBottom = new Rectangle<double>()
	{
		X = 0.5,
		Y = 0.5,
		Width = 0.5,
		Height = 0.5
	};
	public static IRectangle<double> RightTopLeftTop = new Rectangle<double>()
	{
		X = 0.5,
		Y = 0,
		Width = 0.25,
		Height = 0.25
	};
	public static IRectangle<double> RightTopLeftBottomLeft = new Rectangle<double>()
	{
		X = 0.5,
		Y = 0.25,
		Width = 0.125,
		Height = 0.25
	};
	public static IRectangle<double> RightTopLeftBottomRightTop = new Rectangle<double>()
	{
		X = 0.625,
		Y = 0.25,
		Width = 0.125,
		Height = 0.175
	};
	public static IRectangle<double> RightTopLeftBottomRightBottom = new Rectangle<double>()
	{
		X = 0.625,
		Y = 0.425,
		Width = 0.125,
		Height = 0.075
	};
	public static IRectangle<double> RightTopRight1 = new Rectangle<double>()
	{
		X = 0.75,
		Y = 0,
		Width = 0.25,
		Height = 0.5 * 1d / 3
	};
	public static IRectangle<double> RightTopRight2 = new Rectangle<double>()
	{
		X = 0.75,
		Y = 0.5 * 1d / 3,
		Width = 0.25,
		Height = 0.5 * 1d / 3
	};
	public static IRectangle<double> RightTopRight3 = new Rectangle<double>()
	{
		X = 0.75,
		Y = 1d / 3,
		Width = 0.25,
		Height = 0.5 * 1d / 3
	};

	public static IRectangle<double>[] All = new IRectangle<double>[]
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
		IRectangle<int> screen,
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
