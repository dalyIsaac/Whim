using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class FilterManagerTests
{
	[Theory, AutoSubstituteData]
	public void IgnoreWindowClass(IWindow window)
	{
		FilterManager filterManager = new();
		filterManager.IgnoreWindowClass("Test");

		window.WindowClass.Returns("Test");

		Assert.True(filterManager.ShouldBeIgnored(window));
	}

	[Theory, AutoSubstituteData]
	public void IgnoreProcessName(IWindow window)
	{
		FilterManager filterManager = new();
		filterManager.IgnoreProcessName("Test");

		window.ProcessName.Returns("Test");

		Assert.True(filterManager.ShouldBeIgnored(window));
	}

	[Theory, AutoSubstituteData]
	public void IgnoreTitle(IWindow window)
	{
		FilterManager filterManager = new();
		filterManager.IgnoreTitle("Test");

		window.Title.Returns("Test");

		Assert.True(filterManager.ShouldBeIgnored(window));
	}

	[Theory, AutoSubstituteData]
	public void IgnoreTitleMatch(IWindow window)
	{
		FilterManager filterManager = new();
		filterManager.IgnoreTitleMatch("Test");

		window.Title.Returns("Test");

		Assert.True(filterManager.ShouldBeIgnored(window));
	}

	[Theory, AutoSubstituteData]
	public void ClearKeepDefaults(IWindow window, IWindow searchUIWindow)
	{
		FilterManager filterManager = new();
		filterManager.IgnoreWindowClass("Test");

		window.WindowClass.Returns("Test");

		filterManager.Clear(clearDefaults: true);

		Assert.False(filterManager.ShouldBeIgnored(window));

		searchUIWindow.ProcessName.Returns("SearchUI.exe");

		Assert.False(filterManager.ShouldBeIgnored(searchUIWindow));
	}

	[Theory, AutoSubstituteData]
	public void ClearAll(IWindow window, IWindow searchUIWindow)
	{
		FilterManager filterManager = new();
		filterManager.IgnoreWindowClass("Test");

		window.WindowClass.Returns("Test");

		filterManager.Clear(clearDefaults: false);

		Assert.False(filterManager.ShouldBeIgnored(window));

		searchUIWindow.ProcessName.Returns("SearchUI.exe");

		Assert.True(filterManager.ShouldBeIgnored(searchUIWindow));
	}

	[Theory, AutoSubstituteData]
	public void CustomFilter(IWindow window)
	{
		FilterManager filterManager = new();
		filterManager.Add(w => w.WindowClass == "Test");

		window.WindowClass.Returns("Test");

		Assert.True(filterManager.ShouldBeIgnored(window));
	}
}
