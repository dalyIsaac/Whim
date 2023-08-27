using Moq;
using System.Text.Json;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.FloatingLayout.Tests;

public class FloatingLayoutPluginTests
{
	private class Wrapper
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<IWindowManager> WindowManager { get; } = new();
		public Mock<IWorkspaceManager> WorkspaceManager { get; } = new();
		public Mock<IMonitorManager> MonitorManager { get; } = new();
		public Mock<INativeManager> NativeManager { get; } = new();
		public Mock<IWorkspace> Workspace { get; } = new();
		public Mock<ILayoutEngine> LayoutEngine { get; } = new();
		public FloatingLayoutPlugin Plugin;

		public Wrapper()
		{
			Context.SetupGet(x => x.WindowManager).Returns(WindowManager.Object);
			Context.SetupGet(x => x.MonitorManager).Returns(MonitorManager.Object);
			Context.SetupGet(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);
			Context.SetupGet(x => x.NativeManager).Returns(NativeManager.Object);

			WorkspaceManager.SetupGet(x => x.ActiveWorkspace).Returns(Workspace.Object);
			NativeManager.Setup(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>())).Returns((ILocation<int>?)null);

			Plugin = new(Context.Object);

			LayoutEngine.SetupGet(le => le.Identity).Returns(new LayoutEngineIdentity());
		}

		public Wrapper Setup_GetWorkspaceForWindow(IWindow window, IWorkspace workspace)
		{
			WorkspaceManager.Setup(wm => wm.GetWorkspaceForWindow(window)).Returns(workspace);
			return this;
		}

		public Wrapper Setup_TryGetWindowLocation(IWindow window, IWindowState? windowState)
		{
			Workspace.Setup(w => w.TryGetWindowLocation(window)).Returns(windowState);

			if (windowState != null)
			{
				Workspace.Setup(w => w.ActiveLayoutEngine).Returns(LayoutEngine.Object);
			}
			return this;
		}

		public Wrapper Setup_GetMonitorAtPoint(ILocation<int> location, Mock<IMonitor> monitor)
		{
			monitor.Setup(m => m.WorkingArea).Returns(location);
			MonitorManager.Setup(mm => mm.GetMonitorAtPoint(location)).Returns(monitor.Object);
			return this;
		}
	}

	private static void AssertFloatingWindowsEqual(
		IReadOnlyDictionary<IWindow, ISet<LayoutEngineIdentity>> expected,
		IReadOnlyDictionary<IWindow, ISet<LayoutEngineIdentity>> actual
	)
	{
		Assert.Equal(expected.Count, actual.Count);

		foreach (KeyValuePair<IWindow, ISet<LayoutEngineIdentity>> expectedWindow in expected)
		{
			Assert.Contains(expectedWindow.Key, actual.Keys);
			Assert.Equal(expectedWindow.Value, actual[expectedWindow.Key]);
		}
	}

	[Fact]
	public void Name()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutPlugin plugin = wrapper.Plugin;

		// When
		string name = plugin.Name;

		// Then
		Assert.Equal("whim.floating_layout", name);
	}

	[Fact]
	public void PluginCommands()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutPlugin plugin = wrapper.Plugin;

		// When
		IPluginCommands commands = plugin.PluginCommands;

		// Then
		Assert.NotEmpty(commands.Commands);
	}

	[Fact]
	public void PreInitialize()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutPlugin plugin = wrapper.Plugin;

		// When
		plugin.PreInitialize();

		// Then
		wrapper.WorkspaceManager.Verify(x => x.AddProxyLayoutEngine(It.IsAny<CreateProxyLayoutEngine>()), Times.Once);
	}

	[Fact]
	public void PostInitialize()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutPlugin plugin = wrapper.Plugin;

		// When
		plugin.PostInitialize();

		// Then
		wrapper.WorkspaceManager.Verify(x => x.AddProxyLayoutEngine(It.IsAny<CreateProxyLayoutEngine>()), Times.Never);
	}

	[Fact]
	public void WindowManager_WindowRemoved()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<IMonitor> monitor = new();
		Location<int> location = new();
		WindowState windowState =
			new()
			{
				Window = window.Object,
				Location = location,
				WindowSize = WindowSize.Normal
			};

		Wrapper wrapper = new();
		wrapper
			.Setup_GetWorkspaceForWindow(window.Object, wrapper.Workspace.Object)
			.Setup_TryGetWindowLocation(window.Object, windowState)
			.Setup_GetMonitorAtPoint(location, monitor);
		FloatingLayoutPlugin plugin = wrapper.Plugin;

		// When
		plugin.PreInitialize();
		plugin.MarkWindowAsFloating(window.Object);
		wrapper.WindowManager.Raise(x => x.WindowRemoved += null, new WindowEventArgs() { Window = window.Object });

		// Then nothing
		Assert.Empty(plugin.FloatingWindows);
	}

	#region MarkWindowAsFloating
	[Fact]
	public void MarkWindowAsFloating_NoWindow()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutPlugin plugin = wrapper.Plugin;

		// When
		plugin.MarkWindowAsFloating(null);

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Fact]
	public void MarkWindowAsFloating_NoWorkspaceForWindow()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutPlugin plugin = wrapper.Plugin;

		Mock<IWindow> window = new();

		// When
		plugin.MarkWindowAsFloating(window.Object);

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Fact]
	public void MarkWindowAsFloating_NoWorkspaceForWindow_LastFocusedWindow()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutPlugin plugin = wrapper.Plugin;

		Mock<IWindow> window = new();
		wrapper.WorkspaceManager.Setup(wm => wm.ActiveWorkspace.LastFocusedWindow).Returns(window.Object);

		// When
		plugin.MarkWindowAsFloating();

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Fact]
	public void MarkWindowAsFloating_NoWindowState()
	{
		// Given
		Mock<IWindow> window = new();
		Wrapper wrapper = new();
		wrapper
			.Setup_GetWorkspaceForWindow(window.Object, wrapper.Workspace.Object)
			.Setup_TryGetWindowLocation(window.Object, null);

		FloatingLayoutPlugin plugin = wrapper.Plugin;

		// When
		plugin.MarkWindowAsFloating(window.Object);

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Fact]
	public void MarkWindowAsFloating()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<IMonitor> monitor = new();

		Wrapper wrapper = new();
		wrapper
			.Setup_GetWorkspaceForWindow(window.Object, wrapper.Workspace.Object)
			.Setup_TryGetWindowLocation(
				window.Object,
				new WindowState()
				{
					Location = new Location<int>() { X = 1, Y = 2 },
					Window = window.Object,
					WindowSize = WindowSize.Normal
				}
			)
			.Setup_GetMonitorAtPoint(new Location<int>() { X = 1, Y = 2 }, monitor);

		FloatingLayoutPlugin plugin = wrapper.Plugin;

		// When
		plugin.MarkWindowAsFloating(window.Object);

		// Then
		Assert.Single(plugin.FloatingWindows);
		Assert.Equal(window.Object, plugin.FloatingWindows.Keys.First());
		wrapper.Workspace.Verify(w => w.MoveWindowToPoint(window.Object, It.IsAny<IPoint<double>>()), Times.Once);
	}
	#endregion

	#region MarkWindowAsDocked
	[Fact]
	public void MarkWindowAsDocked_WindowIsNotFloating()
	{
		// Given
		Mock<IWindow> window = new();
		Wrapper wrapper = new();
		FloatingLayoutPlugin plugin = wrapper.Plugin;

		// When
		plugin.MarkWindowAsDocked(window.Object);

		// Then
		Assert.Empty(plugin.FloatingWindows);
		wrapper.Workspace.Verify(w => w.MoveWindowToPoint(window.Object, It.IsAny<IPoint<double>>()), Times.Never);
	}

	[Fact]
	public void MarkWindowAsDocked_WindowIsFloating()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<IMonitor> monitor = new();

		Wrapper wrapper = new();
		wrapper
			.Setup_GetWorkspaceForWindow(window.Object, wrapper.Workspace.Object)
			.Setup_TryGetWindowLocation(
				window.Object,
				new WindowState()
				{
					Location = new Location<int>() { X = 1, Y = 2 },
					Window = window.Object,
					WindowSize = WindowSize.Normal
				}
			)
			.Setup_GetMonitorAtPoint(new Location<int>() { X = 1, Y = 2 }, monitor);

		FloatingLayoutPlugin plugin = wrapper.Plugin;
		plugin.MarkWindowAsFloating(window.Object);

		// When
		plugin.MarkWindowAsDocked(window.Object);

		// Then
		Assert.Empty(plugin.FloatingWindows);
		wrapper.Workspace.Verify(w => w.MoveWindowToPoint(window.Object, It.IsAny<IPoint<double>>()), Times.Exactly(2));
	}
	#endregion

	#region ToggleWindowFloating
	[Fact]
	public void ToggleWindowFloating_NoWindow()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutPlugin plugin = wrapper.Plugin;

		// When
		plugin.ToggleWindowFloating(null);

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Fact]
	public void ToggleWindowFloating_ToFloating()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<IMonitor> monitor = new();

		Wrapper wrapper = new();

		wrapper
			.Setup_GetWorkspaceForWindow(window.Object, wrapper.Workspace.Object)
			.Setup_TryGetWindowLocation(
				window.Object,
				new WindowState()
				{
					Location = new Location<int>() { X = 1, Y = 2 },
					Window = window.Object,
					WindowSize = WindowSize.Normal
				}
			)
			.Setup_GetMonitorAtPoint(new Location<int>() { X = 1, Y = 2 }, monitor);

		FloatingLayoutPlugin plugin = wrapper.Plugin;

		// When
		plugin.ToggleWindowFloating(window.Object);

		// Then
		Assert.Single(plugin.FloatingWindows);
		Assert.Equal(window.Object, plugin.FloatingWindows.Keys.First());
	}

	[Fact]
	public void ToggleWindowFloating_ToDocked()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<IMonitor> monitor = new();

		Wrapper wrapper = new();

		wrapper
			.Setup_GetWorkspaceForWindow(window.Object, wrapper.Workspace.Object)
			.Setup_TryGetWindowLocation(
				window.Object,
				new WindowState()
				{
					Location = new Location<int>() { X = 1, Y = 2 },
					Window = window.Object,
					WindowSize = WindowSize.Normal
				}
			)
			.Setup_GetMonitorAtPoint(new Location<int>() { X = 1, Y = 2 }, monitor);

		FloatingLayoutPlugin plugin = wrapper.Plugin;

		plugin.MarkWindowAsFloating(window.Object);

		// When
		plugin.ToggleWindowFloating(window.Object);

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}
	#endregion

	[Fact]
	public void SaveState()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutPlugin plugin = wrapper.Plugin;

		// When
		JsonElement? json = plugin.SaveState();

		// Then
		Assert.Null(json);
	}

	[Fact]
	public void LoadState()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutPlugin plugin = wrapper.Plugin;

		// When
		plugin.LoadState(JsonDocument.Parse("{}").RootElement);

		// Then nothing
		Assert.Empty(plugin.FloatingWindows);
	}

	#region MarkWindowAsDockedInLayoutEngine
	[Fact]
	public void MarkWindowAsDockedInLayoutEngine_WindowIsNotFloating()
	{
		// Given
		Mock<IWindow> window = new();
		Wrapper wrapper = new();
		FloatingLayoutPlugin plugin = wrapper.Plugin;

		Assert.Empty(plugin.FloatingWindows);

		// When
		plugin.MarkWindowAsDockedInLayoutEngine(window.Object, wrapper.LayoutEngine.Object.Identity);

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Fact]
	public void MarkWindowAsDockedInLayoutEngine_WindowIsFloating()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<IMonitor> monitor = new();

		Wrapper wrapper = new();

		wrapper
			.Setup_GetWorkspaceForWindow(window.Object, wrapper.Workspace.Object)
			.Setup_TryGetWindowLocation(
				window.Object,
				new WindowState()
				{
					Location = new Location<int>() { X = 1, Y = 2 },
					Window = window.Object,
					WindowSize = WindowSize.Normal
				}
			)
			.Setup_GetMonitorAtPoint(new Location<int>() { X = 1, Y = 2 }, monitor);

		FloatingLayoutPlugin plugin = wrapper.Plugin;
		plugin.MarkWindowAsFloating(window.Object);

		AssertFloatingWindowsEqual(
			new Dictionary<IWindow, ISet<LayoutEngineIdentity>>()
			{
				{
					window.Object,
					new HashSet<LayoutEngineIdentity>() { wrapper.LayoutEngine.Object.Identity }
				}
			},
			plugin.FloatingWindows
		);

		// When
		plugin.MarkWindowAsDockedInLayoutEngine(window.Object, wrapper.LayoutEngine.Object.Identity);

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Fact]
	public void MarkWindowAsDocked_WindowIsFloatingInMultipleLayoutEngines()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<IMonitor> monitor = new();

		Wrapper wrapper = new();
		Mock<ILayoutEngine> layoutEngine2 = new();
		layoutEngine2.SetupGet(le => le.Identity).Returns(new LayoutEngineIdentity());

		wrapper
			.Setup_GetWorkspaceForWindow(window.Object, wrapper.Workspace.Object)
			.Setup_TryGetWindowLocation(
				window.Object,
				new WindowState()
				{
					Location = new Location<int>() { X = 1, Y = 2 },
					Window = window.Object,
					WindowSize = WindowSize.Normal
				}
			)
			.Setup_GetMonitorAtPoint(new Location<int>() { X = 1, Y = 2 }, monitor);

		// When
		FloatingLayoutPlugin plugin = wrapper.Plugin;
		plugin.MarkWindowAsFloating(window.Object);

		wrapper.Workspace.Setup(w => w.ActiveLayoutEngine).Returns(layoutEngine2.Object);
		plugin.MarkWindowAsFloating(window.Object);

		plugin.MarkWindowAsDockedInLayoutEngine(window.Object, wrapper.LayoutEngine.Object.Identity);

		// Then
		AssertFloatingWindowsEqual(
			new Dictionary<IWindow, ISet<LayoutEngineIdentity>>()
			{
				{
					window.Object,
					new HashSet<LayoutEngineIdentity>() { layoutEngine2.Object.Identity }
				}
			},
			plugin.FloatingWindows
		);
	}

	#endregion
}
