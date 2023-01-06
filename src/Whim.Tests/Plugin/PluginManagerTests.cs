using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Reliability",
	"CA2000:Dispose objects before losing scope",
	Justification = "Unnecessary for tests"
)]
public class PluginManagerTests
{
	private static (
		Mock<IConfigContext>,
		Mock<ICommandManager>,
		Mock<IPlugin>,
		Mock<IPlugin>,
		Mock<IPlugin>
	) CreateStubs()
	{
		Mock<IConfigContext> configContext = new();

		Mock<ICommandManager> commandManager = new();
		commandManager.Setup(cm => cm.GetEnumerator()).Returns(new List<CommandItem>().GetEnumerator());

		configContext.Setup(cc => cc.CommandManager).Returns(commandManager.Object);

		Mock<IPlugin> plugin1 = new();
		plugin1.Setup(p => p.Name).Returns("Plugin1");

		Mock<IPlugin> plugin2 = new();
		plugin2.Setup(p => p.Name).Returns("Plugin2");

		Mock<IPlugin> plugin3 = new();
		plugin3.Setup(p => p.Name).Returns("Plugin3");

		return (configContext, commandManager, plugin1, plugin2, plugin3);
	}

	[Fact]
	public void PreInitialize()
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			Mock<ICommandManager> commandManager,
			Mock<IPlugin> plugin1,
			Mock<IPlugin> plugin2,
			Mock<IPlugin> plugin3
		) = CreateStubs();

		PluginManager pluginManager = new(configContext.Object);
		pluginManager.AddPlugin(plugin1.Object);
		pluginManager.AddPlugin(plugin2.Object);
		pluginManager.AddPlugin(plugin3.Object);

		// When
		pluginManager.PreInitialize();

		// Then
		plugin1.Verify(p => p.PreInitialize(), Times.Once);
		plugin2.Verify(p => p.PreInitialize(), Times.Once);
		plugin3.Verify(p => p.PreInitialize(), Times.Once);
	}

	[Fact]
	public void PostInitialize()
	{
		// Given
		(Mock<IConfigContext> configContext, _, Mock<IPlugin> plugin1, Mock<IPlugin> plugin2, Mock<IPlugin> plugin3) =
			CreateStubs();

		PluginManager pluginManager = new(configContext.Object);
		pluginManager.AddPlugin(plugin1.Object);
		pluginManager.AddPlugin(plugin2.Object);
		pluginManager.AddPlugin(plugin3.Object);

		// When
		pluginManager.PostInitialize();

		// Then
		plugin1.Verify(p => p.PostInitialize(), Times.Once);
		plugin2.Verify(p => p.PostInitialize(), Times.Once);
		plugin3.Verify(p => p.PostInitialize(), Times.Once);
	}

	[Fact]
	public void AddPlugin()
	{
		// Given
		(Mock<IConfigContext> configContext, _, Mock<IPlugin> plugin1, Mock<IPlugin> plugin2, Mock<IPlugin> plugin3) =
			CreateStubs();

		PluginManager pluginManager = new(configContext.Object);

		// When
		pluginManager.AddPlugin(plugin1.Object);
		pluginManager.AddPlugin(plugin2.Object);
		pluginManager.AddPlugin(plugin3.Object);

		// Then
		Assert.Equal(3, pluginManager.LoadedPlugins.Count);
	}

	[Fact]
	public void AddPlugin_DuplicateName()
	{
		// Given
		(Mock<IConfigContext> configContext, _, Mock<IPlugin> plugin1, Mock<IPlugin> plugin2, Mock<IPlugin> plugin3) =
			CreateStubs();

		plugin2.Setup(p => p.Name).Returns("Plugin1");

		PluginManager pluginManager = new(configContext.Object);

		// When
		pluginManager.AddPlugin(plugin1.Object);
		Assert.Throws<InvalidOperationException>(() => pluginManager.AddPlugin(plugin2.Object));

		// Then
		Assert.Equal(1, pluginManager.LoadedPlugins.Count);
	}

	[Fact]
	public void Contains()
	{
		// Given
		(Mock<IConfigContext> configContext, _, Mock<IPlugin> plugin1, Mock<IPlugin> plugin2, Mock<IPlugin> plugin3) =
			CreateStubs();

		PluginManager pluginManager = new(configContext.Object);
		pluginManager.AddPlugin(plugin1.Object);
		pluginManager.AddPlugin(plugin2.Object);
		pluginManager.AddPlugin(plugin3.Object);

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
