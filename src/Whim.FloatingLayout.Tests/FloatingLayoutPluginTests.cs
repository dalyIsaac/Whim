using System.Text.Json;
using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.FloatingLayout.Tests;

public class FloatingLayoutPluginCustomization : ICustomization
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

public class FloatingLayoutPluginTests
{
	private static FloatingLayoutPlugin CreateSut(IContext context) => new(context);

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

	[Theory, AutoSubstituteData<FloatingLayoutPluginCustomization>]
	public void Name(IContext context)
	{
		// Given
		FloatingLayoutPlugin plugin = CreateSut(context);

		// When
		string name = plugin.Name;

		// Then
		Assert.Equal("whim.floating_layout", name);
	}

	[Theory, AutoSubstituteData<FloatingLayoutPluginCustomization>]
	public void PluginCommands(IContext context)
	{
		// Given
		FloatingLayoutPlugin plugin = CreateSut(context);

		// When
		IPluginCommands commands = plugin.PluginCommands;

		// Then
		Assert.NotEmpty(commands.Commands);
	}

	[Theory, AutoSubstituteData<FloatingLayoutPluginCustomization>]
	public void PreInitialize(IContext context)
	{
		// Given
		FloatingLayoutPlugin plugin = CreateSut(context);

		// When
		plugin.PreInitialize();

		// Then
		context.WorkspaceManager.Received(1).AddProxyLayoutEngine(Arg.Any<CreateProxyLayoutEngine>());
	}

	[Theory, AutoSubstituteData<FloatingLayoutPluginCustomization>]
	public void PostInitialize(IContext context)
	{
		// Given
		FloatingLayoutPlugin plugin = CreateSut(context);

		// When
		plugin.PostInitialize();

		// Then
		context.WorkspaceManager.DidNotReceive().AddProxyLayoutEngine(Arg.Any<CreateProxyLayoutEngine>());
	}

	[Theory, AutoSubstituteData<FloatingLayoutPluginCustomization>]
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

		FloatingLayoutPlugin plugin = CreateSut(context);

		// When
		plugin.PreInitialize();
		plugin.PostInitialize();
		plugin.MarkWindowAsFloating(window);
		context.WindowManager.WindowRemoved += Raise.EventWith(new WindowEventArgs() { Window = window });

