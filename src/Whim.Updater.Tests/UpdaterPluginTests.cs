using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using AutoFixture;
using Microsoft.Windows.AppNotifications;
using NSubstitute;
using Octokit;
using Whim.TestUtils;
using Xunit;

namespace Whim.Updater.Tests;

public class UpdaterPluginCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IContext ctx = fixture.Freeze<IContext>();
		ctx.NativeManager.GetWhimVersion().Returns("v0.1.263-alpha+bc5c56c4");
	}
}

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
[SuppressMessage("Usage", "CA2007:Consider calling ConfigureAwait on the awaited task")]
public class UpdaterPluginTests
{
	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public void PreInitialize(IContext ctx)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig());

		// When
		plugin.PreInitialize();

		// Then
		Assert.Equal("whim.updater", plugin.Name);
		ctx.NotificationManager.Received(1)
			.Register("whim.updater.show_window", Arg.Any<Action<AppNotificationActivatedEventArgs>>());
		ctx.NotificationManager.Received(1)
			.Register("whim.updater.cancel", Arg.Any<Action<AppNotificationActivatedEventArgs>>());
	}

	#region LoadState
	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public void LoadState_InvalidFormat(IContext ctx)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig());
		JsonElement json = JsonDocument.Parse("[]").RootElement;

		// When
		plugin.LoadState(json);

		// Then
		Assert.Null(plugin.LastCheckedForUpdates);
	}

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public void LoadState_Valid(IContext ctx)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig());
		JsonElement json = JsonDocument
			.Parse(
				"{\"SkippedReleaseTagName\":\"v0.1.263-alpha+bc5c56c4\",\"LastCheckedForUpdates\":\"2021-09-05T21:00:00.0000000Z\"}"
			)
			.RootElement;

		// When
		plugin.LoadState(json);

		// Then
		Assert.Equal("v0.1.263-alpha+bc5c56c4", plugin.SkippedReleaseTagName);
		Assert.NotNull(plugin.LastCheckedForUpdates);
	}
	#endregion

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public void SkipRelease(IContext ctx)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig());
		Release release = Data.CreateRelease242();

		// When
		plugin.SkipRelease(release.TagName);

		// Then
		Assert.Equal(release.TagName, plugin.SkippedReleaseTagName);
		ctx.Store.Received(1).Dispatch(Arg.Any<SaveStateTransform>());
	}

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public void SaveState(IContext ctx)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig());
		Release skippedRelease = Data.CreateRelease242();
		plugin.SkipRelease(skippedRelease.TagName);

		// When
		JsonElement? json = plugin.SaveState();

		// Then
		Assert.NotNull(json);
		Assert.Equal(skippedRelease.TagName, json.Value.GetProperty("SkippedReleaseTagName").GetString());
		Assert.Null(json.Value.GetProperty("LastCheckedForUpdates").GetString());
	}

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public void Dispose(IContext ctx)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig());

		// When
		plugin.Dispose();

		// Then
		ctx.NotificationManager.Received(1).Unregister("whim.updater.show_window");
		ctx.NotificationManager.Received(1).Unregister("whim.updater.cancel");
	}

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public void PluginCommands(IContext ctx)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig());

		// When
		IPluginCommands pluginCommands = plugin.PluginCommands;

		// Then
		Assert.Single(pluginCommands.Commands);
	}
}
