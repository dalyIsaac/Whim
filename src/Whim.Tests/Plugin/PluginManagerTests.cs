using Moq;
using System;
using Xunit;
using Windows.Win32.UI.Input.KeyboardAndMouse;

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
		public Mock<ICommandManager> CommandManager { get; } = new();
		public Mock<IKeybindManager> KeybindManager { get; } = new();
		public Mock<IFileManager> FileManager { get; } = new();
		public Mock<IPlugin> Plugin1 { get; } = new();
		public Mock<IPlugin> Plugin2 { get; } = new();
		public Mock<IPlugin> Plugin3 { get; } = new();
		public PluginCommands PluginCommands1 { get; } = new("Plugin1");
		public PluginCommands PluginCommands2 { get; } = new("Plugin2");
		public PluginCommands PluginCommands3 { get; } = new("Plugin3");

		public MocksWrapper()
		{
			Context.Setup(cc => cc.CommandManager).Returns(CommandManager.Object);
			Context.Setup(cc => cc.KeybindManager).Returns(KeybindManager.Object);

			FileManager.Setup(fm => fm.SavedStateDir).Returns("C:\\Users\\test\\.whim\\state");

			Plugin1.Setup(p => p.Name).Returns("Plugin1");
			Plugin2.Setup(p => p.Name).Returns("Plugin2");
			Plugin3.Setup(p => p.Name).Returns("Plugin3");

			Plugin1.Setup(p => p.PluginCommands).Returns(PluginCommands1);
			Plugin2.Setup(p => p.PluginCommands).Returns(PluginCommands2);
			Plugin3.Setup(p => p.PluginCommands).Returns(PluginCommands3);

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
	}

	[Fact]
	public void PreInitialize()
	{
		// Given
		MocksWrapper mocks = new();

		PluginManager pluginManager = new(mocks.Context.Object, mocks.FileManager.Object);
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
	public void PostInitialize()
	{
		// Given
		MocksWrapper mocks = new();

		PluginManager pluginManager = new(mocks.Context.Object, mocks.FileManager.Object);
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

		PluginManager pluginManager = new(mocks.Context.Object, mocks.FileManager.Object);

		// When
		pluginManager.AddPlugin(mocks.Plugin1.Object);
		pluginManager.AddPlugin(mocks.Plugin2.Object);
		pluginManager.AddPlugin(mocks.Plugin3.Object);

		// Then
		Assert.Equal(3, pluginManager.LoadedPlugins.Count);

		// mocks.CommandManager.Verify(cm => cm.Add()
		// I want to verify that the command passed into Add is equivalent to the one created in the mocks
		mocks.CommandManager.Verify(cm => cm.Add(It.IsAny<ICommand>()), Times.Exactly(4));
		mocks.CommandManager.Verify(cm => cm.Add(It.Is<ICommand>(c => c.Id == "Plugin1.command1")), Times.Once);
		mocks.CommandManager.Verify(cm => cm.Add(It.Is<ICommand>(c => c.Id == "Plugin2.command2")), Times.Once);
		mocks.CommandManager.Verify(cm => cm.Add(It.Is<ICommand>(c => c.Id == "Plugin2.command22")), Times.Once);
		mocks.CommandManager.Verify(cm => cm.Add(It.Is<ICommand>(c => c.Id == "Plugin3.command3")), Times.Once);

		mocks.KeybindManager.Verify(km => km.Add(It.IsAny<string>(), It.IsAny<Keybind>()), Times.Exactly(2));
		mocks.KeybindManager.Verify(
			km => km.Add("Plugin2.command2", new Keybind(KeyModifiers.LControl, VIRTUAL_KEY.VK_A)),
			Times.Once
		);
		mocks.KeybindManager.Verify(
			km => km.Add("Plugin3.command3", new Keybind(KeyModifiers.LShift, VIRTUAL_KEY.VK_B)),
			Times.Once
		);
	}

	[Fact]
	public void AddPlugin_DuplicateName()
	{
		// Given
		MocksWrapper mocks = new();

		mocks.Plugin2.Setup(p => p.Name).Returns("Plugin1");

		PluginManager pluginManager = new(mocks.Context.Object, mocks.FileManager.Object);

		// When
		pluginManager.AddPlugin(mocks.Plugin1.Object);
		Assert.Throws<InvalidOperationException>(() => pluginManager.AddPlugin(mocks.Plugin2.Object));

		// Then
		Assert.Equal(1, pluginManager.LoadedPlugins.Count);
	}

	[Fact]
	public void Contains()
	{
		// Given
		MocksWrapper mocks = new();

		PluginManager pluginManager = new(mocks.Context.Object, mocks.FileManager.Object);
		pluginManager.AddPlugin(mocks.Plugin1.Object);
		pluginManager.AddPlugin(mocks.Plugin2.Object);
		pluginManager.AddPlugin(mocks.Plugin3.Object);

		// When
		bool contains1 = pluginManager.Contains("Plugin1");
		bool contains2 = pluginManager.Contains("Plugin2");
		bool contains3 = pluginManager.Contains("Plugin3");
		bool contains4 = pluginManager.Contains("Plugin4");

		// Then
		Assert.True(contains1);
		Assert.True(contains2);
		Assert.True(contains3);
		Assert.False(contains4);
	}
}
