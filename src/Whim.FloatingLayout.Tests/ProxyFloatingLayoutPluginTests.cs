using System.Text.Json;
using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.FloatingLayout.Tests;

public class ProxyFloatingLayoutPluginCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IContext context = fixture.Freeze<IContext>();
		IWorkspace workspace = fixture.Freeze<IWorkspace>();

		IMonitor monitor = fixture.Freeze<IMonitor>();
		monitor.WorkingArea.Returns(
			new Rectangle<int>()
			{
				X = 0,
				Y = 0,
				Width = 3,
				Height = 3
			}
		);

		// The workspace will have a null last focused window
		workspace.LastFocusedWindow.Returns((IWindow?)null);

		// The workspace manager should have a single workspace
		context.WorkspaceManager.GetEnumerator().Returns((_) => new List<IWorkspace>() { workspace }.GetEnumerator());
		context.WorkspaceManager.ActiveWorkspace.Returns(workspace);

		// Monitor manager will return the monitor at the point (1, 2).
		context.MonitorManager.GetMonitorAtPoint(Arg.Is<IPoint<int>>(p => p.X == 1 && p.Y == 2)).Returns(monitor);

		fixture.Inject(context);
		fixture.Inject(workspace);
		fixture.Inject(monitor);
	}
}

public class ProxyFloatingLayoutPluginTests
{
	private static ProxyFloatingLayoutPlugin CreateSut(IContext context) => new(context);

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

	[Theory, AutoSubstituteData<ProxyFloatingLayoutPluginCustomization>]
	public void Name(IContext context)
	{
		// Given
		ProxyFloatingLayoutPlugin plugin = CreateSut(context);

		// When
		string name = plugin.Name;

		// Then
		Assert.Equal("whim.floating_layout", name);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutPluginCustomization>]
	public void PluginCommands(IContext context)
	{
		// Given
		ProxyFloatingLayoutPlugin plugin = CreateSut(context);

		// When
		IPluginCommands commands = plugin.PluginCommands;

		// Then
		Assert.NotEmpty(commands.Commands);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutPluginCustomization>]
	public void PreInitialize(IContext context)
	{
		// Given
		ProxyFloatingLayoutPlugin plugin = CreateSut(context);

		// When
		plugin.PreInitialize();

		// Then
		context.WorkspaceManager.Received(1).AddProxyLayoutEngine(Arg.Any<ProxyLayoutEngineCreator>());
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutPluginCustomization>]
	public void PostInitialize(IContext context)
	{
		// Given
		ProxyFloatingLayoutPlugin plugin = CreateSut(context);

		// When
		plugin.PostInitialize();

		// Then
		context.WorkspaceManager.DidNotReceive().AddProxyLayoutEngine(Arg.Any<ProxyLayoutEngineCreator>());
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutPluginCustomization>]
	public void WindowManager_WindowRemoved(
		IContext context,
		IWindow window,
		IMonitor monitor,
		IWorkspace activeWorkspace
	)
	{
		// Given
		Rectangle<int> rect = new();
		WindowState windowState =
			new()
			{
				Window = window,
				Rectangle = rect,
				WindowSize = WindowSize.Normal
			};

		context.Butler.Pantry.GetWorkspaceForWindow(window).Returns(activeWorkspace);
		activeWorkspace.TryGetWindowState(window).Returns(windowState);
		context.MonitorManager.GetMonitorAtPoint(rect).Returns(monitor);

		ProxyFloatingLayoutPlugin plugin = CreateSut(context);

		// When
		plugin.PreInitialize();
		plugin.MarkWindowAsFloating(window);
		context.WindowManager.WindowRemoved += Raise.EventWith(new WindowRemovedEventArgs() { Window = window });

		// Then nothing
		Assert.Empty(plugin.FloatingWindows);
	}

