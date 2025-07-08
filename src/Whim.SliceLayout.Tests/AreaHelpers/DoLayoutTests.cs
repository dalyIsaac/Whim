using FluentAssertions;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.SliceLayout.Tests;

public class DoLayoutTests
{
	private static readonly LayoutEngineIdentity identity = new();

	public static TheoryData<IRectangle<int>, ParentArea, IRectangle<int>[]> DoLayout_PrimaryStack =>
		new()
		{
			// Empty
			{
				new Rectangle<int>(0, 0, 100, 100),
				SliceLayouts.CreatePrimaryStackArea(),
				Array.Empty<IRectangle<int>>()
			},
			// Fill primary
			{
				new Rectangle<int>(0, 0, 100, 100),
				SliceLayouts.CreatePrimaryStackArea(),
				new[] { new Rectangle<int>(0, 0, 100, 100) }
			},
			// Fill overflow
			{
				new Rectangle<int>(0, 0, 100, 100),
				SliceLayouts.CreatePrimaryStackArea(),
				new[]
				{
					new Rectangle<int>(0, 0, 50, 100),
					new Rectangle<int>(50, 0, 50, 50),
					new Rectangle<int>(50, 50, 50, 50),
				}
			},
		};

	public const int third = 100 / 3;

	public static TheoryData<IRectangle<int>, ParentArea, IRectangle<int>[]> DoLayout_MultiColumn =>
		new()
		{
			// Empty
			{
				new Rectangle<int>(0, 0, 100, 100),
				SliceLayouts.CreateMultiColumnArea([2, 1, 0]),
				Array.Empty<IRectangle<int>>()
			},
			// Single window
			{
				new Rectangle<int>(0, 0, 100, 100),
				SliceLayouts.CreateMultiColumnArea([2, 1, 0]),
				new[] { new Rectangle<int>(0, 0, 100, 100) }
			},
			// Fill primary
			{
				new Rectangle<int>(0, 0, 100, 100),
				SliceLayouts.CreateMultiColumnArea([2, 1, 0]),
				new[] { new Rectangle<int>(0, 0, 100, 50), new Rectangle<int>(0, 50, 100, 50) }
			},
			// Fill secondary
			{
				new Rectangle<int>(0, 0, 100, 100),
				SliceLayouts.CreateMultiColumnArea([2, 1, 0]),
				new[]
				{
					new Rectangle<int>(0, 0, 50, 50),
					new Rectangle<int>(0, 50, 50, 50),
					new Rectangle<int>(50, 0, 50, 100),
				}
			},
			// Fill overflow

			{
				new Rectangle<int>(0, 0, 100, 100),
				SliceLayouts.CreateMultiColumnArea([2, 1, 0]),
				new[]
				{
					new Rectangle<int>(0, 0, third, 50),
					new Rectangle<int>(0, 50, third, 50),
					new Rectangle<int>(third, 0, third, 100),
					new Rectangle<int>(2 * third, 0, third, third),
					new Rectangle<int>(2 * third, third, third, third),
					new Rectangle<int>(2 * third, 2 * third, third, third),
				}
			},
		};

	public static TheoryData<IRectangle<int>, ParentArea, IRectangle<int>[]> DoLayout_SecondaryPrimary =>
		new()
		{
			// Empty
			{
				new Rectangle<int>(0, 0, 100, 100),
				SliceLayouts.CreateSecondaryPrimaryArea(1, 2),
				Array.Empty<IRectangle<int>>()
			},
			// Fill primary
			{
				new Rectangle<int>(0, 0, 100, 100),
				SliceLayouts.CreateSecondaryPrimaryArea(1, 2),
				new[] { new Rectangle<int>(0, 0, 100, 100) }
			},
			// Fill secondary
			{
				new Rectangle<int>(0, 0, 100, 100),
				SliceLayouts.CreateSecondaryPrimaryArea(1, 2),
				new[]
				{
					new Rectangle<int>(38, 0, 62, 100),
					new Rectangle<int>(0, 0, 38, 50),
					new Rectangle<int>(0, 50, 38, 50),
				}
			},
			// Fill overflow
			{
				new Rectangle<int>(0, 0, 100, 100),
				SliceLayouts.CreateSecondaryPrimaryArea(1, 2),
				new[]
				{
					new Rectangle<int>(25, 0, 50, 100),
					new Rectangle<int>(0, 0, 25, 50),
					new Rectangle<int>(0, 50, 25, 50),
					new Rectangle<int>(75, 0, 25, 33),
					new Rectangle<int>(75, 33, 25, 33),
					new Rectangle<int>(75, 66, 25, 33),
				}
			},
		};

