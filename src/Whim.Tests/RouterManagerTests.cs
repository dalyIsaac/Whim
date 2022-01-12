using Moq;
using Xunit;

namespace Whim.Tests;

public class RouterManagerTests
{
	private static Mock<IWindow> CreateWindowMock(string? className = null,
											   string? processName = null,
											   string? title = null)
	{
		Mock<IWindow>? windowMock = new();
		windowMock.Setup(w => w.Class).Returns(className ?? "");
		windowMock.Setup(w => w.ProcessName).Returns(processName ?? "");
		windowMock.Setup(w => w.Title).Returns(title ?? "");
		return windowMock;
	}

	private static Mock<IConfigContext> CreateConfigContextMock(string? workspaceName = null,
															 IWorkspace? workspace = null)
	{
		Mock<IConfigContext>? configContextMock = new();

		if (workspaceName != null)
		{
			Mock<IWorkspace> workspaceMock = new();
			workspaceMock.Setup(w => w.Name).Returns(workspaceName);

			configContextMock.Setup(c => c.WorkspaceManager.TryGet(workspaceName))
				.Returns(workspaceMock.Object);
		}

		if (workspace != null)
		{
			configContextMock.Setup(c => c.WorkspaceManager.TryGet(workspace.Name))
				.Returns(workspace);
		}

		return configContextMock;
	}

	[Fact]
	public void AddWindowClassRouteString()
	{
		Logger.Initialize(new LoggerConfig());

		RouterManager routerManager = new(CreateConfigContextMock("Test").Object);
		routerManager.AddWindowClassRoute("Test", "Test");

		Mock<IWindow> window = CreateWindowMock(className: "Test");

		Assert.Equal("Test", routerManager.RouteWindow(window.Object)?.Name);
	}

	[Fact]
	public void AddWindowClassRoute()
	{
		Logger.Initialize(new LoggerConfig());

		Mock<IWorkspace> workspaceMock = new();
		workspaceMock.Setup(w => w.Name).Returns("Test");

		RouterManager routerManager = new(CreateConfigContextMock(workspace: workspaceMock.Object).Object);
		routerManager.AddWindowClassRoute("Test", workspaceMock.Object);

		Mock<IWindow> window = CreateWindowMock(className: "Test");

		Assert.Equal("Test", routerManager.RouteWindow(window.Object)?.Name);
	}

	[Fact]
	public void AddProcessNameRouteString()
	{
		Logger.Initialize(new LoggerConfig());

		RouterManager routerManager = new(CreateConfigContextMock("Test").Object);
		routerManager.AddProcessNameRoute("Test", "Test");

		Mock<IWindow> window = CreateWindowMock(processName: "Test");

		Assert.Equal("Test", routerManager.RouteWindow(window.Object)?.Name);
	}

	[Fact]
	public void AddProcessNameRoute()
	{
		Logger.Initialize(new LoggerConfig());

		Mock<IWorkspace> workspaceMock = new();
		workspaceMock.Setup(w => w.Name).Returns("Test");

		RouterManager routerManager = new(CreateConfigContextMock(workspace: workspaceMock.Object).Object);
		routerManager.AddProcessNameRoute("Test", workspaceMock.Object);

		Mock<IWindow> window = CreateWindowMock(processName: "Test");

		Assert.Equal("Test", routerManager.RouteWindow(window.Object)?.Name);
	}

	[Fact]
	public void AddTitleRouteString()
	{
		Logger.Initialize(new LoggerConfig());

		RouterManager routerManager = new(CreateConfigContextMock("Test").Object);
		routerManager.AddTitleRoute("Test", "Test");

		Mock<IWindow> window = CreateWindowMock(title: "Test");

		Assert.Equal("Test", routerManager.RouteWindow(window.Object)?.Name);
	}

	[Fact]
	public void AddTitleRoute()
	{
		Logger.Initialize(new LoggerConfig());

		Mock<IWorkspace> workspaceMock = new();
		workspaceMock.Setup(w => w.Name).Returns("Test");

		RouterManager routerManager = new(CreateConfigContextMock(workspace: workspaceMock.Object).Object);
		routerManager.AddTitleRoute("Test", workspaceMock.Object);

		Mock<IWindow> window = CreateWindowMock(title: "Test");

		Assert.Equal("Test", routerManager.RouteWindow(window.Object)?.Name);
	}

	[Fact]
	public void AddTitleMatchRouteString()
	{
		Logger.Initialize(new LoggerConfig());

		RouterManager routerManager = new(CreateConfigContextMock("Test").Object);
		routerManager.AddTitleMatchRoute("Test", "Test");

		Mock<IWindow> window = CreateWindowMock(title: "Test");

		Assert.Equal("Test", routerManager.RouteWindow(window.Object)?.Name);
	}

	[Fact]
	public void AddTitleMatchRoute()
	{
		Logger.Initialize(new LoggerConfig());

		Mock<IWorkspace> workspaceMock = new();
		workspaceMock.Setup(w => w.Name).Returns("Test");

		RouterManager routerManager = new(CreateConfigContextMock(workspace: workspaceMock.Object).Object);
		routerManager.AddTitleMatchRoute("Test", workspaceMock.Object);

		Mock<IWindow> window = CreateWindowMock(title: "Test");

		Assert.Equal("Test", routerManager.RouteWindow(window.Object)?.Name);
	}

	[Fact]
	public void Clear()
	{
		Logger.Initialize(new LoggerConfig());

		RouterManager routerManager = new(CreateConfigContextMock("Test").Object);
		routerManager.AddWindowClassRoute("Test", "Test");

		routerManager.Clear();

		Mock<IWindow> window = CreateWindowMock(className: "Test");

		Assert.Null(routerManager.RouteWindow(window.Object));
	}

	[Fact]
	public void CustomRouter()
	{
		Logger.Initialize(new LoggerConfig());

		Mock<IWorkspace> workspaceMock = new();
		workspaceMock.Setup(w => w.Name).Returns("Test");

		RouterManager routerManager = new(CreateConfigContextMock(workspace: workspaceMock.Object).Object);

		routerManager.Add((w) => w.Class == "Not Test" ? new Mock<IWorkspace>().Object : null);
		routerManager.Add((w) => w.Class == "Test" ? workspaceMock.Object : null);

		Mock<IWindow> window = CreateWindowMock(className: "Test");

		Assert.Equal("Test", routerManager.RouteWindow(window.Object)?.Name);
	}
}
