using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Xunit;

namespace Whim.Tests;

public class PluginManagerCustomization : ICustomization
{
	public const string SavedStateDir = "C:\\Users\\test\\.whim\\state";

	public void Customize(IFixture fixture)
	{
		IContext ctx = fixture.Freeze<IContext>();

		ctx.FileManager.SavedStateDir.Returns(SavedStateDir);
		ctx.FileManager.OpenRead(Arg.Any<string>()).Returns(CreateSavedStateStream());
	}

	private static MemoryStream CreateSavedStateStream()
	{
		PluginManagerSavedState savedState = new();
		savedState.Plugins.Add("whim.plugin1", JsonSerializer.SerializeToElement(new Dictionary<string, object>()));
		savedState.Plugins.Add("whim.plugin2", JsonSerializer.SerializeToElement(new Dictionary<string, object>()));

		MemoryStream stream = new();
		stream.Write(JsonSerializer.SerializeToUtf8Bytes(savedState));
		stream.Position = 0;

		return stream;
	}
}

[System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Reliability",
	"CA2000:Dispose objects before losing scope",
	Justification = "Unnecessary for tests"
)]
public class PluginManagerTests
{
	private static (IPlugin, IPlugin, IPlugin) CreatePlugins()
	{
		PluginCommands pluginCommands1 = new("whim.plugin1");
		PluginCommands pluginCommands2 = new("whim.plugin2");
		PluginCommands pluginCommands3 = new("whim.plugin3");

		pluginCommands1.Add("command1", "Command 1", () => { });
		pluginCommands2
			.Add("command2", "Command 2", () => { }, keybind: new Keybind(KeyModifiers.LControl, VIRTUAL_KEY.VK_A))
			.Add("command22", "Command 2.2", () => { });
		pluginCommands3.Add(
			"command3",
			"Command 3",
			() => { },
			keybind: new Keybind(KeyModifiers.LShift, VIRTUAL_KEY.VK_B)
		);

		IPlugin plugin1 = Substitute.For<IPlugin, IDisposable>();
		IPlugin plugin2 = Substitute.For<IPlugin, IDisposable>();
		IPlugin plugin3 = Substitute.For<IPlugin>();

		plugin1.Name.Returns("whim.plugin1");
		plugin2.Name.Returns("whim.plugin2");
		plugin3.Name.Returns("whim.plugin3");

		plugin1.PluginCommands.Returns(pluginCommands1);
		plugin2.PluginCommands.Returns(pluginCommands2);
		plugin3.PluginCommands.Returns(pluginCommands3);

		JsonElement savedPluginState = JsonSerializer.SerializeToElement(new Dictionary<string, object>());
		plugin1.SaveState().Returns(savedPluginState);
		plugin2.SaveState().Returns(savedPluginState);

		return (plugin1, plugin2, plugin3);
	}

	[Theory, AutoSubstituteData<PluginManagerCustomization>]
	internal void PreInitialize(IContext ctx, CommandManager commandManager)
	{
		// Given
		(IPlugin plugin1, IPlugin plugin2, IPlugin plugin3) = CreatePlugins();
		PluginManager pluginManager = new(ctx, commandManager);
		pluginManager.AddPlugin(plugin1);
		pluginManager.AddPlugin(plugin2);
		pluginManager.AddPlugin(plugin3);

		// When
		pluginManager.PreInitialize();

		// Then
		plugin1.Received(1).PreInitialize();
		plugin2.Received(1).PreInitialize();
		plugin3.Received(1).PreInitialize();
	}

	[Theory, AutoSubstituteData<PluginManagerCustomization>]
	internal void PostInitialize_FileDoesNotExist(IContext ctx, CommandManager commandManager)
	{
		// Given
		(IPlugin plugin1, IPlugin plugin2, IPlugin plugin3) = CreatePlugins();
		PluginManager pluginManager = new(ctx, commandManager);
		pluginManager.AddPlugin(plugin1);
		pluginManager.AddPlugin(plugin2);
		pluginManager.AddPlugin(plugin3);

		ctx.FileManager.FileExists(Arg.Any<string>()).Returns(false);

		// When
		pluginManager.PostInitialize();

		// Then
		ctx.FileManager.Received(1).EnsureDirExists(PluginManagerCustomization.SavedStateDir);
		ctx.FileManager.DidNotReceive().OpenRead(Arg.Any<string>());
		plugin1.DidNotReceive().LoadState(Arg.Any<JsonElement>());
	}