	public static TheoryData<IRectangle<int>, ParentArea, IRectangle<int>[]> DoLayout_OverflowColumn =>
		new()
		{
			// Empty
			{
				new Rectangle<int>(0, 0, 100, 100),
				SampleSliceLayouts.CreateOverflowColumnLayout(),
				Array.Empty<IRectangle<int>>()
			},
			// Single window
			{
				new Rectangle<int>(0, 0, 100, 100),
				SampleSliceLayouts.CreateOverflowColumnLayout(),
				new[] { new Rectangle<int>(0, 0, 100, 100) }
			},
			// Fill overflow
			{
				new Rectangle<int>(0, 0, 100, 100),
				SampleSliceLayouts.CreateOverflowColumnLayout(),
				new[]
				{
					new Rectangle<int>(0, 0, 100, 25),
					new Rectangle<int>(0, 25, 100, 25),
					new Rectangle<int>(0, 50, 100, 25),
					new Rectangle<int>(0, 75, 100, 25),
				}
			},
		};

	public static TheoryData<IRectangle<int>, ParentArea, IRectangle<int>[]> DoLayout_OverflowRow =>
		new()
		{
			// Empty
			{
				new Rectangle<int>(0, 0, 100, 100),
				SampleSliceLayouts.CreateOverflowRowLayout(),
				Array.Empty<IRectangle<int>>()
			},
			// Single window
			{
				new Rectangle<int>(0, 0, 100, 100),
				SampleSliceLayouts.CreateOverflowRowLayout(),
				new[] { new Rectangle<int>(0, 0, 100, 100) }
			},
			// Fill overflow
			{
				new Rectangle<int>(0, 0, 100, 100),
				SampleSliceLayouts.CreateOverflowRowLayout(),
				new[]
				{
					new Rectangle<int>(0, 0, 25, 100),
					new Rectangle<int>(25, 0, 25, 100),
					new Rectangle<int>(50, 0, 25, 100),
					new Rectangle<int>(75, 0, 25, 100),
				}
			},
		};

	public static TheoryData<IRectangle<int>, ParentArea, IRectangle<int>[]> DoLayout_Nested =>
		new()
		{
			// Empty
			{
				new Rectangle<int>(0, 0, 100, 100),
				SampleSliceLayouts.CreateNestedLayout(),
				Array.Empty<IRectangle<int>>()
			},
			// Single window
			{
				new Rectangle<int>(0, 0, 100, 100),
				SampleSliceLayouts.CreateNestedLayout(),
				new[] { new Rectangle<int>(0, 0, 100, 100) }
			},
			// Fill primary
			{
				new Rectangle<int>(0, 0, 100, 100),
				SampleSliceLayouts.CreateNestedLayout(),
				new[] { new Rectangle<int>(0, 0, 100, 50), new Rectangle<int>(0, 50, 100, 50) }
			},
			//Fill secondary
			{
				new Rectangle<int>(0, 0, 100, 100),
				SampleSliceLayouts.CreateNestedLayout(),
				new[]
				{
					new Rectangle<int>(0, 0, 50, 50),
					new Rectangle<int>(0, 50, 50, 50),
					new Rectangle<int>(50, 0, 50, 50),
					new Rectangle<int>(50, 50, 50, 50),
				}
			},
			// Fill overflow
			{
				new Rectangle<int>(0, 0, 100, 100),
				SampleSliceLayouts.CreateNestedLayout(),
				new[]
				{
					new Rectangle<int>(0, 0, 50, 50),
					new Rectangle<int>(0, 50, 50, 50),
					new Rectangle<int>(50, 0, 50, 25),
					new Rectangle<int>(50, 25, 50, 25),
					new Rectangle<int>(50, 50, 50, 16),
					new Rectangle<int>(50, 66, 50, 16),
					new Rectangle<int>(50, 82, 50, 16),
				}
			},
		};

