using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class FilterManagerTests
{
	[Theory, AutoSubstituteData]
	public void IgnoreWindowClass(IWindow window)
	{
		// Given
		FilterManager filterManager = new();
		filterManager.IgnoreWindowClass("Test");

		window.WindowClass.Returns("Test");

		// Then
		Assert.True(filterManager.ShouldBeIgnored(window));
	}

	[Theory, AutoSubstituteData]
	public void IgnoreProcessName(IWindow window)
	{
		// Given
		FilterManager filterManager = new();
		filterManager.IgnoreProcessName("Test");

		window.ProcessName.Returns("Test");

		// Then
		Assert.True(filterManager.ShouldBeIgnored(window));
	}

	[Theory, AutoSubstituteData]
	public void IgnoreTitle(IWindow window)
	{
		// Given
		FilterManager filterManager = new();
		filterManager.IgnoreTitle("Test");

		window.Title.Returns("Test");

		// Then
		Assert.True(filterManager.ShouldBeIgnored(window));
	}

	[Theory, AutoSubstituteData]
	public void IgnoreTitleMatch(IWindow window)
	{
		// Given
		FilterManager filterManager = new();
		filterManager.IgnoreTitleMatch("Test");

		window.Title.Returns("Test");

		// Then
		Assert.True(filterManager.ShouldBeIgnored(window));
	}

	[Theory, AutoSubstituteData]
	public void ClearKeepDefaults(IWindow window, IWindow searchUIWindow)
	{
		// Given
		FilterManager filterManager = new();
		filterManager.IgnoreWindowClass("Test");

		window.WindowClass.Returns("Test");
		searchUIWindow.ProcessName.Returns("SearchUI.exe");

		// When the filter manager is cleared, and the defaults are cleared
		filterManager.Clear(clearDefaults: true);

		// Then neither window should be ignored
		Assert.False(filterManager.ShouldBeIgnored(window));
		Assert.False(filterManager.ShouldBeIgnored(searchUIWindow));
	}

	[Theory, AutoSubstituteData]
	public void ClearAll(IWindow window, IWindow searchUIWindow)
	{
		// Given
		FilterManager filterManager = new();
		filterManager.IgnoreWindowClass("Test");

		window.WindowClass.Returns("Test");
		searchUIWindow.ProcessName.Returns("SearchUI.exe");

		// When the filter manager is cleared, and the defaults are kept
		filterManager.Clear(clearDefaults: false);

		// Then the window should be ignored, but not the search UI window
		Assert.False(filterManager.ShouldBeIgnored(window));
		Assert.True(filterManager.ShouldBeIgnored(searchUIWindow));
	}

	[Theory, AutoSubstituteData]
	public void CustomFilter(IWindow window)
	{
		// Given
		FilterManager filterManager = new();
		filterManager.Add(w => w.WindowClass == "Test");

		window.WindowClass.Returns("Test");

		Assert.True(filterManager.ShouldBeIgnored(window));
	}
}