	#region MarkWindowAsFloating
	[Theory, AutoSubstituteData<ProxyFloatingLayoutPluginCustomization>]
	public void MarkWindowAsFloating_NoWindow(IContext context)
	{
		// Given
		ProxyFloatingLayoutPlugin plugin = CreateSut(context);

		// When
		plugin.MarkWindowAsFloating(null);

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutPluginCustomization>]
	public void MarkWindowAsFloating_NoWorkspaceForWindow(IContext context, IWindow window)
	{
		// Given
		ProxyFloatingLayoutPlugin plugin = CreateSut(context);
		context.Butler.Pantry.GetWorkspaceForWindow(window).Returns((IWorkspace?)null);

		// When
		plugin.MarkWindowAsFloating(window);

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutPluginCustomization>]
	public void MarkWindowAsFloating_NoWorkspaceForWindow_LastFocusedWindow(
		IContext context,
		IWindow window,
		IWorkspace activeWorkspace
	)
	{
		// Given
		ProxyFloatingLayoutPlugin plugin = CreateSut(context);

		context.Butler.Pantry.GetWorkspaceForWindow(window).Returns((IWorkspace?)null);
		activeWorkspace.LastFocusedWindow.Returns(window);

		// When
		plugin.MarkWindowAsFloating();

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutPluginCustomization>]
	public void MarkWindowAsFloating_NoWindowState(IContext context, IWindow window, IWorkspace activeWorkspace)
	{
		// Given
		context.Butler.Pantry.GetWorkspaceForWindow(window).Returns(activeWorkspace);
		activeWorkspace.TryGetWindowState(window).Returns((IWindowState?)null);

		ProxyFloatingLayoutPlugin plugin = CreateSut(context);

		// When
		plugin.MarkWindowAsFloating(window);

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutPluginCustomization>]
	public void MarkWindowAsFloating(IContext context, IWindow window, IWorkspace activeWorkspace)
	{
		// Given
		context.Butler.Pantry.GetWorkspaceForWindow(window).Returns(activeWorkspace);
		activeWorkspace
			.TryGetWindowState(window)
			.Returns(
				new WindowState()
				{
					Rectangle = new Rectangle<int>() { X = 1, Y = 2 },
					Window = window,
					WindowSize = WindowSize.Normal
				}
			);

		ProxyFloatingLayoutPlugin plugin = CreateSut(context);

		// When
		plugin.MarkWindowAsFloating(window);

		// Then
		Assert.Single(plugin.FloatingWindows);
		Assert.Equal(window, plugin.FloatingWindows.Keys.First());
		activeWorkspace.Received(1).MoveWindowToPoint(window, Arg.Any<IPoint<double>>());
	}
	#endregion

	#region MarkWindowAsDocked
	[Theory, AutoSubstituteData<ProxyFloatingLayoutPluginCustomization>]
	public void MarkWindowAsDocked_WindowIsNotFloating(IContext context, IWindow window, IWorkspace activeWorkspace)
	{
		// Given
		ProxyFloatingLayoutPlugin plugin = CreateSut(context);

		// When
		plugin.MarkWindowAsDocked(window);

		// Then
		Assert.Empty(plugin.FloatingWindows);
		activeWorkspace.DidNotReceive().MoveWindowToPoint(window, Arg.Any<IPoint<double>>());
		activeWorkspace.DidNotReceive().DoLayout();
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutPluginCustomization>]
	public void MarkWindowAsDocked_WindowIsFloating(IContext context, IWindow window, IWorkspace activeWorkspace)
	{
		// Given
		context.Butler.Pantry.GetWorkspaceForWindow(window).Returns(activeWorkspace);
		activeWorkspace
			.TryGetWindowState(window)
			.Returns(
				new WindowState()
				{
					Rectangle = new Rectangle<int>() { X = 1, Y = 2 },
					Window = window,
					WindowSize = WindowSize.Normal
				}
			);

		ProxyFloatingLayoutPlugin plugin = CreateSut(context);
		plugin.MarkWindowAsFloating(window);

		// When
		plugin.MarkWindowAsDocked(window);

		// Then
		Assert.Empty(plugin.FloatingWindows);
		activeWorkspace.Received(2).MoveWindowToPoint(window, Arg.Any<IPoint<double>>());
	}
	#endregion

	#region ToggleWindowFloating
	[Theory, AutoSubstituteData<ProxyFloatingLayoutPluginCustomization>]
	public void ToggleWindowFloating_NoWindow(IContext context)
	{
		// Given
		ProxyFloatingLayoutPlugin plugin = CreateSut(context);

		// When
		plugin.ToggleWindowFloating(null);

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutPluginCustomization>]
	public void ToggleWindowFloating_ToFloating(IContext context, IWindow window, IWorkspace activeWorkspace)
	{
		// Given
		context.Butler.Pantry.GetWorkspaceForWindow(window).Returns(activeWorkspace);
		activeWorkspace
			.TryGetWindowState(window)
			.Returns(
				new WindowState()
				{
					Rectangle = new Rectangle<int>() { X = 1, Y = 2 },
					Window = window,
					WindowSize = WindowSize.Normal
				}
			);

		ProxyFloatingLayoutPlugin plugin = CreateSut(context);

		// When
		plugin.ToggleWindowFloating(window);

		// Then
		Assert.Single(plugin.FloatingWindows);
		Assert.Equal(window, plugin.FloatingWindows.Keys.First());
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutPluginCustomization>]
	public void ToggleWindowFloating_ToDocked(IContext context, IWindow window, IWorkspace activeWorkspace)
	{
		// Given
		context.Butler.Pantry.GetWorkspaceForWindow(window).Returns(activeWorkspace);
		activeWorkspace
			.TryGetWindowState(window)
			.Returns(
				new WindowState()
				{
					Rectangle = new Rectangle<int>() { X = 1, Y = 2 },
					Window = window,
					WindowSize = WindowSize.Normal
				}
			);

		ProxyFloatingLayoutPlugin plugin = CreateSut(context);

		plugin.MarkWindowAsFloating(window);

		// When
		plugin.ToggleWindowFloating(window);

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}
	#endregion

