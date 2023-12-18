using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using AutoFixture;
using FluentAssertions;
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
		IUpdaterPlugin plugin = new UpdaterPlugin(ctx, new UpdaterConfig());

		// When
		plugin.PreInitialize();

		// Then
		Assert.Equal("whim.updater", plugin.Name);
		ctx.NotificationManager
			.Received(1)
			.Register("whim.updater.show_window", Arg.Any<Action<AppNotificationActivatedEventArgs>>());
		ctx.NotificationManager
			.Received(1)
			.Register("whim.updater.cancel", Arg.Any<Action<AppNotificationActivatedEventArgs>>());
	}

	#region GetNotInstalledReleases
	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async void GetNotInstalledReleases_WhenNoReleases(IContext ctx, IGitHubClient client)
	{
		// Given
		IUpdaterPlugin plugin = new UpdaterPlugin(ctx, new UpdaterConfig());

		// When
		IEnumerable<ReleaseInfo> releases = await plugin.GetNotInstalledReleases(client);

		// Then
		Assert.Empty(releases);
	}

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async void GetNotInstalledReleases_InvalidVersion(IContext ctx, IGitHubClient client)
	{
		// Given
		IUpdaterPlugin plugin = new UpdaterPlugin(ctx, new UpdaterConfig());
		client
			.Repository
			.Release
			.GetAll("dalyIsaac", "Whim", Arg.Any<ApiOptions>())
			.Returns(new[] { Data.CreateRelease242(tagName: "welp") });

		// When
		IEnumerable<ReleaseInfo> releases = await plugin.GetNotInstalledReleases(client);

		// Then
		Assert.Empty(releases);
	}

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async void GetNotInstalledReleases_DifferentChannel(IContext ctx, IGitHubClient client)
	{
		// Given
		IUpdaterPlugin plugin = new UpdaterPlugin(ctx, new UpdaterConfig());
		client
			.Repository
			.Release
			.GetAll("dalyIsaac", "Whim", Arg.Any<ApiOptions>())
			.Returns(new[] { Data.CreateRelease242(tagName: "v0.1.263-beta+bc5c56c4") });

		// When
		IEnumerable<ReleaseInfo> releases = await plugin.GetNotInstalledReleases(client);

		// Then
		Assert.Empty(releases);
	}

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async void GetNotInstalledReleases_OlderVersion(IContext ctx, IGitHubClient client)
	{
		// Given
		IUpdaterPlugin plugin = new UpdaterPlugin(ctx, new UpdaterConfig());
		client
			.Repository
			.Release
			.GetAll("dalyIsaac", "Whim", Arg.Any<ApiOptions>())
			.Returns(new[] { Data.CreateRelease242(tagName: "v0.1.261-alpha+bc5c56c4") });

		// When
		IEnumerable<ReleaseInfo> releases = await plugin.GetNotInstalledReleases(client);

		// Then
		Assert.Empty(releases);
	}

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async void GetNotInstalledReleases_Ordered(IContext ctx, IGitHubClient client)
	{
		// Given
		IUpdaterPlugin plugin = new UpdaterPlugin(ctx, new UpdaterConfig() { ReleaseChannel = ReleaseChannel.Alpha });

		string[] orderedReleases =
		{
			"v0.1.261-alpha+bc5c56c4",
			"v0.1.262-beta+bc5c56c4",
			"v0.1.263-stable+bc5c56c4",
			"v0.1.264-alpha+bc5c56c4",
			"v0.2.265-alpha+bc5c56c4",
			"v1.1.266-alpha+bc5c56c4"
		};
		string[] expectedReleases = new[]
		{
			"v0.1.264-alpha+bc5c56c4",
			"v0.2.265-alpha+bc5c56c4",
			"v1.1.266-alpha+bc5c56c4"
		};

		client
			.Repository
			.Release
			.GetAll("dalyIsaac", "Whim", Arg.Any<ApiOptions>())
			.Returns(orderedReleases.Select(Data.CreateRelease242).ToArray());

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
		IUpdaterPlugin plugin = new UpdaterPlugin(ctx, new UpdaterConfig());
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
		IUpdaterPlugin plugin = new UpdaterPlugin(ctx, new UpdaterConfig());
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
	public async void CheckForUpdates_WhenNoReleases(IContext ctx, IGitHubClient client)
	{
		// Given
		IUpdaterPlugin plugin = new UpdaterPlugin(ctx, new UpdaterConfig());

		// When
		await plugin.CheckForUpdates(client);

		// Then
		ctx.NotificationManager.DidNotReceive().SendToastNotification(Arg.Any<AppNotification>());
	}

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async void CheckForUpdates_ReleaseIsSkipped(IContext ctx, IGitHubClient client)
	{
		// Given
		IUpdaterPlugin plugin = new UpdaterPlugin(ctx, new UpdaterConfig());
		client
			.Repository
			.Release
			.GetAll("dalyIsaac", "Whim", Arg.Any<ApiOptions>())
			.Returns(new[] { Data.CreateRelease242(tagName: "v0.1.263-alpha+bc5c56c4") });

		plugin.LoadState(
			JsonDocument
				.Parse(
					"{\"SkippedReleaseTagName\":\"v0.1.263-alpha+bc5c56c4\",\"LastCheckedForUpdates\":\"2021-09-05T21:00:00.0000000Z\"}"
				)
				.RootElement
		);

		// When
		await plugin.CheckForUpdates(client);

		// Then
		ctx.NotificationManager.DidNotReceive().SendToastNotification(Arg.Any<AppNotification>());
	}
	#endregion
}
