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

	private class Wrapper
	{
		public IContext Context { get; }
		public IWorkspace Workspace { get; }
		public IWorkspace OtherWorkspace { get; }
		public ICommandPalettePlugin Plugin { get; }
		public IWindow[] Windows { get; }
		public CommandPaletteCommands Commands { get; }

		public Wrapper()
		{
			Context = Substitute.For<IContext>();

			Workspace = Substitute.For<IWorkspace>();
			Workspace.Name.Returns("Workspace");

			OtherWorkspace = Substitute.For<IWorkspace>();
			OtherWorkspace.Name.Returns("Other workspace");

			Context.WorkspaceManager.ActiveWorkspace.Returns(Workspace);
			Context
				.WorkspaceManager.GetEnumerator()
				.Returns(_ => new List<IWorkspace> { Workspace, OtherWorkspace }.GetEnumerator());

			Plugin = Substitute.For<ICommandPalettePlugin>();
			Plugin.Name.Returns("whim.command_palette");

			Windows = new IWindow[3];
			Windows[0] = Substitute.For<IWindow>();
			Windows[0].Title.Returns("Window 0");
			Windows[1] = Substitute.For<IWindow>();
			Windows[1].Title.Returns("Window 1");
			Windows[2] = Substitute.For<IWindow>();
			Windows[2].Title.Returns("Window 2");

			Workspace.Windows.Returns(_ => [Windows[0], Windows[1]]);
			OtherWorkspace.Windows.Returns(_ => [Windows[2]]);

			Commands = new(Context, Plugin);
		}
	}

	private record WorkspaceWindowState(Workspace ActiveWorkspace, Workspace OtherWorkspace, IWindow[] Windows);

	private static WorkspaceWindowState SetupWorkspaces(
		IContext ctx,
		MutableRootSector root,
		ICommandPalettePlugin plugin
	)
	{
		Workspace activeWorkspace = StoreTestUtils.CreateWorkspace(ctx) with { BackingName = "Workspace" };
		Workspace otherWorkspace = StoreTestUtils.CreateWorkspace(ctx) with { BackingName = "Other workspace" };

		// Create windows for the workspaces.
		IWindow[] windows = new IWindow[3];
		windows[0] = CreateWindow(new HWND(1));
		windows[0].Title.Returns("Window 1");

		windows[1] = CreateWindow(new HWND(2));
		windows[1].Title.Returns("Window 2");

		windows[2] = CreateWindow(new HWND(3));
		windows[2].Title.Returns("Window 3");

		activeWorkspace = PopulateWindowWorkspaceMap(ctx, root, windows[0], activeWorkspace);
		activeWorkspace = PopulateWindowWorkspaceMap(ctx, root, windows[1], activeWorkspace);
		otherWorkspace = PopulateWindowWorkspaceMap(ctx, root, windows[2], otherWorkspace);

		AddActiveWorkspace(ctx, root, activeWorkspace);

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

	[Fact]
	public void ToggleCommandPalette()
	{
		// Given
		Wrapper wrapper = new();
		ICommand command = new PluginCommandsTestUtils(wrapper.Commands).GetCommand("whim.command_palette.toggle");

		// When
		command.TryExecute();

		// Then
		wrapper.Plugin.Received(1).Toggle();
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

	[Fact]
	public void CreateMoveWindowsToWorkspaceOptions()
	{
		// Given
		Wrapper wrapper = new();

		// When
		CommandPaletteCommands commands = new(wrapper.Context, wrapper.Plugin);
		SelectOption[] options = commands.CreateMoveWindowsToWorkspaceOptions();

		// Then
		Assert.Equal(3, options.Length);
		Assert.Equal("Window 0", options[0].Title);
		Assert.Equal("Window 1", options[1].Title);
		Assert.Equal("Window 2", options[2].Title);
		options.Should().OnlyContain(x => x.IsEnabled);
		options.Should().OnlyContain(x => !x.IsSelected);
	}

	[Fact]
	public void MoveMultipleWindowsToWorkspaceCreator()
	{
		// Given
		Wrapper wrapper = new();

		// When
		CommandPaletteCommands commands = new(wrapper.Context, wrapper.Plugin);
		ICommand command = commands.MoveMultipleWindowsToWorkspaceCreator(wrapper.Windows, wrapper.Workspace);

		// Then
		command.TryExecute();
		wrapper.Context.Butler.Received(1).MoveWindowToWorkspace(wrapper.Workspace, wrapper.Windows[0]);
		wrapper.Context.Butler.Received(1).MoveWindowToWorkspace(wrapper.Workspace, wrapper.Windows[1]);
	}

	[Fact]
	public void MoveMultipleWindowsToWorkspaceCallback()
	{
		// Given
		Wrapper wrapper = new();
		List<SelectOption> options =
		[
			new SelectOption
			{
				Id = "0",
				Title = "Window 0",
				IsSelected = true,
			},
			new SelectOption
			{
				Id = "1",
				Title = "Window 1",
				IsSelected = false,
			},
			new SelectOption
			{
				Id = "2",
				Title = "Window 2",
				IsSelected = true,
			},
		];

		// When
		CommandPaletteCommands commands = new(wrapper.Context, wrapper.Plugin);
		commands.MoveMultipleWindowsToWorkspaceCallback(options);

		// Then
		string[] expectedWorkspaces = ["Workspace", "Other workspace"];
		wrapper
			.Plugin.Received(1)
			.Activate(
				Arg.Is<MenuVariantConfig>(c =>
					c.Hint == "Select workspace" && c.Commands.Select(c => c.Title).SequenceEqual(expectedWorkspaces)
				)
			);
	}

	[Fact]
	public void MoveMultipleWindowsToWorkspace()
	{
		// Given
		Wrapper wrapper = new();
		ICommand command = new PluginCommandsTestUtils(wrapper.Commands).GetCommand(
			"whim.command_palette.move_multiple_windows_to_workspace"
		);

		// When
		command.TryExecute();

		// Then
		wrapper
			.Plugin.Received(1)
			.Activate(
				Arg.Is<SelectVariantConfig>(c =>
					c.Hint == "Select windows"
					&& c.Options.Select(y => y.Title).SequenceEqual(new[] { "Window 0", "Window 1", "Window 2" })
				)
			);
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

		PopulateMonitorWorkspaceMap(ctx, root, CreateMonitor(), state.ActiveWorkspace);

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