	[Theory, AutoSubstituteData<ProxyFloatingLayoutPluginCustomization>]
	public void SaveState(IContext context)
	{
		// Given
		ProxyFloatingLayoutPlugin plugin = CreateSut(context);

		// When
		JsonElement? json = plugin.SaveState();

		// Then
		Assert.Null(json);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutPluginCustomization>]
	public void LoadState(IContext context)
	{
		// Given
		ProxyFloatingLayoutPlugin plugin = CreateSut(context);

		// When
		plugin.LoadState(JsonDocument.Parse("{}").RootElement);

		// Then nothing
		Assert.Empty(plugin.FloatingWindows);
	}

	#region MarkWindowAsDockedInLayoutEngine
	[Theory, AutoSubstituteData<ProxyFloatingLayoutPluginCustomization>]
	public void MarkWindowAsDockedInLayoutEngine_WindowIsNotFloating(
		IContext context,
		IWindow window,
		IWorkspace activeWorkspace
	)
	{
		// Given
		ILayoutEngine layoutEngine = activeWorkspace.ActiveLayoutEngine;
		ProxyFloatingLayoutPlugin plugin = CreateSut(context);

		Assert.Empty(plugin.FloatingWindows);

		// When
		plugin.MarkWindowAsDockedInLayoutEngine(window, layoutEngine.Identity);

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutPluginCustomization>]
	public void MarkWindowAsDockedInLayoutEngine_WindowIsFloating(
		IContext context,
		IWindow window,
		IWorkspace activeWorkspace
	)
	{
		// Given
		ILayoutEngine layoutEngine = activeWorkspace.ActiveLayoutEngine;
		context.Butler.Pantry.GetWorkspaceForWindow(window).Returns(activeWorkspace);
		activeWorkspace
			.TryGetWindowState(window)
			.Returns(
				new WindowState()
				{
					Rectangle = new Rectangle<int>() { X = 1, Y = 2 },
					Window = window,
					WindowSize = WindowSize.Normal
				}
			);

		ProxyFloatingLayoutPlugin plugin = CreateSut(context);
		plugin.MarkWindowAsFloating(window);

		AssertFloatingWindowsEqual(
			new Dictionary<IWindow, ISet<LayoutEngineIdentity>>()
			{
				{
					window,
					new HashSet<LayoutEngineIdentity>() { layoutEngine.Identity }
				}
			},
			plugin.FloatingWindows
		);

		// When
		plugin.MarkWindowAsDockedInLayoutEngine(window, layoutEngine.Identity);

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutPluginCustomization>]
	public void MarkWindowAsDocked_WindowIsFloatingInMultipleLayoutEngines(
		IContext context,
		IWindow window,
		ILayoutEngine layoutEngine2,
		IWorkspace activeWorkspace
	)
	{
		// Given
		ILayoutEngine layoutEngine = activeWorkspace.ActiveLayoutEngine;
		layoutEngine2.Identity.Returns(new LayoutEngineIdentity());

		context.Butler.Pantry.GetWorkspaceForWindow(window).Returns(activeWorkspace);
		activeWorkspace
			.TryGetWindowState(window)
			.Returns(
				new WindowState()
				{
					Rectangle = new Rectangle<int>() { X = 1, Y = 2 },
					Window = window,
					WindowSize = WindowSize.Normal
				}
			);

		// When
		ProxyFloatingLayoutPlugin plugin = CreateSut(context);
		plugin.MarkWindowAsFloating(window);

		activeWorkspace.ActiveLayoutEngine.Returns(layoutEngine2);
		plugin.MarkWindowAsFloating(window);

		plugin.MarkWindowAsDockedInLayoutEngine(window, layoutEngine.Identity);

		// Then
		AssertFloatingWindowsEqual(
			new Dictionary<IWindow, ISet<LayoutEngineIdentity>>()
			{
				{
					window,
					new HashSet<LayoutEngineIdentity>() { layoutEngine2.Identity }
				}
			},
			plugin.FloatingWindows
		);
	}
	#endregion
}