		// Then nothing
		Assert.Empty(plugin.FloatingWindows);
	}

	#region MarkWindowAsFloating
	[Theory, AutoSubstituteData<FloatingLayoutPluginCustomization>]
	public void MarkWindowAsFloating_NoWindow(IContext context)
	{
		// Given
		FloatingLayoutPlugin plugin = CreateSut(context);

		// When
		plugin.MarkWindowAsFloating(null);

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Theory, AutoSubstituteData<FloatingLayoutPluginCustomization>]
	public void MarkWindowAsFloating_NoWorkspaceForWindow(IContext context, IWindow window)
	{
		// Given
		FloatingLayoutPlugin plugin = CreateSut(context);
		context.Butler.Pantry.GetWorkspaceForWindow(window).Returns((IWorkspace?)null);

		// When
		plugin.MarkWindowAsFloating(window);

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Theory, AutoSubstituteData<FloatingLayoutPluginCustomization>]
	public void MarkWindowAsFloating_NoWorkspaceForWindow_LastFocusedWindow(
		IContext context,
		IWindow window,
		IWorkspace activeWorkspace
	)
	{
		// Given
		FloatingLayoutPlugin plugin = CreateSut(context);

		context.Butler.Pantry.GetWorkspaceForWindow(window).Returns((IWorkspace?)null);
		activeWorkspace.LastFocusedWindow.Returns(window);

		// When
		plugin.MarkWindowAsFloating();

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Theory, AutoSubstituteData<FloatingLayoutPluginCustomization>]
	public void MarkWindowAsFloating_NoWindowState(IContext context, IWindow window, IWorkspace activeWorkspace)
	{
		// Given
		context.Butler.Pantry.GetWorkspaceForWindow(window).Returns(activeWorkspace);
		activeWorkspace.TryGetWindowState(window).Returns((IWindowState?)null);

		FloatingLayoutPlugin plugin = CreateSut(context);

		// When
		plugin.MarkWindowAsFloating(window);

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Theory, AutoSubstituteData<FloatingLayoutPluginCustomization>]
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

		FloatingLayoutPlugin plugin = CreateSut(context);

		// When
		plugin.MarkWindowAsFloating(window);

		// Then
		Assert.Single(plugin.FloatingWindows);
		Assert.Equal(window, plugin.FloatingWindows.Keys.First());
		activeWorkspace.Received(1).MoveWindowToPoint(window, Arg.Any<IPoint<double>>());
	}
	#endregion

	#region MarkWindowAsDocked
	[Theory, AutoSubstituteData<FloatingLayoutPluginCustomization>]
	public void MarkWindowAsDocked_WindowIsNotFloating(IContext context, IWindow window, IWorkspace activeWorkspace)
	{
		// Given
		FloatingLayoutPlugin plugin = CreateSut(context);

		// When
		plugin.MarkWindowAsDocked(window);

		// Then
		Assert.Empty(plugin.FloatingWindows);
		activeWorkspace.DidNotReceive().MoveWindowToPoint(window, Arg.Any<IPoint<double>>());
		activeWorkspace.DidNotReceive().DoLayout();
	}

	[Theory, AutoSubstituteData<FloatingLayoutPluginCustomization>]
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

		FloatingLayoutPlugin plugin = CreateSut(context);
		plugin.MarkWindowAsFloating(window);

		// When
		plugin.MarkWindowAsDocked(window);

		// Then
		Assert.Empty(plugin.FloatingWindows);
		activeWorkspace.Received(2).MoveWindowToPoint(window, Arg.Any<IPoint<double>>());
	}
	#endregion

	#region ToggleWindowFloating
	[Theory, AutoSubstituteData<FloatingLayoutPluginCustomization>]
	public void ToggleWindowFloating_NoWindow(IContext context)
	{
		// Given
		FloatingLayoutPlugin plugin = CreateSut(context);

		// When
		plugin.ToggleWindowFloating(null);

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Theory, AutoSubstituteData<FloatingLayoutPluginCustomization>]
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

		FloatingLayoutPlugin plugin = CreateSut(context);

		// When
		plugin.ToggleWindowFloating(window);

		// Then
		Assert.Single(plugin.FloatingWindows);
		Assert.Equal(window, plugin.FloatingWindows.Keys.First());
	}

	[Theory, AutoSubstituteData<FloatingLayoutPluginCustomization>]
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

		FloatingLayoutPlugin plugin = CreateSut(context);

		plugin.MarkWindowAsFloating(window);

		// When
		plugin.ToggleWindowFloating(window);

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}
	#endregion

	[Theory, AutoSubstituteData<FloatingLayoutPluginCustomization>]
	public void SaveState(IContext context)
	{
		// Given
		FloatingLayoutPlugin plugin = CreateSut(context);

		// When
		JsonElement? json = plugin.SaveState();

		// Then
		Assert.Null(json);
	}

	[Theory, AutoSubstituteData<FloatingLayoutPluginCustomization>]
	public void LoadState(IContext context)
	{
		// Given
		FloatingLayoutPlugin plugin = CreateSut(context);

		// When
		plugin.LoadState(JsonDocument.Parse("{}").RootElement);

		// Then nothing
		Assert.Empty(plugin.FloatingWindows);
	}

	#region MarkWindowAsDockedInLayoutEngine
	[Theory, AutoSubstituteData<FloatingLayoutPluginCustomization>]
	public void MarkWindowAsDockedInLayoutEngine_WindowIsNotFloating(
		IContext context,
		IWindow window,
		IWorkspace activeWorkspace
	)
	{
		// Given
		ILayoutEngine layoutEngine = activeWorkspace.ActiveLayoutEngine;
		FloatingLayoutPlugin plugin = CreateSut(context);

		Assert.Empty(plugin.FloatingWindows);

		// When
		plugin.MarkWindowAsDockedInLayoutEngine(window, layoutEngine.Identity);

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Theory, AutoSubstituteData<FloatingLayoutPluginCustomization>]
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

		FloatingLayoutPlugin plugin = CreateSut(context);
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

	[Theory, AutoSubstituteData<FloatingLayoutPluginCustomization>]
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
		FloatingLayoutPlugin plugin = CreateSut(context);
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
