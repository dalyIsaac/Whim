using FluentAssertions;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.SliceLayout.Tests;

public class DoLayoutTests
{
	private static readonly LayoutEngineIdentity identity = new();

	public static IEnumerable<object[]> DoLayout_PrimaryStack()
	{
		// Empty
		yield return new object[]
		{
			new Rectangle<int>(0, 0, 100, 100),
			SliceLayouts.CreatePrimaryStackArea(),
			Array.Empty<IWindowState>(),
		};

		// Fill primary
		yield return new object[]
		{
			new Rectangle<int>(0, 0, 100, 100),
			SliceLayouts.CreatePrimaryStackArea(),
			new[] { new Rectangle<int>(0, 0, 100, 100), },
		};

		// Fill overflow
		yield return new object[]
		{
			new Rectangle<int>(0, 0, 100, 100),
			SliceLayouts.CreatePrimaryStackArea(),
			new[]
			{
				new Rectangle<int>(0, 0, 50, 100),
				new Rectangle<int>(50, 0, 50, 50),
				new Rectangle<int>(50, 50, 50, 50),
			},
		};
	}

	public static IEnumerable<object[]> DoLayout_MultiColumn()
	{
		// Empty
		yield return new object[]
		{
			new Rectangle<int>(0, 0, 100, 100),
			SliceLayouts.CreateMultiColumnArea(new uint[] { 2, 1, 0 }),
			Array.Empty<IWindowState>(),
		};

		// Single window
		yield return new object[]
		{
			new Rectangle<int>(0, 0, 100, 100),
			SliceLayouts.CreateMultiColumnArea(new uint[] { 2, 1, 0 }),
			new[] { new Rectangle<int>(0, 0, 100, 100), },
		};

		// Fill primary
		yield return new object[]
		{
			new Rectangle<int>(0, 0, 100, 100),
			SliceLayouts.CreateMultiColumnArea(new uint[] { 2, 1, 0 }),
			new[] { new Rectangle<int>(0, 0, 100, 50), new Rectangle<int>(0, 50, 100, 50), },
		};

		// Fill secondary
		yield return new object[]
		{
			new Rectangle<int>(0, 0, 100, 100),
			SliceLayouts.CreateMultiColumnArea(new uint[] { 2, 1, 0 }),
			new[]
			{
				new Rectangle<int>(0, 0, 50, 50),
				new Rectangle<int>(0, 50, 50, 50),
				new Rectangle<int>(50, 0, 50, 100),
			},
		};

		// Fill overflow
		int third = 100 / 3;
		yield return new object[]
		{
			new Rectangle<int>(0, 0, 100, 100),
			SliceLayouts.CreateMultiColumnArea(new uint[] { 2, 1, 0 }),
			new[]
			{
				new Rectangle<int>(0, 0, third, 50),
				new Rectangle<int>(0, 50, third, 50),
				new Rectangle<int>(third, 0, third, 100),
				new Rectangle<int>(2 * third, 0, third, third),
				new Rectangle<int>(2 * third, third, third, third),
				new Rectangle<int>(2 * third, 2 * third, third, third),
			},
		};
	}

	public static IEnumerable<object[]> DoLayout_SecondaryPrimary()
	{
		// Empty
		yield return new object[]
		{
			new Rectangle<int>(0, 0, 100, 100),
			SliceLayouts.CreateSecondaryPrimaryStackArea(1, 2),
			Array.Empty<IWindowState>(),
		};

		// Fill primary
		yield return new object[]
		{
			new Rectangle<int>(0, 0, 100, 100),
			SliceLayouts.CreateSecondaryPrimaryStackArea(1, 2),
			new[] { new Rectangle<int>(0, 0, 100, 100), },
		};

		// Fill secondary
		yield return new object[]
		{
			new Rectangle<int>(0, 0, 100, 100),
			SliceLayouts.CreateSecondaryPrimaryStackArea(1, 2),
			new[]
			{
				new Rectangle<int>(38, 0, 62, 100),
				new Rectangle<int>(0, 0, 38, 50),
				new Rectangle<int>(0, 50, 38, 50),
			},
		};

		// Fill overflow
		yield return new object[]
		{
			new Rectangle<int>(0, 0, 100, 100),
			SliceLayouts.CreateSecondaryPrimaryStackArea(1, 2),
			new[]
			{
				new Rectangle<int>(25, 0, 50, 100),
				new Rectangle<int>(0, 0, 25, 50),
				new Rectangle<int>(0, 50, 25, 50),
				new Rectangle<int>(75, 0, 25, 33),
				new Rectangle<int>(75, 33, 25, 33),
				new Rectangle<int>(75, 66, 25, 33),
			},
		};
	}

