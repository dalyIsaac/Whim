using System.Text.Json;
using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.SliceLayout.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class SliceLayoutPluginTests
{
	private class Customization : StoreCustomization
	{
		protected override void PostCustomize(IFixture fixture)
		{
			Workspace workspace = CreateWorkspace(_ctx);
			fixture.Inject(workspace);

			IMonitor monitor = CreateMonitor();
			fixture.Inject(monitor);

			MutableRootSector root = _store._root.MutableRootSector;
			PopulateMonitorWorkspaceMap(_ctx, root, monitor, workspace);
			AddActiveWorkspace(_ctx, root, workspace);
		}

		public static IWindow AddUntrackedWindow(MutableRootSector rootSector)
		{
			IWindow window = CreateWindow(new HWND(1));
			AddWindowToSector(rootSector, window);
			return window;
		}

		public static IWindow SetLastFocusedWindow(IContext ctx, MutableRootSector rootSector, Workspace workspace)
		{
			IWindow window = CreateWindow(new HWND(1));
			workspace = workspace with { LastFocusedWindowHandle = window.Handle };
			PopulateWindowWorkspaceMap(ctx, rootSector, window, workspace);
			return window;
		}
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void Name(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);

		// When
		string name = plugin.Name;

		// Then
		Assert.Equal("whim.slice_layout", name);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void PluginCommands(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);

		// When
		IPluginCommands commands = plugin.PluginCommands;

		// Then
		Assert.NotEmpty(commands.Commands);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void PreInitialize(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);

		// When
		plugin.PreInitialize();

		// Then nothing
		CustomAssert.NoContextCalls(ctx);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void PostInitialize(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);

		// When
		plugin.PostInitialize();

		// Then nothing
		CustomAssert.NoContextCalls(ctx);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void LoadState(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);

		// When
		plugin.LoadState(default);

		// Then nothing
		CustomAssert.NoContextCalls(ctx);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void SaveState(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);

		// When
		JsonElement? state = plugin.SaveState();

		// Then
		Assert.Null(state);
	}

	#region PromoteWindowInStack
	[Theory, AutoSubstituteData<Customization>]
	internal void PromoteWindowInStack_NoWindow(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);

		// When
		plugin.PromoteWindowInStack();

		// Then nothing
		Assert.DoesNotContain(ctx.GetTransforms(), t => t is LayoutEngineCustomActionTransform);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void PromoteWindowInStack_NoWorkspace(IContext ctx, MutableRootSector root)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		IWindow window = Customization.AddUntrackedWindow(root);

		// When
		plugin.PromoteWindowInStack(window);

		// Then nothing
		Assert.DoesNotContain(ctx.GetTransforms(), t => t is LayoutEngineCustomActionTransform);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void PromoteWindowInStack(IContext ctx, MutableRootSector root, Workspace workspace)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		IWindow window = Customization.SetLastFocusedWindow(ctx, root, workspace);

		// When
		plugin.PromoteWindowInStack(window);

		// Then
		LayoutEngineCustomActionTransform expectedTransform = new(
			workspace.Id,
			new LayoutEngineCustomAction { Name = plugin.PromoteWindowActionName, Window = window }
		);
		Assert.Contains(ctx.GetTransforms(), t => (t as LayoutEngineCustomActionTransform) == expectedTransform);
	}
	#endregion

	#region DemoteWindowInStack
	[Theory, AutoSubstituteData<Customization>]
	internal void DemoteWindowInStack_NoWindow(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);

		// When
		plugin.DemoteWindowInStack();

		// Then nothing
		Assert.DoesNotContain(ctx.GetTransforms(), t => t is LayoutEngineCustomActionTransform);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void DemoteWindowInStack_NoWorkspace(IContext ctx, MutableRootSector root)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		IWindow window = Customization.AddUntrackedWindow(root);

		// When
		plugin.DemoteWindowInStack(window);

		// Then nothing
		Assert.DoesNotContain(ctx.GetTransforms(), t => t is LayoutEngineCustomActionTransform);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void DemoteWindowInStack(IContext ctx, MutableRootSector root, Workspace workspace)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		IWindow window = Customization.SetLastFocusedWindow(ctx, root, workspace);

		// When
		plugin.DemoteWindowInStack(window);

		// Then
		Assert.Contains(
			ctx.GetTransforms(),
			t =>
				(t as LayoutEngineCustomActionTransform)
				== new LayoutEngineCustomActionTransform(
					workspace.Id,
					new LayoutEngineCustomAction { Name = plugin.DemoteWindowActionName, Window = window }
				)
		);
	}
	#endregion

	[Theory]
	[InlineAutoSubstituteData(WindowInsertionType.Swap)]
	[InlineAutoSubstituteData(WindowInsertionType.Rotate)]
	internal void WindowInsertionType_Set(WindowInsertionType insertionType)
	{
		// Given
		SliceLayoutPlugin plugin = new(Substitute.For<IContext>())
		{
			// When
			WindowInsertionType = insertionType,
		};

		// Then
		Assert.Equal(insertionType, plugin.WindowInsertionType);
	}

	#region PromoteFocusInStack
	[Theory, AutoSubstituteData<Customization>]
	internal void PromoteFocusInStack_NoWindow(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);

		// When
		plugin.PromoteFocusInStack();

		// Then nothing
		Assert.DoesNotContain(ctx.GetTransforms(), t => t is LayoutEngineCustomActionTransform);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void PromoteFocusInStack_NoWorkspace(IContext ctx, MutableRootSector root)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		IWindow window = Customization.AddUntrackedWindow(root);

		// When
		plugin.PromoteFocusInStack(window);

		// Then nothing
		Assert.DoesNotContain(ctx.GetTransforms(), t => t is LayoutEngineCustomActionTransform);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void PromoteFocusInStack(IContext ctx, MutableRootSector root, Workspace workspace)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		IWindow window = Customization.SetLastFocusedWindow(ctx, root, workspace);

		// When
		plugin.PromoteFocusInStack(window);

		// Then
		LayoutEngineCustomActionTransform expectedTransform = new(
			workspace.Id,
			new LayoutEngineCustomAction { Name = plugin.PromoteFocusActionName, Window = window }
		);
		Assert.Contains(ctx.GetTransforms(), t => (t as LayoutEngineCustomActionTransform) == expectedTransform);
	}
	#endregion

	#region DemoteFocusInStack
	[Theory, AutoSubstituteData<Customization>]
	internal void DemoteFocusInStack_NoWindow(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);

		// When
		plugin.DemoteFocusInStack();

		// Then nothing
		Assert.DoesNotContain(ctx.GetTransforms(), t => t is LayoutEngineCustomActionTransform);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void DemoteFocusInStack_NoWorkspace(IContext ctx, MutableRootSector root)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		IWindow window = Customization.AddUntrackedWindow(root);

		// When
		plugin.DemoteFocusInStack(window);

		// Then nothing
		Assert.DoesNotContain(ctx.GetTransforms(), t => t is LayoutEngineCustomActionTransform);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void DemoteFocusInStack(IContext ctx, MutableRootSector root, Workspace workspace)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		IWindow window = Customization.SetLastFocusedWindow(ctx, root, workspace);

		// When
		plugin.DemoteFocusInStack(window);

		// Then
		LayoutEngineCustomActionTransform expectedTransform = new(
			workspace.Id,
			new LayoutEngineCustomAction { Name = plugin.DemoteFocusActionName, Window = window }
		);
		Assert.Contains(ctx.GetTransforms(), t => (t as LayoutEngineCustomActionTransform) == expectedTransform);
	}
	#endregion
}
