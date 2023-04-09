using Moq;
using Xunit;

namespace Whim.Tests;

public class RouterManagerTests
{
	private static Mock<IWindow> CreateWindowMock(
		string? className = null,
		string? processName = null,
		string? title = null
	)
	{
		Mock<IWindow>? windowMock = new();
		windowMock.Setup(w => w.WindowClass).Returns(className ?? "");
		windowMock.Setup(w => w.ProcessName).Returns(processName ?? "");
		windowMock.Setup(w => w.Title).Returns(title ?? "");
		return windowMock;
	}

	private static Mock<IContext> CreateContextMock(string? workspaceName = null, IWorkspace? workspace = null)
	{
		Mock<IContext>? contextMock = new();

		if (workspaceName != null)
		{
			Mock<IWorkspace> workspaceMock = new();
			workspaceMock.Setup(w => w.Name).Returns(workspaceName);

			contextMock.Setup(c => c.WorkspaceManager.TryGet(workspaceName)).Returns(workspaceMock.Object);
		}

		if (workspace != null)
		{
			contextMock.Setup(c => c.WorkspaceManager.TryGet(workspace.Name)).Returns(workspace);
		}

		return contextMock;
	}

	[Fact]
	public void AddWindowClassRouteString()
	{
		RouterManager routerManager = new(CreateContextMock("Test").Object);
		routerManager.AddWindowClassRoute("Test", "Test");

		Mock<IWindow> window = CreateWindowMock(className: "Test");

		Assert.Equal("Test", routerManager.RouteWindow(window.Object)?.Name);
	}

	[Fact]
	public void AddWindowClassRoute()
	{
		Mock<IWorkspace> workspaceMock = new();
		workspaceMock.Setup(w => w.Name).Returns("Test");

		RouterManager routerManager = new(CreateContextMock(workspace: workspaceMock.Object).Object);
		routerManager.AddWindowClassRoute("Test", workspaceMock.Object);

		Mock<IWindow> window = CreateWindowMock(className: "Test");

		Assert.Equal("Test", routerManager.RouteWindow(window.Object)?.Name);
	}

	[Fact]
	public void AddProcessNameRouteString()
	{
		RouterManager routerManager = new(CreateContextMock("Test").Object);
		routerManager.AddProcessNameRoute("Test", "Test");

		Mock<IWindow> window = CreateWindowMock(processName: "Test");

		Assert.Equal("Test", routerManager.RouteWindow(window.Object)?.Name);
	}

	[Fact]
	public void AddProcessNameRoute()
	{
		Mock<IWorkspace> workspaceMock = new();
		workspaceMock.Setup(w => w.Name).Returns("Test");

		RouterManager routerManager = new(CreateContextMock(workspace: workspaceMock.Object).Object);
		routerManager.AddProcessNameRoute("Test", workspaceMock.Object);

		Mock<IWindow> window = CreateWindowMock(processName: "Test");

		Assert.Equal("Test", routerManager.RouteWindow(window.Object)?.Name);
	}

	[Fact]
	public void AddTitleRouteString()
	{
		RouterManager routerManager = new(CreateContextMock("Test").Object);
		routerManager.AddTitleRoute("Test", "Test");

		Mock<IWindow> window = CreateWindowMock(title: "Test");

		Assert.Equal("Test", routerManager.RouteWindow(window.Object)?.Name);
	}

	[Fact]
	public void AddTitleRoute()
	{
		Mock<IWorkspace> workspaceMock = new();
		workspaceMock.Setup(w => w.Name).Returns("Test");

		RouterManager routerManager = new(CreateContextMock(workspace: workspaceMock.Object).Object);
		routerManager.AddTitleRoute("Test", workspaceMock.Object);

		Mock<IWindow> window = CreateWindowMock(title: "Test");

		Assert.Equal("Test", routerManager.RouteWindow(window.Object)?.Name);
	}

	[Fact]
	public void AddTitleMatchRouteString()
	{
		RouterManager routerManager = new(CreateContextMock("Test").Object);
		routerManager.AddTitleMatchRoute("Test", "Test");

		Mock<IWindow> window = CreateWindowMock(title: "Test");

		Assert.Equal("Test", routerManager.RouteWindow(window.Object)?.Name);
	}

	[Fact]
	public void AddTitleMatchRoute()
	{
		Mock<IWorkspace> workspaceMock = new();
		workspaceMock.Setup(w => w.Name).Returns("Test");

		RouterManager routerManager = new(CreateContextMock(workspace: workspaceMock.Object).Object);
		routerManager.AddTitleMatchRoute("Test", workspaceMock.Object);

		Mock<IWindow> window = CreateWindowMock(title: "Test");

		Assert.Equal("Test", routerManager.RouteWindow(window.Object)?.Name);
	}

	[Fact]
	public void Clear()
	{
		RouterManager routerManager = new(CreateContextMock("Test").Object);
		routerManager.AddWindowClassRoute("Test", "Test");

		routerManager.Clear();

		Mock<IWindow> window = CreateWindowMock(className: "Test");

		Assert.Null(routerManager.RouteWindow(window.Object));
	}

	[Fact]
	public void CustomRouter()
	{
		Mock<IWorkspace> workspaceMock = new();
		workspaceMock.Setup(w => w.Name).Returns("Test");

		RouterManager routerManager = new(CreateContextMock(workspace: workspaceMock.Object).Object);

		routerManager.Add((w) => w.WindowClass == "Not Test" ? new Mock<IWorkspace>().Object : null);
		routerManager.Add((w) => w.WindowClass == "Test" ? workspaceMock.Object : null);

		Mock<IWindow> window = CreateWindowMock(className: "Test");

		Assert.Equal("Test", routerManager.RouteWindow(window.Object)?.Name);
	}
}