	[Theory, AutoSubstituteData<PluginManagerCustomization>]
	internal void PostInitialize_SavedStateIsEmpty(IContext ctx, CommandManager commandManager)
	{
		// Given
		(IPlugin plugin1, IPlugin plugin2, IPlugin plugin3) = CreatePlugins();
		PluginManager pluginManager = new(ctx, commandManager);
		pluginManager.AddPlugin(plugin1);
		pluginManager.AddPlugin(plugin2);
		pluginManager.AddPlugin(plugin3);

		ctx.FileManager.FileExists(Arg.Any<string>()).Returns(true);
		ctx.FileManager.OpenRead(Arg.Any<string>()).Returns(new MemoryStream());

		// When
		pluginManager.PostInitialize();

		// Then
		ctx.FileManager.Received(1).EnsureDirExists(PluginManagerCustomization.SavedStateDir);
		ctx.FileManager.Received(1).OpenRead(Arg.Any<string>());
		plugin1.DidNotReceive().LoadState(Arg.Any<JsonElement>());
	}

	[Theory, AutoSubstituteData<PluginManagerCustomization>]
	internal void PostInitialize_SavedStateIsNull(IContext ctx, CommandManager commandManager)
	{
		// Given
		(IPlugin plugin1, IPlugin plugin2, IPlugin plugin3) = CreatePlugins();
		PluginManager pluginManager = new(ctx, commandManager);
		pluginManager.AddPlugin(plugin1);
		pluginManager.AddPlugin(plugin2);
		pluginManager.AddPlugin(plugin3);

		MemoryStream stream = new();
		stream.Write(System.Text.Encoding.ASCII.GetBytes("null"));

		ctx.FileManager.FileExists(Arg.Any<string>()).Returns(true);
		ctx.FileManager.OpenRead(Arg.Any<string>()).Returns(new MemoryStream());

		// When
		pluginManager.PostInitialize();

		// Then
		ctx.FileManager.Received(1).EnsureDirExists(PluginManagerCustomization.SavedStateDir);
		ctx.FileManager.Received(1).OpenRead(Arg.Any<string>());
		plugin1.DidNotReceive().LoadState(Arg.Any<JsonElement>());
	}

	[Theory, AutoSubstituteData<PluginManagerCustomization>]
	internal void PostInitialize_SavedStateFileExists(IContext ctx, CommandManager commandManager)
	{
		// Given
		(IPlugin plugin1, IPlugin plugin2, IPlugin plugin3) = CreatePlugins();
		PluginManager pluginManager = new(ctx, commandManager);
		pluginManager.AddPlugin(plugin1);
		pluginManager.AddPlugin(plugin2);
		pluginManager.AddPlugin(plugin3);

		ctx.FileManager.FileExists(Arg.Any<string>()).Returns(true);

		// When
		pluginManager.PostInitialize();

		// Then
		plugin1.Received(1).PostInitialize();
		plugin2.Received(1).PostInitialize();
		plugin3.Received(1).PostInitialize();
		ctx.FileManager.Received(1).EnsureDirExists(PluginManagerCustomization.SavedStateDir);
		ctx.FileManager.Received(1).OpenRead(Arg.Any<string>());
		plugin1.Received(1).LoadState(Arg.Any<JsonElement>());
		plugin2.Received(1).LoadState(Arg.Any<JsonElement>());
	}

	[Theory, AutoSubstituteData<PluginManagerCustomization>]
	internal void PostInitialize_Success(IContext ctx, CommandManager commandManager)
	{
		// Given
		(IPlugin plugin1, IPlugin plugin2, IPlugin plugin3) = CreatePlugins();
		PluginManager pluginManager = new(ctx, commandManager);
		pluginManager.AddPlugin(plugin1);
		pluginManager.AddPlugin(plugin2);
		pluginManager.AddPlugin(plugin3);

		// When
		pluginManager.PostInitialize();

		// Then
		plugin1.Received(1).PostInitialize();
		plugin2.Received(1).PostInitialize();
		plugin3.Received(1).PostInitialize();
	}

	[Theory, AutoSubstituteData<PluginManagerCustomization>]
	internal void AddPlugin(IContext ctx, CommandManager commandManager)
	{
		// Given
		(IPlugin plugin1, IPlugin plugin2, IPlugin plugin3) = CreatePlugins();
		PluginManager pluginManager = new(ctx, commandManager);

		// When
		pluginManager.AddPlugin(plugin1);
		pluginManager.AddPlugin(plugin2);
		pluginManager.AddPlugin(plugin3);

		// Then
		Assert.Equal(3, pluginManager.LoadedPlugins.Count);

		// I want to verify that the command passed into Add is equivalent to the one created in the mocks
		Assert.Equal(4, commandManager.Count);
		List<ICommand> commands = commandManager.ToList();
		Assert.Equal("whim.plugin1.command1", commands[0].Id);
		Assert.Equal("whim.plugin2.command2", commands[1].Id);
		Assert.Equal("whim.plugin2.command22", commands[2].Id);
		Assert.Equal("whim.plugin3.command3", commands[3].Id);

		ctx.KeybindManager.Received(2).SetKeybind(Arg.Any<string>(), Arg.Any<Keybind>());
		ctx.KeybindManager
			.Received(1)
			.SetKeybind("whim.plugin2.command2", new Keybind(KeyModifiers.LControl, VIRTUAL_KEY.VK_A));
		ctx.KeybindManager
			.Received(1)
			.SetKeybind("whim.plugin3.command3", new Keybind(KeyModifiers.LShift, VIRTUAL_KEY.VK_B));
	}

