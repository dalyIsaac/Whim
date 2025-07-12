using AutoFixture;
using FluentAssertions;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.CommandPalette.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class CommandPaletteCommandsTests
{
	private class Customization : StoreCustomization
	{
		protected override void PostCustomize(IFixture fixture)
		{
			ICommandPalettePlugin plugin = fixture.Freeze<ICommandPalettePlugin>();
			plugin.Name.Returns("whim.command_palette");

			CommandPaletteCommands commands = new(_ctx, plugin);
			fixture.Inject(commands);
		}
	}

	private class InterceptConfig<TConfig>
		where TConfig : BaseVariantConfig
	{
		public TConfig Config { get; private set; } = default!;

		private InterceptConfig(ICommandPalettePlugin plugin)
		{
			plugin.Activate(Arg.Do<TConfig>(c => Config = c));
		}

		public static InterceptConfig<TConfig> Create(ICommandPalettePlugin plugin) => new(plugin);
	}

	private record WorkspaceWindowState(Workspace ActiveWorkspace, Workspace OtherWorkspace, IWindow[] Windows);

	private static WorkspaceWindowState SetupWorkspaces(
		IContext ctx,
		MutableRootSector root,
		ICommandPalettePlugin plugin
	)
	{
		Workspace activeWorkspace = StoreTestUtils.CreateWorkspace() with { Name = "Workspace" };
		Workspace otherWorkspace = StoreTestUtils.CreateWorkspace() with { Name = "Other workspace" };

		// Create windows for the workspaces.
		IWindow[] windows = new IWindow[3];
		windows[0] = CreateWindow(new HWND(1));
		windows[0].Title.Returns("Window 1");

		windows[1] = CreateWindow(new HWND(2));
		windows[1].Title.Returns("Window 2");

		windows[2] = CreateWindow(new HWND(3));
		windows[2].Title.Returns("Window 3");

		activeWorkspace = PopulateWindowWorkspaceMap(root, windows[0], activeWorkspace);
		activeWorkspace = PopulateWindowWorkspaceMap(root, windows[1], activeWorkspace);
		otherWorkspace = PopulateWindowWorkspaceMap(root, windows[2], otherWorkspace);

		AddActiveWorkspaceToStore(root, activeWorkspace);

		return new WorkspaceWindowState(activeWorkspace, otherWorkspace, windows);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void ActivateWorkspace(
		IContext ctx,
		MutableRootSector root,
		ICommandPalettePlugin plugin,
		CommandPaletteCommands commands
	)
	{
		// Given
		ICommand command = new PluginCommandsTestUtils(commands).GetCommand("whim.command_palette.activate_workspace");
		InterceptConfig<MenuVariantConfig> interceptor = InterceptConfig<MenuVariantConfig>.Create(plugin);

		WorkspaceWindowState state = SetupWorkspaces(ctx, root, plugin);

		command.TryExecute();

		// Call the callback.
		ICommand activeWorkspaceCommand = interceptor.Config.Commands.First();
		activeWorkspaceCommand.TryExecute();

		// Verify that the workspace was activated.
		Assert.Contains(
			ctx.GetTransforms(),
			t => (t as ActivateWorkspaceTransform) == new ActivateWorkspaceTransform(state.OtherWorkspace.Id)
		);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void ToggleCommandPalette(ICommandPalettePlugin plugin, CommandPaletteCommands commands)
	{
		// Given
		ICommand command = new PluginCommandsTestUtils(commands).GetCommand("whim.command_palette.toggle");

		// When
		command.TryExecute();

		// Then
		plugin.Received(1).Toggle();
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void RenameWorkspace(
		IContext ctx,
		MutableRootSector root,
		ICommandPalettePlugin plugin,
		CommandPaletteCommands commands
	)
	{
		// Given
		ICommand command = new PluginCommandsTestUtils(commands).GetCommand("whim.command_palette.rename_workspace");
		InterceptConfig<FreeTextVariantConfig> interceptor = InterceptConfig<FreeTextVariantConfig>.Create(plugin);

		SetupWorkspaces(ctx, root, plugin);

		command.TryExecute();

		// Verify that the plugin was activated
		plugin.Received(1).Activate(Arg.Any<FreeTextVariantConfig>());

		// Call the callback
		interceptor.Config!.Callback("New workspace name");

		// Verify that the workspace was renamed
		Assert.Contains(
			ctx.GetTransforms(),
			t =>
				(t as SetWorkspaceNameTransform)
				== new SetWorkspaceNameTransform(ctx.Store.Pick(Pickers.PickActiveWorkspaceId()), "New workspace name")
		);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void CreateWorkspace(
		IContext ctx,
		MutableRootSector root,
		ICommandPalettePlugin plugin,
		CommandPaletteCommands commands
	)
	{
		// Given
		ICommand command = new PluginCommandsTestUtils(commands).GetCommand("whim.command_palette.create_workspace");
		InterceptConfig<FreeTextVariantConfig> interceptor = InterceptConfig<FreeTextVariantConfig>.Create(plugin);

		WorkspaceWindowState state = SetupWorkspaces(ctx, root, plugin);

		command.TryExecute();

		// Call the callback.
		interceptor.Config!.Callback("New workspace name");

		// Verify that the workspace was created.
		Assert.Contains(
			ctx.GetTransforms(),
			t => (t as AddWorkspaceTransform) == new AddWorkspaceTransform("New workspace name")
		);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void MoveWindowToWorkspaceCommand(
		IContext ctx,
		MutableRootSector root,
		ICommandPalettePlugin plugin,
		CommandPaletteCommands commands
	)
	{
		// Given
		ICommand command = new PluginCommandsTestUtils(commands).GetCommand(
			"whim.command_palette.move_window_to_workspace"
		);
		InterceptConfig<MenuVariantConfig> interceptor = InterceptConfig<MenuVariantConfig>.Create(plugin);
		WorkspaceWindowState state = SetupWorkspaces(ctx, root, plugin);

		// When
		command.TryExecute();

		// Call the callback.
		ICommand moveWindowCommand = interceptor.Config!.Commands.First();
		moveWindowCommand.TryExecute();

		// Verify that MoveWindowToWorkspace was called with the workspace.
		Assert.Contains(
			ctx.GetTransforms(),
			t => (t as MoveWindowToWorkspaceTransform) == new MoveWindowToWorkspaceTransform(state.OtherWorkspace.Id)
		);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void CreateMoveWindowsToWorkspaceOptions(
		IContext ctx,
		MutableRootSector root,
		ICommandPalettePlugin plugin,
		CommandPaletteCommands commands
	)
	{
		// Given
		SetupWorkspaces(ctx, root, plugin);

		// When
		SelectOption[] options = commands.CreateMoveWindowsToWorkspaceOptions();

		// Then
		Assert.Equal(3, options.Length);
		Assert.Equal("Window 1", options[0].Title);
		Assert.Equal("Window 2", options[1].Title);
		Assert.Equal("Window 3", options[2].Title);
		options.Should().OnlyContain(x => x.IsEnabled);
		options.Should().OnlyContain(x => !x.IsSelected);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void MoveMultipleWindowsToWorkspaceCreator(
		IContext ctx,
		MutableRootSector root,
		ICommandPalettePlugin plugin
	)
	{
		// Given
		WorkspaceWindowState state = SetupWorkspaces(ctx, root, plugin);
		CommandPaletteCommands commands = new(ctx, plugin);
		ICommand command = commands.MoveMultipleWindowsToWorkspaceCreator(state.Windows, state.OtherWorkspace);

		// When
		command.TryExecute();

		// Then
		Assert.Contains(
			ctx.GetTransforms(),
			t =>
				(t as MoveWindowToWorkspaceTransform)
				== new MoveWindowToWorkspaceTransform(state.OtherWorkspace.Id, state.Windows[0].Handle)
		);
		Assert.Contains(
			ctx.GetTransforms(),
			t =>
				(t as MoveWindowToWorkspaceTransform)
				== new MoveWindowToWorkspaceTransform(state.OtherWorkspace.Id, state.Windows[1].Handle)
		);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void MoveMultipleWindowsToWorkspaceCallback(
		IContext ctx,
		MutableRootSector root,
		ICommandPalettePlugin plugin,
		CommandPaletteCommands commands
	)
	{
		// Given
		List<SelectOption> options =
		[
			new SelectOption
			{
				Id = "0",
				Title = "Window 1",
				IsSelected = true,
			},
			new SelectOption
			{
				Id = "1",
				Title = "Window 2",
				IsSelected = false,
			},
			new SelectOption
			{
				Id = "2",
				Title = "Window 3",
				IsSelected = true,
			},
		];
		InterceptConfig<MenuVariantConfig> wrapper = InterceptConfig<MenuVariantConfig>.Create(plugin);
		SetupWorkspaces(ctx, root, plugin);

		// When
		commands.MoveMultipleWindowsToWorkspaceCallback(options);

		// Then
		string[] expectedWorkspaces = ["Workspace", "Other workspace"];
		Assert.Equal("Select workspace", wrapper.Config.Hint);
		Assert.Equal(2, wrapper.Config.Commands.Count());
		wrapper.Config.Commands.Select(c => c.Title).Should().Equal(expectedWorkspaces);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void MoveMultipleWindowsToWorkspace(
		IContext ctx,
		MutableRootSector root,
		ICommandPalettePlugin plugin,
		CommandPaletteCommands commands
	)
	{
		// Given
		SetupWorkspaces(ctx, root, plugin);
		ICommand command = new PluginCommandsTestUtils(commands).GetCommand(
			"whim.command_palette.move_multiple_windows_to_workspace"
		);
		InterceptConfig<SelectVariantConfig> interceptor = InterceptConfig<SelectVariantConfig>.Create(plugin);

		// When
		command.TryExecute();

		// Then
		Assert.Equal("Select windows", interceptor.Config.Hint);
		Assert.Equal(3, interceptor.Config.Options.Count());
		interceptor.Config.Options.Select(o => o.Title).Should().Equal(["Window 1", "Window 2", "Window 3"]);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void RemoveWindow(
		IContext ctx,
		MutableRootSector mutableRootSector,
		ICommandPalettePlugin plugin,
		CommandPaletteCommands commands
	)
	{
		// Given
		ICommand command = new PluginCommandsTestUtils(commands).GetCommand("whim.command_palette.remove_window");
		InterceptConfig<MenuVariantConfig> interceptor = InterceptConfig<MenuVariantConfig>.Create(plugin);
		WorkspaceWindowState state = SetupWorkspaces(ctx, mutableRootSector, plugin);

		// When
		command.TryExecute();

		// Call the callback.
		ICommand removeWindowCommand = interceptor.Config!.Commands.First();
		removeWindowCommand.TryExecute();

		// Verify that the window was removed from the workspace.
		Assert.Contains(
			ctx.GetTransforms(),
			t =>
				(t as RemoveWindowFromWorkspaceTransform)
				== new RemoveWindowFromWorkspaceTransform(state.ActiveWorkspace.Id, state.Windows[0])
		);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void FindFocusWindow(
		IContext ctx,
		MutableRootSector root,
		ICommandPalettePlugin plugin,
		CommandPaletteCommands commands
	)
	{
		// Given
		ICommand command = new PluginCommandsTestUtils(commands).GetCommand("whim.command_palette.find_focus_window");
		InterceptConfig<MenuVariantConfig> wrapper = InterceptConfig<MenuVariantConfig>.Create(plugin);
		WorkspaceWindowState state = SetupWorkspaces(ctx, root, plugin);

		// When
		command.TryExecute();

		// Call the callback.
		ICommand findFocusWindowCommand = wrapper.Config!.Commands.First();
		findFocusWindowCommand.TryExecute();

		// Then
		plugin
			.Received(1)
			.Activate(
				Arg.Is<MenuVariantConfig>(c =>
					c.Hint == "Find window"
					&& c.ConfirmButtonText == "Focus"
					&& c.Commands.Select(c => c.Title).SequenceEqual(state.Windows.Select(w => w.Title))
				)
			);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void FocusWindowCommandCreator_CannotFindWorkspace(
		IContext ctx,
		MutableRootSector root,
		ICommandPalettePlugin plugin,
		CommandPaletteCommands commands
	)
	{
		// Given the window is not in a workspace.
		WorkspaceWindowState state = SetupWorkspaces(ctx, root, plugin);
		IWindow window = CreateWindow(new HWND(99));

		// When the command is executed.
		ICommand command = commands.FocusWindowCommandCreator(window);
		command.TryExecute();

		// Then the window is not focused.
		Assert.DoesNotContain(
			ctx.GetTransforms(),
			t => (t as DoWorkspaceLayoutTransform) == new DoWorkspaceLayoutTransform(state.ActiveWorkspace.Id)
		);
		window.DidNotReceive().Restore();
		window.DidNotReceive().Focus();
	}

	[Theory]
	[InlineAutoSubstituteData<Customization>(false, 0, 1)]
	[InlineAutoSubstituteData<Customization>(true, 1, 1)]
	internal void FocusWindowCommandCreator_WorkspaceIsVisible(
		bool isMinimized,
		int restoredCount,
		int focusedCount,
		IContext ctx,
		MutableRootSector root,
		ICommandPalettePlugin plugin,
		CommandPaletteCommands commands
	)
	{
		// Given the window is in a workspace.
		WorkspaceWindowState state = SetupWorkspaces(ctx, root, plugin);

		IWindow window = state.Windows[0];
		window.IsMinimized.Returns(isMinimized);

		PopulateMonitorWorkspaceMap(root, CreateMonitor(), state.ActiveWorkspace);

		// When the command is executed.
		ICommand command = commands.FocusWindowCommandCreator(window);
		command.TryExecute();

		// Then the window is focused.
		window.Received(restoredCount).Restore();
		window.Received(focusedCount).Focus();
	}

	[Theory]
	[InlineAutoSubstituteData<Customization>(false, 0, 1)]
	[InlineAutoSubstituteData<Customization>(true, 1, 1)]
	internal void FocusWindowCommandCreator_WorkspaceIsNotVisible(
		bool isMinimized,
		int restoredCount,
		int focusedCount,
		IContext ctx,
		MutableRootSector root,
		ICommandPalettePlugin plugin,
		CommandPaletteCommands commands
	)
	{
		// Given the window is in a workspace.
		WorkspaceWindowState state = SetupWorkspaces(ctx, root, plugin);

		// Get the window from the not visible workspace (the third window, which is in the other workspace).
		IWindow window = state.Windows[2];
		window.IsMinimized.Returns(isMinimized);

		// When the command is executed.
		ICommand command = commands.FocusWindowCommandCreator(window);
		command.TryExecute();

		// Then the window is focused.
		window.Received(restoredCount).Restore();
		window.Received(focusedCount).Focus();
	}
}
