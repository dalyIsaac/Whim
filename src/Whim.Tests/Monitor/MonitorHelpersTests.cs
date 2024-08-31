namespace Whim.Tests;

public class MonitorHelpersTests
{
	public static TheoryData<Rectangle<int>, Point<int>, Point<double>> NormalizeDeltaPoint_Data =>
		new()
		{
			{
				new Rectangle<int>() { Width = 1920, Height = 1080 },
				new Point<int>() { X = 192, Y = 108 },
				new Point<double>() { X = 0.1, Y = 0.1 }
			},
			{
				new Rectangle<int>() { Width = 1920, Height = 1080 },
				new Point<int>() { X = 960, Y = 270 },
				new Point<double>() { X = 0.5, Y = 0.25 }
			},
			{
				new Rectangle<int>()
				{
					X = 100,
					Y = 100,
					Width = 1920,
					Height = 1080,
				},
				new Point<int>() { X = 192, Y = 108 },
				new Point<double>() { X = 0.1, Y = 0.1 }
			},
			{
				new Rectangle<int>()
				{
					X = 100,
					Y = 100,
					Width = 1920,
					Height = 1080,
				},
				new Point<int>() { X = 960, Y = 270 },
				new Point<double>() { X = 0.5, Y = 0.25 }
			},
			{
				new Rectangle<int>()
				{
					X = -100,
					Y = 100,
					Width = 1920,
					Height = 1080,
				},
				new Point<int>() { X = 192, Y = 108 },
				new Point<double>() { X = 0.1, Y = 0.1 }
			},
			{
				new Rectangle<int>()
				{
					X = -100,
					Y = 100,
					Width = 1920,
					Height = 1080,
				},
				new Point<int>() { X = 960, Y = 270 },
				new Point<double>() { X = 0.5, Y = 0.25 }
			},
		};

	[Theory]
	[MemberData(nameof(NormalizeDeltaPoint_Data))]
	public void NormalizeDeltaPoint(IRectangle<int> monitor, IPoint<int> point, IPoint<double> expected)
	{
		// When
		IPoint<double> actual = monitor.NormalizeDeltaPoint(point);

		// Then
		Assert.Equal(expected.X, actual.X);
		Assert.Equal(expected.Y, actual.Y);
	}

	public static TheoryData<Rectangle<int>, Point<int>, Point<double>> NormalizeAbsolutePoint_Data =>
		new()
		{
			{
				new Rectangle<int>() { Width = 1920, Height = 1080 },
				new Point<int>() { X = 192, Y = 108 },
				new Point<double>() { X = 0.1, Y = 0.1 }
			},
			{
				new Rectangle<int>() { Width = 1920, Height = 1080 },
				new Point<int>() { X = 960, Y = 270 },
				new Point<double>() { X = 0.5, Y = 0.25 }
			},
			{
				new Rectangle<int>()
				{
					X = 100,
					Y = 100,
					Width = 1920,
					Height = 1080,
				},
				new Point<int>() { X = 192 + 100, Y = 108 + 100 },
				new Point<double>() { X = 0.1, Y = 0.1 }
			},
			{
				new Rectangle<int>()
				{
					X = 100,
					Y = 100,
					Width = 1920,
					Height = 1080,
				},
				new Point<int>() { X = 960 + 100, Y = 270 + 100 },
				new Point<double>() { X = 0.5, Y = 0.25 }
			},
			{
				new Rectangle<int>()
				{
					X = -100,
					Y = 100,
					Width = 1920,
					Height = 1080,
				},
				new Point<int>() { X = 192 - 100, Y = 108 + 100 },
				new Point<double>() { X = 0.1, Y = 0.1 }
			},
			{
				new Rectangle<int>()
				{
					X = -100,
					Y = 100,
					Width = 1920,
					Height = 1080,
				},
				new Point<int>() { X = 960 - 100, Y = 270 + 100 },
				new Point<double>() { X = 0.5, Y = 0.25 }
			},
		};

	[Theory]
	[MemberData(nameof(NormalizeAbsolutePoint_Data))]
	public void NormalizeAbsolutePoint(IRectangle<int> monitor, IPoint<int> point, IPoint<double> expected)
	{
		// When
		IPoint<double> actual = monitor.NormalizeAbsolutePoint(point);

		// Then
		Assert.Equal(expected.X, actual.X);
		Assert.Equal(expected.Y, actual.Y);
	}

	public static TheoryData<Rectangle<int>, Point<int>, Point<double>> NormalizeAbsolutePoint_Data_RespectSign =>
		new()
		{
			{
				new Rectangle<int>() { Width = 1920, Height = 1080 },
				new Point<int>() { X = -192, Y = 108 },
				new Point<double>() { X = -0.1, Y = 0.1 }
			},
			{
				new Rectangle<int>() { Width = 1920, Height = 1080 },
				new Point<int>() { X = 960, Y = -270 },
				new Point<double>() { X = 0.5, Y = -0.25 }
			},
			{
				new Rectangle<int>()
				{
					X = 100,
					Y = 100,
					Width = 1920,
					Height = 1080,
				},
				new Point<int>() { X = -192 + 100, Y = -108 + 100 },
				new Point<double>() { X = -0.1, Y = -0.1 }
			},
			{
				new Rectangle<int>()
				{
					X = 100,
					Y = 100,
					Width = 1920,
					Height = 1080,
				},
				new Point<int>() { X = -960 + 100, Y = 270 + 100 },
				new Point<double>() { X = -0.5, Y = 0.25 }
			},
			{
				new Rectangle<int>()
				{
					X = -100,
					Y = 100,
					Width = 1920,
					Height = 1080,
				},
				new Point<int>() { X = 192 - 100, Y = -108 + 100 },
				new Point<double>() { X = 0.1, Y = -0.1 }
			},
			{
				new Rectangle<int>()
				{
					X = -100,
					Y = 100,
					Width = 1920,
					Height = 1080,
				},
				new Point<int>() { X = -960 - 100, Y = -270 + 100 },
				new Point<double>() { X = -0.5, Y = -0.25 }
			},
		};