	[InlineAutoSubstituteData<PluginManagerCustomization>(
		"whim.plugin1",
		"Plugin with name 'whim.plugin1' already exists."
	)]
	[InlineAutoSubstituteData<PluginManagerCustomization>(
		"whim.custom",
		"Name 'whim.custom' is reserved for user-defined commands."
	)]
	[InlineAutoSubstituteData<PluginManagerCustomization>("whim", "Name 'whim' is reserved for internal use.")]
	[InlineAutoSubstituteData<PluginManagerCustomization>("", "Plugin name cannot be empty.")]
	[InlineAutoSubstituteData<PluginManagerCustomization>(
		"Hello world",
		"Plugin name must be in the format [first](.[second])*. For more, see the regex in PluginManager.cs."
	)]
	[InlineAutoSubstituteData<PluginManagerCustomization>(
		"whim.name.",
		"Plugin name must be in the format [first](.[second])*. For more, see the regex in PluginManager.cs."
	)]
	[InlineAutoSubstituteData<PluginManagerCustomization>(
		"whim.name..",
		"Plugin name must be in the format [first](.[second])*. For more, see the regex in PluginManager.cs."
	)]
	[InlineAutoSubstituteData<PluginManagerCustomization>(
		"whim..name",
		"Plugin name must be in the format [first](.[second])*. For more, see the regex in PluginManager.cs."
	)]
	[Theory]
	internal void AddPlugin_InvalidName(
		string name,
		string expectedMessage,
		IContext ctx,
		CommandManager commandManager
	)
	{
		// Given
		(IPlugin plugin1, IPlugin plugin2, _) = CreatePlugins();
		plugin2.Name.Returns(name);

		PluginManager pluginManager = new(ctx, commandManager);

		// When
		pluginManager.AddPlugin(plugin1);
		Exception ex = Assert.Throws<InvalidOperationException>(() => pluginManager.AddPlugin(plugin2));

		// Then
		Assert.Equal(expectedMessage, ex.Message);
		Assert.Single(pluginManager.LoadedPlugins);
	}

	[Theory, AutoSubstituteData<PluginManagerCustomization>]
	internal void Contains(IContext ctx, CommandManager commandManager)
	{
		// Given
		(IPlugin plugin1, IPlugin plugin2, IPlugin plugin3) = CreatePlugins();
		PluginManager pluginManager = new(ctx, commandManager);
		pluginManager.AddPlugin(plugin1);
		pluginManager.AddPlugin(plugin2);
		pluginManager.AddPlugin(plugin3);

		// When
		bool contains1 = pluginManager.Contains("whim.plugin1");
		bool contains2 = pluginManager.Contains("whim.plugin2");
		bool contains3 = pluginManager.Contains("whim.plugin3");
		bool contains4 = pluginManager.Contains("whim.plugin4");

		// Then
		Assert.True(contains1);
		Assert.True(contains2);
		Assert.True(contains3);
		Assert.False(contains4);
	}

	[Theory, AutoSubstituteData<PluginManagerCustomization>]
	internal void Dispose(IContext ctx, CommandManager commandManager)
	{
		// Given
		(IPlugin plugin1, IPlugin plugin2, IPlugin plugin3) = CreatePlugins();
		string writtenTextContents = "";
		ctx.FileManager
			.WhenForAnyArgs(fm => fm.WriteAllText(Arg.Any<string>(), Arg.Any<string>()))
			.Do(call =>
			{
				writtenTextContents = call.ArgAt<string>(1);
			});

		PluginManager pluginManager = new(ctx, commandManager);
		pluginManager.AddPlugin(plugin1);
		pluginManager.AddPlugin(plugin2);
		pluginManager.AddPlugin(plugin3);

		// When
		pluginManager.Dispose();

		// Then
		ctx.FileManager
			.Received(1)
			.WriteAllText($"{PluginManagerCustomization.SavedStateDir}\\plugins.json", Arg.Any<string>());
		Assert.Equal("""{"Plugins":{"whim.plugin1":{},"whim.plugin2":{}}}""", writtenTextContents);

		((IDisposable)plugin1).Received(1).Dispose();
		((IDisposable)plugin2).Received(1).Dispose();
	}
}
