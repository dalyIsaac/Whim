using FluentAssertions;
using Moq;
using Xunit;

namespace Whim.Gaps.Tests;

public class ImmutableGapsLayoutEngineTests
{
	public static IEnumerable<object[]> DoLayout_Data()
	{
		yield return new object[]
		{
			new GapsConfig() { OuterGap = 10, InnerGap = 5 },
			1,
			100,
			new IWindowState[]
			{
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
					Location = new Location<int>()
					{
						X = 10 + 5,
						Y = 10 + 5,
						Width = 1920 - (10 * 2) - (5 * 2),
						Height = 1080 - (10 * 2) - (5 * 2)
					},
					WindowSize = WindowSize.Normal
				}
			}
		};

		yield return new object[]
		{
			new GapsConfig() { OuterGap = 10, InnerGap = 5 },
			2,
			100,
			new IWindowState[]
			{
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
					Location = new Location<int>()
					{
						X = 10 + 5,
						Y = 10 + 5,
						Width = 960 - 10 - (5 * 2),
						Height = 1080 - (10 * 2) - (5 * 2)
					},
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
					Location = new Location<int>()
					{
						X = 960 + 5,
						Y = 10 + 5,
						Width = 960 - 10 - (5 * 2),
						Height = 1080 - (10 * 2) - (5 * 2)
					},
					WindowSize = WindowSize.Normal
				}
			}
		};

		yield return new object[]
		{
			new GapsConfig { OuterGap = 10, InnerGap = 5 },
			1,
			150,
			new IWindowState[]
			{
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
					Location = new Location<int>()
					{
						X = 15 + 7,
						Y = 15 + 7,
						Width = 1920 - (15 * 2) - (7 * 2),
						Height = 1080 - (15 * 2) - (7 * 2)
					},
					WindowSize = WindowSize.Normal
				}
			}
		};
	}

	[Theory]
	[MemberData(nameof(DoLayout_Data))]
	public void DoLayout(GapsConfig gapsConfig, int windowsCount, int scale, IWindowState[] expectedWindowStates)
	{
		// Given
		IImmutableLayoutEngine innerLayoutEngine = new ImmutableColumnLayoutEngine();

		for (int i = 0; i < windowsCount; i++)
		{
			innerLayoutEngine = innerLayoutEngine.Add(new Mock<IWindow>().Object);
		}

		ImmutableGapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine);

		Location<int> location =
			new()
			{
				X = 0,
				Y = 0,
				Width = 1920,
				Height = 1080
			};

		Mock<IMonitor> monitor = new();
		monitor.Setup(m => m.ScaleFactor).Returns(scale);

		// When
		IWindowState[] windowStates = gapsLayoutEngine.DoLayout(location, monitor.Object).ToArray();

		// Then
		windowStates.Should().BeEquivalentTo(expectedWindowStates);
	}
}
