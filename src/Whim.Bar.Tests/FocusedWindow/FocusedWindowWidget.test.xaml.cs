using NSubstitute;
using Xunit;

namespace Whim.Bar.Tests;

public class FocusedWindowWidgetTests
{
	[Theory, AutoSubstituteData]
	public void GetTitle(IWindow window)
	{
		// Given
		window.Title.Returns("FocusedWindowWidget.test.xaml.cs - Whim - Visual Studio Code");

		// When
		string title = FocusedWindowWidget.GetTitle(window);

		// Then
		Assert.Equal("FocusedWindowWidget.test.xaml.cs - Whim - Visual Studio Code", title);
	}

	[Theory, AutoSubstituteData]
	public void GetShortTitle(IWindow window)
	{
		// Given
		window.Title.Returns("FocusedWindowWidget.test.xaml.cs - Whim - Visual Studio Code");

		// When
		string title = FocusedWindowWidget.GetShortTitle(window);

		// Then
		Assert.Equal("FocusedWindowWidget.test.xaml.cs", title);
	}

	[Theory, AutoSubstituteData]
	public void GetShortTitle_NoParts(IWindow window)
	{
		// Given
		window.Title.Returns("");

		// When
		string title = FocusedWindowWidget.GetShortTitle(window);

		// Then
		Assert.Equal("", title);
	}

	[Theory, AutoSubstituteData]
	public void GetProcessName(IWindow window)
	{
		// Given
		window.ProcessName.Returns("code");

		// When
		string processName = FocusedWindowWidget.GetProcessName(window);

		// Then
		Assert.Equal("code", processName);
	}
}
