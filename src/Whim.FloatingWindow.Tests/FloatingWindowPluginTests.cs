using System.Text.Json;
using Whim.TestUtils;
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
	public void PreInitialize(IContext context, List<object> transforms)
	{
		// Given
		FloatingWindowPlugin plugin = CreateSut(context);

		// When
		plugin.PreInitialize();

		// Then
		Assert.IsType<AddProxyLayoutEngineTransform>(transforms[0]);
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
