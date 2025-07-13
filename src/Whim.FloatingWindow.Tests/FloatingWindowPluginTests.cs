using System.Collections.Immutable;
using System.Text.Json;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.FloatingWindow.Tests;

public class FloatingWindowPluginTests
{
	private static FloatingWindowPlugin CreateSut(IContext ctx) => new(ctx);

	[Theory, AutoSubstituteData<StoreCustomization>]
	public void Name(IContext ctx)
	{
		// Given
		FloatingWindowPlugin plugin = CreateSut(ctx);

		// When
		string name = plugin.Name;

		// Then
		Assert.Equal("whim.floating_window", name);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	public void PluginCommands(IContext ctx)
	{
		// Given
		FloatingWindowPlugin plugin = CreateSut(ctx);

		// When
		IPluginCommands commands = plugin.PluginCommands;

		// Then
		Assert.NotEmpty(commands.Commands);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	public void PreInitialize(IContext ctx, List<object> transforms, ILayoutEngine layoutEngine)
	{
		// Given
		FloatingWindowPlugin plugin = CreateSut(ctx);

		// When
		plugin.PreInitialize();

		// Then
		Assert.IsType<AddProxyLayoutEngineTransform>(transforms[0]);
		ProxyLayoutEngineCreator creator = ((AddProxyLayoutEngineTransform)transforms[0]).ProxyLayoutEngineCreator;
		ILayoutEngine result = creator(layoutEngine);
		Assert.IsType<ProxyFloatingLayoutEngine>(result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	public void PostInitialize(IContext ctx, List<object> transforms)
	{
		// Given
		FloatingWindowPlugin plugin = CreateSut(ctx);

		// When
		plugin.PostInitialize();

		// Then
		Assert.Empty(transforms);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowManager_WindowRemoved(IContext ctx, MutableRootSector root, IWindow window)
	{
		// Given
		Workspace activeWorkspace = CreateWorkspace();
		PopulateWindowWorkspaceMap(root, window, activeWorkspace);

		FloatingWindowPlugin plugin = CreateSut(ctx);

		// When
		plugin.PreInitialize();
		plugin.MarkWindowAsFloating(window);
		Assert.Single(plugin.FloatingWindows);

		root.WindowSector.QueueEvent(new WindowRemovedEventArgs() { Window = window });
		root.WindowSector.DispatchEvents();

		// Then nothing
		Assert.Empty(plugin.FloatingWindows);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	public void SaveState(IContext ctx)
	{
		// Given
		FloatingWindowPlugin plugin = CreateSut(ctx);

		// When
		JsonElement? json = plugin.SaveState();

		// Then
		Assert.Null(json);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	public void LoadState(IContext ctx)
	{
		// Given
		FloatingWindowPlugin plugin = CreateSut(ctx);

		// When
		plugin.LoadState(JsonDocument.Parse("{}").RootElement);

		// Then nothing
		Assert.Empty(plugin.FloatingWindows);
	}
}

public class FloatingWindowPlugin_MarkWindowAsFloatingTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MarkWindowAsFloating_NoWindow(IContext ctx, MutableRootSector root)
	{
		// Given
		FloatingWindowPlugin plugin = new(ctx);
		AddActiveWorkspaceToStore(root, CreateWorkspace());

		// When
		plugin.MarkWindowAsFloating();

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	public void MarkWindowAsFloating_NoWindowPosition(IContext ctx, IWindow window)
	{
		// Given
		FloatingWindowPlugin plugin = new(ctx);

		// When
		plugin.MarkWindowAsFloating(window);

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MarkWindowAsFloating(IContext ctx, MutableRootSector root, IWindow window, List<object> transforms)
	{
		// Given
		FloatingWindowPlugin plugin = new(ctx);
		Workspace activeWorkspace = CreateWorkspace() with
		{
			WindowPositions = new Dictionary<HWND, WindowPosition>()
			{
				{ window.Handle, new WindowPosition(WindowSize.Normal, new Rectangle<int>(0, 0, 100, 100)) },
			}.ToImmutableDictionary(),
		};
		PopulateWindowWorkspaceMap(root, window, activeWorkspace);

		// When
		plugin.MarkWindowAsFloating(window);

		// Then
		Assert.Single(plugin.FloatingWindows);
		Assert.Contains(
			transforms,
			t => t.Equals(new MoveWindowToPointTransform(window.Handle, new Rectangle<int>(0, 0, 100, 100)))
		);
	}
}

public class FloatingWindowPlugin_MarkWindowAsDockedTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MarkWindowAsDocked_NoWindow(IContext ctx, MutableRootSector root)
	{
		// Given
		FloatingWindowPlugin plugin = new(ctx);
		AddActiveWorkspaceToStore(root, CreateWorkspace());

		// When
		plugin.MarkWindowAsDocked();

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MarkWindowAsDocked_NoWindowPosition(IContext ctx, IWindow window)
	{
		// Given
		FloatingWindowPlugin plugin = new(ctx);

		// When
		plugin.MarkWindowAsDocked(window);

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MarkWindowAsDocked(IContext ctx, MutableRootSector root, IWindow window, List<object> transforms)
	{
		// Given
		FloatingWindowPlugin plugin = new(ctx);
		Workspace activeWorkspace = CreateWorkspace() with
		{
			WindowPositions = new Dictionary<HWND, WindowPosition>()
			{
				{ window.Handle, new WindowPosition(WindowSize.Normal, new Rectangle<int>(0, 0, 100, 100)) },
			}.ToImmutableDictionary(),
		};
		PopulateWindowWorkspaceMap(root, window, activeWorkspace);
		plugin.MarkWindowAsFloating(window);

		// When
		plugin.MarkWindowAsDocked(window);

		// Then
		Assert.Empty(plugin.FloatingWindows);
		Assert.Contains(
			transforms,
			t => t.Equals(new MoveWindowToPointTransform(window.Handle, new Rectangle<int>(0, 0, 100, 100)))
		);
	}
}

public class FloatingWindowPlugin_ToggleWindowFloatingTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ToggleWindowFloating_NoWindow(IContext ctx, MutableRootSector root)
	{
		// Given
		FloatingWindowPlugin plugin = new(ctx);
		AddActiveWorkspaceToStore(root, CreateWorkspace());

		// When
		plugin.ToggleWindowFloating();

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ToggleWindowFloating_FloatWindow(
		IContext ctx,
		MutableRootSector root,
		IWindow window,
		List<object> transforms
	)
	{
		// Given
		FloatingWindowPlugin plugin = new(ctx);
		Workspace activeWorkspace = CreateWorkspace() with
		{
			WindowPositions = new Dictionary<HWND, WindowPosition>()
			{
				{ window.Handle, new WindowPosition(WindowSize.Normal, new Rectangle<int>(0, 0, 100, 100)) },
			}.ToImmutableDictionary(),
		};
		PopulateWindowWorkspaceMap(root, window, activeWorkspace);

		// When
		plugin.ToggleWindowFloating(window);

		// Then
		Assert.Single(plugin.FloatingWindows);
		Assert.Contains(
			transforms,
			t => t.Equals(new MoveWindowToPointTransform(window.Handle, new Rectangle<int>(0, 0, 100, 100)))
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ToggleWindowFloating_DockWindow(
		IContext ctx,
		MutableRootSector root,
		IWindow window,
		List<object> transforms
	)
	{
		// Given
		FloatingWindowPlugin plugin = new(ctx);
		Workspace activeWorkspace = CreateWorkspace() with
		{
			WindowPositions = new Dictionary<HWND, WindowPosition>()
			{
				{ window.Handle, new WindowPosition(WindowSize.Normal, new Rectangle<int>(0, 0, 100, 100)) },
			}.ToImmutableDictionary(),
		};
		PopulateWindowWorkspaceMap(root, window, activeWorkspace);
		plugin.MarkWindowAsFloating(window);

		// When
		plugin.ToggleWindowFloating(window);

		// Then
		Assert.Empty(plugin.FloatingWindows);
		Assert.Contains(
			transforms,
			t => t.Equals(new MoveWindowToPointTransform(window.Handle, new Rectangle<int>(0, 0, 100, 100)))
		);
	}
}
