using System.Collections.Immutable;
using System.Text.Json;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.FloatingWindow.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class FloatingWindowPluginTests
{
	private static FloatingWindowPlugin CreateSut(IContext context) => new(context);

	[Theory, AutoSubstituteData<StoreCustomization>]
	public void Name(IContext context)
	{
		// Given
		FloatingWindowPlugin plugin = CreateSut(context);

		// When
		string name = plugin.Name;

		// Then
		Assert.Equal("whim.floating_window", name);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	public void PluginCommands(IContext context)
	{
		// Given
		FloatingWindowPlugin plugin = CreateSut(context);

		// When
		IPluginCommands commands = plugin.PluginCommands;

		// Then
		Assert.NotEmpty(commands.Commands);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	public void PreInitialize(IContext context, List<object> transforms, ILayoutEngine layoutEngine)
	{
		// Given
		FloatingWindowPlugin plugin = CreateSut(context);

		// When
		plugin.PreInitialize();

		// Then
		Assert.IsType<AddProxyLayoutEngineTransform>(transforms[0]);
		ProxyLayoutEngineCreator creator = ((AddProxyLayoutEngineTransform)transforms[0]).ProxyLayoutEngineCreator;
		ILayoutEngine result = creator(layoutEngine);
		Assert.IsType<ProxyFloatingLayoutEngine>(result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	public void PostInitialize(IContext context, List<object> transforms)
	{
		// Given
		FloatingWindowPlugin plugin = CreateSut(context);

		// When
		plugin.PostInitialize();

		// Then
		Assert.Empty(transforms);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowManager_WindowRemoved(IContext ctx, MutableRootSector root, IWindow window)
	{
		// Given
		Workspace activeWorkspace = StoreTestUtils.CreateWorkspace(ctx);
		StoreTestUtils.PopulateWindowWorkspaceMap(ctx, root, window, activeWorkspace);

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
	public void SaveState(IContext context)
	{
		// Given
		FloatingWindowPlugin plugin = CreateSut(context);

		// When
		JsonElement? json = plugin.SaveState();

		// Then
		Assert.Null(json);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	public void LoadState(IContext context)
	{
		// Given
		FloatingWindowPlugin plugin = CreateSut(context);

		// When
		plugin.LoadState(JsonDocument.Parse("{}").RootElement);

		// Then nothing
		Assert.Empty(plugin.FloatingWindows);
	}
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class FloatingWindowPlugin_MarkWindowAsFloatingTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MarkWindowAsFloating_NoWindow(IContext context, MutableRootSector root)
	{
		// Given
		FloatingWindowPlugin plugin = new(context);
		StoreTestUtils.AddActiveWorkspace(context, root, StoreTestUtils.CreateWorkspace(context));

		// When
		plugin.MarkWindowAsFloating();

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	public void MarkWindowAsFloating_NoWindowPosition(IContext context, IWindow window)
	{
		// Given
		FloatingWindowPlugin plugin = new(context);

		// When
		plugin.MarkWindowAsFloating(window);

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MarkWindowAsFloating(
		IContext context,
		MutableRootSector root,
		IWindow window,
		List<object> transforms
	)
	{
		// Given
		FloatingWindowPlugin plugin = new(context);
		Workspace activeWorkspace = StoreTestUtils.CreateWorkspace(context) with
		{
			WindowPositions = new Dictionary<HWND, WindowPosition>()
			{
				{ window.Handle, new WindowPosition(WindowSize.Normal, new Rectangle<int>(0, 0, 100, 100)) },
			}.ToImmutableDictionary(),
		};
		StoreTestUtils.PopulateWindowWorkspaceMap(context, root, window, activeWorkspace);

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

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class FloatingWindowPlugin_MarkWindowAsDockedTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MarkWindowAsDocked_NoWindow(IContext context, MutableRootSector root)
	{
		// Given
		FloatingWindowPlugin plugin = new(context);
		StoreTestUtils.AddActiveWorkspace(context, root, StoreTestUtils.CreateWorkspace(context));

		// When
		plugin.MarkWindowAsDocked();

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MarkWindowAsDocked_NoWindowPosition(IContext context, IWindow window)
	{
		// Given
		FloatingWindowPlugin plugin = new(context);

		// When
		plugin.MarkWindowAsDocked(window);

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MarkWindowAsDocked(IContext context, MutableRootSector root, IWindow window, List<object> transforms)
	{
		// Given
		FloatingWindowPlugin plugin = new(context);
		Workspace activeWorkspace = StoreTestUtils.CreateWorkspace(context) with
		{
			WindowPositions = new Dictionary<HWND, WindowPosition>()
			{
				{ window.Handle, new WindowPosition(WindowSize.Normal, new Rectangle<int>(0, 0, 100, 100)) },
			}.ToImmutableDictionary(),
		};
		StoreTestUtils.PopulateWindowWorkspaceMap(context, root, window, activeWorkspace);
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

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class FloatingWindowPlugin_ToggleWindowFloatingTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ToggleWindowFloating_NoWindow(IContext context, MutableRootSector root)
	{
		// Given
		FloatingWindowPlugin plugin = new(context);
		StoreTestUtils.AddActiveWorkspace(context, root, StoreTestUtils.CreateWorkspace(context));

		// When
		plugin.ToggleWindowFloating();

		// Then
		Assert.Empty(plugin.FloatingWindows);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ToggleWindowFloating_FloatWindow(
		IContext context,
		MutableRootSector root,
		IWindow window,
		List<object> transforms
	)
	{
		// Given
		FloatingWindowPlugin plugin = new(context);
		Workspace activeWorkspace = StoreTestUtils.CreateWorkspace(context) with
		{
			WindowPositions = new Dictionary<HWND, WindowPosition>()
			{
				{ window.Handle, new WindowPosition(WindowSize.Normal, new Rectangle<int>(0, 0, 100, 100)) },
			}.ToImmutableDictionary(),
		};
		StoreTestUtils.PopulateWindowWorkspaceMap(context, root, window, activeWorkspace);

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
		IContext context,
		MutableRootSector root,
		IWindow window,
		List<object> transforms
	)
	{
		// Given
		FloatingWindowPlugin plugin = new(context);
		Workspace activeWorkspace = StoreTestUtils.CreateWorkspace(context) with
		{
			WindowPositions = new Dictionary<HWND, WindowPosition>()
			{
				{ window.Handle, new WindowPosition(WindowSize.Normal, new Rectangle<int>(0, 0, 100, 100)) },
			}.ToImmutableDictionary(),
		};
		StoreTestUtils.PopulateWindowWorkspaceMap(context, root, window, activeWorkspace);
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
