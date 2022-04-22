using Moq;
using Xunit;

namespace Whim.Tests;

public class FilterManagerTests
{
	private static Mock<IWindow> CreateWindowMock(string? className = null,
											   string? processName = null,
											   string? title = null)
	{
		Mock<IWindow>? windowMock = new();
		windowMock.Setup(w => w.WindowClass).Returns(className ?? "");
		windowMock.Setup(w => w.ProcessName).Returns(processName ?? "");
		windowMock.Setup(w => w.Title).Returns(title ?? "");
		return windowMock;
	}

	[Fact]
	public void IgnoreWindowClass()
	{
		FilterManager filterManager = new();
		filterManager.IgnoreWindowClass("Test");

		Mock<IWindow> window = CreateWindowMock(className: "Test");

		Assert.True(filterManager.ShouldBeIgnored(window.Object));
	}

	[Fact]
	public void IgnoreProcessName()
	{
		FilterManager filterManager = new();
		filterManager.IgnoreProcessName("Test");

		Mock<IWindow> window = CreateWindowMock(processName: "Test");

		Assert.True(filterManager.ShouldBeIgnored(window.Object));
	}

	[Fact]
	public void IgnoreTitle()
	{
		FilterManager filterManager = new();
		filterManager.IgnoreTitle("Test");

		Mock<IWindow> window = CreateWindowMock(title: "Test");

		Assert.True(filterManager.ShouldBeIgnored(window.Object));
	}

	[Fact]
	public void IgnoreTitleMatch()
	{
		FilterManager filterManager = new();
		filterManager.IgnoreTitleMatch("Test");

		Mock<IWindow> window = CreateWindowMock(title: "Test");

		Assert.True(filterManager.ShouldBeIgnored(window.Object));
	}

	[Fact]
	public void ClearKeepDefaults()
	{
		FilterManager filterManager = new();
		filterManager.IgnoreWindowClass("Test");

		Mock<IWindow> window = CreateWindowMock(className: "Test");

		filterManager.Clear(clearDefaults: true);

		Assert.False(filterManager.ShouldBeIgnored(window.Object));

		Mock<IWindow> searchUIWindow = CreateWindowMock();
		searchUIWindow.Setup(w => w.ProcessName).Returns("SearchUI.exe");

		Assert.False(filterManager.ShouldBeIgnored(searchUIWindow.Object));
	}

	[Fact]
	public void ClearAll()
	{
		FilterManager filterManager = new();
		filterManager.IgnoreWindowClass("Test");

		Mock<IWindow> window = CreateWindowMock(className: "Test");

		filterManager.Clear(clearDefaults: false);

		Assert.False(filterManager.ShouldBeIgnored(window.Object));

		Mock<IWindow> searchUIWindow = CreateWindowMock();
		searchUIWindow.Setup(w => w.ProcessName).Returns("SearchUI.exe");

		Assert.True(filterManager.ShouldBeIgnored(searchUIWindow.Object));
	}

	[Fact]
	public void CustomFilter()
	{
		FilterManager filterManager = new();
		filterManager.Add(w => w.WindowClass == "Test");

		Mock<IWindow> window = CreateWindowMock(className: "Test");

		Assert.True(filterManager.ShouldBeIgnored(window.Object));
	}
}
