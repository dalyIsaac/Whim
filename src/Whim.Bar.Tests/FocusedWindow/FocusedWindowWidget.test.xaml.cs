using Moq;
using Xunit;

namespace Whim.Bar.Tests;

public class FocusedWindowWidgetTests
{
	[Fact]
	public void GetTitle()
	{
		// Given
		Mock<IWindow> window = new();
		window.SetupGet(w => w.Title).Returns("FocusedWindowWidget.test.xaml.cs - Whim - Visual Studio Code");

		// When
		string title = FocusedWindowWidget.GetTitle(window.Object);

		// Then
		Assert.Equal("FocusedWindowWidget.test.xaml.cs - Whim - Visual Studio Code", title);
	}

	[Fact]
	public void GetShortTitle()
	{
		// Given
		Mock<IWindow> window = new();
		window.SetupGet(w => w.Title).Returns("FocusedWindowWidget.test.xaml.cs - Whim - Visual Studio Code");

		// When
		string title = FocusedWindowWidget.GetShortTitle(window.Object);

		// Then
		Assert.Equal("FocusedWindowWidget.test.xaml.cs", title);
	}

	[Fact]
	public void GetProcessName()
	{
		// Given
		Mock<IWindow> window = new();
		window.SetupGet(w => w.ProcessName).Returns("code");

		// When
		string processName = FocusedWindowWidget.GetProcessName(window.Object);

		// Then
		Assert.Equal("code", processName);
	}
}