	[Theory]
	[MemberData(nameof(NormalizeAbsolutePoint_Data_RespectSign))]
	public void NormalizeAbsolutePoint_RespectSignTheory(
		IRectangle<int> monitor,
		IPoint<int> point,
		IPoint<double> expected
	)
	{
		// When
		IPoint<double> actual = monitor.NormalizeAbsolutePoint(point, respectSign: true);

		// Then
		Assert.Equal(expected.X, actual.X);
		Assert.Equal(expected.Y, actual.Y);
	}

	public static TheoryData<Rectangle<int>, Rectangle<int>, Rectangle<double>> NormalizeRectangle_Double =>
		new()
		{
			// Monitor: 1920x1080, from the origin. Rectangle: at (192, 108).
			{
				new Rectangle<int>() { Width = 1920, Height = 1080 },
				new Rectangle<int>()
				{
					X = 192,
					Y = 108,
					Width = 192,
					Height = 108,
				},
				new Rectangle<double>()
				{
					X = 0.1,
					Y = 0.1,
					Width = 0.1,
					Height = 0.1,
				}
			},
			// Monitor 1920x1080, from (100, 100). Rectangle: at (192, 108).
			{
				new Rectangle<int>()
				{
					X = 100,
					Y = 100,
					Width = 1920,
					Height = 1080,
				},
				new Rectangle<int>()
				{
					X = 192 + 100,
					Y = 108 + 100,
					Width = 192,
					Height = 108,
				},
				new Rectangle<double>()
				{
					X = 0.1,
					Y = 0.1,
					Width = 0.1,
					Height = 0.1,
				}
			},
			// Monitor 1920x1080, from (-100, -100). Rectangle: at (98, 8).
			{
				new Rectangle<int>()
				{
					X = -100,
					Y = -100,
					Width = 1920,
					Height = 1080,
				},
				new Rectangle<int>()
				{
					X = 192 - 100,
					Y = 108 - 100,
					Width = 192,
					Height = 108,
				},
				new Rectangle<double>()
				{
					X = 0.1,
					Y = 0.1,
					Width = 0.1,
					Height = 0.1,
				}
			},
			// Monitor 1920x1080, from the origin. Rectangle: at (-192, -108).
			{
				new Rectangle<int>() { Width = 1920, Height = 1080 },
				new Rectangle<int>()
				{
					X = -192,
					Y = -108,
					Width = 192,
					Height = 108,
				},
				new Rectangle<double>()
				{
					X = 0,
					Y = 0,
					Width = 0.1,
					Height = 0.1,
				}
			},
			// Monitor 1920x1080, from the origin. Rectangle at (1920 + 192, 1080 + 108).
			{
				new Rectangle<int>() { Width = 1920, Height = 1080 },
				new Rectangle<int>()
				{
					X = 1920 + 192,
					Y = 1080 + 108,
					Width = 192,
					Height = 108,
				},
				new Rectangle<double>()
				{
					X = 0.9,
					Y = 0.9,
					Width = 0.1,
					Height = 0.1,
				}
			},
		};

	[Theory]
	[MemberData(nameof(NormalizeRectangle_Double))]
	public void NormalizeRectangle(IRectangle<int> monitor, IRectangle<int> rect, IRectangle<double> expected)
	{
		// When
		IRectangle<double> actual = monitor.NormalizeRectangle(rect);

		// Then
		Assert.Equal(expected.X, actual.X);
		Assert.Equal(expected.Y, actual.Y);
	}

	public static TheoryData<Rectangle<int>, Rectangle<double>, Rectangle<int>> ToMonitor_Data =>
		new()
		{
			{
				new Rectangle<int>() { Width = 1920, Height = 1080 },
				new Rectangle<double>()
				{
					X = 0.1,
					Y = 0.1,
					Width = 0.1,
					Height = 0.1,
				},
				new Rectangle<int>()
				{
					X = 192,
					Y = 108,
					Width = 192,
					Height = 108,
				}
			},
			{
				new Rectangle<int>()
				{
					X = 100,
					Y = 100,
					Width = 1920,
					Height = 1080,
				},
				new Rectangle<double>()
				{
					X = 0.1,
					Y = 0.1,
					Width = 0.1,
					Height = 0.1,
				},
				new Rectangle<int>()
				{
					X = 192 + 100,
					Y = 108 + 100,
					Width = 192,
					Height = 108,
				}
			},
			{
				new Rectangle<int>()
				{
					X = -1920,
					Y = -1080,
					Width = 1920,
					Height = 1080,
				},
				new Rectangle<double>()
				{
					X = 0.1,
					Y = 0.1,
					Width = 0.1,
					Height = 0.1,
				},
				new Rectangle<int>()
				{
					X = -1920 + 192,
					Y = -1080 + 108,
					Width = 192,
					Height = 108,
				}
			},
		};

	[Theory]
	[MemberData(nameof(ToMonitor_Data))]
	public void ToMonitor_Theory(IRectangle<int> monitor, IRectangle<double> rect, IRectangle<int> expected)
	{
		// When
		IRectangle<int> actual = monitor.ToMonitor(rect);

		// Then
		Assert.Equal(expected.X, actual.X);
		Assert.Equal(expected.Y, actual.Y);
	}
}