	public static TheoryData<IRectangle<int>, ParentArea, IRectangle<int>[]> DoLayout_Column =>
		new()
		{
			// Empty
			{ new Rectangle<int>(0, 0, 100, 100), SliceLayouts.CreateColumnArea(), Array.Empty<IRectangle<int>>() },
			// Single window
			{
				new Rectangle<int>(0, 0, 100, 100),
				SliceLayouts.CreateColumnArea(),
				new[] { new Rectangle<int>(0, 0, 100, 100) }
			},
			// Three windows
			{
				new Rectangle<int>(0, 0, 100, 100),
				SliceLayouts.CreateColumnArea(),
				new[]
				{
					new Rectangle<int>(0, 0, 100, 33),
					new Rectangle<int>(0, 33, 100, 33),
					new Rectangle<int>(0, 66, 100, 33),
				}
			},
		};

	public static TheoryData<IRectangle<int>, ParentArea, IRectangle<int>[]> DoLayout_Row =>
		new()
		{
			// Empty
			{ new Rectangle<int>(0, 0, 100, 100), SliceLayouts.CreateRowArea(), Array.Empty<IRectangle<int>>() },
			// Single window
			{
				new Rectangle<int>(0, 0, 100, 100),
				SliceLayouts.CreateRowArea(),
				new[] { new Rectangle<int>(0, 0, 100, 100) }
			},
			// Three windows
			{
				new Rectangle<int>(0, 0, 100, 100),
				SliceLayouts.CreateRowArea(),
				new[]
				{
					new Rectangle<int>(0, 0, 33, 100),
					new Rectangle<int>(33, 0, 33, 100),
					new Rectangle<int>(66, 0, 33, 100),
				}
			},
		};

	[Theory]
	[MemberAutoSubstituteData(nameof(DoLayout_PrimaryStack))]
	[MemberAutoSubstituteData(nameof(DoLayout_MultiColumn))]
	[MemberAutoSubstituteData(nameof(DoLayout_SecondaryPrimary))]
	[MemberAutoSubstituteData(nameof(DoLayout_OverflowColumn))]
	[MemberAutoSubstituteData(nameof(DoLayout_OverflowRow))]
	[MemberAutoSubstituteData(nameof(DoLayout_Nested))]
	[MemberAutoSubstituteData(nameof(DoLayout_Column))]
	[MemberAutoSubstituteData(nameof(DoLayout_Row))]
	internal void DoLayout(
		IRectangle<int> rectangle,
		ParentArea area,
		IRectangle<int>[] expectedRectangles,
		IContext ctx,
		ISliceLayoutPlugin plugin
	)
	{
		// Given
		int minimizedWindowCount = 2;
		int windowCount = expectedRectangles.Length;

		IWindowState[] expectedWindowStates = new IWindowState[windowCount + minimizedWindowCount];
		for (int i = 0; i < windowCount; i++)
		{
			expectedWindowStates[i] = new WindowState()
			{
				Rectangle = expectedRectangles[i],
				Window = Substitute.For<IWindow>(),
				WindowSize = WindowSize.Normal,
			};
		}
		for (int i = 0; i < minimizedWindowCount; i++)
		{
			expectedWindowStates[windowCount + i] = new WindowState()
			{
				Rectangle = new Rectangle<int>(),
				Window = Substitute.For<IWindow>(),
				WindowSize = WindowSize.Minimized,
			};
		}

		ILayoutEngine sliceLayoutEngine = new SliceLayoutEngine(ctx, plugin, identity, area);

		// When
		for (int i = 0; i < windowCount; i++)
		{
			sliceLayoutEngine = sliceLayoutEngine.AddWindow(expectedWindowStates[i].Window);
		}
		for (int i = 0; i < minimizedWindowCount; i++)
		{
			sliceLayoutEngine = sliceLayoutEngine.MinimizeWindowStart(expectedWindowStates[windowCount + i].Window);
		}
		IWindowState[] windowStates = [.. sliceLayoutEngine.DoLayout(rectangle, Substitute.For<IMonitor>())];

		// Then
		Assert.Equal(windowCount + minimizedWindowCount, windowStates.Length);
		expectedWindowStates.Should().BeEquivalentTo(windowStates);
	}
}
