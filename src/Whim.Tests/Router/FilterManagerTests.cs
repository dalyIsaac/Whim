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
	public void Clear(IWindow window)
	{
		// Given
		FilterManager filterManager = new();
		FilteredWindows.LoadWindowsIgnoredByWhim(filterManager);
		filterManager.IgnoreWindowClass("Test");

		window.WindowClass.Returns("Test");

		// When the filter manager is cleared
		filterManager.Clear();

		// Then the window should be ignored
		Assert.False(filterManager.ShouldBeIgnored(window));
	}ShouldIgnore

	[Theory, AutoSubstituteData]
	public void CustomFilter(IWindow window)
	{
		// Given
		FilterManager filterManager = new();
		filterManager.Add(w => w.WindowClass == "Test");

		window.WindowClass.Returns("Test");

		Assert.True(filterManager.CheckWindow(window));
	}
}
ShouldIgnoreShouldIgnoreShouldIgnore
