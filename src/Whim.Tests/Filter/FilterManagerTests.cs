namespace Whim.Tests;

public class FilterManagerTests
{
	[Theory, AutoSubstituteData]
	public void AddWindowClassFilter(IWindow window)
	{
		// Given
		FilterManager filterManager = new();
		filterManager.AddWindowClassFilter("Test");

		window.WindowClass.Returns("Test");

		// Then
		Assert.True(filterManager.ShouldBeIgnored(window));
	}

	[Theory, AutoSubstituteData]
	public void AddProcessFileNameFilter(IWindow window)
	{
		// Given
		FilterManager filterManager = new();
		filterManager.AddProcessFileNameFilter("Test.exe");

		window.ProcessFileName.Returns("Test.exe");

		// Then
		Assert.True(filterManager.ShouldBeIgnored(window));
	}

	[Theory, AutoSubstituteData]
	public void AddTitleFilter(IWindow window)
	{
		// Given
		FilterManager filterManager = new();
		filterManager.AddTitleFilter("Test");

		window.Title.Returns("Test");

		// Then
		Assert.True(filterManager.ShouldBeIgnored(window));
	}

	[Theory, AutoSubstituteData]
	public void AddTitleMatchFilter(IWindow window)
	{
		// Given
		FilterManager filterManager = new();
		filterManager.AddTitleMatchFilter("Test");

		window.Title.Returns("Test");

		// Then
		Assert.True(filterManager.ShouldBeIgnored(window));
	}

	[Theory, AutoSubstituteData]
	public void Clear(IWindow window)
	{
		// Given
		FilterManager filterManager = new();
		DefaultFilteredWindows.LoadWindowsIgnoredByWhim(filterManager);

		filterManager.AddWindowClassFilter("Test");
		filterManager.AddProcessFileNameFilter("Test");
		filterManager.AddTitleFilter("Test");
		filterManager.AddTitleMatchFilter("Test");

		window.WindowClass.Returns("Test");
		window.ProcessFileName.Returns("Test");
		window.Title.Returns("Test");

		// When the filter manager is cleared
		filterManager.Clear();

		// Then the window should be ignored, but not the search UI window
		Assert.False(filterManager.ShouldBeIgnored(window));
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
