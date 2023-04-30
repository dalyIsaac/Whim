using Moq;
using System;
using Xunit;

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
		public Mock<IPlugin> Plugin1 { get; } = new();
		public Mock<IPlugin> Plugin2 { get; } = new();
		public Mock<IPlugin> Plugin3 { get; } = new();

		public MocksWrapper()
		{
			Context.Setup(cc => cc.CommandManager).Returns(CommandManager.Object);

			Plugin1.Setup(p => p.Name).Returns("Plugin1");
			Plugin2.Setup(p => p.Name).Returns("Plugin2");
			Plugin3.Setup(p => p.Name).Returns("Plugin3");

			Plugin1.Setup(p => p.PluginCommands).Returns(new PluginCommands("Plugin1"));
			Plugin2.Setup(p => p.PluginCommands).Returns(new PluginCommands("Plugin2"));
			Plugin3.Setup(p => p.PluginCommands).Returns(new PluginCommands("Plugin3"));
		}
	}

	[Fact]
	public void PreInitialize()
	{
		// Given
		MocksWrapper mocks = new();

		PluginManager pluginManager = new(mocks.Context.Object);
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

		PluginManager pluginManager = new(mocks.Context.Object);
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

		PluginManager pluginManager = new(mocks.Context.Object);

		// When
		pluginManager.AddPlugin(mocks.Plugin1.Object);
		pluginManager.AddPlugin(mocks.Plugin2.Object);
		pluginManager.AddPlugin(mocks.Plugin3.Object);

		// Then
		Assert.Equal(3, pluginManager.LoadedPlugins.Count);
	}

	[Fact]
	public void AddPlugin_DuplicateName()
	{
		// Given
		MocksWrapper mocks = new();

		mocks.Plugin2.Setup(p => p.Name).Returns("Plugin1");

		PluginManager pluginManager = new(mocks.Context.Object);

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

		PluginManager pluginManager = new(mocks.Context.Object);
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
