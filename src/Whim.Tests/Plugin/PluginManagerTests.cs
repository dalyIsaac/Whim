using Moq;
using System;
using Xunit;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Reliability",
	"CA2000:Dispose objects before losing scope",
	Justification = "Unnecessary for tests"
)]
public class PluginManagerTests
{
	private class MocksWrapper
	{
		public Mock<IContext> Context { get; } = new();
		public CommandManager CommandManager { get; } = new();
		public Mock<IKeybindManager> KeybindManager { get; } = new();
		public Mock<IFileManager> FileManager { get; } = new();
		public Mock<IPlugin> Plugin1 { get; } = new();
		public Mock<IPlugin> Plugin2 { get; } = new();
		public Mock<IPlugin> Plugin3 { get; } = new();
		public PluginCommands PluginCommands1 { get; } = new("whim.plugin1");
		public PluginCommands PluginCommands2 { get; } = new("whim.plugin2");
		public PluginCommands PluginCommands3 { get; } = new("whim.plugin3");
		public string WrittenTextContents { get; private set; } = string.Empty;

		public MocksWrapper()
		{
			Context.Setup(cc => cc.CommandManager).Returns(CommandManager);
			Context.Setup(cc => cc.KeybindManager).Returns(KeybindManager.Object);
			Context.Setup(cc => cc.FileManager).Returns(FileManager.Object);

			FileManager.Setup(fm => fm.SavedStateDir).Returns("C:\\Users\\test\\.whim\\state");
			FileManager.Setup(fm => fm.OpenRead(It.IsAny<string>())).Returns(CreateSavedStateStream());
			FileManager
				.Setup(fm => fm.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
				.Callback<string, string>((filePath, contents) => WrittenTextContents = contents);

			Plugin1.Setup(p => p.Name).Returns("whim.plugin1");
			Plugin2.Setup(p => p.Name).Returns("whim.plugin2");
			Plugin3.Setup(p => p.Name).Returns("whim.plugin3");

			Plugin1.Setup(p => p.PluginCommands).Returns(PluginCommands1);
			Plugin2.Setup(p => p.PluginCommands).Returns(PluginCommands2);
			Plugin3.Setup(p => p.PluginCommands).Returns(PluginCommands3);

			JsonElement savedPluginState = JsonSerializer.SerializeToElement(new Dictionary<string, object>());
			Plugin1.Setup(p => p.SaveState()).Returns(savedPluginState);
			Plugin2.Setup(p => p.SaveState()).Returns(savedPluginState);

			Plugin1.As<IDisposable>();
			Plugin2.As<IDisposable>();

			PluginCommands1.Add("command1", "Command 1", () => { });
			PluginCommands2
				.Add("command2", "Command 2", () => { }, keybind: new Keybind(KeyModifiers.LControl, VIRTUAL_KEY.VK_A))
				.Add("command22", "Command 2.2", () => { });
			PluginCommands3.Add(
				"command3",
				"Command 3",
				() => { },
				keybind: new Keybind(KeyModifiers.LShift, VIRTUAL_KEY.VK_B)
			);
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

	[Fact]
	public void PreInitialize()
	{
		// Given
		MocksWrapper mocks = new();

		PluginManager pluginManager = new(mocks.Context.Object, mocks.CommandManager);
		pluginManager.AddPlugin(mocks.Plugin1.Object);
		pluginManager.AddPlugin(mocks.Plugin2.Object);
		pluginManager.AddPlugin(mocks.Plugin3.Object);

		// When
		pluginManager.PreInitialize();

		// Then
		mocks.Plugin1.Verify(p => p.PreInitialize(), Times.Once);
		mocks.Plugin2.Verify(p => p.PreInitialize(), Times.Once);
		mocks.Plugin3.Verify(p => p.PreInitialize(), Times.Once);
	}

	[Fact]
	public void PostInitialize_FileDoesNotExist()
	{
		// Given
		MocksWrapper mocks = new();

		PluginManager pluginManager = new(mocks.Context.Object, mocks.CommandManager);
		pluginManager.AddPlugin(mocks.Plugin1.Object);
		pluginManager.AddPlugin(mocks.Plugin2.Object);
		pluginManager.AddPlugin(mocks.Plugin3.Object);

		mocks.FileManager.Setup(fm => fm.FileExists(It.IsAny<string>())).Returns(false);

		// When
		pluginManager.PostInitialize();

		// Then
		mocks.FileManager.Verify(fm => fm.EnsureDirExists(mocks.FileManager.Object.SavedStateDir), Times.Once);
		mocks.FileManager.Verify(fm => fm.OpenRead(It.IsAny<string>()), Times.Never);
		mocks.Plugin1.Verify(p => p.LoadState(It.IsAny<JsonElement>()), Times.Never);
	}

	[Fact]
	public void PostInitialize_SavedStateIsEmpty()
	{
		// Given
		MocksWrapper mocks = new();

		PluginManager pluginManager = new(mocks.Context.Object, mocks.CommandManager);
		pluginManager.AddPlugin(mocks.Plugin1.Object);
		pluginManager.AddPlugin(mocks.Plugin2.Object);
		pluginManager.AddPlugin(mocks.Plugin3.Object);

		mocks.FileManager.Setup(fm => fm.FileExists(It.IsAny<string>())).Returns(true);
		mocks.FileManager.Setup(fm => fm.OpenRead(It.IsAny<string>())).Returns(new MemoryStream());

		// When
		pluginManager.PostInitialize();

		// Then
		mocks.FileManager.Verify(fm => fm.EnsureDirExists(mocks.FileManager.Object.SavedStateDir), Times.Once);
		mocks.FileManager.Verify(fm => fm.OpenRead(It.IsAny<string>()), Times.Once);
		mocks.Plugin1.Verify(p => p.LoadState(It.IsAny<JsonElement>()), Times.Never);
	}

	[Fact]
	public void PostInitialize_SavedStateIsNull()
	{
		// Given
		MocksWrapper mocks = new();

		PluginManager pluginManager = new(mocks.Context.Object, mocks.CommandManager);
		pluginManager.AddPlugin(mocks.Plugin1.Object);
		pluginManager.AddPlugin(mocks.Plugin2.Object);
		pluginManager.AddPlugin(mocks.Plugin3.Object);

		MemoryStream stream = new();
		stream.Write(System.Text.Encoding.ASCII.GetBytes("null"));

		mocks.FileManager.Setup(fm => fm.FileExists(It.IsAny<string>())).Returns(true);
		mocks.FileManager.Setup(fm => fm.OpenRead(It.IsAny<string>())).Returns(new MemoryStream());

		// When
		pluginManager.PostInitialize();

		// Then
		mocks.FileManager.Verify(fm => fm.EnsureDirExists(mocks.FileManager.Object.SavedStateDir), Times.Once);
		mocks.FileManager.Verify(fm => fm.OpenRead(It.IsAny<string>()), Times.Once);
		mocks.Plugin1.Verify(p => p.LoadState(It.IsAny<JsonElement>()), Times.Never);
	}

	[Fact]
	public void PostInitialize_SavedStateFileExists()
	{
		// Given
		MocksWrapper mocks = new();

		PluginManager pluginManager = new(mocks.Context.Object, mocks.CommandManager);
		pluginManager.AddPlugin(mocks.Plugin1.Object);
		pluginManager.AddPlugin(mocks.Plugin2.Object);
		pluginManager.AddPlugin(mocks.Plugin3.Object);

		mocks.FileManager.Setup(fm => fm.FileExists(It.IsAny<string>())).Returns(true);

		// When
		pluginManager.PostInitialize();

		// Then
		mocks.Plugin1.Verify(p => p.PostInitialize(), Times.Once);
		mocks.Plugin2.Verify(p => p.PostInitialize(), Times.Once);
		mocks.Plugin3.Verify(p => p.PostInitialize(), Times.Once);
		mocks.FileManager.Verify(fm => fm.EnsureDirExists(mocks.FileManager.Object.SavedStateDir), Times.Once);
		mocks.FileManager.Verify(fm => fm.OpenRead(It.IsAny<string>()), Times.Once);
		mocks.Plugin1.Verify(p => p.LoadState(It.IsAny<JsonElement>()), Times.Once);
		mocks.Plugin2.Verify(p => p.LoadState(It.IsAny<JsonElement>()), Times.Once);
	}

	[Fact]
	public void PostInitialize_Success()
	{
		// Given
		MocksWrapper mocks = new();

		PluginManager pluginManager = new(mocks.Context.Object, mocks.CommandManager);
		pluginManager.AddPlugin(mocks.Plugin1.Object);
		pluginManager.AddPlugin(mocks.Plugin2.Object);
		pluginManager.AddPlugin(mocks.Plugin3.Object);

		// When
		pluginManager.PostInitialize();

		// Then
		mocks.Plugin1.Verify(p => p.PostInitialize(), Times.Once);
		mocks.Plugin2.Verify(p => p.PostInitialize(), Times.Once);
		mocks.Plugin3.Verify(p => p.PostInitialize(), Times.Once);
	}

	[Fact]
	public void AddPlugin()
	{
		// Given
		MocksWrapper mocks = new();

		PluginManager pluginManager = new(mocks.Context.Object, mocks.CommandManager);

		// When
		pluginManager.AddPlugin(mocks.Plugin1.Object);
		pluginManager.AddPlugin(mocks.Plugin2.Object);
		pluginManager.AddPlugin(mocks.Plugin3.Object);

		// Then
		Assert.Equal(3, pluginManager.LoadedPlugins.Count);

		// I want to verify that the command passed into Add is equivalent to the one created in the mocks
		Assert.Equal(4, mocks.CommandManager.Count);
		List<ICommand> commands = mocks.CommandManager.ToList();
		Assert.Equal("whim.plugin1.command1", commands[0].Id);
		Assert.Equal("whim.plugin2.command2", commands[1].Id);
		Assert.Equal("whim.plugin2.command22", commands[2].Id);
		Assert.Equal("whim.plugin3.command3", commands[3].Id);

		mocks.KeybindManager.Verify(km => km.Add(It.IsAny<string>(), It.IsAny<Keybind>()), Times.Exactly(2));
		mocks.KeybindManager.Verify(
			km => km.Add("whim.plugin2.command2", new Keybind(KeyModifiers.LControl, VIRTUAL_KEY.VK_A)),
			Times.Once
		);
		mocks.KeybindManager.Verify(
			km => km.Add("whim.plugin3.command3", new Keybind(KeyModifiers.LShift, VIRTUAL_KEY.VK_B)),
			Times.Once
		);
	}

	[InlineData("whim.plugin1", "Plugin with name 'whim.plugin1' already exists.")]
	[InlineData("whim.custom", "Name 'whim.custom' is reserved for user-defined commands.")]
	[InlineData("whim", "Name 'whim' is reserved for internal use.")]
	[InlineData("", "Plugin name cannot be empty.")]
	[InlineData(
		"Hello world",
		"Plugin name must be in the format [first](.[second])*. For more, see the regex in PluginManager.cs."
	)]
	[InlineData(
		"whim.name.",
		"Plugin name must be in the format [first](.[second])*. For more, see the regex in PluginManager.cs."
	)]
	[InlineData(
		"whim.name..",
		"Plugin name must be in the format [first](.[second])*. For more, see the regex in PluginManager.cs."
	)]
	[InlineData(
		"whim..name",
		"Plugin name must be in the format [first](.[second])*. For more, see the regex in PluginManager.cs."
	)]
	[Theory]
	public void AddPlugin_InvalidName(string name, string expectedMessage)
	{
		// Given
		MocksWrapper mocks = new();

		mocks.Plugin2.Setup(p => p.Name).Returns(name);

		PluginManager pluginManager = new(mocks.Context.Object, mocks.CommandManager);

		// When
		pluginManager.AddPlugin(mocks.Plugin1.Object);
		Exception ex = Assert.Throws<InvalidOperationException>(() => pluginManager.AddPlugin(mocks.Plugin2.Object));

		// Then
		Assert.Equal(expectedMessage, ex.Message);
		Assert.Single(pluginManager.LoadedPlugins);
	}

	[Fact]
	public void Contains()
	{
		// Given
		MocksWrapper mocks = new();

		PluginManager pluginManager = new(mocks.Context.Object, mocks.CommandManager);
		pluginManager.AddPlugin(mocks.Plugin1.Object);
		pluginManager.AddPlugin(mocks.Plugin2.Object);
		pluginManager.AddPlugin(mocks.Plugin3.Object);

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

	[Fact]
	public void Dispose()
	{
		// Given
		MocksWrapper mocks = new();

		PluginManager pluginManager = new(mocks.Context.Object, mocks.CommandManager);
		pluginManager.AddPlugin(mocks.Plugin1.Object);
		pluginManager.AddPlugin(mocks.Plugin2.Object);
		pluginManager.AddPlugin(mocks.Plugin3.Object);

		// When
		pluginManager.Dispose();

		// Then
		mocks.FileManager.Verify(
			fm => fm.WriteAllText($"{mocks.FileManager.Object.SavedStateDir}\\plugins.json", It.IsAny<string>()),
			Times.Once
		);
		Assert.Equal("""{"Plugins":{"whim.plugin1":{},"whim.plugin2":{}}}""", mocks.WrittenTextContents);

		mocks.Plugin1.As<IDisposable>().Verify(p => p.Dispose(), Times.Once);
		mocks.Plugin2.As<IDisposable>().Verify(p => p.Dispose(), Times.Once);
	}
}