	public static IEnumerable<object[]> DoLayout_OverflowColumn()
	{
		// Empty
		yield return new object[]
		{
			new Rectangle<int>(0, 0, 100, 100),
			SampleSliceLayouts.CreateOverflowColumnLayout(),
			Array.Empty<IWindowState>(),
		};

		// Single window
		yield return new object[]
		{
			new Rectangle<int>(0, 0, 100, 100),
			SampleSliceLayouts.CreateOverflowColumnLayout(),
			new[] { new Rectangle<int>(0, 0, 100, 100), },
		};

		// Fill overflow
		yield return new object[]
		{
			new Rectangle<int>(0, 0, 100, 100),
			SampleSliceLayouts.CreateOverflowColumnLayout(),
			new[]
			{
				new Rectangle<int>(0, 0, 100, 25),
				new Rectangle<int>(0, 25, 100, 25),
				new Rectangle<int>(0, 50, 100, 25),
				new Rectangle<int>(0, 75, 100, 25),
			},
		};
	}

	public static IEnumerable<object[]> DoLayout_Nested()
	{
		// Empty
		yield return new object[]
		{
			new Rectangle<int>(0, 0, 100, 100),
			SampleSliceLayouts.CreateNestedLayout(),
			Array.Empty<IWindowState>(),
		};

		// Single window
		yield return new object[]
		{
			new Rectangle<int>(0, 0, 100, 100),
			SampleSliceLayouts.CreateNestedLayout(),
			new[] { new Rectangle<int>(0, 0, 100, 100), },
		};

		// Fill primary
		yield return new object[]
		{
			new Rectangle<int>(0, 0, 100, 100),
			SampleSliceLayouts.CreateNestedLayout(),
			new[] { new Rectangle<int>(0, 0, 100, 50), new Rectangle<int>(0, 50, 100, 50), },
		};

		//Fill secondary
		yield return new object[]
		{
			new Rectangle<int>(0, 0, 100, 100),
			SampleSliceLayouts.CreateNestedLayout(),
			new[]
			{
				new Rectangle<int>(0, 0, 50, 50),
				new Rectangle<int>(0, 50, 50, 50),
				new Rectangle<int>(50, 0, 50, 50),
				new Rectangle<int>(50, 50, 50, 50),
			},
		};

		// Fill overflow
		yield return new object[]
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
			},
		};
	}

	[Theory]
	[MemberAutoSubstituteData(nameof(DoLayout_PrimaryStack))]
	[MemberAutoSubstituteData(nameof(DoLayout_MultiColumn))]
	[MemberAutoSubstituteData(nameof(DoLayout_SecondaryPrimary))]
	[MemberAutoSubstituteData(nameof(DoLayout_OverflowColumn))]
	[MemberAutoSubstituteData(nameof(DoLayout_Nested))]
	internal void DoLayout(
		IRectangle<int> rectangle,
		ParentArea area,
		IRectangle<int>[] expectedRectangles,
		IContext ctx,
		ISliceLayoutPlugin plugin
	)
	{
		// Given
		int windowCount = expectedRectangles.Length;
		IWindow[] windows = Enumerable.Range(0, windowCount).Select(i => Substitute.For<IWindow>()).ToArray();

		IWindowState[] expectedWindowStates = new IWindowState[windowCount];
		for (int i = 0; i < windowCount; i++)
		{
			expectedWindowStates[i] = new WindowState()
			{
				Rectangle = expectedRectangles[i],
				Window = windows[i],
				WindowSize = WindowSize.Normal
			};
		}

		ILayoutEngine sliceLayoutEngine = new SliceLayoutEngine(ctx, plugin, identity, area);

		// When
		for (int i = 0; i < windowCount; i++)
		{
			sliceLayoutEngine = sliceLayoutEngine.AddWindow(windows[i]);
		}
		IWindowState[] windowStates = sliceLayoutEngine.DoLayout(rectangle, Substitute.For<IMonitor>()).ToArray();

		// Then
		Assert.Equal(windowCount, windowStates.Length);
		expectedWindowStates.Should().BeEquivalentTo(windowStates);
	}
}
