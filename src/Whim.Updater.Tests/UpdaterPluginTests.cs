using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppNotifications;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
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

	#region GetNotInstalledReleases
	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async Task GetNotInstalledReleases_WhenNoReleases(IContext ctx, IGitHubClient client)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig());

		// When
		IEnumerable<ReleaseInfo> releases = await plugin.GetNotInstalledReleases(client);

		// Then
		Assert.Empty(releases);
	}

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async Task GetNotInstalledReleases_InvalidVersion(IContext ctx, IGitHubClient client)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig());
		client
			.Repository.Release.GetAll("dalyIsaac", "Whim", Arg.Any<ApiOptions>())
			.Returns([Data.CreateRelease242(tagName: "welp")]);

		// When
		IEnumerable<ReleaseInfo> releases = await plugin.GetNotInstalledReleases(client);

		// Then
		Assert.Empty(releases);
	}

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async Task GetNotInstalledReleases_DifferentChannel(IContext ctx, IGitHubClient client)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig());
		client
			.Repository.Release.GetAll("dalyIsaac", "Whim", Arg.Any<ApiOptions>())
			.Returns([Data.CreateRelease242(tagName: "v0.1.263-beta+bc5c56c4")]);

		// When
		IEnumerable<ReleaseInfo> releases = await plugin.GetNotInstalledReleases(client);

		// Then
		Assert.Empty(releases);
	}

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async Task GetNotInstalledReleases_OlderVersion(IContext ctx, IGitHubClient client)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig());
		client
			.Repository.Release.GetAll("dalyIsaac", "Whim", Arg.Any<ApiOptions>())
			.Returns([Data.CreateRelease242(tagName: "v0.1.261-alpha+bc5c56c4")]);

		// When
		IEnumerable<ReleaseInfo> releases = await plugin.GetNotInstalledReleases(client);

		// Then
		Assert.Empty(releases);
	}

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async Task GetNotInstalledReleases_Ordered(IContext ctx, IGitHubClient client)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig() { ReleaseChannel = ReleaseChannel.Alpha });

		string[] orderedReleases =
		[
			"v0.1.261-alpha+bc5c56c4",
			"v0.1.262-beta+bc5c56c4",
			"v0.1.263-stable+bc5c56c4",
			"v0.1.264-alpha+bc5c56c4",
			"v0.2.265-alpha+bc5c56c4",
			"v1.1.266-alpha+bc5c56c4"
		];
		string[] expectedReleases = ["v0.1.264-alpha+bc5c56c4", "v0.2.265-alpha+bc5c56c4", "v1.1.266-alpha+bc5c56c4"];

		client
			.Repository.Release.GetAll("dalyIsaac", "Whim", Arg.Any<ApiOptions>())
			.Returns(orderedReleases.Select(t => Data.CreateRelease242(tagName: t)).ToArray());

		// When
		IEnumerable<ReleaseInfo> releases = await plugin.GetNotInstalledReleases(client);

		// Then
		releases.Select(r => r.Release.TagName).Should().BeEquivalentTo(expectedReleases);
	}
	#endregion

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

	#region CheckForUpdates
	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async Task CheckForUpdates_WhenNoReleases(IContext ctx, IGitHubClient client)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig());

		// When
		await plugin.CheckForUpdates(client);

		// Then
		ctx.NotificationManager.DidNotReceive().SendToastNotification(Arg.Any<AppNotification>());
	}

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async Task CheckForUpdates_ReleaseIsSkipped(IContext ctx, IGitHubClient client)
	{
		// Given
		Release release = Data.CreateRelease242(tagName: "v0.1.265-alpha+bc5c56c4");
		client.Repository.Release.GetAll("dalyIsaac", "Whim", Arg.Any<ApiOptions>()).Returns([release]);

		UpdaterPlugin plugin = new(ctx, new UpdaterConfig() { ReleaseChannel = ReleaseChannel.Alpha });
		plugin.SkipRelease(release);

		// When
		await plugin.CheckForUpdates(client);

		// Then
		ctx.NotificationManager.DidNotReceive().SendToastNotification(Arg.Any<AppNotification>());
	}
	#endregion

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public void SkipRelease(IContext ctx)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig());
		Release release = Data.CreateRelease242();

		// When
		plugin.SkipRelease(release);

		// Then
		Assert.Equal(release.TagName, plugin.SkippedReleaseTagName);
	}

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public void SaveState(IContext ctx)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig());
		Release skippedRelease = Data.CreateRelease242();
		plugin.SkipRelease(skippedRelease);

		// When
		JsonElement? json = plugin.SaveState();

		// Then
		Assert.NotNull(json);
		Assert.Equal(skippedRelease.TagName, json.Value.GetProperty("SkippedReleaseTagName").GetString());
		Assert.Null(json.Value.GetProperty("LastCheckedForUpdates").GetString());
	}

	#region InstallRelease
	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async Task InstallRelease_NoAssets(IContext ctx)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig());
		Release release = Data.CreateRelease242(assets: []);

		// When
		await plugin.InstallRelease(release);

		// Then
		ctx.NativeManager.DidNotReceive().TryEnqueue(Arg.Any<DispatcherQueueHandler>());
	}

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async Task InstallRelease_DownloadThrows(IContext ctx)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig());
		Release release = Data.CreateRelease242();

		ctx.NativeManager.DownloadFileAsync(Arg.Any<Uri>(), Arg.Any<string>()).ThrowsAsync(new Exception());

		// When
		await plugin.InstallRelease(release);

		// Then
		await ctx.NativeManager.Received(1).DownloadFileAsync(Arg.Any<Uri>(), Arg.Any<string>());
		ctx.NativeManager.DidNotReceive().TryEnqueue(Arg.Any<DispatcherQueueHandler>());
	}

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async Task InstallRelease_InstallerExitsWithNonZeroCode(IContext ctx)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig());
		Release release = Data.CreateRelease242();

		ctx.NativeManager.DownloadFileAsync(Arg.Any<Uri>(), Arg.Any<string>()).Returns(Task.CompletedTask);
		ctx.NativeManager.RunFileAsync(Arg.Any<string>()).Returns(1);

		// When
		await plugin.InstallRelease(release);

		// Then
		await ctx.NativeManager.Received(1).DownloadFileAsync(Arg.Any<Uri>(), Arg.Any<string>());
		ctx.NativeManager.DidNotReceive().TryEnqueue(Arg.Any<DispatcherQueueHandler>());
	}

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async Task InstallRelease_InstallerThrows(IContext ctx)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig());
		Release release = Data.CreateRelease242();

		ctx.NativeManager.DownloadFileAsync(Arg.Any<Uri>(), Arg.Any<string>()).Returns(Task.CompletedTask);
		ctx.NativeManager.RunFileAsync(Arg.Any<string>()).ThrowsAsync(new Exception());

		// When
		await plugin.InstallRelease(release);

		// Then
		await ctx.NativeManager.Received(1).DownloadFileAsync(Arg.Any<Uri>(), Arg.Any<string>());
		await ctx.NativeManager.Received(1).RunFileAsync(Arg.Any<string>());
		ctx.NativeManager.DidNotReceive().TryEnqueue(Arg.Any<DispatcherQueueHandler>());
	}

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async Task InstallRelease_InstallerExitsWithZeroCode(IContext ctx)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig());
		Release release = Data.CreateRelease242();

		ctx.NativeManager.DownloadFileAsync(Arg.Any<Uri>(), Arg.Any<string>()).Returns(Task.CompletedTask);
		ctx.NativeManager.RunFileAsync(Arg.Any<string>()).Returns(0);

		// When
		await plugin.InstallRelease(release);

		// Then
		await ctx.NativeManager.Received(1).DownloadFileAsync(Arg.Any<Uri>(), Arg.Any<string>());
		await ctx.NativeManager.Received(1).RunFileAsync(Arg.Any<string>());
		ctx.NativeManager.Received(1).TryEnqueue(Arg.Any<DispatcherQueueHandler>());
	}
	#endregion

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
